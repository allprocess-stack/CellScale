// Resumen: Formulario para visualizar y consultar el peso de las celdas de carga
// conectadas al bus RS-485. Muestra hasta 4 celdas en slots (label + TextBox + botón).
// Los slots se habilitan/deshabilitan según la cantidad de celdas detectadas.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    public partial class ViewCeldas : Form
    {
        private CeldaManager manager;
        private ConectionBD conexion;
        private Timer timerActualizacion;
        private TextBox[] txtConsultCelda;

        public ViewCeldas(CeldaManager manager, ConectionBD conexion)
        {
            InitializeComponent();

            this.manager = manager;
            this.conexion = conexion;

            // Eventos ya suscriptos en Designer.cs (btnCelda1..4, btnPesos, Load)
            this.FormClosing += ViewCeldas_FormClosing;

            timerActualizacion = new Timer();
            timerActualizacion.Interval = 250;
            timerActualizacion.Tick += TimerActualizacion_Tick;

            InicializarConsultTextBoxes();
        }

        private void InicializarConsultTextBoxes()
        {
            int[] xPositions = { 26, 140, 250, 363 };
            int yPos = 270;
            txtConsultCelda = new TextBox[4];

            for (int i = 0; i < 4; i++)
            {
                txtConsultCelda[i] = new TextBox
                {
                    Location = new Point(xPositions[i], yPos),
                    Size = new Size(100, 20),
                    Name = $"txtConsultCelda{i + 1}",
                    Text = $"S0{i}"
                };
                Controls.Add(txtConsultCelda[i]);
            }

            this.ClientSize = new Size(483, 320);
        }

        private void ViewCeldas_Load(object sender, EventArgs e)
        {
            if (manager == null)
            {
                MessageBox.Show("Error: Sin referencia al manager", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ActualizarSlots();

            manager.PesoActualizado += Manager_PesoActualizado;
            timerActualizacion.Start();
        }

        private void ActualizarSlots()
        {
            if (manager == null) return;

            var celdasConectadas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .ToList();

            for (int i = 0; i < 4; i++)
            {
                Label lbl = ObtenerLabel(i);
                TextBox txt = ObtenerTextBox(i);
                Button btn = ObtenerButton(i);

                if (i < celdasConectadas.Count)
                {
                    var celda = celdasConectadas[i];

                    lbl.Text = $"Celda #{celda.SlaveNumber:D2}";
                    lbl.ForeColor = SystemColors.ControlText;

                    txt.Text = $"{celda.CalibratedWeight:F2} kg";
                    txt.Enabled = true;

                    btn.Tag = celda.SlaveNumber;
                    btn.Enabled = true;
                }
                else if (manager.IsOpen)
                {
                    int addr = i + 1;
                    lbl.Text = $"Celda #{addr:D2}";
                    lbl.ForeColor = SystemColors.ControlText;

                    txt.Text = "---";
                    txt.Enabled = true;

                    btn.Tag = addr;
                    btn.Enabled = true;
                }
                else
                {
                    lbl.Text = $"Celda #--";
                    lbl.ForeColor = Color.Gray;

                    txt.Text = "---";
                    txt.Enabled = false;

                    btn.Tag = null;
                    btn.Enabled = false;
                }
            }
        }

        private Label ObtenerLabel(int index)
        {
            switch (index)
            {
                case 0: return label2;
                case 1: return label3;
                case 2: return label4;
                case 3: return label5;
                default: return null;
            }
        }

        private TextBox ObtenerTextBox(int index)
        {
            switch (index)
            {
                case 0: return txtCelda1;
                case 1: return txtCelda2;
                case 2: return txtCelda3;
                case 3: return txtCelda4;
                default: return null;
            }
        }

        private Button ObtenerButton(int index)
        {
            switch (index)
            {
                case 0: return btnCelda1;
                case 1: return btnCelda2;
                case 2: return btnCelda3;
                case 3: return btnCelda4;
                default: return null;
            }
        }

        private TextBox ObtenerConsultTextBox(int index)
        {
            if (index >= 0 && index < txtConsultCelda.Length)
                return txtConsultCelda[index];
            return null;
        }

        private int ParsearDireccionConsult(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return -1;
            string limpio = text.Trim().ToUpper().Replace("S", "").Replace(" ", "");
            if (int.TryParse(limpio, out int addr))
                return addr;
            return -1;
        }

        private async void btnCelda1_Click(object sender, EventArgs e) => await ConsultarPesoSlotAsync(0);
        private async void btnCelda2_Click(object sender, EventArgs e) => await ConsultarPesoSlotAsync(1);
        private async void btnCelda3_Click(object sender, EventArgs e) => await ConsultarPesoSlotAsync(2);
        private async void btnCelda4_Click(object sender, EventArgs e) => await ConsultarPesoSlotAsync(3);

        private async Task ConsultarPesoSlotAsync(int slotIndex)
        {
            if (manager == null || !manager.IsOpen) { 
                MessageBox.Show("No se puede consultar peso: manager no inicializado o puerto cerrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TextBox txtConsult = ObtenerConsultTextBox(slotIndex);
            Button btn = ObtenerButton(slotIndex);
            int direccion;

            if (txtConsult != null && !string.IsNullOrWhiteSpace(txtConsult.Text))
            {
                direccion = ParsearDireccionConsult(txtConsult.Text);
                if (direccion < 0)
                {
                    MessageBox.Show($"Dirección inválida en txtConsultCelda{slotIndex + 1}. Use formato: S00, 00, 0, etc.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                txtConsult.Text = $"S{direccion:D2}";
            }
            else if (btn?.Tag is int tagAddr)
            {
                direccion = tagAddr;
            }
            else
            {
                MessageBox.Show("No se puede consultar peso: la celda no está disponible.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btn.Enabled = false;
            try
            {
                double peso = await Task.Run(() => manager.ConsultarPesoMultiLinea(direccion));

                ObtenerTextBox(slotIndex).Text = $"{peso:F2} kg";

                GuardarPesoEnBD($"Celda #{direccion:D2}", peso);

                MessageBox.Show($"Peso de Celda #{direccion:D2}: {peso:F2} kg guardado en BD.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                btn.Enabled = true;
            }
        }

        private void GuardarPesoEnBD(string nombreCelda, double peso)
        {
            if (conexion == null) return;

            try
            {
                var parametros = new Dictionary<string, object>
                {
                    {"@nombre_celda", nombreCelda},
                    {"@valor_peso", peso},
                    {"@fecha_registro", DateTime.Now}
                };

                string query = "INSERT INTO celda_peso (nombre_celda, valor_peso, fecha_registro) VALUES(@nombre_celda, @valor_peso, @fecha_registro)";
                conexion.EjecutarNonQuery(query, parametros);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar peso en BD: {ex.Message}");
            }
        }

        private async void btnPesos_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            btnPesos.Enabled = false;
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    TextBox txtConsult = ObtenerConsultTextBox(i);
                    int direccion;

                    if (txtConsult != null && !string.IsNullOrWhiteSpace(txtConsult.Text))
                    {
                        direccion = ParsearDireccionConsult(txtConsult.Text);
                        if (direccion < 0) continue;
                        txtConsult.Text = $"S{direccion:D2}";
                    }
                    else
                    {
                        direccion = i + 1;
                    }

                    double peso = await Task.Run(() => manager.ConsultarPesoMultiLinea(direccion));
                    ObtenerTextBox(i).Text = $"{peso:F2} kg";
                    GuardarPesoEnBD($"Celda #{direccion:D2}", peso);
                }

                ActualizarSlots();
                MessageBox.Show("Todas las celdas consultadas y guardadas en BD.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                btnPesos.Enabled = true;
            }
        }

        private void TimerActualizacion_Tick(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            ActualizarSlots();
        }

        private void Manager_PesoActualizado(int direccion, double pesoCalibrado)
        {
            if (this.IsHandleCreated)
                this.Invoke(new Action(() => ActualizarSlots()));
        }

        private void ViewCeldas_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerActualizacion?.Stop();
            timerActualizacion?.Dispose();

            if (manager != null)
                manager.PesoActualizado -= Manager_PesoActualizado;
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
