// Resumen: Formulario para visualizar y consultar el peso de las celdas de carga
// conectadas al bus RS-485. Muestra hasta 4 celdas en slots (label + TextBox + botón),
// con un ComboBox para navegar entre ellas. Incluye timer de actualización periódica.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    public partial class ViewCeldas : Form
    {
        private CeldaManager manager;
        private ConectionBD conexion;
        private Timer timerActualizacion;

        public ViewCeldas(CeldaManager manager, ConectionBD conexion)
        {
            InitializeComponent();

            this.manager = manager;
            this.conexion = conexion;

            // Eventos ya suscriptos en Designer.cs (cbxCeldas, btnCelda1..4, btnPesos, Load)
            this.FormClosing += ViewCeldas_FormClosing;

            timerActualizacion = new Timer();
            timerActualizacion.Interval = 250;
            timerActualizacion.Tick += TimerActualizacion_Tick;
        }

        private void ViewCeldas_Load(object sender, EventArgs e)
        {
            if (manager == null)
            {
                cbxCeldas.Items.Clear();
                cbxCeldas.Items.Add("Error: Sin referencia al manager");
                return;
            }

            CargarCeldasConectadas();

            if (cbxCeldas.Items.Count > 0)
                cbxCeldas.SelectedIndex = 0;

            manager.PesoActualizado += Manager_PesoActualizado;
            timerActualizacion.Start();
        }

        private void CargarCeldasConectadas()
        {
            cbxCeldas.Items.Clear();

            var celdasConectadas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .ToList();

            if (celdasConectadas.Count == 0)
            {
                cbxCeldas.Items.Add("No hay celdas conectadas");
                return;
            }

            foreach (var celda in celdasConectadas)
            {
                string serial = celda.SerialNumber ?? "N/A";
                if (serial.Length > 20)
                    serial = serial.Substring(0, 20) + "...";
                cbxCeldas.Items.Add($"Celda #{celda.SlaveNumber:D2} - {serial}");
            }
        }

        private void cbxCeldas_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarSlots();
        }

        private void ActualizarSlots()
        {
            if (manager == null || cbxCeldas.SelectedIndex < 0) return;

            var celdasConectadas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .ToList();

            if (celdasConectadas.Count == 0) return;

            int startIndex = cbxCeldas.SelectedIndex;

            for (int i = 0; i < 4; i++)
            {
                Label lbl = ObtenerLabel(i);
                TextBox txt = ObtenerTextBox(i);
                Button btn = ObtenerButton(i);

                int celdaIndex = startIndex + i;

                if (celdaIndex < celdasConectadas.Count)
                {
                    var celda = celdasConectadas[celdaIndex];

                    lbl.Text = $"Celda #{celda.SlaveNumber:D2}";
                    lbl.ForeColor = SystemColors.ControlText;

                    txt.Text = $"{celda.CalibratedWeight:F2} kg";
                    txt.Enabled = true;

                    btn.Tag = celda.SlaveNumber;
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

        private void btnCelda1_Click(object sender, EventArgs e) => ConsultarPesoSlot(0);
        private void btnCelda2_Click(object sender, EventArgs e) => ConsultarPesoSlot(1);
        private void btnCelda3_Click(object sender, EventArgs e) => ConsultarPesoSlot(2);
        private void btnCelda4_Click(object sender, EventArgs e) => ConsultarPesoSlot(3);

        private void ConsultarPesoSlot(int slotIndex)
        {
            if (manager == null || !manager.IsOpen) { 
                MessageBox.Show("No se puede consultar peso: manager no inicializado o puerto cerrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var celdasConectadas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .ToList();

            int startIndex = cbxCeldas.SelectedIndex;
            if (startIndex < 0 || startIndex + slotIndex >= celdasConectadas.Count)
            {
                MessageBox.Show("No se puede consultar peso: la celda no está disponible.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int direccion = celdasConectadas[startIndex + slotIndex].SlaveNumber;
            double peso = manager.ConsultarPeso(direccion);

            ObtenerTextBox(slotIndex).Text = $"{peso:F2} kg";

            GuardarPesoEnBD($"Celda #{direccion:D2}", peso);
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

        private void btnPesos_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            foreach (var celda in manager.Celdas.Values)
            {
                if (celda.Connected)
                    manager.ConsultarPeso(celda.SlaveNumber);
            }

            ActualizarSlots();
        }

        private void TimerActualizacion_Tick(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            int cantidadConectadas = manager.Celdas.Values.Count(c => c.Connected);

            if (cantidadConectadas != cbxCeldas.Items.Count
                && !cbxCeldas.Text.Contains("No hay")
                && !cbxCeldas.Text.Contains("Error"))
            {
                int selectedIndex = cbxCeldas.SelectedIndex;
                CargarCeldasConectadas();
                if (selectedIndex >= 0 && selectedIndex < cbxCeldas.Items.Count)
                    cbxCeldas.SelectedIndex = selectedIndex;
                else if (cbxCeldas.Items.Count > 0)
                    cbxCeldas.SelectedIndex = 0;
            }

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
    }
}
