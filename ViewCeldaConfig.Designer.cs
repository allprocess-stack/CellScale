namespace FormulaGaussExample
{
    partial class ViewCeldaConfig
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

        private void InitializeComponent()
        {
            this.txtCelda1 = new System.Windows.Forms.TextBox();
            this.txtCelda2 = new System.Windows.Forms.TextBox();
            this.txtCelda3 = new System.Windows.Forms.TextBox();
            this.txtCelda4 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.txtCelda1.Location = new System.Drawing.Point(100, 30);
            this.txtCelda1.Name = "txtCelda1";
            this.txtCelda1.Size = new System.Drawing.Size(120, 22);
            this.txtCelda1.TabIndex = 0;

            this.txtCelda2.Location = new System.Drawing.Point(100, 70);
            this.txtCelda2.Name = "txtCelda2";
            this.txtCelda2.Size = new System.Drawing.Size(120, 22);
            this.txtCelda2.TabIndex = 1;

            this.txtCelda3.Location = new System.Drawing.Point(100, 110);
            this.txtCelda3.Name = "txtCelda3";
            this.txtCelda3.Size = new System.Drawing.Size(120, 22);
            this.txtCelda3.TabIndex = 2;

            this.txtCelda4.Location = new System.Drawing.Point(100, 150);
            this.txtCelda4.Name = "txtCelda4";
            this.txtCelda4.Size = new System.Drawing.Size(120, 22);
            this.txtCelda4.TabIndex = 3;

            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Celda 1";

            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "Celda 2";

            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Celda 3";

            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 153);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 16);
            this.label4.TabIndex = 7;
            this.label4.Text = "Celda 4";

            this.btnGuardar.Location = new System.Drawing.Point(100, 200);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(120, 40);
            this.btnGuardar.TabIndex = 8;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = true;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 280);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCelda4);
            this.Controls.Add(this.txtCelda3);
            this.Controls.Add(this.txtCelda2);
            this.Controls.Add(this.txtCelda1);
            this.Name = "ViewCeldaConfig";
            this.Text = "Configuración de Celdas";
            this.Load += new System.EventHandler(this.ViewCeldaConfig_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtCelda1;
        private System.Windows.Forms.TextBox txtCelda2;
        private System.Windows.Forms.TextBox txtCelda3;
        private System.Windows.Forms.TextBox txtCelda4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnGuardar;
    }
}
