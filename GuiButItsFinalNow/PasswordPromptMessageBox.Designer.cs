namespace GuiButItsFinalNow
{
    partial class PasswordPromptMessageBox
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
            this.PPMBPasswordLabel = new System.Windows.Forms.Label();
            this.PPMBPasswordTextBox = new System.Windows.Forms.TextBox();
            this.PPMBOkButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PPMBPasswordLabel
            // 
            this.PPMBPasswordLabel.AutoSize = true;
            this.PPMBPasswordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PPMBPasswordLabel.Location = new System.Drawing.Point(32, 44);
            this.PPMBPasswordLabel.Name = "PPMBPasswordLabel";
            this.PPMBPasswordLabel.Size = new System.Drawing.Size(112, 25);
            this.PPMBPasswordLabel.TabIndex = 0;
            this.PPMBPasswordLabel.Text = "Password:";
            // 
            // PPMBPasswordTextBox
            // 
            this.PPMBPasswordTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PPMBPasswordTextBox.Location = new System.Drawing.Point(150, 44);
            this.PPMBPasswordTextBox.Name = "PPMBPasswordTextBox";
            this.PPMBPasswordTextBox.Size = new System.Drawing.Size(322, 31);
            this.PPMBPasswordTextBox.TabIndex = 1;
            // 
            // PPMBOkButton
            // 
            this.PPMBOkButton.AutoSize = true;
            this.PPMBOkButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PPMBOkButton.Location = new System.Drawing.Point(374, 114);
            this.PPMBOkButton.Name = "PPMBOkButton";
            this.PPMBOkButton.Size = new System.Drawing.Size(75, 35);
            this.PPMBOkButton.TabIndex = 3;
            this.PPMBOkButton.Text = "OK";
            this.PPMBOkButton.UseVisualStyleBackColor = true;
            this.PPMBOkButton.Click += new System.EventHandler(this.PPMBOkButton_Click);
            // 
            // PasswordPromptMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 161);
            this.Controls.Add(this.PPMBOkButton);
            this.Controls.Add(this.PPMBPasswordTextBox);
            this.Controls.Add(this.PPMBPasswordLabel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasswordPromptMessageBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PPMBPasswordLabel;
        private System.Windows.Forms.TextBox PPMBPasswordTextBox;
        private System.Windows.Forms.Button PPMBOkButton;
    }
}