using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    public partial class ViewWeightCeldas : Form
    {
        private CeldaManager manager;

        public ViewWeightCeldas(CeldaManager manager)
        {
            InitializeComponent();
            this.manager = manager;
            timerActualizacion.Tick += TimerActualizacion_Tick;
        }

        private void ViewWeightCeldas_Load(object sender, EventArgs e)
        {
            if (manager == null)
            {
                MessageBox.Show("Error: Sin referencia al manager", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            timerActualizacion.Interval = 1000;
            timerActualizacion.Start();
        }

        private async void TimerActualizacion_Tick(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            int[] direcciones = { 0, 1, 2, 3 };
            TextBox[] txts = { txtViewCelda1, txtViewCelda2, txtViewCelda3, txtViewCelda4 };

            for (int i = 0; i < 4; i++)
            {
                await Task.Run(() => manager.ConsultarPeso(direcciones[i]));
                double peso = 0;
                if (manager.Celdas.ContainsKey(direcciones[i]) && manager.Celdas[direcciones[i]].Connected)
                {
                    peso = manager.Celdas[direcciones[i]].CalibratedWeight/10;
                }
                txts[i].Text = $"{peso:F2} kg";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timerActualizacion?.Stop();
            timerActualizacion?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
