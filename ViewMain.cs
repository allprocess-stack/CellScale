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

        // Labels creados programáticamente para mostrar pesos (no existen en el diseñador)
        //private Label lblPesoUnificado;
        //private Label lblPesoIndividual;

        // Índice round-robin para consultar celdas cada 250ms


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
            //tsbCeldasConfig.Visible=true;
            //tsbPesoCeldas.Visible=true;
            tsslblTrama.Text = "Sin trama";
            tsslblTrama.ForeColor = Color.Red;


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
                //tstbCalibracion.Text = config.CalibracionBalanza;
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

            

            // Agregar opción de Calibración Gauss en el menú
            var tsmiCalGauss = new ToolStripMenuItem
            {
                Name = "tsmiCalGauss",
                Text = "Calibración Gauss 5-Ptos"
            };
            tsmiCalGauss.Click += TsmiCalGauss_Click;
            tsddbMenu.DropDownItems.Add(tsmiCalGauss);

            // Conectar automáticamente la balanza al iniciar la aplicación
            if (config != null && !string.IsNullOrEmpty(config.COMBalanza))
                _ = ConectarBalanza(config.COMBalanza);
        }

      
        private List<PuntoCalibracion> PuntosCalGauss = new List<PuntoCalibracion>();
        /// <summary>Ejecuta la calibración Gauss de 5 puntos desde el menú. Captura lecturas y resuelve el sistema.</summary>
        private void TsmiCalGauss_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen)
            {
                MessageBox.Show("La balanza no está conectada.\nConecte la balanza primero o use el SIMULADOR.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(tstbPesoCalibrar.Text.Trim(), out double pesoConocido) || pesoConocido <= 0)
            {
                MessageBox.Show("Ingrese un peso conocido válido en 'Peso Calibración'.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var celdas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .Take(4)
                .ToList();

            if (celdas.Count < 4)
            {
                MessageBox.Show($"Se requieren 4 celdas conectadas. Solo hay {celdas.Count}.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                double x1 = 0, x2 = 0, x3 = 0, x4 = 0;
                for (int i = 0; i < 4; i++)
                {
                    manager.ConsultarPeso(i);
                    switch (i)
                    {
                        case 0: x1 = celdas[i].RawWeight; break;
                        case 1: x2 = celdas[i].RawWeight; break;
                        case 2: x3 = celdas[i].RawWeight; break;
                        case 3: x4 = celdas[i].RawWeight; break;
                    }
                }

                PuntosCalGauss.Add(new PuntoCalibracion
                {
                    X1 = x1, X2 = x2, X3 = x3, X4 = x4,
                    PesoConocido = pesoConocido
                });

                int restantes = 5 - PuntosCalGauss.Count;

                if (restantes > 0)
                {
                    MessageBox.Show(
                        $"Punto #{PuntosCalGauss.Count} registrado.\n" +
                        $"Celdas: S{celdas[0].SlaveNumber:D2} S{celdas[1].SlaveNumber:D2} " +
                        $"S{celdas[2].SlaveNumber:D2} S{celdas[3].SlaveNumber:D2}\n" +
                        $"Lecturas: {x1:F1}, {x2:F1}, {x3:F1}, {x4:F1}\n" +
                        $"Peso conocido: {pesoConocido} kg\n\n" +
                        $"Faltan {restantes} punto(s). Cambie el peso y presione nuevamente.",
                        "Punto registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var calGauss = new CalibracionLineal();
                if (calGauss.Calibrar(PuntosCalGauss))
                {
                    config.CoeficienteM1 = calGauss.Coeficientes[0];
                    config.CoeficienteM2 = calGauss.Coeficientes[1];
                    config.CoeficienteM3 = calGauss.Coeficientes[2];
                    config.CoeficienteM4 = calGauss.Coeficientes[3];
                    config.BiasB = calGauss.Bias;
                    config.CalibracionMultivariableActiva = true;
                    config.CompensacionEsquinasActiva = false;
                    ConfigManager.GuardarConfig(config);

                    manager.ConfigurarCalibracionMultivariable(
                        config.CoeficienteM1, config.CoeficienteM2,
                        config.CoeficienteM3, config.CoeficienteM4, config.BiasB);

                    string informe = calGauss.GenerarInforme(PuntosCalGauss);
                    MessageBox.Show(informe, "Calibración Gauss Exitosa",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    PuntosCalGauss.Clear();
                }
                else
                {
                    MessageBox.Show("No se pudo resolver el sistema. Verifique los puntos.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en calibración Gauss: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    //MessageBox.Show(trama);
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
                        //tsbCeldasConfig.Visible=true;
                        //tsbPesoCeldas.Visible=true;

                    }
                    else if (userMaster)
                    {

                        MessageBox.Show("Login exitoso(Usuario Master)", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //tsddbConfiguracion.Visible = true;
                        //tsddbMenu.Visible = true;
                        //tsbCeldasConfig.Visible=true;
                        //tsbPesoCeldas.Visible=true;
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
                string pesoCalibrar = tstbPesoCalibrar.Text;
                string balanzaCOM = tscbBalanza.Text;
                if (string.IsNullOrEmpty(balanzaCOM) || string.IsNullOrEmpty(pesoCalibrar))
                {
                    MessageBox.Show("Seleccione un puerto COM e ingrese un valor a Calibrar", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (conexion == null)
                {
                    MessageBox.Show("No hay conexión a la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Guardar valor de Puerto de balanza
                config.COMBalanza=balanzaCOM;
                config.CalibracionBalanza=pesoCalibrar;
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

        /// <summary>Conecta la balanza, enumera las celdas e inicia los timers de pesaje.</summary>
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

                    List<CeldaInfo> celdas = await Task.Run(() => manager.InicializarCeldasTemporal());

                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("Celdas detectadas:");
                    foreach (var c in celdas.OrderBy(c => c.SlaveNumber))
                        sb.AppendLine($"  S{c.SlaveNumber:D2} -> Peso: {c.RawWeight:F2} kg");
                    MessageBox.Show(sb.ToString(), "Celdas detectadas", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        private bool actualizandoPesaje = false;

        /// <summary>
        /// Timer principal de pesaje que se ejecuta cada 250ms mientras
        /// la balanza está conectada. Consulta las 4 celdas S00-S03.
        /// </summary>
        private async void TimerPesaje_Tick(object sender, EventArgs e)
        {
            if (manager != null && manager.IsOpen && !actualizandoPesaje)
            {
                actualizandoPesaje = true;
                try
                {
                    await Task.Run(() =>
                    {
                        manager.ConsultarPeso(0);
                        manager.ConsultarPeso(1);
                        manager.ConsultarPeso(2);
                        manager.ConsultarPeso(3);
                    });
                }
                finally
                {
                    actualizandoPesaje = false;
                }

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


        /// <summary>Abre el formulario de configuración de base de datos.</summary>
        private void tsbBd_Click(object sender, EventArgs e)
        {
            new ViewBd().Show();
        }

        /// <summary>Abre el formulario de calibración y consulta de celdas.</summary>
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

        /// <summary>Abre el formulario de monitoreo en tiempo real de pesos individuales.</summary>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (manager == null)
            {
                MessageBox.Show("No hay conexión activa con la balanza.\nConecte la balanza primero.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            new ViewWeightCeldas(manager).Show();
        }

    }
}
