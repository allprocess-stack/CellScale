// Resumen: Formulario para visualizar y consultar el peso de las celdas de carga
// conectadas al bus RS-485. Muestra hasta 4 celdas en slots (label + TextBox + botón).
// Los slots se habilitan/deshabilitan según la cantidad de celdas detectadas.
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

            // Eventos ya suscriptos en Designer.cs (btnCelda1..4, btnPesos, Load)
            this.FormClosing += ViewCeldas_FormClosing;

            timerActualizacion = new Timer();
            timerActualizacion.Interval = 250;
            timerActualizacion.Tick += TimerActualizacion_Tick;
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

            if (slotIndex >= celdasConectadas.Count)
            {
                MessageBox.Show("No se puede consultar peso: la celda no está disponible.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int direccion = celdasConectadas[slotIndex].SlaveNumber;
            double peso = manager.ConsultarPeso(direccion);

            ObtenerTextBox(slotIndex).Text = $"{peso:F2} kg";

            GuardarPesoEnBD($"Celda #{direccion:D2}", peso);

            // Muestra mensaje de confirmación al guardar el peso individual en BD
            MessageBox.Show($"Peso de Celda #{direccion:D2}: {peso:F2} kg guardado en BD.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                {
                    manager.ConsultarPeso(celda.SlaveNumber);
                    // Guarda cada peso en la base de datos
                    GuardarPesoEnBD($"Celda #{celda.SlaveNumber:D2}", celda.CalibratedWeight);
                }
            }

            ActualizarSlots();
            // Muestra mensaje de confirmación al guardar todos los pesos en BD
            MessageBox.Show("Todos los pesos de las celdas conectadas se han guardado en la base de datos.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}
