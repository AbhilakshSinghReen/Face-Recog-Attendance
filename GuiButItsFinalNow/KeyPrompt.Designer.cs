namespace GuiButItsFinalNow
{
    partial class KeyPrompt
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
            this.KPMBOkButton = new System.Windows.Forms.Button();
            this.KPMBkeyTextBox = new System.Windows.Forms.TextBox();
            this.KPMBkeyLabel = new System.Windows.Forms.Label();
            this.ExitButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // KPMBOkButton
            // 
            this.KPMBOkButton.AutoSize = true;
            this.KPMBOkButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KPMBOkButton.Location = new System.Drawing.Point(412, 122);
            this.KPMBOkButton.Name = "KPMBOkButton";
            this.KPMBOkButton.Size = new System.Drawing.Size(75, 35);
            this.KPMBOkButton.TabIndex = 6;
            this.KPMBOkButton.Text = "OK";
            this.KPMBOkButton.UseVisualStyleBackColor = true;
            this.KPMBOkButton.Click += new System.EventHandler(this.KPMBOkButton_Click);
            // 
            // KPMBkeyTextBox
            // 
            this.KPMBkeyTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KPMBkeyTextBox.Location = new System.Drawing.Point(88, 58);
            this.KPMBkeyTextBox.Name = "KPMBkeyTextBox";
            this.KPMBkeyTextBox.Size = new System.Drawing.Size(500, 31);
            this.KPMBkeyTextBox.TabIndex = 5;
            // 
            // KPMBkeyLabel
            // 
            this.KPMBkeyLabel.BackColor = System.Drawing.Color.White;
            this.KPMBkeyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KPMBkeyLabel.Location = new System.Drawing.Point(26, 58);
            this.KPMBkeyLabel.Name = "KPMBkeyLabel";
            this.KPMBkeyLabel.Size = new System.Drawing.Size(56, 31);
            this.KPMBkeyLabel.TabIndex = 4;
            this.KPMBkeyLabel.Text = "Key:";
            this.KPMBkeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ExitButton
            // 
            this.ExitButton.AutoSize = true;
            this.ExitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExitButton.Location = new System.Drawing.Point(512, 122);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(75, 35);
            this.ExitButton.TabIndex = 7;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // KeyPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(600, 200);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.KPMBOkButton);
            this.Controls.Add(this.KPMBkeyTextBox);
            this.Controls.Add(this.KPMBkeyLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "KeyPrompt";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "KeyPrompt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button KPMBOkButton;
        private System.Windows.Forms.TextBox KPMBkeyTextBox;
        private System.Windows.Forms.Label KPMBkeyLabel;
        private System.Windows.Forms.Button ExitButton;
    }
}