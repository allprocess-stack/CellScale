namespace FormulaGaussExample
{
    partial class ViewCeldas
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtCelda1 = new System.Windows.Forms.TextBox();
            this.txtCelda2 = new System.Windows.Forms.TextBox();
            this.txtCelda3 = new System.Windows.Forms.TextBox();
            this.txtCelda4 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnCelda1 = new System.Windows.Forms.Button();
            this.btnCelda2 = new System.Windows.Forms.Button();
            this.btnCelda3 = new System.Windows.Forms.Button();
            this.btnCelda4 = new System.Windows.Forms.Button();
            this.btnPesos = new System.Windows.Forms.Button();
            this.txtConsultCelda1 = new System.Windows.Forms.TextBox();
            this.txtConsultCelda2 = new System.Windows.Forms.TextBox();
            this.txtConsultCelda3 = new System.Windows.Forms.TextBox();
            this.txtConsultCelda4 = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.lblTituloCalibracion = new System.Windows.Forms.Label();
            this.lblPesoCalibracion = new System.Windows.Forms.Label();
            this.txtPesoCalibracion = new System.Windows.Forms.TextBox();
            this.btnCeroCalibracion = new System.Windows.Forms.Button();
            this.btnEsquina1 = new System.Windows.Forms.Button();
            this.btnEsquina2 = new System.Windows.Forms.Button();
            this.btnEsquina3 = new System.Windows.Forms.Button();
            this.btnEsquina4 = new System.Windows.Forms.Button();
            this.btnAplicarGauss = new System.Windows.Forms.Button();
            this.btnCapturarPuntoGauss = new System.Windows.Forms.Button();
            this.lblCoefGauss = new System.Windows.Forms.Label();
            this.lblTituloGauss = new System.Windows.Forms.Label();
            this.lblPuntosGauss = new System.Windows.Forms.Label();
            this.lblEstadoCalibracion = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtCelda1
            // 
            this.txtCelda1.Location = new System.Drawing.Point(26, 124);
            this.txtCelda1.Name = "txtCelda1";
            this.txtCelda1.Size = new System.Drawing.Size(100, 20);
            this.txtCelda1.TabIndex = 2;
            // 
            // txtCelda2
            // 
            this.txtCelda2.Location = new System.Drawing.Point(140, 123);
            this.txtCelda2.Name = "txtCelda2";
            this.txtCelda2.Size = new System.Drawing.Size(100, 20);
            this.txtCelda2.TabIndex = 3;
            // 
            // txtCelda3
            // 
            this.txtCelda3.Location = new System.Drawing.Point(250, 123);
            this.txtCelda3.Name = "txtCelda3";
            this.txtCelda3.Size = new System.Drawing.Size(100, 20);
            this.txtCelda3.TabIndex = 4;
            // 
            // txtCelda4
            // 
            this.txtCelda4.Location = new System.Drawing.Point(363, 123);
            this.txtCelda4.Name = "txtCelda4";
            this.txtCelda4.Size = new System.Drawing.Size(100, 20);
            this.txtCelda4.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(41, 56);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Celda1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(132, 56);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Celda2";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(211, 56);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Celda3";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(297, 56);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Celda4";
            // 
            // btnCelda1
            // 
            this.btnCelda1.Location = new System.Drawing.Point(35, 167);
            this.btnCelda1.Name = "btnCelda1";
            this.btnCelda1.Size = new System.Drawing.Size(75, 38);
            this.btnCelda1.TabIndex = 10;
            this.btnCelda1.Text = "Consultar Peso";
            this.btnCelda1.UseVisualStyleBackColor = true;
            this.btnCelda1.Click += new System.EventHandler(this.btnCelda1_Click);
            // 
            // btnCelda2
            // 
            this.btnCelda2.Location = new System.Drawing.Point(153, 167);
            this.btnCelda2.Name = "btnCelda2";
            this.btnCelda2.Size = new System.Drawing.Size(75, 38);
            this.btnCelda2.TabIndex = 11;
            this.btnCelda2.Text = "Consultar Peso";
            this.btnCelda2.UseVisualStyleBackColor = true;
            this.btnCelda2.Click += new System.EventHandler(this.btnCelda2_Click);
            // 
            // btnCelda3
            // 
            this.btnCelda3.Location = new System.Drawing.Point(261, 167);
            this.btnCelda3.Name = "btnCelda3";
            this.btnCelda3.Size = new System.Drawing.Size(75, 38);
            this.btnCelda3.TabIndex = 12;
            this.btnCelda3.Text = "Consultar Peso";
            this.btnCelda3.UseVisualStyleBackColor = true;
            this.btnCelda3.Click += new System.EventHandler(this.btnCelda3_Click);
            // 
            // btnCelda4
            // 
            this.btnCelda4.Location = new System.Drawing.Point(373, 167);
            this.btnCelda4.Name = "btnCelda4";
            this.btnCelda4.Size = new System.Drawing.Size(75, 38);
            this.btnCelda4.TabIndex = 13;
            this.btnCelda4.Text = "Consultar Peso";
            this.btnCelda4.UseVisualStyleBackColor = true;
            this.btnCelda4.Click += new System.EventHandler(this.btnCelda4_Click);
            // 
            // btnPesos
            // 
            this.btnPesos.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnPesos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPesos.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnPesos.Location = new System.Drawing.Point(199, 220);
            this.btnPesos.Name = "btnPesos";
            this.btnPesos.Size = new System.Drawing.Size(75, 39);
            this.btnPesos.TabIndex = 14;
            this.btnPesos.Text = "Consultar Pesos";
            this.btnPesos.UseVisualStyleBackColor = false;
            this.btnPesos.Click += new System.EventHandler(this.btnPesos_Click);
            // 
            // txtConsultCelda1
            // 
            this.txtConsultCelda1.Location = new System.Drawing.Point(26, 76);
            this.txtConsultCelda1.Margin = new System.Windows.Forms.Padding(2);
            this.txtConsultCelda1.Name = "txtConsultCelda1";
            this.txtConsultCelda1.Size = new System.Drawing.Size(58, 20);
            this.txtConsultCelda1.TabIndex = 15;
            // 
            // txtConsultCelda2
            // 
            this.txtConsultCelda2.Location = new System.Drawing.Point(115, 76);
            this.txtConsultCelda2.Margin = new System.Windows.Forms.Padding(2);
            this.txtConsultCelda2.Name = "txtConsultCelda2";
            this.txtConsultCelda2.Size = new System.Drawing.Size(58, 20);
            this.txtConsultCelda2.TabIndex = 16;
            // 
            // txtConsultCelda3
            // 
            this.txtConsultCelda3.Location = new System.Drawing.Point(196, 76);
            this.txtConsultCelda3.Margin = new System.Windows.Forms.Padding(2);
            this.txtConsultCelda3.Name = "txtConsultCelda3";
            this.txtConsultCelda3.Size = new System.Drawing.Size(58, 20);
            this.txtConsultCelda3.TabIndex = 17;
            // 
            // txtConsultCelda4
            // 
            this.txtConsultCelda4.Location = new System.Drawing.Point(280, 76);
            this.txtConsultCelda4.Margin = new System.Windows.Forms.Padding(2);
            this.txtConsultCelda4.Name = "txtConsultCelda4";
            this.txtConsultCelda4.Size = new System.Drawing.Size(58, 20);
            this.txtConsultCelda4.TabIndex = 18;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // lblTituloCalibracion
            // 
            this.lblTituloCalibracion.AutoSize = true;
            this.lblTituloCalibracion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTituloCalibracion.Location = new System.Drawing.Point(11, 20);
            this.lblTituloCalibracion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTituloCalibracion.Name = "lblTituloCalibracion";
            this.lblTituloCalibracion.Size = new System.Drawing.Size(264, 15);
            this.lblTituloCalibracion.TabIndex = 19;
            this.lblTituloCalibracion.Text = "Calibración de Esquinas (Excentricidad)";
            // 
            // lblPesoCalibracion
            // 
            this.lblPesoCalibracion.AutoSize = true;
            this.lblPesoCalibracion.Location = new System.Drawing.Point(16, 44);
            this.lblPesoCalibracion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPesoCalibracion.Name = "lblPesoCalibracion";
            this.lblPesoCalibracion.Size = new System.Drawing.Size(110, 13);
            this.lblPesoCalibracion.TabIndex = 20;
            this.lblPesoCalibracion.Text = "Peso Calibración (kg):";
            // 
            // txtPesoCalibracion
            // 
            this.txtPesoCalibracion.Location = new System.Drawing.Point(123, 41);
            this.txtPesoCalibracion.Margin = new System.Windows.Forms.Padding(2);
            this.txtPesoCalibracion.Name = "txtPesoCalibracion";
            this.txtPesoCalibracion.Size = new System.Drawing.Size(47, 20);
            this.txtPesoCalibracion.TabIndex = 21;
            this.txtPesoCalibracion.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPesoCalibracion.TextChanged += new System.EventHandler(this.txtPesoCalibracion_TextChanged);
            // 
            // btnCeroCalibracion
            // 
            this.btnCeroCalibracion.Location = new System.Drawing.Point(241, 41);
            this.btnCeroCalibracion.Margin = new System.Windows.Forms.Padding(2);
            this.btnCeroCalibracion.Name = "btnCeroCalibracion";
            this.btnCeroCalibracion.Size = new System.Drawing.Size(79, 19);
            this.btnCeroCalibracion.TabIndex = 22;
            this.btnCeroCalibracion.Text = "1. Capturar Cero";
            this.btnCeroCalibracion.UseVisualStyleBackColor = true;
            this.btnCeroCalibracion.Click += new System.EventHandler(this.btnCeroCalibracion_Click);
            // 
            // btnEsquina1
            // 
            this.btnEsquina1.Location = new System.Drawing.Point(16, 67);
            this.btnEsquina1.Margin = new System.Windows.Forms.Padding(2);
            this.btnEsquina1.Name = "btnEsquina1";
            this.btnEsquina1.Size = new System.Drawing.Size(68, 20);
            this.btnEsquina1.TabIndex = 23;
            this.btnEsquina1.Text = "2. Esquina 1";
            this.btnEsquina1.UseVisualStyleBackColor = true;
            this.btnEsquina1.Click += new System.EventHandler(this.btnEsquina1_Click);
            // 
            // btnEsquina2
            // 
            this.btnEsquina2.Location = new System.Drawing.Point(94, 67);
            this.btnEsquina2.Margin = new System.Windows.Forms.Padding(2);
            this.btnEsquina2.Name = "btnEsquina2";
            this.btnEsquina2.Size = new System.Drawing.Size(68, 20);
            this.btnEsquina2.TabIndex = 24;
            this.btnEsquina2.Text = "3. Esquina 2";
            this.btnEsquina2.UseVisualStyleBackColor = true;
            this.btnEsquina2.Click += new System.EventHandler(this.btnEsquina2_Click);
            // 
            // btnEsquina3
            // 
            this.btnEsquina3.Location = new System.Drawing.Point(173, 67);
            this.btnEsquina3.Margin = new System.Windows.Forms.Padding(2);
            this.btnEsquina3.Name = "btnEsquina3";
            this.btnEsquina3.Size = new System.Drawing.Size(68, 20);
            this.btnEsquina3.TabIndex = 25;
            this.btnEsquina3.Text = "4. Esquina 3";
            this.btnEsquina3.UseVisualStyleBackColor = true;
            this.btnEsquina3.Click += new System.EventHandler(this.btnEsquina3_Click);
            // 
            // btnEsquina4
            // 
            this.btnEsquina4.Location = new System.Drawing.Point(252, 67);
            this.btnEsquina4.Margin = new System.Windows.Forms.Padding(2);
            this.btnEsquina4.Name = "btnEsquina4";
            this.btnEsquina4.Size = new System.Drawing.Size(68, 20);
            this.btnEsquina4.TabIndex = 26;
            this.btnEsquina4.Text = "5. Esquina 4";
            this.btnEsquina4.UseVisualStyleBackColor = true;
            this.btnEsquina4.Click += new System.EventHandler(this.btnEsquina4_Click);
            // 
            // btnAplicarGauss
            // 
            this.btnAplicarGauss.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnAplicarGauss.Enabled = false;
            this.btnAplicarGauss.Location = new System.Drawing.Point(144, 156);
            this.btnAplicarGauss.Margin = new System.Windows.Forms.Padding(2);
            this.btnAplicarGauss.Name = "btnAplicarGauss";
            this.btnAplicarGauss.Size = new System.Drawing.Size(112, 20);
            this.btnAplicarGauss.TabIndex = 32;
            this.btnAplicarGauss.Text = "Aplicar Calibración Gauss";
            this.btnAplicarGauss.UseVisualStyleBackColor = false;
            this.btnAplicarGauss.Click += new System.EventHandler(this.btnAplicarGauss_Click);
            // 
            // btnCapturarPuntoGauss
            // 
            this.btnCapturarPuntoGauss.Location = new System.Drawing.Point(17, 156);
            this.btnCapturarPuntoGauss.Margin = new System.Windows.Forms.Padding(2);
            this.btnCapturarPuntoGauss.Name = "btnCapturarPuntoGauss";
            this.btnCapturarPuntoGauss.Size = new System.Drawing.Size(101, 20);
            this.btnCapturarPuntoGauss.TabIndex = 30;
            this.btnCapturarPuntoGauss.Text = "Capturar Punto #1";
            this.btnCapturarPuntoGauss.Click += new System.EventHandler(this.btnCapturarPuntoGauss_Click);
            // 
            // lblCoefGauss
            // 
            this.lblCoefGauss.AutoSize = true;
            this.lblCoefGauss.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCoefGauss.Location = new System.Drawing.Point(16, 195);
            this.lblCoefGauss.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCoefGauss.Name = "lblCoefGauss";
            this.lblCoefGauss.Size = new System.Drawing.Size(175, 13);
            this.lblCoefGauss.TabIndex = 31;
            this.lblCoefGauss.Text = "Coeficientes: (pendiente...)";
            // 
            // lblTituloGauss
            // 
            this.lblTituloGauss.AutoSize = true;
            this.lblTituloGauss.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTituloGauss.Location = new System.Drawing.Point(11, 123);
            this.lblTituloGauss.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTituloGauss.Name = "lblTituloGauss";
            this.lblTituloGauss.Size = new System.Drawing.Size(292, 15);
            this.lblTituloGauss.TabIndex = 28;
            this.lblTituloGauss.Text = "Calibración Multivariable (Gauss — 5 puntos)";
            // 
            // lblPuntosGauss
            // 
            this.lblPuntosGauss.AutoSize = true;
            this.lblPuntosGauss.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPuntosGauss.Location = new System.Drawing.Point(16, 141);
            this.lblPuntosGauss.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPuntosGauss.Name = "lblPuntosGauss";
            this.lblPuntosGauss.Size = new System.Drawing.Size(74, 13);
            this.lblPuntosGauss.TabIndex = 29;
            this.lblPuntosGauss.Text = "Puntos: 0/5";
            // 
            // lblEstadoCalibracion
            // 
            this.lblEstadoCalibracion.AutoSize = true;
            this.lblEstadoCalibracion.Location = new System.Drawing.Point(16, 95);
            this.lblEstadoCalibracion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEstadoCalibracion.Name = "lblEstadoCalibracion";
            this.lblEstadoCalibracion.Size = new System.Drawing.Size(406, 13);
            this.lblEstadoCalibracion.TabIndex = 27;
            this.lblEstadoCalibracion.Text = "Estado: Configure el peso de calibración y presione \"Capturar Cero\" (balanza vací" +
    "a).";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblTituloCalibracion);
            this.groupBox1.Controls.Add(this.lblPesoCalibracion);
            this.groupBox1.Controls.Add(this.btnAplicarGauss);
            this.groupBox1.Controls.Add(this.txtPesoCalibracion);
            this.groupBox1.Controls.Add(this.lblCoefGauss);
            this.groupBox1.Controls.Add(this.btnCeroCalibracion);
            this.groupBox1.Controls.Add(this.btnCapturarPuntoGauss);
            this.groupBox1.Controls.Add(this.btnEsquina1);
            this.groupBox1.Controls.Add(this.lblPuntosGauss);
            this.groupBox1.Controls.Add(this.btnEsquina2);
            this.groupBox1.Controls.Add(this.lblTituloGauss);
            this.groupBox1.Controls.Add(this.btnEsquina3);
            this.groupBox1.Controls.Add(this.lblEstadoCalibracion);
            this.groupBox1.Controls.Add(this.btnEsquina4);
            this.groupBox1.Location = new System.Drawing.Point(9, 25);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(457, 240);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Calibraciones";
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(9, 273);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(457, 290);
            this.groupBox2.TabIndex = 34;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Monitoreo de Celdas";
            // 
            // ViewCeldas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 610);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtConsultCelda4);
            this.Controls.Add(this.txtConsultCelda3);
            this.Controls.Add(this.txtConsultCelda2);
            this.Controls.Add(this.txtConsultCelda1);
            this.Controls.Add(this.btnPesos);
            this.Controls.Add(this.btnCelda4);
            this.Controls.Add(this.btnCelda3);
            this.Controls.Add(this.btnCelda2);
            this.Controls.Add(this.btnCelda1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtCelda4);
            this.Controls.Add(this.txtCelda3);
            this.Controls.Add(this.txtCelda2);
            this.Controls.Add(this.txtCelda1);
            this.Name = "ViewCeldas";
            this.Text = "VISTA CELDAS";
            this.Load += new System.EventHandler(this.ViewCeldas_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCelda1;
        private System.Windows.Forms.TextBox txtCelda2;
        private System.Windows.Forms.TextBox txtCelda3;
        private System.Windows.Forms.TextBox txtCelda4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnCelda1;
        private System.Windows.Forms.Button btnCelda2;
        private System.Windows.Forms.Button btnCelda3;
        private System.Windows.Forms.Button btnCelda4;
        private System.Windows.Forms.Button btnPesos;
        private System.Windows.Forms.TextBox txtConsultCelda1;
        private System.Windows.Forms.TextBox txtConsultCelda2;
        private System.Windows.Forms.TextBox txtConsultCelda3;
        private System.Windows.Forms.TextBox txtConsultCelda4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Label lblTituloCalibracion;
        private System.Windows.Forms.Label lblPesoCalibracion;
        private System.Windows.Forms.TextBox txtPesoCalibracion;
        private System.Windows.Forms.Button btnCeroCalibracion;
        private System.Windows.Forms.Button btnEsquina1;
        private System.Windows.Forms.Button btnEsquina2;
        private System.Windows.Forms.Button btnEsquina3;
        private System.Windows.Forms.Button btnEsquina4;
        private System.Windows.Forms.Label lblEstadoCalibracion;
        private System.Windows.Forms.Label lblTituloGauss;
        private System.Windows.Forms.Label lblPuntosGauss;
        private System.Windows.Forms.Button btnCapturarPuntoGauss;
        private System.Windows.Forms.Label lblCoefGauss;
        private System.Windows.Forms.Button btnAplicarGauss;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}
