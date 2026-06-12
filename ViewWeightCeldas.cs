using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    public partial class ViewWeightCeldas : Form
    {
        private CeldaManager manager;

        /// <summary>Inicializa el formulario de monitoreo de pesos de celdas.</summary>
        public ViewWeightCeldas(CeldaManager manager)
        {
            InitializeComponent();
            this.manager = manager;
        }

        /// <summary>Inicia el timer de actualización al cargar el formulario.</summary>
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

        /// <summary>Obtiene los labels de las 4 celdas.</summary>
        private Label[] GetLabels()
        {
            return new[] { label1, label2, label3, label4 };
        }

        /// <summary>Obtiene los TextBox de las 4 celdas.</summary>
        private TextBox[] GetTextBoxes()
        {
            return new[] { txtViewCelda1, txtViewCelda2, txtViewCelda3, txtViewCelda4 };
        }

        /// <summary>Timer que actualiza los pesos de todas las celdas cada 1 segundo.</summary>
        private async void TimerActualizacion_Tick(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            var celdasConectadas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .Take(4)
                .ToList();

            var txts = GetTextBoxes();
            var lbls = GetLabels();

            for (int i = 0; i < 4; i++)
            {
                if (i < celdasConectadas.Count)
                {
                    int addr = celdasConectadas[i].SlaveNumber;
                    await Task.Run(() => manager.ConsultarPeso(addr));
                    double peso = manager.Celdas.ContainsKey(addr)
                        ? manager.Celdas[addr].CalibratedWeight
                        : 0;
                    txts[i].Text = $"{peso:F2} kg";
                    lbls[i].Text = $"Celda S{addr:D2}";
                }
                else
                {
                    txts[i].Text = "---";
                    lbls[i].Text = $"Celda S{i:D2} (sin conexión)";
                }
            }
        }

        /// <summary>Detiene y libera el timer al cerrar el formulario.</summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timerActualizacion?.Stop();
            timerActualizacion?.Dispose();
            base.OnFormClosing(e);
        }

        
    }
}
