namespace FormulaGaussExample
{
    partial class ViewMain
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewMain));
            this.btnRegistrar = new System.Windows.Forms.Button();
            this.lblBalanza = new System.Windows.Forms.Label();
            this.txtBalanza = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsddbtnLogin = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiUsuario = new System.Windows.Forms.ToolStripMenuItem();
            this.txtUsuario = new System.Windows.Forms.ToolStripTextBox();
            this.tsmiContraseña = new System.Windows.Forms.ToolStripMenuItem();
            this.txtContrasena = new System.Windows.Forms.ToolStripTextBox();
            this.tsmiIngresar = new System.Windows.Forms.ToolStripMenuItem();
            this.tsddbMenu = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmiBalanza = new System.Windows.Forms.ToolStripMenuItem();
            this.tscbBalanza = new System.Windows.Forms.ToolStripComboBox();
            this.tsmiAbrirBalanza = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCerrarBalanza = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tstbPesoCalibrar = new System.Windows.Forms.ToolStripTextBox();
            this.tsmiGuardarMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbBd = new System.Windows.Forms.ToolStripButton();
            this.tsbCeldasConfig = new System.Windows.Forms.ToolStripButton();
            this.tsbPesoCeldas = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslblStatusConexion = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslblTiempoConexion = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslblTrama = new System.Windows.Forms.ToolStripStatusLabel();
            this.timerTiempoConexion = new System.Windows.Forms.Timer(this.components);
            this.timerDataTrama = new System.Windows.Forms.Timer(this.components);
            this.lstCeldas = new System.Windows.Forms.ListBox();
            this.TimerPesaje = new System.Windows.Forms.Timer(this.components);
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRegistrar
            // 
            this.btnRegistrar.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRegistrar.Location = new System.Drawing.Point(271, 297);
            this.btnRegistrar.Name = "btnRegistrar";
            this.btnRegistrar.Size = new System.Drawing.Size(228, 40);
            this.btnRegistrar.TabIndex = 0;
            this.btnRegistrar.Text = "REGISTRAR PESO";
            this.btnRegistrar.UseVisualStyleBackColor = true;
            this.btnRegistrar.Click += new System.EventHandler(this.btnRegistrar_Click);
            // 
            // lblBalanza
            // 
            this.lblBalanza.AutoSize = true;
            this.lblBalanza.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBalanza.Location = new System.Drawing.Point(10, 84);
            this.lblBalanza.Name = "lblBalanza";
            this.lblBalanza.Size = new System.Drawing.Size(642, 73);
            this.lblBalanza.TabIndex = 1;
            this.lblBalanza.Text = "PESO DE BALANZA";
            this.lblBalanza.Click += new System.EventHandler(this.label1_Click);
            // 
            // txtBalanza
            // 
            this.txtBalanza.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBalanza.Location = new System.Drawing.Point(36, 178);
            this.txtBalanza.Name = "txtBalanza";
            this.txtBalanza.Size = new System.Drawing.Size(565, 80);
            this.txtBalanza.TabIndex = 3;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsddbtnLogin,
            this.tsddbMenu,
            this.tsbBd,
            this.tsbCeldasConfig,
            this.tsbPesoCeldas});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(645, 25);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            this.toolStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip1_ItemClicked);
            // 
            // tsddbtnLogin
            // 
            this.tsddbtnLogin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsddbtnLogin.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiUsuario,
            this.tsmiContraseña,
            this.tsmiIngresar});
            this.tsddbtnLogin.Image = ((System.Drawing.Image)(resources.GetObject("tsddbtnLogin.Image")));
            this.tsddbtnLogin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbtnLogin.Name = "tsddbtnLogin";
            this.tsddbtnLogin.Size = new System.Drawing.Size(55, 22);
            this.tsddbtnLogin.Text = "LOGIN";
            // 
            // tsmiUsuario
            // 
            this.tsmiUsuario.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtUsuario});
            this.tsmiUsuario.Name = "tsmiUsuario";
            this.tsmiUsuario.Size = new System.Drawing.Size(151, 22);
            this.tsmiUsuario.Text = "USUARIO";
            // 
            // txtUsuario
            // 
            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtUsuario.Name = "txtUsuario";
            this.txtUsuario.Size = new System.Drawing.Size(100, 23);
            // 
            // tsmiContraseña
            // 
            this.tsmiContraseña.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtContrasena});
            this.tsmiContraseña.Name = "tsmiContraseña";
            this.tsmiContraseña.Size = new System.Drawing.Size(151, 22);
            this.tsmiContraseña.Text = "CONTRASEÑA";
            // 
            // txtContrasena
            // 
            this.txtContrasena.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtContrasena.Name = "txtContrasena";
            this.txtContrasena.Size = new System.Drawing.Size(121, 23);
            // 
            // tsmiIngresar
            // 
            this.tsmiIngresar.Name = "tsmiIngresar";
            this.tsmiIngresar.Size = new System.Drawing.Size(151, 22);
            this.tsmiIngresar.Text = "INGRESAR";
            this.tsmiIngresar.Click += new System.EventHandler(this.tsmiIngresar_Click);
            // 
            // tsddbMenu
            // 
            this.tsddbMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsddbMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiBalanza,
            this.toolStripTextBox1,
            this.tsmiGuardarMenu});
            this.tsddbMenu.Image = ((System.Drawing.Image)(resources.GetObject("tsddbMenu.Image")));
            this.tsddbMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsddbMenu.Name = "tsddbMenu";
            this.tsddbMenu.Size = new System.Drawing.Size(54, 22);
            this.tsddbMenu.Text = "MENÚ";
            this.tsddbMenu.Click += new System.EventHandler(this.toolStripDropDownButton1_Click);
            // 
            // tsmiBalanza
            // 
            this.tsmiBalanza.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tscbBalanza,
            this.tsmiAbrirBalanza,
            this.tsmiCerrarBalanza});
            this.tsmiBalanza.Name = "tsmiBalanza";
            this.tsmiBalanza.Size = new System.Drawing.Size(193, 22);
            this.tsmiBalanza.Text = "Conexión balanza";
            // 
            // tscbBalanza
            // 
            this.tscbBalanza.Name = "tscbBalanza";
            this.tscbBalanza.Size = new System.Drawing.Size(121, 23);
            // 
            // tsmiAbrirBalanza
            // 
            this.tsmiAbrirBalanza.Name = "tsmiAbrirBalanza";
            this.tsmiAbrirBalanza.Size = new System.Drawing.Size(181, 22);
            this.tsmiAbrirBalanza.Text = "Abrir";
            this.tsmiAbrirBalanza.Click += new System.EventHandler(this.tsmiAbrirBalanza_Click);
            // 
            // tsmiCerrarBalanza
            // 
            this.tsmiCerrarBalanza.Name = "tsmiCerrarBalanza";
            this.tsmiCerrarBalanza.Size = new System.Drawing.Size(181, 22);
            this.tsmiCerrarBalanza.Text = "Cerrar";
            this.tsmiCerrarBalanza.Click += new System.EventHandler(this.tsmiCerrarBalanza_Click);
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tstbPesoCalibrar});
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(193, 22);
            this.toolStripTextBox1.Text = "Peso Calibración";
            // 
            // tstbPesoCalibrar
            // 
            this.tstbPesoCalibrar.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tstbPesoCalibrar.Name = "tstbPesoCalibrar";
            this.tstbPesoCalibrar.Size = new System.Drawing.Size(100, 23);
            // 
            // tsmiGuardarMenu
            // 
            this.tsmiGuardarMenu.Name = "tsmiGuardarMenu";
            this.tsmiGuardarMenu.Size = new System.Drawing.Size(193, 22);
            this.tsmiGuardarMenu.Text = "Guardar configuración";
            this.tsmiGuardarMenu.Click += new System.EventHandler(this.tsmiGuardarMenu_Click);
            // 
            // tsbBd
            // 
            this.tsbBd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbBd.Image = ((System.Drawing.Image)(resources.GetObject("tsbBd.Image")));
            this.tsbBd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbBd.Name = "tsbBd";
            this.tsbBd.Size = new System.Drawing.Size(26, 22);
            this.tsbBd.Text = "BD";
            this.tsbBd.Click += new System.EventHandler(this.tsbBd_Click);
            // 
            // tsbCeldasConfig
            // 
            this.tsbCeldasConfig.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbCeldasConfig.Image = ((System.Drawing.Image)(resources.GetObject("tsbCeldasConfig.Image")));
            this.tsbCeldasConfig.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCeldasConfig.Name = "tsbCeldasConfig";
            this.tsbCeldasConfig.Size = new System.Drawing.Size(99, 22);
            this.tsbCeldasConfig.Text = "CELDAS CONFIG";
            this.tsbCeldasConfig.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // tsbPesoCeldas
            // 
            this.tsbPesoCeldas.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbPesoCeldas.Image = ((System.Drawing.Image)(resources.GetObject("tsbPesoCeldas.Image")));
            this.tsbPesoCeldas.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPesoCeldas.Name = "tsbPesoCeldas";
            this.tsbPesoCeldas.Size = new System.Drawing.Size(84, 22);
            this.tsbPesoCeldas.Text = "PESO CELDAS";
            this.tsbPesoCeldas.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tsslblStatusConexion,
            this.toolStripStatusLabel3,
            this.tsslblTiempoConexion,
            this.toolStripStatusLabel2,
            this.tsslblTrama});
            this.statusStrip1.Location = new System.Drawing.Point(0, 558);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(645, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(125, 17);
            this.toolStripStatusLabel1.Text = "CONEXIÓN BALANZA";
            // 
            // tsslblStatusConexion
            // 
            this.tsslblStatusConexion.Name = "tsslblStatusConexion";
            this.tsslblStatusConexion.Size = new System.Drawing.Size(40, 17);
            this.tsslblStatusConexion.Text = "NONE";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(124, 17);
            this.toolStripStatusLabel3.Text = "TIEMPO CONECTADO";
            // 
            // tsslblTiempoConexion
            // 
            this.tsslblTiempoConexion.Name = "tsslblTiempoConexion";
            this.tsslblTiempoConexion.Size = new System.Drawing.Size(19, 17);
            this.tsslblTiempoConexion.Text = "0S";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(37, 17);
            this.toolStripStatusLabel2.Text = "DATA";
            // 
            // tsslblTrama
            // 
            this.tsslblTrama.Name = "tsslblTrama";
            this.tsslblTrama.Size = new System.Drawing.Size(40, 17);
            this.tsslblTrama.Text = "NONE";
            // 
            // timerTiempoConexion
            // 
            this.timerTiempoConexion.Tick += new System.EventHandler(this.tsslblTiempoConexion_Tick);
            // 
            // timerDataTrama
            // 
            this.timerDataTrama.Tick += new System.EventHandler(this.timerDataTrama_Tick);
            // 
            // lstCeldas
            // 
            this.lstCeldas.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstCeldas.FormattingEnabled = true;
            this.lstCeldas.ItemHeight = 16;
            this.lstCeldas.Location = new System.Drawing.Point(11, 370);
            this.lstCeldas.Margin = new System.Windows.Forms.Padding(2);
            this.lstCeldas.Name = "lstCeldas";
            this.lstCeldas.Size = new System.Drawing.Size(622, 132);
            this.lstCeldas.TabIndex = 9;
            // 
            // TimerPesaje
            // 
            this.TimerPesaje.Tick += new System.EventHandler(this.TimerPesaje_Tick);
            // 
            // ViewMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 580);
            this.Controls.Add(this.lstCeldas);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.txtBalanza);
            this.Controls.Add(this.lblBalanza);
            this.Controls.Add(this.btnRegistrar);
            this.Name = "ViewMain";
            this.Text = "PRINCIPAL";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRegistrar;
        private System.Windows.Forms.Label lblBalanza;
        private System.Windows.Forms.TextBox txtBalanza;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton tsddbMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiBalanza;
        private System.Windows.Forms.ToolStripMenuItem tsmiGuardarMenu;
        private System.Windows.Forms.ToolStripComboBox tscbBalanza;
        private System.Windows.Forms.ToolStripMenuItem tsmiAbrirBalanza;
        private System.Windows.Forms.ToolStripMenuItem tsmiCerrarBalanza;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tsslblStatusConexion;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel tsslblTiempoConexion;
        private System.Windows.Forms.Timer timerTiempoConexion;
        private System.Windows.Forms.Timer timerDataTrama;
        private System.Windows.Forms.ToolStripDropDownButton tsddbtnLogin;
        private System.Windows.Forms.ToolStripMenuItem tsmiUsuario;
        private System.Windows.Forms.ToolStripMenuItem tsmiContraseña;
        private System.Windows.Forms.ToolStripMenuItem tsmiIngresar;
        //private System.Windows.Forms.ToolStripTextBox txtUsuario;
        //private System.Windows.Forms.ToolStripTextBox txtContraseña;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel tsslblTrama;
        private System.Windows.Forms.ToolStripTextBox txtUsuario;
        private System.Windows.Forms.ToolStripTextBox txtContrasena;
        private System.Windows.Forms.ListBox lstCeldas;
        private System.Windows.Forms.Timer TimerPesaje;
        private System.Windows.Forms.ToolStripButton tsbBd;
        private System.Windows.Forms.ToolStripButton tsbCeldasConfig;
        private System.Windows.Forms.ToolStripButton tsbPesoCeldas;
        private System.Windows.Forms.ToolStripMenuItem toolStripTextBox1;
        private System.Windows.Forms.ToolStripTextBox tstbPesoCalibrar;
    }
}

