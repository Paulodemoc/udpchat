namespace TakeChatClientForm
{
    partial class Client
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtMessageField = new System.Windows.Forms.TextBox();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtMessageField
            // 
            this.txtMessageField.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtMessageField.Location = new System.Drawing.Point(0, 371);
            this.txtMessageField.Multiline = true;
            this.txtMessageField.Name = "txtMessageField";
            this.txtMessageField.Size = new System.Drawing.Size(800, 79);
            this.txtMessageField.TabIndex = 0;
            this.txtMessageField.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtMessageField_KeyUp);
            // 
            // txtConsole
            // 
            this.txtConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtConsole.Location = new System.Drawing.Point(0, 0);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.Size = new System.Drawing.Size(800, 371);
            this.txtConsole.TabIndex = 1;
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.txtMessageField);
            this.Name = "Client";
            this.Text = "Take Chat - Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMessageField;
        private System.Windows.Forms.TextBox txtConsole;
    }
}

