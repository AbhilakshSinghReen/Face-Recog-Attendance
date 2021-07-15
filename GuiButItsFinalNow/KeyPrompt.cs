using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuiButItsFinalNow
{
    public partial class KeyPrompt : Form
    {
        public string ReturnValue1 { get; set; }

        public KeyPrompt(bool StartAtCentre,Point StartLocation)
        {
            //MessageBox.Show(StartAtCentre.ToString());

            if (StartAtCentre)
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                this.StartPosition = FormStartPosition.Manual;
            }

            InitializeComponent();

            if (!(StartAtCentre))
            {
                this.Location = StartLocation;
            }
            else
            {
                this.Location = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - (this.Size.Width / 2), (Screen.PrimaryScreen.Bounds.Size.Height / 2) - (this.Size.Height / 2));
                this.BackColor = Color.FromArgb(255, 50, 50, 50);
            }
            
        }

        private void KPMBOkButton_Click(object sender, EventArgs e)
        {
            this.ReturnValue1 = KPMBkeyTextBox.Text;
            if (this.ReturnValue1 == "")
            {
                this.ReturnValue1 = "_";
            }
            //MessageBox.Show(ReturnValue1);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
