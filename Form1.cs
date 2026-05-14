using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    public partial class Form1 : Form
    {
        private AppConfig config;
        private ConectionBD conexion;
        public Form1()
        {
            InitializeComponent();
            //Opcion de Menu y Configuracion ocultas hasta que se loguee
            tsddbMenu.Visible = false;
            tsddbConfiguracion.Visible = false;
            config = ConfigManager.CargarConfig();
            Console.WriteLine($"Conectando a {config.Servidor}:{config.Puerto} / BD: {config.BD}");

            // Inicializar conexión con la configuración
            conexion = new ConectionBD(config);
            conexion.AbrirConexion();
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tsmiIngresar_Click(object sender, EventArgs e)
        {
            try {
                //Varibles
                string usuario = txtUsuario.Text.Trim();
                string contrasena= txtContraseña.Text.Trim();

                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
                {
                    MessageBox.Show("Ingrese Credenciales", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ConectionBD conexion = new ConectionBD(ConfigManager.CargarConfig());
                conexion.AbrirConexion();

                string query = "SELECT * FROM usuario WHERE nombre=@usuario AND contrasena=@contrasena";
                using(var cmd=new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    cmd.Parameters.AddWithValue("@usuario",usuario);
                    cmd.Parameters.AddWithValue("@contrasena", contrasena);

                    using(var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            MessageBox.Show("Login exitoso", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            tsddbConfiguracion.Visible = true;
                            tsddbMenu.Visible = true;
                        }
                        else
                        {
                            MessageBox.Show("Credenciales incorrectas\nIntente nuevamente", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    conexion.CerrarConexion();
                }

            } catch(Exception ex) {
                MessageBox.Show("Error al ingresar:"+ex.Message,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
