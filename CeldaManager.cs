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
    /// Las direcciones válidas son 1-15 (la dirección 0 no se usa, la 98 es especial para asignación).
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
        public void DesactivarCalibracionMultivariable()
        {
            calibracionMultivariable = null;
            UsarCalibracionMultivariable = false;
        }

        /// <summary>
        /// Configura la calibración matricial con los coeficientes resueltos.
        /// Activa el modo de calibración matricial automáticamente.
        /// </summary>
        public void ConfigurarCalibracionMatricial(double[] coeficientes)
        {
            if (coeficientes == null || coeficientes.Length != 4)
                throw new ArgumentException("Debe proporcionar 4 coeficientes de corrección.");

            calibracionMatricial = new BalanzaMatricial();
            calibracionMatricial.EstablecerCoeficientes(coeficientes);
            UsarCalibracionMatricial = true;
            UsarCalibracionMultivariable = false;
        }

        /// <summary>
        /// Desactiva la calibración matricial y vuelve al modo simple.
        /// </summary>
        public void DesactivarCalibracionMatricial()
        {
            calibracionMatricial = null;
            UsarCalibracionMatricial = false;
        }

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
        /// Desactiva la compensación de esquinas y vuelve al modo simple.
        /// </summary>
        public void DesactivarCompensacionEsquinas()
        {
            UsarCompensacionEsquinas = false;
        }

        public double[] ObtenerCerosCompensacion()
        {
            return (double[])cerosCompensacion.Clone();
        }

        public double[] ObtenerFactoresCompensacion()
        {
            return (double[])factoresCompensacion.Clone();
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
        /// Extrae el primer valor numérico (incluyendo signo negativo) encontrado
        /// en una trama de texto usando una expresión regular.
        /// </summary>
        /// <param name="trama">Trama de texto que contiene un valor numérico.</param>
        /// <returns>Valor numérico extraído, o 0 si no se encuentra ningún número.</returns>
        public double ExtraerValorNumerico(string trama)
        {
            if (string.IsNullOrEmpty(trama))
                return 0;

            MatchCollection matches = Regex.Matches(trama, @"[-+]?\d+\.?\d*");
            if (matches.Count > 0)
            {
                Match ultimo = matches[matches.Count - 1];
                if (double.TryParse(ultimo.Value, out double valor))
                    return valor;
            }

            return 0;
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

            // Buscar patrón HBM al final: espacio o guión + 7 dígitos
            Match match = Regex.Match(trama, @"([- ])(\d{7})$");
            if (match.Success && int.TryParse(match.Groups[2].Value, out int digitos))
            {
                double peso = digitos / 100.0;
                if (match.Groups[1].Value == "-")
                    peso = -peso;
                return peso;
            }

            // Fallback: buscar cualquier número al final de la trama
            MatchCollection matches = Regex.Matches(trama, @"[-+]?\d+\.?\d*");
            if (matches.Count > 0)
            {
                Match ultimo = matches[matches.Count - 1];
                if (double.TryParse(ultimo.Value, out double valor))
                    return valor;
            }

            return 0;
        }

        /// <summary>
        /// Envía un comando a una celda específica y espera la respuesta.
        /// Formato del comando: S{dir:D2};{COMANDO};\
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

                    // Formato clásico HBM: S01;MSV?;
                    string comandoCompleto = $"S{direccion:D2};{comando};\r\n";

                    // Esperar a que el bus esté libre antes de enviar
                    Thread.Sleep(100);
                    puerto.DiscardInBuffer();
                    puerto.Write(comandoCompleto);

                    // Esperar la respuesta de la celda
                    Thread.Sleep(200);
                    string respuesta = puerto.ReadExisting();
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
        /// Consulta el peso de una celda usando el protocolo multi-línea HBM.
        /// Envía 3 mensajes consecutivos: S98 -> MSV? -> S{dir}
        /// Esto es requerido para compatibilidad con ciertos firmwares HBM.
        /// </summary>
        /// <param name="direccion">Dirección de la celda (0-15).</param>
        /// <returns>Peso calibrado de la celda en kg.</returns>
        public double ConsultarPesoMultiLinea(int direccion)
        {
            if (puerto == null || !puerto.IsOpen)
                return 0;

            string limpia = null;
            double rawWeight = 0;
            string errorMsg = null;

            // Solo el trabajo serial va dentro del lock
            lock (serialLock)
            {
                try
                {
                    comandoEnProgreso = true;

                    Thread.Sleep(100);
                    puerto.DiscardInBuffer();
                    puerto.Write($"S98\r\n");

                    Thread.Sleep(100);
                    puerto.DiscardInBuffer();
                    puerto.Write($"MSV?\r\n");

                    Thread.Sleep(100);
                    puerto.DiscardInBuffer();
                    puerto.Write($"S{direccion:D2}\r\n");

                    Thread.Sleep(300);
                    string respuesta = puerto.ReadExisting();
                    limpia = LimpiarTrama(respuesta);
                    rawWeight = ExtraerPesoHBM(limpia);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }
                finally
                {
                    comandoEnProgreso = false;
                }
            }

            // Eventos FUERA del lock para evitar deadlocks con Control.Invoke()
            if (errorMsg != null)
            {
                TramaRecibida?.Invoke($"ERROR ML ({direccion:D2}): {errorMsg}");
                return 0;
            }

            TramaRecibida?.Invoke($"ML ({direccion:D2}): {limpia}");

            bool respuestaValida = !string.IsNullOrEmpty(limpia)
                                   && limpia.Length > 0;

            if (!respuestaValida)
                return 0;

            if (!Celdas.ContainsKey(direccion))
                Celdas[direccion] = new CeldaInfo { SlaveNumber = direccion };

            Celdas[direccion].RawWeight = rawWeight;
            Celdas[direccion].LastRead = DateTime.Now;
            Celdas[direccion].Connected = true;

            if (UsarCalibracionMultivariable && calibracionMultivariable != null)
            {
                Celdas[direccion].CalibratedWeight = rawWeight;
            }
            else
            {
                Celdas[direccion].CalibratedWeight = rawWeight * GetFactorCalibracion(direccion);
            }

            PesoActualizado?.Invoke(direccion, Celdas[direccion].CalibratedWeight);
            return Celdas[direccion].CalibratedWeight;
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
            double rawWeight = ExtraerValorNumerico(respuesta);

            // Solo actualizar la celda si la respuesta es válida (tiene un número)
            bool respuestaValida = !string.IsNullOrEmpty(respuesta)
                                   && !respuesta.StartsWith("ERROR")
                                   && !respuesta.StartsWith("?")
                                   && respuesta.Length > 0;

            if (respuestaValida)
            {
                // Crear entrada para la celda si no existe
                if (!Celdas.ContainsKey(direccion))
                    Celdas[direccion] = new CeldaInfo { SlaveNumber = direccion };

                Celdas[direccion].RawWeight = rawWeight;
                Celdas[direccion].LastRead = DateTime.Now;
                Celdas[direccion].Connected = true;

                // En modo multivariable, el peso individual es el raw (sin calibrar)
                // El peso total se calcula con la fórmula PESO = X1*m1 + X2*m2 + X3*m3 + X4*m4 + B
                if (UsarCalibracionMultivariable && calibracionMultivariable != null)
                {
                    Celdas[direccion].CalibratedWeight = rawWeight;
                }
                else
                {
                    // Modo simple: aplicar factor de calibración individual
                    Celdas[direccion].CalibratedWeight = rawWeight * GetFactorCalibracion(direccion);
                }

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
        /// Asigna una nueva dirección a una celda identificada por su número de serie.
        /// Utiliza la dirección especial 98 para emitir el comando de asignación.
        /// Comando: S98;ADR2,"{NUMERO_SERIE}";\
        /// Luego verifica el cambio y guarda en EEPROM.
        /// </summary>
        /// <param name="nuevaDireccion">Nueva dirección a asignar (1-15).</param>
        /// <param name="numeroSerie">Número de serie de la celda a reasignar.</param>
        /// <returns>True si la asignación y verificación fueron exitosas.</returns>
        public bool AsignarDireccion(int nuevaDireccion, string numeroSerie)
        {
            // Comando según documento: S98;ADR2,"M64702";\
            string comando = $"ADR2,\"{numeroSerie}\"";
            string respuesta = EnviarComando(98, comando);

            if (!string.IsNullOrEmpty(respuesta) && !respuesta.StartsWith("ERROR"))
            {
                // Esperar a que la celda cambie de dirección
                Thread.Sleep(500);

                // Verificar el cambio consultando la nueva dirección
                string verificacion = ConsultarSerie(nuevaDireccion);
                if (!string.IsNullOrEmpty(verificacion) && verificacion.Contains(numeroSerie))
                {
                    // Guardar la configuración en la EEPROM de la celda
                    GuardarEEPROM(nuevaDireccion);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Guarda la configuración actual de la celda en su EEPROM para que
        /// los cambios persistan después de un ciclo de alimentación.
        /// Comando: S{dir:D2};TDD1;\
        /// </summary>
        /// <param name="direccion">Dirección de la celda.</param>
        /// <returns>Respuesta de la celda al comando.</returns>
        public string GuardarEEPROM(int direccion)
        {
            return EnviarComando(direccion, "TDD1");
        }

        /// <summary>
        /// Enumerar todas las celdas conectadas en el bus RS-485.
        /// Escanea secuencialmente las direcciones 1 a 15 consultando el número de serie.
        /// Si una celda responde correctamente, se registra como detectada y se consulta su peso.
        /// 
        /// Nota: Este método puede tardar varios segundos (~4.5s) debido a los delays
        /// de comunicación serial. Se recomienda ejecutarlo en un hilo en segundo plano.
        /// 
        /// La dirección 98 es especial y se usa solo para comandos de asignación.
        /// </summary>
        /// <returns>Lista de objetos CeldaInfo con las celdas detectadas y sus datos.</returns>
        public List<CeldaInfo> EnumerarCeldas()
        {
            var celdasDetectadas = new List<CeldaInfo>();

            // Escanear direcciones de 1 a 15 (la dirección 0 no se usa según el protocolo)
            for (int addr = 1; addr <= 15; addr++)
            {
                string respuesta = ConsultarSerie(addr);

                // Verificar si hay una celda válida en esta dirección.
                // Una respuesta real de IDN? tiene formato "HBM,C16iC3/40t,Mxxxxx,Pxx"
                // (contiene comas y NO contiene el comando "IDN?" como eco).
                // Se rechazan: vacíos, errores, ecos del comando, respuestas muy cortas.
                if (!string.IsNullOrEmpty(respuesta) &&
                    !respuesta.StartsWith("ERROR") &&
                    !respuesta.StartsWith("?") &&
                    !respuesta.Contains("IDN?") &&
                    respuesta.Length > 3)
                {
                    if (!Celdas.ContainsKey(addr))
                        Celdas[addr] = new CeldaInfo { SlaveNumber = addr };

                    Celdas[addr].SerialNumber = respuesta;
                    Celdas[addr].Connected = true;

                    // Consultar el peso también para tener datos iniciales
                    ConsultarPeso(addr);

                    celdasDetectadas.Add(Celdas[addr]);

                    TramaRecibida?.Invoke($"Celda detectada: #{addr:D2} - {respuesta}");
                }
            }

            CeldasEnumeradas?.Invoke(celdasDetectadas);
            return celdasDetectadas;
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

        /// <summary>
        /// Calibra el sistema completo aplicando un peso conocido de referencia.
        /// 
        /// Calcula un factor de calibración único para todo el sistema basado en:
        /// factor = pesoConocido / suma(pesosRaw)
        /// Este factor se aplica a todas las celdas por igual.
        /// 
        /// Si se requiere calibración individual por celda, se necesitaría
        /// un peso conocido aplicado a cada celda por separado.
        /// </summary>
        /// <param name="pesoConocido">Peso de referencia conocido en kg, colocado sobre la báscula.</param>
        /// <returns>
        /// Diccionario con los factores de calibración calculados para cada celda,
        /// o diccionario vacío si no se pudo calibrar.
        /// </returns>
        public Dictionary<int, double> CalibrarSistema(double pesoConocido)
        {
            Dictionary<int, double> factores = new Dictionary<int, double>();
            double pesoRawTotal = 0;
            int celdasActivas = 0;

            // Primero, obtener el peso raw actual de todas las celdas
            foreach (var celda in Celdas.Values)
            {
                if (celda.Connected)
                {
                    // Consultar peso raw nuevamente para asegurar datos actualizados
                    string respuesta = EnviarComando(celda.SlaveNumber, "MSV?");
                    double rawWeight = ExtraerValorNumerico(respuesta);
                    celda.RawWeight = rawWeight;

                    if (rawWeight > 0)
                    {
                        pesoRawTotal += rawWeight;
                        celdasActivas++;
                    }
                }
            }

            // Calcular factor único del sistema si hay datos válidos
            if (pesoRawTotal > 0 && celdasActivas > 0)
            {
                double factorSistema = pesoConocido / pesoRawTotal;

                foreach (var celda in Celdas.Values)
                {
                    if (celda.Connected)
                    {
                        SetFactorCalibracion(celda.SlaveNumber, factorSistema);
                        celda.CalibratedWeight = celda.RawWeight * factorSistema;
                        factores[celda.SlaveNumber] = factorSistema;
                    }
                }
            }

            return factores;
        }

        /// <summary>
        /// Verifica si hay comunicación con una celda consultando su número de serie.
        /// </summary>
        /// <param name="direccion">Dirección de la celda a verificar (1-15).</param>
        /// <returns>True si la celda responde con una identificación válida.</returns>
        public bool VerificarComunicacion(int direccion)
        {
            string respuesta = ConsultarSerie(direccion);
            return !string.IsNullOrEmpty(respuesta) && !respuesta.StartsWith("ERROR");
        }
    }
}
