// Resumen: Formulario para visualizar y consultar el peso de las celdas de carga
// conectadas al bus RS-485. Muestra hasta 4 celdas en slots (label + TextBox + botón).
// Los slots se habilitan/deshabilitan según la cantidad de celdas detectadas.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

        // Campos de calibración de esquinas (excentricidad)
        private double[] ceros = new double[4];
        private double[] factores = new double[4] { 1.0, 1.0, 1.0, 1.0 };
        private bool cerosCapturados = false;
        private int esquinasCapturadas = 0;
        private double pesoCalibracion = 100.0;
        private bool usarCompensacionEsquinas = false;

        // Campos de calibración multivariable (Gauss 5 puntos)
        private CalibracionLineal calibracionGauss;
        private List<PuntoCalibracion> puntosGauss;
        private int puntoActualGauss = 0;

        // Monitoreo de celdas en vivo
        private Label[] lblMonDir;
        private Label[] lblMonRaw;
        private Label[] lblMonCal;
        private Label[] lblMonTime;
        private TextBox txtPesoCalRapido;
        private Button btnCalRapido;
        private Label[] lblFactores;
        private CheckBox chkSimular;
        private TextBox[] txtSimW;
        private TextBox txtPosX;
        private TextBox txtPosY;
        private Button btnCalcPos;
        private Label[] lblDistPct;
        private Button[] btnPreset;

        /// <summary>Inicializa el formulario de visualización y calibración de celdas.</summary>
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
            InicializarEstadoCalibracion();
            InicializarCalibracionGauss();
            InicializarMonitoreo();
            CargarPesoCalibracionConfig();
        }

        /// <summary>Inicializa los campos de calibración de esquinas a sus valores por defecto.</summary>
        private void InicializarEstadoCalibracion()
        {
            ceros = new double[4];
            factores = new double[4] { 1.0, 1.0, 1.0, 1.0 };
            cerosCapturados = false;
            esquinasCapturadas = 0;
            usarCompensacionEsquinas = false;
            ActualizarEstadoCalibracionUI();
        }

        /// <summary>Actualiza la UI según el estado actual de la calibración de esquinas.</summary>
        private void ActualizarEstadoCalibracionUI()
        {
            btnEsquina1.Enabled = cerosCapturados;
            btnEsquina2.Enabled = cerosCapturados;
            btnEsquina3.Enabled = cerosCapturados;
            btnEsquina4.Enabled = cerosCapturados;

            if (!cerosCapturados)
            {
                lblEstadoCalibracion.Text = "Estado: Configure el peso de calibración y presione \"Capturar Cero\" (balanza vacía).";
            }
            else if (!usarCompensacionEsquinas)
            {
                lblEstadoCalibracion.Text = $"Estado: Cero capturado. Coloque el peso de {pesoCalibracion} kg en Esquina {esquinasCapturadas + 1} y presione el botón correspondiente.";
            }
            else
            {
                string fText = string.Join(", ", factores.Select((f, i) => $"F{i + 1}={f:F4}"));
                lblEstadoCalibracion.Text = $"Calibración completa. Factores: {fText}";
            }
        }

        /// <summary>Inicializa el estado de la calibración Gauss multivariable.</summary>
        private void InicializarCalibracionGauss()
        {
            calibracionGauss = new CalibracionLineal();
            puntosGauss = new List<PuntoCalibracion>();
            puntoActualGauss = 0;
            btnAplicarGauss.Enabled = false;
            lblCoefGauss.Text = "Coeficientes: (pendiente...)";
            ActualizarPuntosGaussUI();
        }

        /// <summary>Actualiza los labels de progreso de la calibración Gauss.</summary>
        private void ActualizarPuntosGaussUI()
        {
            lblPuntosGauss.Text = $"Puntos: {puntoActualGauss}/5";
            btnCapturarPuntoGauss.Text = puntoActualGauss >= 5
                ? "Calibración completa"
                : $"Capturar Punto #{puntoActualGauss + 1}";
            btnCapturarPuntoGauss.Enabled = puntoActualGauss < 5;
        }

        /// <summary>Carga el peso de calibración desde config.json al campo de texto.</summary>
        private void CargarPesoCalibracionConfig()
        {
            var config = ConfigManager.CargarConfig();
            if (config != null && !string.IsNullOrEmpty(config.CalibracionBalanza))
            {
                txtPesoCalibracion.Text = config.CalibracionBalanza;
            }
        }

        /// <summary>Inicializa la tabla de monitoreo en vivo dentro de groupBox2.</summary>
        private void InicializarMonitoreo()
        {
            int[] xCols = { 10, 55, 145, 235 };
            int[] colWidths = { 35, 80, 80, 80 };
            int btnX = 340;
            int btnW = 45;

            string[] headers = { "Dir", "Raw (kg)", "Cal (kg)", "Lectura", "Cal" };
            for (int c = 0; c < 4; c++)
            {
                var h = new Label
                {
                    Location = new Point(xCols[c], 15),
                    Size = new Size(colWidths[c], 14),
                    Font = new Font(this.Font, FontStyle.Bold),
                    Text = headers[c]
                };
                groupBox2.Controls.Add(h);
            }
            var hBtn = new Label
            {
                Location = new Point(btnX + 5, 15),
                Size = new Size(btnW, 14),
                Font = new Font(this.Font, FontStyle.Bold),
                Text = headers[4]
            };
            groupBox2.Controls.Add(hBtn);

            lblMonDir = new Label[4];
            lblMonRaw = new Label[4];
            lblMonCal = new Label[4];
            lblMonTime = new Label[4];
            var btnCalRow = new Button[4];

            for (int i = 0; i < 4; i++)
            {
                int y = 32 + i * 17;
                lblMonDir[i] = new Label { Location = new Point(xCols[0], y), Size = new Size(colWidths[0], 14), Text = $"S00" };
                lblMonRaw[i] = new Label { Location = new Point(xCols[1], y), Size = new Size(colWidths[1], 14), Text = "---" };
                lblMonCal[i] = new Label { Location = new Point(xCols[2], y), Size = new Size(colWidths[2], 14), Text = "---" };
                lblMonTime[i] = new Label { Location = new Point(xCols[3], y), Size = new Size(colWidths[3], 14), Text = "" };
                groupBox2.Controls.Add(lblMonDir[i]);
                groupBox2.Controls.Add(lblMonRaw[i]);
                groupBox2.Controls.Add(lblMonCal[i]);
                groupBox2.Controls.Add(lblMonTime[i]);

                int idx = i;
                btnCalRow[i] = new Button
                {
                    Location = new Point(btnX, y - 2),
                    Size = new Size(btnW, 18),
                    Text = $"S{idx:D2}",
                    Tag = idx
                };
                btnCalRow[i].Click += BtnCalRow_Click;
                groupBox2.Controls.Add(btnCalRow[i]);
            }

            int calY = 105;
            var lblCalTitle = new Label
            {
                Location = new Point(16, calY),
                Size = new Size(200, 14),
                Font = new Font(this.Font, FontStyle.Bold),
                Text = "Calibración por celda (esquinas):"
            };
            groupBox2.Controls.Add(lblCalTitle);

            var lblPeso = new Label
            {
                Location = new Point(16, calY + 18),
                Size = new Size(110, 16),
                Text = "Peso conocido (kg):"
            };
            groupBox2.Controls.Add(lblPeso);

            txtPesoCalRapido = new TextBox
            {
                Location = new Point(130, calY + 15),
                Size = new Size(60, 20),
                TextAlign = HorizontalAlignment.Right,
                Text = "100"
            };
            groupBox2.Controls.Add(txtPesoCalRapido);

            btnCalRapido = new Button
            {
                Location = new Point(210, calY + 14),
                Size = new Size(110, 22),
                Text = "Calcular Factores"
            };
            btnCalRapido.Click += BtnCalRapido_Click;
            groupBox2.Controls.Add(btnCalRapido);

            var btnConsultarPeso = new Button
            {
                Location = new Point(325, calY + 14),
                Size = new Size(110, 22),
                Text = "Consultar Peso"
            };
            btnConsultarPeso.Click += BtnConsultarPeso_Click;
            groupBox2.Controls.Add(btnConsultarPeso);

            lblFactores = new Label[4];
            int[] xFact = { 16, 110, 210, 310 };
            for (int i = 0; i < 4; i++)
            {
                lblFactores[i] = new Label
                {
                    Location = new Point(xFact[i], calY + 40),
                    Size = new Size(90, 14),
                    Text = $"F{i + 1}=1.0000"
                };
                groupBox2.Controls.Add(lblFactores[i]);
            }

            int simY = calY + 60;
            chkSimular = new CheckBox
            {
                Location = new Point(16, simY),
                Size = new Size(250, 18),
                Text = "Usar distribución simulada (no mover peso)"
            };
            groupBox2.Controls.Add(chkSimular);

            var lblPos = new Label
            {
                Location = new Point(16, simY + 20),
                Size = new Size(56, 14),
                Text = "Posición:"
            };
            groupBox2.Controls.Add(lblPos);

            var lblX = new Label { Location = new Point(76, simY + 20), Size = new Size(14, 14), Text = "X:" };
            groupBox2.Controls.Add(lblX);

            txtPosX = new TextBox
            {
                Location = new Point(92, simY + 18),
                Size = new Size(42, 20),
                Text = "200",
                Enabled = false
            };
            groupBox2.Controls.Add(txtPosX);

            var lblY = new Label { Location = new Point(142, simY + 20), Size = new Size(14, 14), Text = "Y:" };
            groupBox2.Controls.Add(lblY);

            txtPosY = new TextBox
            {
                Location = new Point(158, simY + 18),
                Size = new Size(42, 20),
                Text = "200",
                Enabled = false
            };
            groupBox2.Controls.Add(txtPosY);

            btnCalcPos = new Button
            {
                Location = new Point(210, simY + 17),
                Size = new Size(75, 22),
                Text = "Calcular",
                Enabled = false
            };
            btnCalcPos.Click += BtnCalcPos_Click;
            groupBox2.Controls.Add(btnCalcPos);

            lblDistPct = new Label[4];
            int[] xDist = { 16, 115, 215, 315 };
            for (int i = 0; i < 4; i++)
            {
                lblDistPct[i] = new Label
                {
                    Location = new Point(xDist[i], simY + 42),
                    Size = new Size(90, 14),
                    Text = $"S0{i}=25.0%"
                };
                groupBox2.Controls.Add(lblDistPct[i]);
            }

            var lblSimDist = new Label
            {
                Location = new Point(16, simY + 58),
                Size = new Size(140, 14),
                Text = "Peso simulado por celda:"
            };
            groupBox2.Controls.Add(lblSimDist);

            int[] xSim = { 150, 230, 310, 390 };
            string[] simLabels = { "S00", "S01", "S02", "S03" };
            txtSimW = new TextBox[4];
            for (int i = 0; i < 4; i++)
            {
                var l = new Label
                {
                    Location = new Point(xSim[i] - 30, simY + 58),
                    Size = new Size(28, 14),
                    Text = simLabels[i]
                };
                groupBox2.Controls.Add(l);
                txtSimW[i] = new TextBox
                {
                    Location = new Point(xSim[i], simY + 56),
                    Size = new Size(50, 20),
                    TextAlign = HorizontalAlignment.Right,
                    Text = "0",
                    Enabled = false
                };
                groupBox2.Controls.Add(txtSimW[i]);
            }

            btnPreset = new Button[4];
            int[] xPreset = { 16, 110, 210, 310 };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                btnPreset[i] = new Button
                {
                    Location = new Point(xPreset[i], simY + 80),
                    Size = new Size(80, 22),
                    Text = $"Peso→S0{i}",
                    Enabled = false
                };
                btnPreset[i].Click += (s, e) => CargarDistribucionSimulada(idx);
                groupBox2.Controls.Add(btnPreset[i]);
            }

            chkSimular.CheckedChanged += (s, e) =>
            {
                bool enable = chkSimular.Checked;
                txtPosX.Enabled = enable;
                txtPosY.Enabled = enable;
                btnCalcPos.Enabled = enable;
                for (int i = 0; i < 4; i++)
                {
                    txtSimW[i].Enabled = enable;
                    btnPreset[i].Enabled = enable;
                }
            };

            BtnCalcPos_Click(null, null);
        }

        private void CargarDistribucionSimulada(int esquina)
        {
            if (!double.TryParse(txtPesoCalRapido.Text, out double total) || total <= 0)
                total = 100.0;

            double[][] dist = {
                new double[] { 92, 3, 3, 2 },
                new double[] { 3, 92, 2, 3 },
                new double[] { 3, 2, 92, 3 },
                new double[] { 2, 3, 3, 92 }
            };

            for (int i = 0; i < 4; i++)
                txtSimW[i].Text = $"{total * dist[esquina][i] / 100.0:F2}";
        }

        private void BtnCalcPos_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(txtPesoCalRapido.Text, out double total) || total <= 0)
                total = 100.0;

            if (!double.TryParse(txtPosX.Text, out double posX)) posX = 200;
            if (!double.TryParse(txtPosY.Text, out double posY)) posY = 200;

            const double SQUARE_SIZE = 400.0;
            posX = Math.Max(0, Math.Min(SQUARE_SIZE, posX));
            posY = Math.Max(0, Math.Min(SQUARE_SIZE, posY));

            double nx = posX / SQUARE_SIZE;
            double ny = posY / SQUARE_SIZE;

            double w00 = (1 - nx) * (1 - ny);
            double w01 = nx * (1 - ny);
            double w02 = (1 - nx) * ny;
            double w03 = nx * ny;

            double sum = w00 + w01 + w02 + w03;
            if (sum > 0) { w00 /= sum; w01 /= sum; w02 /= sum; w03 /= sum; }

            double[] pcts = { w00 * 100, w01 * 100, w02 * 100, w03 * 100 };
            double[] weights = { w00 * total, w01 * total, w02 * total, w03 * total };

            for (int i = 0; i < 4; i++)
            {
                lblDistPct[i].Text = $"S0{i}={pcts[i]:F1}%";
                txtSimW[i].Text = weights[i].ToString("F2");
            }
        }

        /// <summary>Actualiza las etiquetas de monitoreo con los datos actuales de las celdas.</summary>
        private void ActualizarMonitoreo()
        {
            if (manager == null) return;

            var celdas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .Take(4)
                .ToList();

            for (int i = 0; i < 4; i++)
            {
                if (lblMonDir == null) break;

                if (i < celdas.Count)
                {
                    var c = celdas[i];
                    lblMonDir[i].Text = $"S{c.SlaveNumber:D2}";
                    lblMonRaw[i].Text = $"{c.RawWeight:F2}";
                    lblMonCal[i].Text = $"{c.CalibratedWeight:F2}";
                    lblMonTime[i].Text = c.LastRead != default ? c.LastRead.ToString("HH:mm:ss") : "";
                }
                else
                {
                    lblMonDir[i].Text = $"S{i:D2}";
                    lblMonRaw[i].Text = "---";
                    lblMonCal[i].Text = "---";
                    lblMonTime[i].Text = "";
                }
            }
        }

        /// <summary>Crea los TextBox para consultar direcciones personalizadas de celdas.</summary>
        private void InicializarConsultTextBoxes()
        {
            int[] xPositions = { 26, 140, 250, 363 };
            int yPos = 575;
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
        }

        /// <summary>Configura la vista al cargar: suscribe eventos e inicia el timer.</summary>
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

        /// <summary>Actualiza los labels, TextBox y botones de cada slot según las celdas conectadas.</summary>
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

                    double peso;
                    if (manager.UsarCalibracionMultivariable)
                    {
                        peso = manager.ObtenerPesoUnificado();
                    }
                    else if (usarCompensacionEsquinas && i < 4)
                    {
                        double neto = celda.CalibratedWeight - ceros[i];
                        peso = neto * factores[i];
                    }
                    else
                    {
                        peso = celda.CalibratedWeight;
                    }
                    txt.Text = $"{peso:F2} kg";
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

        /// <summary>Obtiene el Label del slot indicado (0-3).</summary>
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

        /// <summary>Obtiene el TextBox de peso del slot indicado (0-3).</summary>
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

        /// <summary>Obtiene el Button de consulta del slot indicado (0-3).</summary>
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

        /// <summary>Obtiene el TextBox de dirección personalizada del slot indicado.</summary>
        private TextBox ObtenerConsultTextBox(int index)
        {
            if (index >= 0 && index < txtConsultCelda.Length)
                return txtConsultCelda[index];
            return null;
        }

        /// <summary>Convierte un texto como "S01" o "1" a una dirección numérica de celda.</summary>
        private int ParsearDireccionConsult(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return -1;
            string limpio = text.Trim().ToUpper().Replace("S", "").Replace(" ", "");
            if (int.TryParse(limpio, out int addr))
                return addr;
            return -1;
        }

        /// <summary>Consulta el peso de la celda en el slot 0.</summary>
        private async void btnCelda1_Click(object sender, EventArgs e) => await ConsultarPesoSlotAsync(0);
        /// <summary>Consulta el peso de la celda en el slot 1.</summary>
        private async void btnCelda2_Click(object sender, EventArgs e) => await ConsultarPesoSlotAsync(1);
        /// <summary>Consulta el peso de la celda en el slot 2.</summary>
        private async void btnCelda3_Click(object sender, EventArgs e) => await ConsultarPesoSlotAsync(2);
        /// <summary>Consulta el peso de la celda en el slot 3.</summary>
        private async void btnCelda4_Click(object sender, EventArgs e) => await ConsultarPesoSlotAsync(3);

        /// <summary>Consulta el peso de una celda en segundo plano y actualiza la UI.</summary>
        private async Task ConsultarPesoSlotAsync(int slotIndex)
        {
            if (manager == null || !manager.IsOpen) { 
                MessageBox.Show("No se puede consultar peso: manager no inicializado o puerto cerrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TextBox txtConsult = ObtenerConsultTextBox(slotIndex);
            Button btn = ObtenerButton(slotIndex);
            TextBox txtPeso = ObtenerTextBox(slotIndex);
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

            // Deshabilitar txtCelda y botón durante la consulta
            txtPeso.Enabled = false;
            btn.Enabled = false;
            try
            {
                double peso = await Task.Run(() => manager.ConsultarPeso(direccion));

                // Aplicar compensación de esquinas si está calibrada
                double pesoMostrar = peso;
                if (usarCompensacionEsquinas && slotIndex < 4)
                {
                    double neto = peso - ceros[slotIndex];
                    pesoMostrar = neto * factores[slotIndex];
                }

                txtPeso.Text = $"{pesoMostrar:F2} kg";

                GuardarPesoEnBD($"Celda #{direccion:D2}", peso);

                MessageBox.Show($"Peso de Celda #{direccion:D2}: {pesoMostrar:F2} kg guardado en BD.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                txtPeso.Enabled = true;
                btn.Enabled = true;
            }
        }

        /// <summary>Guarda el peso de una celda en la tabla celda_peso de la BD.</summary>
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

        /// <summary>Consulta los pesos de todas las celdas simultáneamente y los guarda en BD.</summary>
        private async void btnPesos_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            // Deshabilitar todos los txtCelda y botones durante la consulta
            btnPesos.Enabled = false;
            for (int i = 0; i < 4; i++)
            {
                TextBox txt = ObtenerTextBox(i);
                if (txt != null) txt.Enabled = false;
                Button btn = ObtenerButton(i);
                if (btn != null) btn.Enabled = false;
            }
            try
            {
                var celdasConsulta = manager.Celdas.Values
                    .Where(c => c.Connected)
                    .OrderBy(c => c.SlaveNumber)
                    .Take(4)
                    .ToList();

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
                    else if (i < celdasConsulta.Count)
                    {
                        direccion = celdasConsulta[i].SlaveNumber;
                    }
                    else
                    {
                        direccion = i + 1;
                    }

                    double peso = await Task.Run(() => manager.ConsultarPeso(direccion));

                    double pesoMostrar = peso;
                    if (usarCompensacionEsquinas && i < 4)
                    {
                        double neto = peso - ceros[i];
                        pesoMostrar = neto * factores[i];
                    }

                    ObtenerTextBox(i).Text = $"{pesoMostrar:F2} kg";
                    GuardarPesoEnBD($"Celda #{direccion:D2}", peso);
                }

                ActualizarSlots();
                MessageBox.Show("Todas las celdas consultadas y guardadas en BD.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                btnPesos.Enabled = true;
                for (int i = 0; i < 4; i++)
                {
                    TextBox txt = ObtenerTextBox(i);
                    if (txt != null) txt.Enabled = true;
                    Button btn = ObtenerButton(i);
                    if (btn != null) btn.Enabled = true;
                }
            }
        }

        /// <summary>Timer que actualiza los slots y la tabla de monitoreo periódicamente (250ms).</summary>
        private void TimerActualizacion_Tick(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen) return;

            ActualizarSlots();
            ActualizarMonitoreo();
        }

        /// <summary>Actualiza los slots cuando se recibe un nuevo peso desde el manager.</summary>
        private void Manager_PesoActualizado(int direccion, double pesoCalibrado)
        {
            if (this.IsHandleCreated)
                this.Invoke(new Action(() => ActualizarSlots()));
        }

        // Métodos de Calibración de Esquinas (Compensación de Excentricidad)
        private async void btnCeroCalibracion_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen)
            {
                MessageBox.Show("El puerto serial no está abierto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!double.TryParse(txtPesoCalibracion.Text, out double pesoCal) || pesoCal <= 0)
            {
                MessageBox.Show("Ingrese un peso de calibración válido (> 0).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            pesoCalibracion = pesoCal;

            btnCeroCalibracion.Enabled = false;
            lblEstadoCalibracion.Text = "Capturando cero en todas las celdas... (balanza debe estar vacía)";

            try
            {
                var celdas = manager.Celdas.Values
                    .Where(c => c.Connected)
                    .OrderBy(c => c.SlaveNumber)
                    .Take(4)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[btnCeroCalibracion] celdas conectadas={celdas.Count}, cerosCapturados antes={cerosCapturados}");

                if (celdas.Count < 4)
                {
                    MessageBox.Show($"Se requieren 4 celdas conectadas. Solo hay {celdas.Count}.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                for (int i = 0; i < 4; i++)
                {
                    double peso = await Task.Run(() => manager.ConsultarPeso(celdas[i].SlaveNumber));
                    double raw = celdas[i].RawWeight;
                    ceros[i] = raw;
                    System.Diagnostics.Debug.WriteLine($"[btnCeroCalibracion] celda S{celdas[i].SlaveNumber:D2}: peso={peso}, raw={raw}");
                    ObtenerTextBox(i).Text = $"Z={raw:F2} kg";
                }

                cerosCapturados = true;
                esquinasCapturadas = 0;
                usarCompensacionEsquinas = false;
                lblEstadoCalibracion.Text = $"Cero capturado. Coloque el peso de {pesoCalibracion} kg en la Esquina 1 y presione \"Esquina 1\".";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al capturar cero: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCeroCalibracion.Enabled = true;
            }

            ActualizarEstadoCalibracionUI();
        }

        private async Task CapturarEsquinaAsync(int esquinaIndex)
        {
            if (!cerosCapturados)
            {
                MessageBox.Show("Debe capturar el cero primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Button btn = null;
            switch (esquinaIndex)
            {
                case 0: btn = btnEsquina1; break;
                case 1: btn = btnEsquina2; break;
                case 2: btn = btnEsquina3; break;
                case 3: btn = btnEsquina4; break;
            }

            if (btn != null) btn.Enabled = false;
            lblEstadoCalibracion.Text = $"Capturando lecturas para Esquina {esquinaIndex + 1}...";

            try
            {
                var celdas = manager.Celdas.Values
                    .Where(c => c.Connected)
                    .OrderBy(c => c.SlaveNumber)
                    .Take(4)
                    .ToList();

                if (celdas.Count < 4)
                {
                    MessageBox.Show($"Se requieren 4 celdas conectadas. Solo hay {celdas.Count}.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                double[] rawReadings = new double[4];
                for (int i = 0; i < 4; i++)
                {
                    double peso = await Task.Run(() => manager.ConsultarPeso(celdas[i].SlaveNumber));
                    rawReadings[i] = celdas[i].RawWeight;
                    System.Diagnostics.Debug.WriteLine($"[CapturarEsquina] S{celdas[i].SlaveNumber:D2}: peso={peso}, raw={rawReadings[i]}");
                }

                double sumaNeta = 0;
                double[] netos = new double[4];
                for (int i = 0; i < 4; i++)
                {
                    netos[i] = rawReadings[i] - ceros[i];
                    sumaNeta += netos[i];
                }

                if (sumaNeta <= 0)
                {
                    MessageBox.Show("La suma neta de las lecturas es cero o negativa. Verifique que el peso esté colocado en la esquina correcta.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                factores[esquinaIndex] = pesoCalibracion / sumaNeta;
                esquinasCapturadas++;

                ObtenerTextBox(esquinaIndex).Text = $"F{esquinaIndex + 1}={factores[esquinaIndex]:F4}";

                if (esquinasCapturadas >= 4)
                {
                    AplicarCompensacionEsquinas();
                }
                else
                {
                    lblEstadoCalibracion.Text = $"Esquina {esquinaIndex + 1} capturada (F{esquinaIndex + 1} = {factores[esquinaIndex]:F4}). " +
                        $"Coloque el peso de {pesoCalibracion} kg en la Esquina {esquinasCapturadas + 1} y presione su botón.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al capturar Esquina {esquinaIndex + 1}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (btn != null) btn.Enabled = true;
            }
        }

        private async void btnEsquina1_Click(object sender, EventArgs e)
        {
            if (chkSimular.Checked)
                await CapturarEsquinaSimuladaAsync(0);
            else
                await CapturarEsquinaAsync(0);
        }
        private async void btnEsquina2_Click(object sender, EventArgs e)
        {
            if (chkSimular.Checked)
                await CapturarEsquinaSimuladaAsync(1);
            else
                await CapturarEsquinaAsync(1);
        }
        private async void btnEsquina3_Click(object sender, EventArgs e)
        {
            if (chkSimular.Checked)
                await CapturarEsquinaSimuladaAsync(2);
            else
                await CapturarEsquinaAsync(2);
        }
        private async void btnEsquina4_Click(object sender, EventArgs e)
        {
            if (chkSimular.Checked)
                await CapturarEsquinaSimuladaAsync(3);
            else
                await CapturarEsquinaAsync(3);
        }

        // ============================================================
        // Calibración Multivariable (Gauss 5 puntos)
        // ============================================================

        private async void btnCapturarPuntoGauss_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen)
            {
                MessageBox.Show("El puerto serial no está abierto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!double.TryParse(txtPesoCalibracion.Text.Trim(), out double pesoConocido) || pesoConocido <= 0)
            {
                MessageBox.Show("Ingrese un peso conocido válido en 'Peso Calibración (kg)'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var celdasGauss = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .Take(4)
                .ToList();

            if (celdasGauss.Count < 4)
            {
                MessageBox.Show($"Se requieren 4 celdas conectadas para calibración Gauss. Solo hay {celdasGauss.Count}.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnCapturarPuntoGauss.Enabled = false;
            lblCoefGauss.Text = $"Capturando punto #{puntoActualGauss + 1}...";

            try
            {
                double[] lecturas = new double[4];
                for (int i = 0; i < 4; i++)
                {
                    int addr = celdasGauss[i].SlaveNumber;
                    double peso = await Task.Run(() => manager.ConsultarPeso(addr));
                    double raw = 0;
                    if (manager.Celdas.ContainsKey(addr))
                        raw = manager.Celdas[addr].RawWeight;
                    lecturas[i] = raw;
                }

                double x1 = lecturas[0], x2 = lecturas[1], x3 = lecturas[2], x4 = lecturas[3];

                var punto = new PuntoCalibracion
                {
                    X1 = x1,
                    X2 = x2,
                    X3 = x3,
                    X4 = x4,
                    PesoConocido = pesoConocido
                };
                puntosGauss.Add(punto);
                puntoActualGauss++;

                lblCoefGauss.Text = $"Punto #{puntoActualGauss}: X1={x1:F2}  X2={x2:F2}  X3={x3:F2}  X4={x4:F2}  ->  Peso={pesoConocido:F2} kg";

                if (puntoActualGauss >= 5)
                {
                    ResolverGauss();
                }
                else
                {
                    lblPuntosGauss.Text = $"Puntos: {puntoActualGauss}/5";
                    btnCapturarPuntoGauss.Text = $"Capturar Punto #{puntoActualGauss + 1}";
                    MessageBox.Show($"Punto #{puntoActualGauss} registrado.\n\nCambie el peso sobre la báscula a {pesoConocido} kg y presione nuevamente.",
                        "Punto registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al capturar punto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCapturarPuntoGauss.Enabled = puntoActualGauss < 5;
            }
        }

        private void ResolverGauss()
        {
            try
            {
                bool exito = calibracionGauss.Calibrar(puntosGauss);

                if (exito)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"m1 = {calibracionGauss.Coeficientes[0]:F10}");
                    sb.AppendLine($"m2 = {calibracionGauss.Coeficientes[1]:F10}");
                    sb.AppendLine($"m3 = {calibracionGauss.Coeficientes[2]:F10}");
                    sb.AppendLine($"m4 = {calibracionGauss.Coeficientes[3]:F10}");
                    sb.AppendLine($" B = {calibracionGauss.Bias:F10}");

                    string informe = calibracionGauss.GenerarInforme(puntosGauss);
                    lblCoefGauss.Text = sb.ToString();
                    btnAplicarGauss.Enabled = true;
                    btnCapturarPuntoGauss.Enabled = false;
                    lblPuntosGauss.Text = "Puntos: 5/5 — Resuelto";

                    MessageBox.Show(informe, "Calibración Gauss Exitosa",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se pudo resolver el sistema. Verifique los puntos.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al resolver Gauss: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAplicarGauss_Click(object sender, EventArgs e)
        {
            if (!calibracionGauss.EstaCalibrado)
            {
                MessageBox.Show("No hay calibración resuelta para aplicar.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                double m1 = calibracionGauss.Coeficientes[0];
                double m2 = calibracionGauss.Coeficientes[1];
                double m3 = calibracionGauss.Coeficientes[2];
                double m4 = calibracionGauss.Coeficientes[3];
                double b = calibracionGauss.Bias;

                // Activar en CeldaManager para que ViewMain lo use en tiempo real
                manager.ConfigurarCalibracionMultivariable(m1, m2, m3, m4, b);

                // Notificar al usuario
                lblCoefGauss.Text = $"APLICADO — m1={m1:F6} m2={m2:F6} m3={m3:F6} m4={m4:F6} B={b:F6}";
                btnAplicarGauss.Enabled = false;
                btnAplicarGauss.Text = "✓ Calibración Activa";

                MessageBox.Show(
                    "Coeficientes aplicados al sistema.\n\n" +
                    "txtBalanza en ViewMain ahora refleja el peso calibrado en tiempo real.\n\n" +
                    $"Ecuación: PESO = X1·{m1:F6} + X2·{m2:F6} + X3·{m3:F6} + X4·{m4:F6} + ({b:F6})",
                    "Calibración Gauss Aplicada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar calibración: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewCeldas_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerActualizacion?.Stop();
            timerActualizacion?.Dispose();

            if (manager != null)
            {
                manager.PesoActualizado -= Manager_PesoActualizado;
            }

            for (int i = 0; i < 4; i++)
            {
                TextBox txtPeso = ObtenerTextBox(i);
                if (txtPeso != null)
                    txtPeso.Text = "---";

                TextBox txtConsult = ObtenerConsultTextBox(i);
                if (txtConsult != null)
                    txtConsult.Text = $"S0{i}";
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void txtPesoCalibracion_TextChanged(object sender, EventArgs e)
        {
            var config = ConfigManager.CargarConfig() ?? new AppConfig();
            config.CalibracionBalanza = txtPesoCalibracion.Text.Trim();
            ConfigManager.GuardarConfig(config);
        }

        // ============================================================
        // Calibración por celda desde la tabla de monitoreo
        // ============================================================

        private async void BtnCalRow_Click(object sender, EventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int idx))
                return;

            if (manager == null || !manager.IsOpen)
            {
                MessageBox.Show("El puerto serial no está abierto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btn.Enabled = false;
            try
            {
                var celda = manager.Celdas.Values
                    .Where(c => c.Connected)
                    .OrderBy(c => c.SlaveNumber)
                    .Skip(idx)
                    .FirstOrDefault();

                if (celda == null)
                {
                    MessageBox.Show($"Celda S0{idx} no conectada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await Task.Run(() => manager.ConsultarPeso(celda.SlaveNumber));
                ceros[idx] = celda.RawWeight;
                cerosCapturados = true;

                ObtenerTextBox(idx).Text = $"Z={ceros[idx]:F2} kg";
                lblEstadoCalibracion.Text = $"Celda S0{idx} tarada (cero = {ceros[idx]:F2} kg).";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al tarar celda S0{idx}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn.Enabled = true;
            }
            ActualizarMonitoreo();
            ActualizarEstadoCalibracionUI();
        }

        private async Task CapturarEsquinaSimuladaAsync(int esquinaIndex)
        {
            if (!cerosCapturados)
            {
                MessageBox.Show("Debe capturar el cero primero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Button btn = null;
            switch (esquinaIndex)
            {
                case 0: btn = btnEsquina1; break;
                case 1: btn = btnEsquina2; break;
                case 2: btn = btnEsquina3; break;
                case 3: btn = btnEsquina4; break;
            }
            if (btn != null) btn.Enabled = false;

            try
            {
                double[] rawReadings = new double[4];
                for (int i = 0; i < 4; i++)
                {
                    if (!double.TryParse(txtSimW[i].Text, out rawReadings[i]))
                    {
                        MessageBox.Show($"Valor inválido en celda S0{i}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                double sumaNeta = 0;
                double[] netos = new double[4];
                for (int i = 0; i < 4; i++)
                {
                    netos[i] = rawReadings[i] - ceros[i];
                    sumaNeta += netos[i];
                }

                if (sumaNeta <= 0)
                {
                    MessageBox.Show("La suma neta de las lecturas simuladas es cero o negativa.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                factores[esquinaIndex] = pesoCalibracion / sumaNeta;
                esquinasCapturadas++;

                ObtenerTextBox(esquinaIndex).Text = $"F{esquinaIndex + 1}={factores[esquinaIndex]:F4}";

                if (esquinasCapturadas >= 4)
                    AplicarCompensacionEsquinas();
                else
                    lblEstadoCalibracion.Text = $"Esquina {esquinaIndex + 1} simulada (F{esquinaIndex + 1} = {factores[esquinaIndex]:F4}). " +
                        $"Presione botón para la Esquina {esquinasCapturadas + 1}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al capturar esquina simulada: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (btn != null) btn.Enabled = true;
            }
        }

        private void AplicarCompensacionEsquinas()
        {
            usarCompensacionEsquinas = true;
            string fText = string.Join(", ", factores.Select((f, i) => $"F{i + 1}={f:F4}"));

            manager.ConfigurarCompensacionEsquinas(ceros, factores);

            var config = ConfigManager.CargarConfig() ?? new AppConfig();
            config.CerosCompensacion = (double[])ceros.Clone();
            config.FactoresCompensacion = (double[])factores.Clone();
            config.CompensacionEsquinasActiva = true;
            config.CalibracionMultivariableActiva = false;
            ConfigManager.GuardarConfig(config);

            lblEstadoCalibracion.Text = $"Calibración completa. Factores: {fText}";
            MessageBox.Show($"Calibración de esquinas completada.\n\nFactores de corrección:\n{fText}",
                "Calibración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ActualizarSlots();
        }

        private async void BtnConsultarPeso_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen)
            {
                MessageBox.Show("El puerto serial no está abierto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var celdas = manager.Celdas.Values
                .Where(c => c.Connected)
                .OrderBy(c => c.SlaveNumber)
                .Take(4)
                .ToList();

            if (celdas.Count < 1)
            {
                MessageBox.Show("No hay celdas conectadas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var btn = sender as Button;
            if (btn != null) btn.Enabled = false;
            string originalText = btn?.Text ?? "Consultar Peso";
            if (btn != null) btn.Text = "Procesando...";

            try
            {
                if (!cerosCapturados)
                {
                    lblEstadoCalibracion.Text = "Capturando cero en todas las celdas...";
                    for (int i = 0; i < celdas.Count; i++)
                    {
                        await Task.Run(() => manager.ConsultarPeso(celdas[i].SlaveNumber));
                        ceros[i] = celdas[i].RawWeight;
                        ObtenerTextBox(i).Text = $"Z={ceros[i]:F2} kg";
                    }
                    cerosCapturados = true;
                    esquinasCapturadas = 0;
                    usarCompensacionEsquinas = false;
                    lblEstadoCalibracion.Text = $"Cero capturado. Coloque el peso de calibración y presione \"Consultar Peso\" nuevamente.";
                    ActualizarEstadoCalibracionUI();
                    return;
                }

                if (!double.TryParse(txtPesoCalRapido.Text, out double pesoConocido) || pesoConocido <= 0)
                {
                    MessageBox.Show("Ingrese un peso conocido válido (> 0).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                pesoCalibracion = pesoConocido;

                lblEstadoCalibracion.Text = "Leyendo celdas para calibración...";

                double[] rawReadings = new double[4];
                double[] expected = new double[4];

                for (int i = 0; i < celdas.Count; i++)
                {
                    await Task.Run(() => manager.ConsultarPeso(celdas[i].SlaveNumber));
                    rawReadings[i] = celdas[i].RawWeight;
                }

                if (chkSimular.Checked)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (!double.TryParse(txtSimW[i].Text, out expected[i]))
                            expected[i] = 0;
                    }
                }
                else
                {
                    for (int i = 0; i < celdas.Count; i++)
                        expected[i] = pesoCalibracion / celdas.Count;
                }

                int ajustadas = 0;
                for (int i = 0; i < celdas.Count; i++)
                {
                    double neto = rawReadings[i] - ceros[i];
                    if (neto > 0.01 && expected[i] > 0.01)
                    {
                        factores[i] = expected[i] / neto;
                        manager.SetFactorCalibracion(celdas[i].SlaveNumber, factores[i]);
                        celdas[i].CalibratedWeight = celdas[i].RawWeight * factores[i];
                        lblFactores[i].Text = $"S{celdas[i].SlaveNumber:D2} F={factores[i]:F4}";
                        ajustadas++;
                    }
                    else
                    {
                        lblFactores[i].Text = $"S{celdas[i].SlaveNumber:D2} F=---";
                    }
                }

                AplicarCompensacionEsquinas();

                ActualizarMonitoreo();
                ActualizarSlots();

                lblEstadoCalibracion.Text = $"Calibración aplicada a {ajustadas}/{celdas.Count} celdas.";

                MessageBox.Show($"Calibración completada.\nCeldas ajustadas: {ajustadas}/{celdas.Count}\nPeso conocido: {pesoCalibracion} kg",
                    "Consultar Peso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (btn != null) { btn.Enabled = true; btn.Text = originalText; }
            }
        }

        private async void BtnCalRapido_Click(object sender, EventArgs e)
        {
            if (manager == null || !manager.IsOpen)
            {
                MessageBox.Show("El puerto serial no está abierto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!double.TryParse(txtPesoCalRapido.Text, out double pesoConocido) || pesoConocido <= 0)
            {
                MessageBox.Show("Ingrese un peso conocido válido (> 0).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnCalRapido.Enabled = false;
            btnCalRapido.Text = "Calculando...";

            try
            {
                var celdas = manager.Celdas.Values
                    .Where(c => c.Connected)
                    .OrderBy(c => c.SlaveNumber)
                    .Take(4)
                    .ToList();

                if (celdas.Count < 1)
                {
                    MessageBox.Show("No hay celdas conectadas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Leer peso actual de cada celda
                double sumaRaw = 0;
                for (int i = 0; i < celdas.Count; i++)
                {
                    await Task.Run(() => manager.ConsultarPeso(celdas[i].SlaveNumber));
                    sumaRaw += celdas[i].RawWeight;
                }

                if (sumaRaw <= 0)
                {
                    MessageBox.Show("La suma de pesos raw es cero o negativa. Verifique las lecturas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                double factor = pesoConocido / sumaRaw;

                for (int i = 0; i < celdas.Count; i++)
                {
                    manager.SetFactorCalibracion(celdas[i].SlaveNumber, factor);
                    celdas[i].CalibratedWeight = celdas[i].RawWeight * factor;
                    lblFactores[i].Text = $"S{celdas[i].SlaveNumber:D2} F={factor:F4}";
                }

                ActualizarMonitoreo();
                ActualizarSlots();

                MessageBox.Show($"Factor calculado: {factor:F6}\nAplicado a {celdas.Count} celdas.\nPeso conocido: {pesoConocido} kg\nSuma raw: {sumaRaw:F2} kg",
                    "Calibración rápida", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calibrar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCalRapido.Enabled = true;
                btnCalRapido.Text = "Calcular Factor";
            }
        }
    }
}
