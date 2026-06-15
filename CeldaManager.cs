using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    /// <summary>
    /// Gestiona la comunicación serial con las celdas de carga a través del puerto RS-485.
    /// Implementa el protocolo de comunicación HBM para celdas C16iC3.
    /// 
    /// Formato de comandos:
    /// - Envío: S{dir:D2};{COMANDO};\
    /// - Respuesta: valor numérico o cadena de identificación
    /// 
    /// Las direcciones válidas son 0-15 (la 98 es especial para asignación).
    /// 
    /// Soporta dos modos de calibración:
    /// - Simple: factor único por celda (peso = raw * factor)
    /// - Multivariable: modelo lineal PESO = X1*m1 + X2*m2 + X3*m3 + X4*m4 + B
    /// </summary>
    // MODIFICADO: cambiado de internal a public para que ViewCeldas (público)
    //             pueda recibir el manager en su constructor.
    public class CeldaManager
    {
        // Puerto serial para comunicación con las celdas
        private SerialPort puerto;

        // Diccionario de factores de calibración por dirección de celda (modo simple)
        private Dictionary<int, double> factoresCalibracion = new Dictionary<int, double>();

        // Motor de calibración multivariable (modo avanzado)
        private CalibracionLineal calibracionMultivariable;

        // Motor de calibración matricial para corrección de excentricidad
        private BalanzaMatricial calibracionMatricial;

        // Campos para compensación de esquinas (excentricidad)
        private double[] cerosCompensacion = new double[4];
        private double[] factoresCompensacion = new double[4] { 1.0, 1.0, 1.0, 1.0 };
        public bool UsarCompensacionEsquinas { get; private set; } = false;

        /// <summary>
        /// Obtiene el motor de calibración multivariable activo.
        /// </summary>
        public CalibracionLineal CalibracionMultivariable => calibracionMultivariable;

        /// <summary>
        /// Obtiene el motor de calibración matricial activo.
        /// </summary>
        public BalanzaMatricial CalibracionMatricial => calibracionMatricial;

        /// <summary>
        /// Indica qué modo de calibración está activo.
        /// true = multivariable (m1..m4, B), false = simple (factor por celda).
        /// </summary>
        public bool UsarCalibracionMultivariable { get; set; } = false;

        /// <summary>
        /// Indica si la calibración matricial está activa.
        /// </summary>
        public bool UsarCalibracionMatricial { get; set; } = false;

        /// <summary>
        /// Diccionario de celdas detectadas en el bus, indexadas por dirección esclavo.
        /// </summary>
        public Dictionary<int, CeldaInfo> Celdas { get; private set; } = new Dictionary<int, CeldaInfo>();

        // Objeto de bloqueo para acceso seguro al puerto serial desde múltiples hilos
        private readonly object serialLock = new object();

        // Bandera volátil para indicar que hay un comando en progreso,
        // evita que el evento DataReceived interfiera con la respuesta del comando
        private volatile bool comandoEnProgreso = false;

        /// <summary>
        /// Evento disparado cuando se recibe una trama de datos del puerto serial.
        /// </summary>
        public event Action<string> TramaRecibida;

        /// <summary>
        /// Evento disparado cuando se actualiza el peso de una celda.
        /// Proporciona la dirección de la celda y el peso calibrado.
        /// </summary>
        public event Action<int, double> PesoActualizado;

        /// <summary>
        /// Evento disparado cuando se completa la enumeración de celdas en el bus.
        /// </summary>
        public event Action<List<CeldaInfo>> CeldasEnumeradas;

        /// <summary>
        /// Indica si el puerto serial está actualmente abierto y operativo.
        /// </summary>
        public bool IsOpen => puerto != null && puerto.IsOpen;

        /// <summary>
        /// Configura el motor de calibración multivariable con los coeficientes resueltos.
        /// Activa el modo de calibración multivariable automáticamente.
        /// </summary>
        /// <param name="m1">Coeficiente de celda 1.</param>
        /// <param name="m2">Coeficiente de celda 2.</param>
        /// <param name="m3">Coeficiente de celda 3.</param>
        /// <param name="m4">Coeficiente de celda 4.</param>
        /// <param name="b">Bias del sistema.</param>
        public void ConfigurarCalibracionMultivariable(double m1, double m2, double m3, double m4, double b)
        {
            calibracionMultivariable = new CalibracionLineal(m1, m2, m3, m4, b);
            UsarCalibracionMultivariable = true;
            UsarCompensacionEsquinas = false;
        }

        /// <summary>
        /// Desactiva la calibración multivariable y vuelve al modo simple.
        /// </summary>
        /// <summary>
        /// Configura la compensación de esquinas (excentricidad) con los valores
        /// de cero y factores calculados.
        /// Desactiva los otros modos de calibración (multivariable y matricial).
        /// </summary>
        public void ConfigurarCompensacionEsquinas(double[] ceros, double[] factores)
        {
            if (ceros == null || ceros.Length != 4)
                throw new ArgumentException("Debe proporcionar 4 valores de cero.");
            if (factores == null || factores.Length != 4)
                throw new ArgumentException("Debe proporcionar 4 factores de corrección.");

            cerosCompensacion = (double[])ceros.Clone();
            factoresCompensacion = (double[])factores.Clone();
            UsarCompensacionEsquinas = true;
            UsarCalibracionMultivariable = false;
            UsarCalibracionMatricial = false;
        }

        /// <summary>
        /// Conecta al puerto serial de la balanza con configuración predeterminada:
        /// 9600 baud, 8 bits de datos, sin paridad, 1 bit de parada (9600,8,N,1).
        /// </summary>
        /// <param name="nombrePuerto">Nombre del puerto COM (ej: COM1, COM3).</param>
        public void Conectar(string nombrePuerto)
        {
            try
            {
                puerto = new SerialPort(nombrePuerto, 9600, Parity.None, 8, StopBits.One);
                puerto.ReadTimeout = 2000;
                puerto.WriteTimeout = 2000;
                puerto.DataReceived += Puerto_DataReceived;
                puerto.Open();
                TramaRecibida?.Invoke($"Conectado a {nombrePuerto}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Desconecta el puerto serial y libera los recursos asociados.
        /// </summary>
        public void Desconectar()
        {
            try
            {
                if (puerto != null && puerto.IsOpen)
                {
                    puerto.DataReceived -= Puerto_DataReceived;
                    puerto.Close();
                    puerto.Dispose();
                    puerto = null;
                    TramaRecibida?.Invoke("Desconectado");
                }

                // Limpiar el diccionario de celdas para evitar datos fantasma
                // en reconexiones posteriores.
                Celdas.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desconectar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Establece el factor de calibración para una celda específica.
        /// </summary>
        /// <param name="direccion">Dirección de la celda (1-15).</param>
        /// <param name="factor">Factor de calibración a aplicar (peso conocido / peso raw).</param>
        public void SetFactorCalibracion(int direccion, double factor)
        {
            factoresCalibracion[direccion] = factor;
        }

        /// <summary>
        /// Obtiene el factor de calibración de una celda.
        /// Retorna 1.0 (sin calibración) si no hay un factor configurado para esa celda.
        /// </summary>
        /// <param name="direccion">Dirección de la celda (1-15).</param>
        /// <returns>Factor de calibración, o 1.0 por defecto.</returns>
        public double GetFactorCalibracion(int direccion)
        {
            return factoresCalibracion.ContainsKey(direccion) ? factoresCalibracion[direccion] : 1.0;
        }

        /// <summary>
        /// Maneja el evento de recepción de datos asíncrona desde el puerto serial.
        /// Ignora los datos entrantes si hay un comando en progreso para evitar
        /// interferencias con el patrón comando-respuesta (el comando se encarga
        /// de leer su propia respuesta).
        /// </summary>
        private void Puerto_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Si hay un comando en progreso, ignorar datos entrantes
            // para no consumir la respuesta que EnviarComando está esperando
            if (comandoEnProgreso) return;

            try
            {
                string trama = puerto.ReadExisting();
                string limpia = LimpiarTrama(trama);

                if (!string.IsNullOrEmpty(limpia))
                {
                    TramaRecibida?.Invoke(limpia);
                }
            }
            catch (Exception ex)
            {
                TramaRecibida?.Invoke("ERROR: " + ex.Message);
            }
        }

        /// <summary>
        /// Limpia una trama eliminando caracteres de control y espacios en blanco.
        /// </summary>
        /// <param name="trama">Trama cruda recibida del puerto serial.</param>
        /// <returns>Trama limpia sin caracteres \r, \n, \0.</returns>
        public string LimpiarTrama(string trama)
        {
            if (string.IsNullOrEmpty(trama))
                return string.Empty;

            trama = trama.Trim();
            trama = trama.Replace("\r", "").Replace("\n", "").Replace("\0", "");

            return trama;
        }

        /// <summary>
        /// Lee el puerto durante una ventana de tiempo acumulando todos los fragmentos recibidos.
        /// En RS-485 las celdas responden escalonadas, por eso un solo ReadExisting puede dejar fuera S02/S03.
        /// </summary>
        private string LeerRespuestaAcumulada(int timeoutMs, int quietMs = 150)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int elapsed = 0;
            int quiet = 0;

            while (elapsed < timeoutMs)
            {
                string chunk = puerto.ReadExisting();
                if (!string.IsNullOrEmpty(chunk))
                {
                    sb.Append(chunk);
                    quiet = 0;
                }
                else
                {
                    quiet += 25;
                }

                if (sb.Length > 0 && quiet >= quietMs)
                    break;

                Thread.Sleep(25);
                elapsed += 25;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Extrae el peso desde una respuesta en formato HBM (signo + 7 dígitos, centésimas de kg).
        /// La respuesta típica es: S98MSV?S00 00057208  →  extrae " 0057208" → 572.08 kg
        /// Formato: [espacio o -] + 7 dígitos, en centésimas de kilogramo.
        /// Busca el patrón al FINAL de la trama para ignorar eco de comandos.
        /// </summary>
        /// <param name="trama">Trama completa recibida del puerto serial.</param>
        /// <returns>Peso en kg extraído, o 0 si no se pudo parsear.</returns>
        public double ExtraerPesoHBM(string trama)
        {
            if (string.IsNullOrEmpty(trama))
                return 0;

            // Patrón HBM: signo (+/-/espacio) + 7 dígitos al final (centésimas de kg)
            Match match = Regex.Match(trama, @"([-+])(\d{7})$");
            if (match.Success && int.TryParse(match.Groups[2].Value, out int digitos))
            {
                double peso = digitos / 100.0;
                if (match.Groups[1].Value == "-")
                    peso = -peso;
                return peso;
            }

            // Fallback: si encuentra un número de 6+ dígitos al final, son centésimas
            // (respuesta HBM sin eco). Si tiene decimales, es kg directo.
            MatchCollection matches = Regex.Matches(trama, @"[-+]?\d+\.?\d*");
            if (matches.Count > 0)
            {
                Match ultimo = matches[matches.Count - 1];
                if (double.TryParse(ultimo.Value, out double valor))
                {
                    string num = ultimo.Value.Replace("+", "").Replace("-", "");
                    int dot = num.IndexOf('.');
                    if (dot < 0 && num.Length >= 6)
                        return valor / 100.0;
                    return valor;
                }
            }

            return 0;
        }

        /// <summary>
        /// Parsea una respuesta multi-celda del hardware real HBM.
        /// Formato esperado: "S00;MSV?;+0000000\r\nS01;MSV?;-0029677\r\nS02;MSV?;-0191501\r\nS03;MSV?;+0037845\r\n"
        /// Retorna un diccionario: addr -> peso_raw_kg
        /// </summary>
        public Dictionary<int, double> ExtraerMultiplesPesosHBM(string trama)
        {
            var resultados = new Dictionary<int, double>();
            if (string.IsNullOrEmpty(trama)) return resultados;

            // Buscar TODAS las ocurrencias de S{addr};...;{signo}{7digitos}
            // Esto funciona tanto con \r\n separando celdas como sin ellos
            // (porque LimpiarTrama ya removió los \r\n antes de llegar aquí)
            MatchCollection matches = Regex.Matches(trama, @"S(\d{2});.*?([-+])(\d{7})");
            foreach (Match m in matches)
            {
                if (int.TryParse(m.Groups[1].Value, out int addr) && int.TryParse(m.Groups[3].Value, out int digitos))
                {
                    double peso = digitos / 100.0;
                    if (m.Groups[2].Value == "-") peso = -peso;
                    resultados[addr] = peso;
                }
            }
            return resultados;
        }

        /// <summary>
        /// Envía un comando a una celda específica y espera la respuesta.
        /// Formato del comando: S{dir:D2}{COMANDO}\r\n
        /// 
        /// La operación está sincronizada con un lock para evitar accesos concurrentes
        /// al puerto serial desde el hilo de la UI y el hilo de DataReceived.
        /// </summary>
        /// <param name="direccion">Dirección de la celda destino (1-15, o 98 para comandos especiales).</param>
        /// <param name="comando">Comando a enviar (ej: MSV? para consultar peso, IDN? para número de serie).</param>
        /// <returns>Respuesta de la celda, o null si el puerto no está abierto.</returns>
        public string EnviarComando(int direccion, string comando)
        {
            if (puerto == null || !puerto.IsOpen)
                return null;

            // Bloquear el puerto serial para evitar acceso concurrente
            lock (serialLock)
            {
                try
                {
                    comandoEnProgreso = true;

                    // Formato HBM real (sin punto y coma extra antes de CRLF)
                    string comandoCompleto = $"S{direccion:D2};{comando};\r\n";

                    puerto.DiscardInBuffer();
                    puerto.Write(comandoCompleto);

                    // Esperar respuesta acumulada de la celda
                    string respuesta = LeerRespuestaAcumulada(900);
                    string limpia = LimpiarTrama(respuesta);

                    TramaRecibida?.Invoke($"Enviado: {comandoCompleto.Trim()} | Respuesta: {limpia}");

                    return limpia;
                }
                catch (Exception ex)
                {
                    return $"ERROR: {ex.Message}";
                }
                finally
                {
                    comandoEnProgreso = false;
                }
            }
        }

        /// <summary>
        /// Consulta el peso actual de una celda específica.
        /// Comando enviado: S{dir:D2};MSV?;\
        /// Respuesta esperada: valor numérico del peso en unidades internas.
        /// 
        /// En modo simple: retorna raw * factor de calibración.
        /// En modo multivariable: retorna el valor raw (el peso total se calcula con CalcularPesoMultivariable).
        /// </summary>
        /// <param name="direccion">Dirección de la celda (1-15).</param>
        /// <returns>Peso calibrado de la celda en kg (modo simple) o valor raw (modo multivariable).</returns>
        public double ConsultarPeso(int direccion)
        {
            string respuesta = EnviarComando(direccion, "MSV?");

            var multi = ExtraerMultiplesPesosHBM(respuesta);

            if (multi.Count > 0)
            {
                foreach (var kvp in multi)
                {
                    int addr = kvp.Key;
                    double raw = kvp.Value;

                    if (!Celdas.ContainsKey(addr))
                        Celdas[addr] = new CeldaInfo { SlaveNumber = addr };

                    Celdas[addr].RawWeight = raw;
                    Celdas[addr].LastRead = DateTime.Now;
                    Celdas[addr].Connected = true;

                    if (UsarCalibracionMultivariable && calibracionMultivariable != null)
                        Celdas[addr].CalibratedWeight = raw;
                    else
                        Celdas[addr].CalibratedWeight = raw * GetFactorCalibracion(addr);

                    PesoActualizado?.Invoke(addr, Celdas[addr].CalibratedWeight);
                }

                if (multi.ContainsKey(direccion))
                    return Celdas[direccion].CalibratedWeight;

                return 0;
            }

            double rawWeight = ExtraerPesoHBM(respuesta);

            bool respuestaValida = !string.IsNullOrEmpty(respuesta)
                                   && !respuesta.StartsWith("ERROR")
                                   && !respuesta.StartsWith("?")
                                   && Regex.IsMatch(respuesta, @"\d{4,}");

            if (respuestaValida)
            {
                if (!Celdas.ContainsKey(direccion))
                    Celdas[direccion] = new CeldaInfo { SlaveNumber = direccion };

                Celdas[direccion].RawWeight = rawWeight;
                Celdas[direccion].LastRead = DateTime.Now;
                Celdas[direccion].Connected = true;

                if (UsarCalibracionMultivariable && calibracionMultivariable != null)
                    Celdas[direccion].CalibratedWeight = rawWeight;
                else
                    Celdas[direccion].CalibratedWeight = rawWeight * GetFactorCalibracion(direccion);

                PesoActualizado?.Invoke(direccion, Celdas[direccion].CalibratedWeight);

                return Celdas[direccion].CalibratedWeight;
            }

            return 0;
        }

        /// <summary>
        /// Calcula el peso total del sistema usando el modelo multivariable.
        /// PESO = X1*m1 + X2*m2 + X3*m3 + X4*m4 + B
        /// 
        /// Las lecturas raw se toman de las celdas en las direcciones 1, 2, 3, 4.
        /// </summary>
        /// <returns>Peso total calculado con el modelo multivariable, o 0 si no está calibrado.</returns>
        public double CalcularPesoMultivariable()
        {
            if (!UsarCalibracionMultivariable || calibracionMultivariable == null || !calibracionMultivariable.EstaCalibrado)
                return 0;

            try
            {
                var celdas = ObtenerCeldasOrdenadas();
                double x1 = celdas.Count > 0 ? celdas[0].RawWeight : 0;
                double x2 = celdas.Count > 1 ? celdas[1].RawWeight : 0;
                double x3 = celdas.Count > 2 ? celdas[2].RawWeight : 0;
                double x4 = celdas.Count > 3 ? celdas[3].RawWeight : 0;

                return calibracionMultivariable.PesoCalculado(x1, x2, x3, x4);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Consulta el número de serie y datos de identificación de una celda.
        /// Comando enviado: S{dir:D2};IDN?;\
        /// Respuesta esperada: HBM,C16iC3/40t,{NRO_SERIE},P52
        /// </summary>
        /// <param name="direccion">Dirección de la celda (1-15).</param>
        /// <returns>Cadena completa con la respuesta de identificación.</returns>
        public string ConsultarSerie(int direccion)
        {
            string respuesta = EnviarComando(direccion, "IDN?");
            return respuesta;
        }

        /// <summary>
        /// Inicializa las celdas S00-S03 y les consulta MSV? como alternativa temporal.
        /// Sin broadcast, sin IDN, sin esperas largas.
        /// Crea las entradas en el diccionario Celdas si no existen y las marca como Connected.
        /// </summary>
        /// <returns>Lista de celdas inicializadas.</returns>
        public List<CeldaInfo> InicializarCeldasTemporal()
        {
            var celdasInicializadas = new List<CeldaInfo>();

            for (int addr = 0; addr <= 3; addr++)
            {
                if (!Celdas.ContainsKey(addr))
                    Celdas[addr] = new CeldaInfo { SlaveNumber = addr };

                Celdas[addr].Connected = true;
                Celdas[addr].LastRead = DateTime.Now;

                ConsultarPeso(addr);

                celdasInicializadas.Add(Celdas[addr]);
                TramaRecibida?.Invoke($"Celda temporal #{addr:D2} - peso={Celdas[addr].RawWeight}");
            }

            CeldasEnumeradas?.Invoke(celdasInicializadas);
            return celdasInicializadas;
        }

        /// <summary>
        /// Obtiene el peso total del sistema.
        /// 
        /// En modo multivariable: aplica la fórmula PESO = X1*m1 + X2*m2 + X3*m3 + X4*m4 + B
        /// usando las lecturas raw de las 4 celdas.
        /// 
        /// En modo simple: suma los pesos calibrados de todas las celdas conectadas.
        /// </summary>
        /// <returns>Peso total del sistema en kg.</returns>
        /// <summary>Obtiene hasta 4 celdas conectadas ordenadas por dirección esclavo ascendente.</summary>
        private List<CeldaInfo> ObtenerCeldasOrdenadas()
        {
            return Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .Take(4)
                .ToList();
        }

        public double ObtenerPesoUnificado()
        {
            var celdasOrdenadas = ObtenerCeldasOrdenadas();

            // Usar compensación de esquinas si está activa (tiene prioridad)
            if (UsarCompensacionEsquinas)
            {
                double peso = 0;
                for (int i = 0; i < 4 && i < celdasOrdenadas.Count; i++)
                {
                    double raw = celdasOrdenadas[i].RawWeight;
                    double neto = raw - cerosCompensacion[i];
                    peso += neto * factoresCompensacion[i];
                }
                return peso;
            }

            // Usar modelo matricial si está activo (corrección de excentricidad)
            if (UsarCalibracionMatricial && calibracionMatricial != null && calibracionMatricial.EstaCalibrado)
            {
                double[] lecturas = new double[4];
                for (int i = 0; i < 4 && i < celdasOrdenadas.Count; i++)
                {
                    lecturas[i] = celdasOrdenadas[i].CalibratedWeight;
                }
                return calibracionMatricial.ObtenerPesoCorregido(lecturas);
            }

            // Usar modelo multivariable si está activo
            if (UsarCalibracionMultivariable && calibracionMultivariable != null && calibracionMultivariable.EstaCalibrado)
            {
                return CalcularPesoMultivariable();
            }

            // Modo simple: sumar pesos calibrados individuales
            double pesoTotal = 0;

            foreach (var celda in Celdas.Values)
            {
                if (celda.Connected)
                {
                    pesoTotal += celda.CalibratedWeight;
                }
            }

            return pesoTotal;
        }

    }
}
