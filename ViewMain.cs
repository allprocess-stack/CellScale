using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    /// <summary>
    /// Formulario principal de la aplicación de báscula multicelda.
    /// Gestiona la conexión a la balanza vía RS-485, la visualización de pesos
    /// individuales y unificados, la calibración del sistema y el registro
    /// de pesadas en la base de datos MySQL.
    /// </summary>
    public partial class ViewMain : Form
    {
        // Configuración de la aplicación cargada desde config.json
        private AppConfig config;

        // Conexión a la base de datos MySQL
        private ConectionBD conexion;

        // Manager de comunicación con las celdas de carga
        private CeldaManager manager;

        // Dirección de la celda actualmente seleccionada en la UI
        private int celdaSeleccionada = 1;

        // Último peso calibrado registrado (para usar en el registro en BD)
        private double ultimoPesoCalibrado = 0;

        // Estado de conexión de la balanza
        private bool balanzaConectada = false;

        // Tiempo de inicio de la conexión/desconexión actual
        private DateTime tiempoInicioConexion;
        private DateTime tiempoInicioDesconexion;

        // Indica si se recibió al menos una trama válida en el último intervalo
        private bool ultimaTramaRecibida = false;

        // Motor de calibración lineal multivariable
        private CalibracionLineal calibracionLineal;

        // Labels creados programáticamente para mostrar pesos (no existen en el diseñador)
        //private Label lblPesoUnificado;
        //private Label lblPesoIndividual;

        // Índice round-robin para consultar celdas cada 250ms
        private int indiceConsulta = 0;

        /// <summary>
        /// Inicializa el formulario principal.
        /// Carga la configuración desde config.json, establece la conexión con la
        /// base de datos y configura la visibilidad inicial de los menús.
        /// </summary>
        public ViewMain()
        {
            InitializeComponent();

            // Ocultar menús hasta que el usuario inicie sesión
            //tsddbMenu.Visible = false;
            //tsddbConfiguracion.Visible = false;

            // Cargar configuración desde config.json
            config = ConfigManager.CargarConfig();

            // Verificar que la configuración se haya cargado correctamente
            if (config == null)
            {
                MessageBox.Show("No se pudo cargar la configuración. Verifique el archivo config.json.\n" +
                               "Se utilizarán valores por defecto.",
                    "Error crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Crear configuración por defecto para evitar null reference
                config = new AppConfig
                {
                    Servidor = "",
                    BD = "",
                    Puerto = "",
                    Usuario = "",
                    Contrasena = "",
                    COMBalanza = "",
                    CalibracionBalanza = ""
                };
            }

            Console.WriteLine($"Configuración cargada: {config.Servidor}:{config.Puerto} / BD: {config.BD}");

            // Intentar conectar a la base de datos
            try
            {
                conexion = new ConectionBD(config);
                conexion.AbrirConexion();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar a la base de datos: {ex.Message}");
                // La aplicación puede continuar sin BD para pesaje local
                // El registro de pesos requerirá BD funcionando
            }
        }

        /// <summary>
        /// Carga los puertos COM disponibles, la configuración de celdas,
        /// configura los timers y crea los labels de peso dinámicos.
        /// Se ejecuta al cargar el formulario.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            CargarPuertosCOM();

            if (config != null)
            {
                tstbCalibracion.Text = config.CalibracionBalanza;
                tscbBalanza.Text = config.COMBalanza;
            }

            CargarCeldasConfig();
            //CargarSlaveNumbers();

            // Inicializar el manager de celdas y suscribir eventos
            manager = new CeldaManager();
            manager.TramaRecibida += Manager_TramaRecibida;
            manager.PesoActualizado += Manager_PesoActualizado;
            manager.CeldasEnumeradas += Manager_CeldasEnumeradas;

            // Estado inicial de conexión
            balanzaConectada = false;
            tiempoInicioDesconexion = DateTime.Now;

            // Timer para mostrar el tiempo de conexión/desconexión en la barra de estado
            timerTiempoConexion.Interval = 1000;
            timerTiempoConexion.Start();

            // Timer de verificación de trama (se inicia al conectar la balanza)
            timerDataTrama.Interval = 1000;

            // Deshabilitar botón de registro hasta que la balanza esté conectada
            btnRegistrar.Enabled = false;

            // Configurar el timer de pesaje (declarado en Form1.Designer.cs)
            // Este timer consulta el peso periódicamente cuando la balanza está conectada
            TimerPesaje.Interval = 250;

            // Configurar campo de contraseña para ocultar caracteres
            // Se accede al TextBox subyacente porque ToolStripTextBox no expone PasswordChar directamente
            txtContrasena.TextBox.PasswordChar = '*';

            // Conectar automáticamente la balanza al iniciar la aplicación
            if (config != null && !string.IsNullOrEmpty(config.COMBalanza))
                _ = ConectarBalanza(config.COMBalanza);
        }

        /// <summary>
        /// Carga los factores de calibración (modo simple) guardados en la configuración
        /// hacia el manager de celdas.
        /// Si existe calibración multivariable activa, carga los coeficientes m1..m4, B.
        /// </summary>
        private void CargarCeldasConfig()
        {
            // Cargar factores de calibración simple (per-cell factors)
            if (config.FactoresCalibracion == null)
                config.FactoresCalibracion = new Dictionary<string, double>();

            if (manager != null)
            {
                foreach (var factor in config.FactoresCalibracion)
                {
                    if (factor.Key.StartsWith("CELDA_") && int.TryParse(factor.Key.Substring(6), out int direccion))
                    {
                        manager.SetFactorCalibracion(direccion, factor.Value);
                    }
                }
            }

            // Cargar compensación de esquinas si está activa en la configuración
            if (config.CompensacionEsquinasActiva &&
                config.CerosCompensacion != null && config.CerosCompensacion.Length == 4 &&
                config.FactoresCompensacion != null && config.FactoresCompensacion.Length == 4)
            {
                if (manager != null)
                {
                    manager.ConfigurarCompensacionEsquinas(config.CerosCompensacion, config.FactoresCompensacion);
                    Console.WriteLine("Compensación de esquinas cargada desde configuración.");
                }
            }
        }

        /// <summary>
        /// Maneja el evento de trama recibida desde el puerto serial.
        /// Actualiza el indicador de estado "DATA" en la barra de estado
        /// según si la trama es válida, contiene error o está vacía.
        /// </summary>
        private void Manager_TramaRecibida(string trama)
        {
            this.Invoke(new Action(() =>
            {
                if (!string.IsNullOrEmpty(trama) && !trama.StartsWith("ERROR"))
                {
                    tsslblTrama.Text = "Trama recibida";
                    tsslblTrama.ForeColor = Color.Green;
                    ultimaTramaRecibida = true;
                }
                else if (trama != null && trama.StartsWith("ERROR"))
                {
                    tsslblTrama.Text = "Error trama";
                    tsslblTrama.ForeColor = Color.OrangeRed;
                }
                else
                {
                    tsslblTrama.Text = "Sin trama";
                    tsslblTrama.ForeColor = Color.Red;
                }
            }));
        }

        /// <summary>
        /// Maneja el evento de peso actualizado de una celda.
        /// Actualiza los labels de peso individual (celda seleccionada)
        /// y peso total unificado (suma de todas las celdas).
        /// Se ejecuta mediante Invoke para garantir thread-safety.
        /// </summary>
        private void Manager_PesoActualizado(int direccion, double pesoCalibrado)
        {
            this.Invoke(new Action(() =>
            {
                //if (direccion == celdaSeleccionada)
                //{
                //    lblPesoIndividual.Text = $"Celda #{direccion:D2}: {pesoCalibrado:F2} kg";
                //}

                double pesoUnificado = manager.ObtenerPesoUnificado();
                txtBalanza.Text = pesoUnificado.ToString("F2");
                ultimoPesoCalibrado = pesoUnificado;
                //lblPesoUnificado.Text = $"Peso Total: {pesoUnificado:F2} kg";
                //lblCeldaActiva.Text = $"Celda activa: #{celdaSeleccionada:D2}";
            }));
        }

        /// <summary>
        /// Maneja el evento de enumeración de celdas completada.
        /// Actualiza el ListBox de celdas y el indicador de estado
        /// con la cantidad de celdas detectadas.
        /// </summary>
        private void Manager_CeldasEnumeradas(List<CeldaInfo> celdas)
        {
            this.Invoke(new Action(() =>
            {
                lstCeldas.Items.Clear();
                foreach (var c in celdas)
                {
                    lstCeldas.Items.Add(c.ToString());
                }
                if (celdas.Count > 0)
                {
                    tsslblStatusConexion.Text = $"{celdas.Count} celda(s)";
                }
            }));
        }

        /// <summary>
        /// Maneja el click del botón INGRESAR del menú LOGIN.
        /// Verifica las credenciales contra la tabla 'usuario' de la base de datos MySQL.
        /// Si el login es exitoso, muestra los menús de configuración.
        /// </summary>
        private void tsmiIngresar_Click(object sender, EventArgs e)
        {
            try
            {
                // Variables de login
                string usuario = txtUsuario.Text.Trim();
                string contrasena = txtContrasena.Text.Trim();
                // Usuario Master
                var userMaster = (usuario == "anthony" && contrasena == "12345");


                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
                {
                    MessageBox.Show("Ingrese Credenciales", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Verificar que haya conexión a la base de datos
                if (conexion == null)
                {
                    MessageBox.Show("No hay conexión a la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Consultar usuario en la base de datos
                string query = "SELECT * FROM usuario WHERE nombre=@usuario AND contrasena=@contrasena";

                var parametros = new Dictionary<string, object>
                {
                    {"@usuario", usuario},
                    {"@contrasena", contrasena}
                };

                using (var reader = conexion.EjecutarConsulta(query, parametros))
                {
                    if (reader.Read())
                    {
                        MessageBox.Show("Login exitoso", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //tsddbConfiguracion.Visible = true;
                        //tsddbMenu.Visible = true;
                    }
                    else if (userMaster)
                    {

                        MessageBox.Show("Login exitoso(Usuario Master)", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //tsddbConfiguracion.Visible = true;
                        //tsddbMenu.Visible = true;
                    }
                    else
                    {
                        MessageBox.Show("Credenciales incorrectas\nIntente nuevamente", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al ingresar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Guarda el puerto COM seleccionado en la tabla 'balanza' de la base de datos.
        /// </summary>
        private void tsmiGuardarMenu_Click(object sender, EventArgs e)
        {
            try
            {
                // Validar que se haya seleccionado un puerto COM
                string balanzaCOM = tscbBalanza.Text;
                if (string.IsNullOrEmpty(balanzaCOM))
                {
                    MessageBox.Show("Seleccione un puerto COM", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (conexion == null)
                {
                    MessageBox.Show("No hay conexión a la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Guardar valor de Puerto de balanza
                config.COMBalanza=balanzaCOM;
                // Persistir en el archivo config.json
                ConfigManager.GuardarConfig(config);
                MessageBox.Show("Configuración Balanza guardada correctamente\n",
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Abre la conexión con la balanza en el puerto COM seleccionado.
        /// Luego enumera las celdas conectadas en segundo plano (Task.Run)
        /// para no bloquear la interfaz de usuario durante el escaneo.
        /// </summary>
        private async void tsmiAbrirBalanza_Click(object sender, EventArgs e)
        {
            var menu = ((ToolStripDropDownMenu)((ToolStripMenuItem)sender).Owner);
            menu.AutoClose = false;

            string puertoBalanza = tscbBalanza.Text;
            if (string.IsNullOrEmpty(puertoBalanza))
            {
                MessageBox.Show("Seleccione un puerto COM para la balanza", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                menu.AutoClose = true;
                return;
            }

            await ConectarBalanza(puertoBalanza);

            menu.AutoClose = true;
        }

        private async Task ConectarBalanza(string puertoBalanza)
        {
            try
            {
                manager.Conectar(puertoBalanza);

                if (manager.IsOpen)
                {
                    tscbBalanza.Enabled = false;
                    balanzaConectada = true;
                    tiempoInicioConexion = DateTime.Now;
                    btnRegistrar.Enabled = true;

                    // Enumerar celdas en segundo plano para no congelar la UI
                    // (~4.5 segundos de escaneo de direcciones 1-15)
                    List<CeldaInfo> celdas = await Task.Run(() => manager.EnumerarCeldas());

                    if (celdas.Count > 0)
                    {
                        MessageBox.Show($"Balanza conectada en {puertoBalanza}\n{celdas.Count} celda(s) detectada(s)",
                            "Conexión exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Balanza conectada en {puertoBalanza}\nNo se detectaron celdas",
                            "Conexión exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // Iniciar timer de pesaje periódico
                    TimerPesaje.Start();

                    // Iniciar timer de monitoreo de tramas
                    timerDataTrama.Start();

                    // Cargar factores de calibración guardados
                    CargarFactoresCalibracion();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Carga los factores de calibración (modo simple y multivariable)
        /// desde el archivo de configuración hacia el manager.
        /// Se ejecuta al conectar la balanza.
        /// </summary>
        private void CargarFactoresCalibracion()
        {
            // Cargar factores de calibración simple
            if (config.FactoresCalibracion != null)
            {
                foreach (var factor in config.FactoresCalibracion)
                {
                    if (factor.Key.StartsWith("CELDA_") && int.TryParse(factor.Key.Substring(6), out int direccion))
                    {
                        manager.SetFactorCalibracion(direccion, factor.Value);
                    }
                }
            }

            // Cargar calibración multivariable si está activa
            if (config.CalibracionMultivariableActiva &&
                config.CoeficienteM1 != 0 && config.CoeficienteM2 != 0 &&
                config.CoeficienteM3 != 0 && config.CoeficienteM4 != 0)
            {
                manager.ConfigurarCalibracionMultivariable(
                    config.CoeficienteM1,
                    config.CoeficienteM2,
                    config.CoeficienteM3,
                    config.CoeficienteM4,
                    config.BiasB
                );

                Console.WriteLine("Calibración multivariable cargada al manager.");
            }

            // Cargar compensación de esquinas si está activa
            if (config.CompensacionEsquinasActiva &&
                config.CerosCompensacion != null && config.CerosCompensacion.Length == 4 &&
                config.FactoresCompensacion != null && config.FactoresCompensacion.Length == 4)
            {
                manager.ConfigurarCompensacionEsquinas(config.CerosCompensacion, config.FactoresCompensacion);
                Console.WriteLine("Compensación de esquinas cargada al manager.");
            }
        }

        /// <summary>
        /// Cierra la conexión con la balanza, detiene los timers de pesaje
        /// y monitoreo, y restaura la interfaz al estado inicial de desconexión.
        /// </summary>
        private void tsmiCerrarBalanza_Click(object sender, EventArgs e)
        {
            var menu = ((ToolStripDropDownMenu)((ToolStripMenuItem)sender).Owner);
            menu.AutoClose = false;

            // Detener timers
            TimerPesaje.Stop();
            timerDataTrama.Stop();

            // Desconectar el puerto serial
            manager.Desconectar();

            // Restaurar controles de la UI
            tscbBalanza.Enabled = true;
            balanzaConectada = false;
            tiempoInicioDesconexion = DateTime.Now;
            btnRegistrar.Enabled = false;
            txtBalanza.Clear();
            //lblCeldaActiva.Text = "Celda #--";
            lstCeldas.Items.Clear();
            //lblPesoUnificado.Text = "Peso Total: 0.00 kg";
            //lblPesoIndividual.Text = "Celda #--: 0.00 kg";

            MessageBox.Show("Balanza desconectada correctamente.", "Desconexión", MessageBoxButtons.OK, MessageBoxIcon.Information);

            menu.AutoClose = true;
        }

        /// <summary>
        /// Calibra el sistema usando el método multivariable.
        /// Recopila lecturas raw de las 4 celdas con el peso conocido actual,
        /// los acumula como puntos de calibración y, al llegar a 5 puntos,
        /// resuelve el sistema de ecuaciones por eliminación de Gauss.
        /// 
        /// Guarda los coeficientes resultantes (m1..m4, B) en config.json
        /// y activa el modo de calibración multivariable.
        /// </summary>
        private void tsmiGuardarConfiguracion_Click(object sender, EventArgs e)
        {
            try
            {
                // Validar el peso conocido ingresado
                if (!double.TryParse(tstbCalibracion.Text.Trim(), out double pesoConocido) || pesoConocido <= 0)
                {
                    MessageBox.Show("Ingrese un peso conocido válido para calibrar el sistema", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Verificar que haya al menos 4 celdas conectadas
                //if (manager.Celdas.Count < 4)
                //{
                //    if (manager.Celdas.Count == 0)
                //    {
                //        MessageBox.Show("No hay celdas conectadas para calibrar.\nConecte la balanza primero.",
                //            "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //        return;
                //    }

                //    // Permitir continuar con las celdas disponibles
                //    DialogResult dr = MessageBox.Show(
                //        $"Solo se detectaron {manager.Celdas.Count} celda(s).\n" +
                //        $"¿Desea calibrar con las celdas disponibles?",
                //        "Celdas insuficientes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                //    if (dr == DialogResult.No)
                //        return;
                //}

                // Crear o inicializar el recolector de puntos de calibración
                if (calibracionLineal == null)
                    calibracionLineal = new CalibracionLineal();

                // Obtener celdas conectadas ordenadas por SlaveNumber
                var celdas = manager.Celdas.Values
                    .Where(c => c.Connected)
                    .OrderBy(c => c.SlaveNumber)
                    .ToList();

                // Recolectar lecturas raw de las celdas (hasta 4)
                double x1 = celdas.Count > 0 ? celdas[0].RawWeight : 0;
                double x2 = celdas.Count > 1 ? celdas[1].RawWeight : 0;
                double x3 = celdas.Count > 2 ? celdas[2].RawWeight : 0;
                double x4 = celdas.Count > 3 ? celdas[3].RawWeight : 0;

                // Mostrar las lecturas actuales y confirmar
                string mensajePunto =
                    $"Punto de calibración #{PuntosCalibracion.Count}:\n\n" +
                    $"Peso conocido: {pesoConocido} kg\n" +
                    $"Celda {(celdas.Count > 0 ? celdas[0].SlaveNumber.ToString() : "?")} (raw): {x1}\n" +
                    $"Celda {(celdas.Count > 1 ? celdas[1].SlaveNumber.ToString() : "?")} (raw): {x2}\n" +
                    $"Celda {(celdas.Count > 2 ? celdas[2].SlaveNumber.ToString() : "?")} (raw): {x3}\n" +
                    $"Celda {(celdas.Count > 3 ? celdas[3].SlaveNumber.ToString() : "?")} (raw): {x4}\n\n" +
                    $"¿Agregar este punto y continuar?";

                DialogResult resultado = MessageBox.Show(mensajePunto, "Registrar punto de calibración",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultado == DialogResult.No)
                    return;

                // Guardar el punto de calibración
                PuntosCalibracion.Add(new PuntoCalibracion
                {
                    X1 = x1,
                    X2 = x2,
                    X3 = x3,
                    X4 = x4,
                    PesoConocido = pesoConocido
                });

                int puntosRestantes = 5 - PuntosCalibracion.Count;

                if (puntosRestantes > 0)
                {
                    MessageBox.Show(
                        $"Punto registrado correctamente.\n\n" +
                        $"Faltan {puntosRestantes} punto(s) para completar la calibración.\n" +
                        $"Cambie el peso sobre la báscula y presione 'Guardar Configuración' nuevamente.",
                        "Punto registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Con 5 puntos, resolver el sistema por eliminación de Gauss
                bool exito = calibracionLineal.Calibrar(PuntosCalibracion);

                if (exito)
                {
                    // Aplicar al manager en memoria
                    manager.ConfigurarCalibracionMultivariable(
                        calibracionLineal.Coeficientes[0],
                        calibracionLineal.Coeficientes[1],
                        calibracionLineal.Coeficientes[2],
                        calibracionLineal.Coeficientes[3],
                        calibracionLineal.Bias
                    );

                    // Mostrar informe completo de calibración
                    string informe = calibracionLineal.GenerarInforme(PuntosCalibracion);
                    MessageBox.Show(informe, "Calibración multivariable exitosa",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Limpiar puntos para futuras recalibraciones
                    PuntosCalibracion.Clear();

                    ActualizarListaCeldas();
                }
                else
                {
                    MessageBox.Show("No se pudo calibrar. Verifique los puntos de calibración.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calibrar sistema: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Lista estática para acumular puntos de calibración durante la sesión
        private List<PuntoCalibracion> PuntosCalibracion = new List<PuntoCalibracion>();
        /// <summary>
        /// Actualiza el ListBox de celdas con las celdas conectadas actualmente.
        /// </summary>
        private void ActualizarListaCeldas()
        {
            lstCeldas.Items.Clear();
            foreach (var celda in manager.Celdas.Values)
            {
                if (celda.Connected)
                {
                    lstCeldas.Items.Add(celda.ToString());
                }
            }
        }

        /// <summary>
        /// Timer principal de pesaje que se ejecuta cada 250ms mientras
        /// la balanza está conectada. Consulta una celda distinta cada tick
        /// (round-robin) para registrar la medición de todas las celdas.
        /// </summary>
        private async void TimerPesaje_Tick(object sender, EventArgs e)
        {
            if (manager != null && manager.IsOpen)
            {
                var celdasConectadas = manager.Celdas.Values
                    .Where(c => c.Connected)
                    .OrderBy(c => c.SlaveNumber)
                    .ToList();

                if (celdasConectadas.Count > 0)
                {
                    int idx = indiceConsulta % celdasConectadas.Count;
                    var celda = celdasConectadas[idx];
                    await Task.Run(() => manager.ConsultarPeso(celda.SlaveNumber));
                    indiceConsulta++;
                }

                // Actualizar peso unificado
                double pesoUnificado = manager.ObtenerPesoUnificado();
                txtBalanza.Text = pesoUnificado.ToString("F2");
                ultimoPesoCalibrado = pesoUnificado;
            }
        }

        /// <summary>
        /// Registra el peso actual en la base de datos MySQL.
        /// Primero intenta insertar incluyendo el ID de la celda seleccionada.
        /// Si la tabla no tiene la columna celda_id (versiones anteriores de la BD),
        /// reintenta la inserción sin ese campo.
        /// </summary>
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                double peso = ultimoPesoCalibrado;

                // Si no hay peso calibrado, intentar obtenerlo del campo de texto
                if (peso <= 0 && !double.TryParse(txtBalanza.Text.Trim(), out peso))
                {
                    MessageBox.Show("No hay un peso válido para registrar", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (conexion == null)
                {
                    MessageBox.Show("No hay conexión a la base de datos.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Intentar insertar con el ID de celda
                var parametrosCompletos = new Dictionary<string, object>
                {
                    {"@peso", peso},
                    {"@fecha", DateTime.Now},
                    {"@celda", celdaSeleccionada}
                };

                string queryCompleta = "INSERT INTO peso (peso, fecha, celda_id) VALUES(@peso, @fecha, @celda)";

                try
                {
                    int filas = conexion.EjecutarNonQuery(queryCompleta, parametrosCompletos);
                    if (filas > 0)
                    {
                        MessageBox.Show($"Peso registrado: {peso:F2} kg (Celda #{celdaSeleccionada:D2})",
                            "Registro exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    // Si la tabla no tiene la columna celda_id (BD antigua),
                    // reintentar la inserción solo con peso y fecha
                    System.Diagnostics.Debug.WriteLine(
                        $"Error al insertar con celda_id, reintentando sin ella: {ex.Message}");

                    var parametrosSimples = new Dictionary<string, object>
                    {
                        {"@peso", peso},
                        {"@fecha", DateTime.Now}
                    };

                    string querySimples = "INSERT INTO peso (peso, fecha) VALUES(@peso, @fecha)";
                    int filas = conexion.EjecutarNonQuery(querySimples, parametrosSimples);

                    if (filas > 0)
                    {
                        MessageBox.Show($"Peso registrado: {peso:F2} kg",
                            "Registro exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar peso: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Obtiene la lista de puertos COM disponibles en el sistema
        /// y los carga en el combobox de selección de puerto.
        /// </summary>
        private void CargarPuertosCOM()
        {
            string[] puertos = SerialPort.GetPortNames();
            tscbBalanza.Items.Clear();
            foreach (string puerto in puertos)
            {
                tscbBalanza.Items.Add(puerto);
            }

            if (tscbBalanza.Items.Count > 0)
            {
                int index = -1;
                if (!string.IsNullOrEmpty(config?.COMBalanza))
                    index = tscbBalanza.FindStringExact(config.COMBalanza);

                tscbBalanza.SelectedIndex = index >= 0 ? index : 0;
            }
        }

        /// <summary>
        /// Timer que actualiza el indicador de estado de conexión
        /// y el tiempo transcurrido en la barra de estado.
        /// Se ejecuta cada 1 segundo.
        /// </summary>
        private void tsslblTiempoConexion_Tick(object sender, EventArgs e)
        {
            TimeSpan tiempo;

            if (balanzaConectada)
            {
                tiempo = DateTime.Now - tiempoInicioConexion;
                tsslblStatusConexion.Text = "Conectado";
                tsslblStatusConexion.ForeColor = Color.Green;
                tsslblTiempoConexion.Text = $"{tiempo:hh\\:mm\\:ss}";
                tsslblTiempoConexion.ForeColor = Color.Green;
            }
            else
            {
                tiempo = DateTime.Now - tiempoInicioDesconexion;
                tsslblStatusConexion.Text = "Desconectado";
                tsslblStatusConexion.ForeColor = Color.Red;
                tsslblTiempoConexion.Text = $"{tiempo:hh\\:mm\\:ss}";
                tsslblTiempoConexion.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Timer que monitorea la recepción de tramas del puerto serial.
        /// Si no se recibe una trama dentro del intervalo (1s), muestra
        /// "Sin Trama" en rojo. La bandera ultimaTramaRecibida es reseteada
        /// por Manager_TramaRecibida cada vez que llega una trama válida.
        /// </summary>
        private void timerDataTrama_Tick(object sender, EventArgs e)
        {
            if (ultimaTramaRecibida)
            {
                tsslblTrama.Text = "Trama OK";
                tsslblTrama.ForeColor = Color.Green;
            }
            else
            {
                tsslblTrama.Text = "Sin Trama";
                tsslblTrama.ForeColor = Color.Red;
            }
            ultimaTramaRecibida = false;
        }

        // --- Event handlers requeridos por el diseñador (aunque estén vacíos) ---

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }
        private void toolStripDropDownButton1_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }


        private void tsbBd_Click(object sender, EventArgs e)
        {
            new ViewBd().Show();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (manager == null)
            {
                MessageBox.Show("No hay conexión activa con la balanza.\nConecte la balanza primero.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            new ViewCeldas(manager, conexion).Show();
        }
    }
}
