using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FormulaGaussExample
{
    /// <summary>
    /// MODIFICADO: Formulario completamente funcional para visualizar y seleccionar celdas.
    ///
    /// MAPA DE CONTROLES (del diseñador, sin modificar):
    ///   cbxCeldas  -> cbxCeldas       (ComboBox de selección de celdas conectadas)
    ///   label1     -> "CELDAS:"       (etiqueta del ComboBox)
    ///   label2..5  -> lblSlot1..4     (etiquetas "Celda1".."Celda4")
    ///   textBox1..4-> txtSlot1..4     (TextBox que muestran el peso)
    ///   button1..4 -> btnSlot1..4     (Botones "Consultar Peso" individual)
    ///   button5    -> btnConsultarTodos (Botón "Consultar Pesos" general)
    ///
    /// FUNCIONAMIENTO:
    ///   - El ComboBox lista todas las celdas CONECTADAS al COM.
    ///   - Al seleccionar una celda en el ComboBox, los 4 slots se actualizan
    ///     mostrando la celda seleccionada y las 3 siguientes conectadas.
    ///   - Cada slot (label + textBox + button) permite consultar el peso
    ///     de su celda individualmente.
    ///   - Un timer interno actualiza los pesos cada 1 segundo.
    ///   - El botón "Consultar Pesos" (button5) refresca todas las celdas.
    /// </summary>
    public partial class ViewCeldas : Form
    {
        // Referencia al manager de celdas recibida desde ViewMain
        private CeldaManager manager;

        // Timer interno para actualización periódica de pesos (1 segundo)
        private Timer timerActualizacion;

        // Constructor: recibe el manager de celdas desde el formulario principal
        public ViewCeldas(CeldaManager manager)
        {
            InitializeComponent();

            // Guardar referencia al manager de celdas
            this.manager = manager;

            // --- Vincular eventos manualmente en código (sin tocar el diseñador) ---
            // Evento del ComboBox al cambiar selección
            this.cbxCeldas.SelectedIndexChanged += cbxCeldas_SelectedIndexChanged;

            // Eventos Click de los botones individuales de cada slot
            this.btnCelda1.Click += BtnSlot1_Click;
            this.btnCelda2.Click += BtnSlot2_Click;
            this.btnCelda3.Click += BtnSlot3_Click;
            this.btnCelda4.Click += BtnSlot4_Click;

            // Evento Click del botón "Consultar Pesos" general
            this.btnPesos.Click += BtnConsultarTodos_Click;

            // Evento de cierre del formulario para limpiar recursos
            this.FormClosing += ViewCeldas_FormClosing;

            // Crear y configurar timer interno de actualización (1 segundo)
            timerActualizacion = new Timer();
            timerActualizacion.Interval = 1000;
            timerActualizacion.Tick += TimerActualizacion_Tick;
        }

        // Al cargar el formulario: poblar el ComboBox y mostrar las primeras celdas
        private void ViewCeldas_Load(object sender, EventArgs e)
        {
            // Verificar que el manager sea válido
            if (manager == null)
            {
                cbxCeldas.Items.Clear();
                cbxCeldas.Items.Add("Error: Sin referencia al manager");
                return;
            }

            // Poblar el ComboBox con las celdas conectadas
            CargarCeldasConectadas();

            // Seleccionar el primer elemento por defecto para activar la vista
            if (cbxCeldas.Items.Count > 0)
            {
                cbxCeldas.SelectedIndex = 0;
            }

            // Suscribirse al evento de peso actualizado del manager
            // para refrescar la UI cuando lleguen nuevos datos
            manager.PesoActualizado += Manager_PesoActualizado;

            // Iniciar el timer de actualización periódica
            timerActualizacion.Start();
        }

        // -----------------------------------------------------------------------
        // CARGA DE CELDAS EN EL COMBOBOX
        // -----------------------------------------------------------------------

        // Recorre el diccionario de celdas del manager y agrega al ComboBox
        // solo aquellas que estén conectadas, ordenadas por dirección.
        private void CargarCeldasConectadas()
        {
            cbxCeldas.Items.Clear();

            // Obtener celdas conectadas ordenadas por número de esclavo
            var celdasConectadas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .ToList();

            if (celdasConectadas.Count == 0)
            {
                cbxCeldas.Items.Add("No hay celdas conectadas");
                return;
            }

            // Agregar cada celda al ComboBox con formato: "Celda #01 - SERIAL"
            foreach (var celda in celdasConectadas)
            {
                string serial = celda.SerialNumber ?? "N/A";
                if (serial.Length > 20)
                    serial = serial.Substring(0, 20) + "...";
                cbxCeldas.Items.Add($"Celda #{celda.SlaveNumber:D2} - {serial}");
            }
        }

        // -----------------------------------------------------------------------
        // EVENTO PRINCIPAL: cbxCeldas_SelectedIndexChanged
        // -----------------------------------------------------------------------

        // Cuando el usuario selecciona una celda en el ComboBox, los 4 slots
        // se actualizan para mostrar la celda seleccionada y las 3 siguientes
        // que estén conectadas (efecto "paginación" de 4 celdas).
        private void cbxCeldas_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarSlots();
        }

        // -----------------------------------------------------------------------
        // ACTUALIZACIÓN DE LOS 4 SLOTS (label + textBox + button)
        // -----------------------------------------------------------------------

        // Toma la celda seleccionada en el ComboBox como punto de partida
        // y muestra 4 celdas consecutivas en los slots 0..3.
        // Si no hay suficientes celdas, los slots restantes se deshabilitan.
        private void ActualizarSlots()
        {
            // Validaciones básicas
            if (manager == null || cbxCeldas.SelectedIndex < 0) return;

            // Obtener lista actual de celdas conectadas ordenadas
            var celdasConectadas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .ToList();

            if (celdasConectadas.Count == 0) return;

            // Índice de inicio: la celda seleccionada en el ComboBox
            int startIndex = cbxCeldas.SelectedIndex;

            // Actualizar cada uno de los 4 slots
            for (int i = 0; i < 4; i++)
            {
                Label lbl = ObtenerLabel(i);
                TextBox txt = ObtenerTextBox(i);
                Button btn = ObtenerButton(i);

                int celdaIndex = startIndex + i;

                if (celdaIndex < celdasConectadas.Count)
                {
                    var celda = celdasConectadas[celdaIndex];

                    // Actualizar label con el número de celda
                    lbl.Text = $"Celda #{celda.SlaveNumber:D2}";
                    lbl.ForeColor = SystemColors.ControlText;

                    // Actualizar TextBox con el peso calibrado
                    txt.Text = $"{celda.CalibratedWeight:F2} kg";
                    txt.Enabled = true;

                    // Guardar dirección en el Tag del botón para referencia
                    btn.Tag = celda.SlaveNumber;
                    btn.Enabled = true;
                }
                else
                {
                    // No hay más celdas conectadas: slot vacío/deshabilitado
                    lbl.Text = $"Celda #--";
                    lbl.ForeColor = Color.Gray;

                    txt.Text = "---";
                    txt.Enabled = false;

                    btn.Tag = null;
                    btn.Enabled = false;
                }
            }
        }

        // -----------------------------------------------------------------------
        // MÉTODOS HELPER: obtener controles por índice de slot (0..3)
        // -----------------------------------------------------------------------

        // label2="Celda1" -> slot 0, label3="Celda2" -> slot 1, etc.
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

        // textBox1 -> slot 0, textBox2 -> slot 1, etc.
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

        // button1 -> slot 0, button2 -> slot 1, etc.
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

        // -----------------------------------------------------------------------
        // EVENTOS DE BOTONES INDIVIDUALES (Consultar Peso por slot)
        // -----------------------------------------------------------------------

        // Cada botón consulta el peso de la celda correspondiente a su slot
        private void BtnSlot1_Click(object sender, EventArgs e) => ConsultarPesoSlot(0);
        private void BtnSlot2_Click(object sender, EventArgs e) => ConsultarPesoSlot(1);
        private void BtnSlot3_Click(object sender, EventArgs e) => ConsultarPesoSlot(2);
        private void BtnSlot4_Click(object sender, EventArgs e) => ConsultarPesoSlot(3);

        // Consulta el peso de la celda en el slot especificado
        private void ConsultarPesoSlot(int slotIndex)
        {
            if (manager == null || !manager.IsOpen) return;

            // Obtener la lista actual de celdas conectadas
            var celdasConectadas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .ToList();

            int startIndex = cbxCeldas.SelectedIndex;
            if (startIndex < 0 || startIndex + slotIndex >= celdasConectadas.Count) return;

            // Consultar el peso de la celda específica
            int direccion = celdasConectadas[startIndex + slotIndex].SlaveNumber;
            double peso = manager.ConsultarPeso(direccion);

            // Actualizar el TextBox del slot con el nuevo peso
            ObtenerTextBox(slotIndex).Text = $"{peso:F2} kg";
        }

        // -----------------------------------------------------------------------
        // BOTÓN "Consultar Pesos" GENERAL (button5)
        // -----------------------------------------------------------------------

        // Consulta el peso de TODAS las celdas conectadas y luego refresca
        // los slots con los nuevos valores.
        private void BtnConsultarTodos_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            // Consultar peso de cada celda conectada
            foreach (var celda in manager.Celdas.Values)
            {
                if (celda.Connected)
                {
                    manager.ConsultarPeso(celda.SlaveNumber);
                }
            }

            // Refrescar la vista de los slots
            ActualizarSlots();
        }

        // -----------------------------------------------------------------------
        // TIMER DE ACTUALIZACIÓN PERIÓDICA (1 segundo)
        // -----------------------------------------------------------------------

        // El timer refresca los pesos mostrados en los slots cada 1 segundo
        // (misma frecuencia que el timer principal de ViewMain).
        // Si la cantidad de celdas conectadas cambió, repoblar el ComboBox.
        private void TimerActualizacion_Tick(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            // Verificar si la cantidad de celdas conectadas cambió
            int cantidadConectadas = manager.Celdas.Values.Count(c => c.Connected);

            // Repoblar el ComboBox si la cantidad de celdas cambió
            // (se conectó/desconectó una celda)
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

            // Refrescar los valores mostrados
            ActualizarSlots();
        }

        // -----------------------------------------------------------------------
        // EVENTO DEL MANAGER: cuando se actualiza un peso externamente
        // -----------------------------------------------------------------------

        // Si otra parte de la aplicación (como ViewMain) actualiza un peso,
        // este evento refresca la UI para mostrar los nuevos valores.
        private void Manager_PesoActualizado(int direccion, double pesoCalibrado)
        {
            if (this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    ActualizarSlots();
                }));
            }
        }

        // -----------------------------------------------------------------------
        // CIERRE DEL FORMULARIO: limpiar recursos
        // -----------------------------------------------------------------------

        // Al cerrar, se detiene el timer y se desuscribe del evento del manager
        // para evitar llamadas a un formulario ya destruido.
        private void ViewCeldas_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerActualizacion?.Stop();
            timerActualizacion?.Dispose();

            if (manager != null)
            {
                manager.PesoActualizado -= Manager_PesoActualizado;
            }
        }
    }
}
