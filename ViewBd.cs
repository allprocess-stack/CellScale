using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    /// <summary>
    /// MODIFICADO: Formulario completamente funcional para configurar la conexión
    /// a la base de datos MySQL. Carga datos desde config.json y permite
    /// guardarlos y probar la conexión.
    ///
    /// Controles (creados en código, no en el diseñador):
    ///   txtServidor    - TextBox: IP/host del servidor MySQL
    ///   txtBd          - TextBox: Nombre de la base de datos
    ///   txtPuerto      - TextBox: Puerto del servidor MySQL
    ///   txtUsuario     - TextBox: Usuario de BD
    ///   txtContrasena  - TextBox: Contraseña (con PasswordChar = '*')
    ///   txtRuta        - TextBox (read-only): Ruta del archivo config.json
    ///   btnGuardarBd   - Button: Guarda la configuración en config.json
    ///   btnProbarConexion - Button: Prueba la conexión a la BD
    /// </summary>
    public partial class ViewBd : Form
    {
        // Configuración cargada desde config.json
        private AppConfig config;

        // Ruta completa del archivo config.json
        private string rutaConfig;

        //// --------------------- CONTROLES CREADOS EN CÓDIGO ---------------------
        //private Label lblServidor;
        //private TextBox txtServidor;
        //private Label lblBd;
        //private TextBox txtBd;
        //private Label lblPuerto;
        //private TextBox txtPuerto;
        //private Label lblUsuario;
        //private TextBox txtUsuario;
        //private Label lblContrasena;
        //private TextBox txtContrasena;
        //private Label lblRuta;
        //private TextBox txtRuta;
        //private Button btnGuardarBd;
        //private Button btnProbarConexion;

        // Constructor: crea los controles y carga la configuración
        public ViewBd()
        {
            InitializeComponent();

            // Configurar el formulario
            this.Text = "Configuración de Base de Datos";
            this.StartPosition = FormStartPosition.CenterParent;
        }
     
        // CARGA DE CONFIGURACIÓN DESDE config.json A LOS CAMPOS DEL FORMULARIO
        private void CargarConfig()
        {
            // Cargar la configuración desde el archivo JSON
            config = ConfigManager.CargarConfig();
            if (config == null)
            {
                MessageBox.Show("No se pudo cargar config.json.\nSe usarán valores por defecto.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                config = new AppConfig
                {
                    Servidor = config.Servidor,
                    BD = config.BD,
                    Puerto = config.Puerto,
                    Usuario = config.Usuario,
                    Contrasena = config.Contrasena
                };
            }

            // Determinar la ruta completa del archivo config.json
            rutaConfig = Path.Combine(Application.StartupPath, "config.json");

            // Volcar los valores a los controles del formulario
            txtServidor.Text = config.Servidor ?? "";
            txtBd.Text = config.BD ?? "";
            txtPuerto.Text = config.Puerto ?? "";
            txtUsuario.Text = config.Usuario ?? "";
            txtContrasena.Text = config.Contrasena ?? "";
            txtRuta.Text = rutaConfig;
            txtRuta.ReadOnly = true;
        }

        // BOTÓN: GUARDAR CONFIGURACIÓN
        private void BtnGuardarBd_Click(object sender, EventArgs e)
        {
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(txtServidor.Text))
            {
                MessageBox.Show("El campo Servidor es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServidor.Focus();
                return;
            }

            // Guardar los valores del formulario en el objeto AppConfig
            config.Servidor = txtServidor.Text.Trim();
            config.BD = txtBd.Text.Trim();
            config.Puerto = txtPuerto.Text.Trim();
            config.Usuario = txtUsuario.Text.Trim();
            config.Contrasena = txtContrasena.Text;

            // Persistir en el archivo config.json
            ConfigManager.GuardarConfig(config);

            MessageBox.Show("Configuración guardada correctamente en:\n" + rutaConfig,
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // BOTÓN: PROBAR CONEXIÓN A LA BASE DE DATOS
        private void BtnProbarConexion_Click(object sender, EventArgs e)
        {
            // Crear una configuración temporal con los valores actuales del formulario
            var configPrueba = new AppConfig
            {
                Servidor = txtServidor.Text.Trim(),
                BD = txtBd.Text.Trim(),
                Puerto = txtPuerto.Text.Trim(),
                Usuario = txtUsuario.Text.Trim(),
                Contrasena = txtContrasena.Text
            };

            try
            {
                // Intentar abrir una conexión con esos parámetros
                var conexion = new ConectionBD(configPrueba);
                conexion.AbrirConexion();
                conexion.CerrarConexion();

                MessageBox.Show("Conexión exitosa a la base de datos.\n\n" +
                                $"Servidor: {configPrueba.Servidor}\n" +
                                $"Base de datos: {configPrueba.BD}",
                    "Prueba de conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar a la base de datos:\n\n" + ex.Message,
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewBd_Load(object sender, EventArgs e)
        {
            // Cargar datos desde config.json a los campos
            CargarConfig();
        }
    }
}
