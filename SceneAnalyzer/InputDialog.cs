
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InputDialog
{
    public partial class InputDialog : Form
    {
        string input_text;

        public string ResultText
        {
            get { return input_text; }
            private set { input_text = value; }
        }

        string id_subject;


        private System.Windows.Forms.Timer timer1;
        private int counter = 15;
      

        private void timer1_Tick(object sender, EventArgs e)
        {
            counter--;
            if (counter == 0)
                timer1.Stop();
            lblCountDown.Text = "( "+counter.ToString()+" )";
        }


        public InputDialog(string title, string idSubject, string textbox_string)
        {
            InitializeComponent();
            this.Text = title;

            this.txtInput.Text = textbox_string;
            this.id_subject = idSubject;

            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // 1 second
            lblCountDown.Text = "( "+counter.ToString()+" )";

            System.Timers.Timer tim = new System.Timers.Timer();
            tim.Interval = 15 * 1000;
            tim.Elapsed += new System.Timers.ElapsedEventHandler(tim_Elapsed);
            tim.Start();
            timer1.Start();

            
        }

        void tim_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
                //do pic
                //this.Close();
                btnCancel_Click(sender, e);
        }

        public InputDialog()
        {
            InitializeComponent();
        }

        

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (txtName.Text != "")
                ResultText = txtInput.Text.Trim() + ";" + txtName.Text.Trim()+";"+id_subject;
            else
                ResultText = txtInput.Text.Trim() + ";unknown" + ";" + id_subject;

        }

        private void txtInput_TextChanged_1(object sender, EventArgs e)
        {
            if (txtInput.Text.Trim().Length > 0)
            {
                btnOk.Enabled = true;
            }
            else
            {
                btnOk.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ResultText = "0;unknown" + ";" + id_subject;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

      
    }
}

