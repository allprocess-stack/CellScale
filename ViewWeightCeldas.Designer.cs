namespace FormulaGaussExample
{
    partial class ViewWeightCeldas
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Timer timerActualizacion;
        private System.Windows.Forms.TextBox txtViewCelda1;
        private System.Windows.Forms.TextBox txtViewCelda2;
        private System.Windows.Forms.TextBox txtViewCelda3;
        private System.Windows.Forms.TextBox txtViewCelda4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;

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
            this.components = new System.ComponentModel.Container();
            this.timerActualizacion = new System.Windows.Forms.Timer(this.components);
            this.txtViewCelda1 = new System.Windows.Forms.TextBox();
            this.txtViewCelda2 = new System.Windows.Forms.TextBox();
            this.txtViewCelda3 = new System.Windows.Forms.TextBox();
            this.txtViewCelda4 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // timerActualizacion
            // 
            this.timerActualizacion.Interval = 250;
            this.timerActualizacion.Tick += new System.EventHandler(this.TimerActualizacion_Tick);
            // 
            // txtViewCelda1
            // 
            this.txtViewCelda1.BackColor = System.Drawing.Color.White;
            this.txtViewCelda1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.txtViewCelda1.Location = new System.Drawing.Point(20, 50);
            this.txtViewCelda1.Name = "txtViewCelda1";
            this.txtViewCelda1.ReadOnly = true;
            this.txtViewCelda1.Size = new System.Drawing.Size(110, 29);
            this.txtViewCelda1.TabIndex = 4;
            this.txtViewCelda1.Text = "---";
            this.txtViewCelda1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtViewCelda2
            // 
            this.txtViewCelda2.BackColor = System.Drawing.Color.White;
            this.txtViewCelda2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.txtViewCelda2.Location = new System.Drawing.Point(150, 50);
            this.txtViewCelda2.Name = "txtViewCelda2";
            this.txtViewCelda2.ReadOnly = true;
            this.txtViewCelda2.Size = new System.Drawing.Size(110, 29);
            this.txtViewCelda2.TabIndex = 5;
            this.txtViewCelda2.Text = "---";
            this.txtViewCelda2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtViewCelda3
            // 
            this.txtViewCelda3.BackColor = System.Drawing.Color.White;
            this.txtViewCelda3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.txtViewCelda3.Location = new System.Drawing.Point(280, 50);
            this.txtViewCelda3.Name = "txtViewCelda3";
            this.txtViewCelda3.ReadOnly = true;
            this.txtViewCelda3.Size = new System.Drawing.Size(110, 29);
            this.txtViewCelda3.TabIndex = 6;
            this.txtViewCelda3.Text = "---";
            this.txtViewCelda3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtViewCelda4
            // 
            this.txtViewCelda4.BackColor = System.Drawing.Color.White;
            this.txtViewCelda4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.txtViewCelda4.Location = new System.Drawing.Point(410, 50);
            this.txtViewCelda4.Name = "txtViewCelda4";
            this.txtViewCelda4.ReadOnly = true;
            this.txtViewCelda4.Size = new System.Drawing.Size(110, 29);
            this.txtViewCelda4.TabIndex = 7;
            this.txtViewCelda4.Text = "---";
            this.txtViewCelda4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //this.txtViewCelda4.TextChanged += new System.EventHandler(this.txtViewCelda4_TextChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Celda S00";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(150, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Celda S01";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(280, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Celda S02";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(410, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 20);
            this.label4.TabIndex = 3;
            this.label4.Text = "Celda S03";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ViewWeightCeldas
            // 
            this.ClientSize = new System.Drawing.Size(620, 120);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtViewCelda1);
            this.Controls.Add(this.txtViewCelda2);
            this.Controls.Add(this.txtViewCelda3);
            this.Controls.Add(this.txtViewCelda4);
            this.Name = "ViewWeightCeldas";
            this.Text = "ViewWeightCeldas";
            this.Load += new System.EventHandler(this.ViewWeightCeldas_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
