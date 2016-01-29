
﻿namespace InputDialog
{
    partial class InputDialog
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
            this.lblInput = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbl = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblCountDown = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblInput
            // 
            this.lblInput.AutoSize = true;
            this.lblInput.Location = new System.Drawing.Point(33, 16);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(24, 13);
            this.lblInput.TabIndex = 0;
            this.lblInput.Text = "Id :";
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(83, 14);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(100, 21);
            this.txtInput.TabIndex = 0;
            this.txtInput.TextChanged += new System.EventHandler(this.txtInput_TextChanged_1);
            // 
            // btnOk
            // 
            this.btnOk.AutoSize = true;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Enabled = false;
            this.btnOk.Location = new System.Drawing.Point(101, 87);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(41, 26);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "&OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(148, 87);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(57, 26);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lbl
            // 
            this.lbl.AutoSize = true;
            this.lbl.Location = new System.Drawing.Point(30, 54);
            this.lbl.Name = "lbl";
            this.lbl.Size = new System.Drawing.Size(41, 13);
            this.lbl.TabIndex = 3;
            this.lbl.Text = "Name :";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(83, 51);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(100, 21);
            this.txtName.TabIndex = 4;
            // 
            // lblCountDown
            // 
            this.lblCountDown.AutoSize = true;
            this.lblCountDown.Location = new System.Drawing.Point(22, 94);
            this.lblCountDown.Name = "lblCountDown";
            this.lblCountDown.Size = new System.Drawing.Size(27, 13);
            this.lblCountDown.TabIndex = 5;
            this.lblCountDown.Text = "( 9 )";
            this.lblCountDown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // InputDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(254, 160);
            this.ControlBox = false;
            this.Controls.Add(this.lblCountDown);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lbl);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.lblInput);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = " Input Dialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtInput;
  
    
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.Label lbl;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblCountDown;
    }

}