using System;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    public partial class ViewCeldaConfig : Form
    {
        private AppConfig config;

        public ViewCeldaConfig(AppConfig config)
        {
            InitializeComponent();
            this.config = config;
        }

        private void ViewCeldaConfig_Load(object sender, EventArgs e)
        {
            CargarConfig();
        }

        private void CargarConfig()
        {
            if (config == null) return;
            txtCelda1.Text = config.Celda1;
            txtCelda2.Text = config.Celda2;
            txtCelda3.Text = config.Celda3;
            txtCelda4.Text = config.Celda4;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (config == null) return;

            config.Celda1 = txtCelda1.Text.Trim();
            config.Celda2 = txtCelda2.Text.Trim();
            config.Celda3 = txtCelda3.Text.Trim();
            config.Celda4 = txtCelda4.Text.Trim();

            config.Celdas = $"{config.Celda1},{config.Celda2},{config.Celda3},{config.Celda4}";
            ConfigManager.GuardarConfig(config);

            MessageBox.Show("Configuración de celdas guardada correctamente",
                "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
