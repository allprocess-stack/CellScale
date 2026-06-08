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
        }

        private void InicializarEstadoCalibracion()
        {
            ceros = new double[4];
            factores = new double[4] { 1.0, 1.0, 1.0, 1.0 };
            cerosCapturados = false;
            esquinasCapturadas = 0;
            usarCompensacionEsquinas = false;
            ActualizarEstadoCalibracionUI();
        }

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

        private void InicializarCalibracionGauss()
        {
            calibracionGauss = new CalibracionLineal();
            puntosGauss = new List<PuntoCalibracion>();
            puntoActualGauss = 0;
            btnAplicarGauss.Enabled = false;
            lblCoefGauss.Text = "Coeficientes: (pendiente...)";
            ActualizarPuntosGaussUI();
        }

        private void ActualizarPuntosGaussUI()
        {
            lblPuntosGauss.Text = $"Puntos: {puntoActualGauss}/5";
            btnCapturarPuntoGauss.Text = puntoActualGauss >= 5
                ? "Calibración completa"
                : $"Capturar Punto #{puntoActualGauss + 1}";
            btnCapturarPuntoGauss.Enabled = puntoActualGauss < 5;
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

                    double peso = celda.CalibratedWeight;
                    if (usarCompensacionEsquinas && i < 4)
                    {
                        double neto = celda.CalibratedWeight - ceros[i];
                        peso = neto * factores[i];
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
                double peso = await Task.Run(() => manager.ConsultarPesoMultiLinea(direccion));

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

        // ============================================================
        // Métodos de Calibración de Esquinas (Compensación de Excentricidad)
        // ============================================================

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

                if (celdas.Count < 4)
                {
                    MessageBox.Show($"Se requieren 4 celdas conectadas. Solo hay {celdas.Count}.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                for (int i = 0; i < 4; i++)
                {
                    await Task.Run(() => manager.ConsultarPeso(celdas[i].SlaveNumber));
                    double raw = celdas[i].RawWeight;
                    ceros[i] = raw;
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
                    await Task.Run(() => manager.ConsultarPeso(celdas[i].SlaveNumber));
                    rawReadings[i] = celdas[i].RawWeight;
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
                    usarCompensacionEsquinas = true;
                    string fText = string.Join(", ", factores.Select((f, i) => $"F{i + 1}={f:F4}"));

                    // Aplicar al manager para que impacte en txtBalanza de ViewMain
                    manager.ConfigurarCompensacionEsquinas(ceros, factores);

                    // Guardar en config.json para persistencia
                    var config = ConfigManager.CargarConfig() ?? new AppConfig();
                    config.CerosCompensacion = (double[])ceros.Clone();
                    config.FactoresCompensacion = (double[])factores.Clone();
                    config.CompensacionEsquinasActiva = true;
                    config.CalibracionMultivariableActiva = false;
                    ConfigManager.GuardarConfig(config);

                    lblEstadoCalibracion.Text = $"Calibración completa. Factores: {fText}";
                    MessageBox.Show($"Calibración de esquinas completada.\n\nFactores de corrección:\n{fText}\n\nLa compensación de excentricidad está activa. txtBalanza reflejará el peso corregido.",
                        "Calibración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ActualizarSlots();
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

        private async void btnEsquina1_Click(object sender, EventArgs e) => await CapturarEsquinaAsync(0);
        private async void btnEsquina2_Click(object sender, EventArgs e) => await CapturarEsquinaAsync(1);
        private async void btnEsquina3_Click(object sender, EventArgs e) => await CapturarEsquinaAsync(2);
        private async void btnEsquina4_Click(object sender, EventArgs e) => await CapturarEsquinaAsync(3);

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

            btnCapturarPuntoGauss.Enabled = false;
            lblCoefGauss.Text = $"Capturando punto #{puntoActualGauss + 1}...";

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

                double[] lecturas = new double[4];
                for (int i = 0; i < 4; i++)
                {
                    await Task.Run(() => manager.ConsultarPeso(celdas[i].SlaveNumber));
                    lecturas[i] = celdas[i].RawWeight;
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

                // 1. Guardar en config.json
                var config = ConfigManager.CargarConfig() ?? new AppConfig();
                config.CoeficienteM1 = m1;
                config.CoeficienteM2 = m2;
                config.CoeficienteM3 = m3;
                config.CoeficienteM4 = m4;
                config.BiasB = b;
                config.CalibracionMultivariableActiva = true;
                config.CompensacionEsquinasActiva = false;
                config.FactoresCalibracion?.Clear();
                ConfigManager.GuardarConfig(config);

                // 2. Activar en CeldaManager para que ViewMain lo use en tiempo real
                manager.ConfigurarCalibracionMultivariable(m1, m2, m3, m4, b);

                // 3. Notificar al usuario
                lblCoefGauss.Text = $"APLICADO — m1={m1:F6} m2={m2:F6} m3={m3:F6} m4={m4:F6} B={b:F6}";
                btnAplicarGauss.Enabled = false;
                btnAplicarGauss.Text = "✓ Calibración Activa";

                MessageBox.Show(
                    "Coeficientes guardados en config.json y aplicados al sistema.\n\n" +
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
    }
}
