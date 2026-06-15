using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    public partial class ViewWeightCeldas : Form
    {
        private CeldaManager manager;
        private bool actualizando;

        public ViewWeightCeldas(CeldaManager manager)
        {
            InitializeComponent();
            this.manager = manager;
        }

        private void ViewWeightCeldas_Load(object sender, EventArgs e)
        {
            if (manager == null)
            {
                MessageBox.Show("Error: Sin referencia al manager", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            timerActualizacion.Interval = 500;
            timerActualizacion.Start();
        }

        private Label[] GetLabels()
        {
            return new[] { label1, label2, label3, label4 };
        }

        private TextBox[] GetTextBoxes()
        {
            return new[] { txtViewCelda1, txtViewCelda2, txtViewCelda3, txtViewCelda4 };
        }

        private async void TimerActualizacion_Tick(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;
            if (actualizando) return;

            actualizando = true;
            try
            {
                await Task.Run(() => manager.InicializarCeldasTemporal());
            }
            finally
            {
                actualizando = false;
            }

            var celdas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .Take(4)
                .ToList();

            var txts = GetTextBoxes();
            var lbls = GetLabels();

            for (int i = 0; i < 4; i++)
            {
                if (i < celdas.Count)
                {
                    var c = celdas[i];
                    txts[i].Text = $"{c.CalibratedWeight:F2} kg";
                    lbls[i].Text = $"Celda S{c.SlaveNumber:D2}";
                }
                else
                {
                    txts[i].Text = "---";
                    lbls[i].Text = $"Celda S{i:D2} (sin conexión)";
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timerActualizacion?.Stop();
            timerActualizacion?.Dispose();
            base.OnFormClosing(e);
        }

        private void txtViewCelda2_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
