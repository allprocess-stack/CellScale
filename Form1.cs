using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
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

        private void Form1_Load(object sender, EventArgs e)
        {
            CargarPuertosCOM();
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
                // Varibles
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

                var parametros = new Dictionary<string, object>
                {
                    {"@usuario", usuario},
                    {"@contrasena", contrasena}
                };

                using(var reader = conexion.EjecutarConsulta(query,parametros))
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

            } catch(Exception ex) {
                MessageBox.Show("Error al ingresar:"+ex.Message,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tsmiGuardarMenu_Click(object sender, EventArgs e)
        {
            try
            {
                string conexionBalanza = tscbBalanza.Text;

                var parametros = new Dictionary<string, object>
                {
                    {"@COM", conexionBalanza }
                };

                string query = "INSERT INTO(COM) VALUES(@COM)";
                using (var reader = conexion.EjecutarConsulta(query,parametros))
                {
                    if (reader.Read())
                    {
                        MessageBox.Show("Configuración guardada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error al guardar configuración", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }catch(Exception ex)
            {
                MessageBox.Show("Error al guardar:"+ex.Message,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tsmiAbrirBalanza_Click(object sender, EventArgs e)
        {
            var menu = ((ToolStripDropDownMenu)((ToolStripMenuItem)sender).Owner);
            menu.AutoClose = false; // evita que se cierre

            string puertoBalanza = tscbBalanza.Text;
            if (string.IsNullOrEmpty(puertoBalanza))
            {
                MessageBox.Show("Seleccione un puerto COM para la balanza", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                menu.AutoClose = true; // vuelve al comportamiento normal
                return;
            }

            tscbBalanza.Enabled = false; // bloquea el cambio de ítem
            MessageBox.Show($"Balanza conectada al puerto {puertoBalanza}", "Conexión exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

            menu.AutoClose = true; // vuelve al comportamiento normal
        }

        private void tsmiCerrarBalanza_Click(object sender, EventArgs e)
        {
            var menu = ((ToolStripDropDownMenu)((ToolStripMenuItem)sender).Owner);
            menu.AutoClose = false;

            tscbBalanza.Enabled = true; // desbloquea el ítem
            MessageBox.Show("Balanza desconectada correctamente.", "Desconexión", MessageBoxButtons.OK, MessageBoxIcon.Information);

            menu.AutoClose = true;
        }

        private void tsmiGuardarConfiguracion_Click(object sender, EventArgs e)
        {
            try
            {
                string calibracion = tsmiCalibraciónBalanza.Text.Trim();

                if (string.IsNullOrEmpty(calibracion))
                {
                    MessageBox.Show("Ingrese un valor para calibrar\nla balanza", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ConectionBD conexion = new ConectionBD(ConfigManager.CargarConfig());
                conexion.AbrirConexion();

                var parametro = new Dictionary<string, object>
            {
                {"@calibracion", calibracion }
            };

                string query = "INSERT INTO balanza (COM) VALUES (@calibracion)";
                using (var reader = conexion.EjecutarConsulta(query, parametro))
                {
                    if (reader.Read())
                    {
                        MessageBox.Show("Calibración guardada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error al guardar calibración", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                conexion.CerrarConexion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar calibración:" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Función para Cragar puertos COM disponibles en el sistema y mostrarlos en el ComboBox de la interfaz
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
                tscbBalanza.SelectedIndex = 0;
            }
        }

        
    }
}
