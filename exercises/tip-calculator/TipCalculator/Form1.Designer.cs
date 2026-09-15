
namespace TipCalculator
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.TotalBillText = new System.Windows.Forms.TextBox();
            this.TotalTipBox = new System.Windows.Forms.TextBox();
            this.TipPercentLabel = new System.Windows.Forms.Label();
            this.TipPercentageBox = new System.Windows.Forms.TextBox();
            this.TotalTip = new System.Windows.Forms.Label();
            this.TotalBillBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TotalBill = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.BlueViolet;
            this.label1.Location = new System.Drawing.Point(68, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter Total Bill:";
            // 
            // TotalBillText
            // 
            this.TotalBillText.Location = new System.Drawing.Point(452, 58);
            this.TotalBillText.Name = "TotalBillText";
            this.TotalBillText.Size = new System.Drawing.Size(195, 31);
            this.TotalBillText.TabIndex = 1;
            this.TotalBillText.TextChanged += new System.EventHandler(this.TotalBillText_TextChanged);
            // 
            // TotalTipBox
            // 
            this.TotalTipBox.AcceptsReturn = true;
            this.TotalTipBox.Location = new System.Drawing.Point(452, 241);
            this.TotalTipBox.Name = "TotalTipBox";
            this.TotalTipBox.Size = new System.Drawing.Size(195, 31);
            this.TotalTipBox.TabIndex = 2;
            // 
            // TipPercentLabel
            // 
            this.TipPercentLabel.AutoSize = true;
            this.TipPercentLabel.BackColor = System.Drawing.Color.LawnGreen;
            this.TipPercentLabel.Location = new System.Drawing.Point(68, 109);
            this.TipPercentLabel.Name = "TipPercentLabel";
            this.TipPercentLabel.Size = new System.Drawing.Size(158, 25);
            this.TipPercentLabel.TabIndex = 4;
            this.TipPercentLabel.Text = "Tip Percentage";
            // 
            // TipPercentageBox
            // 
            this.TipPercentageBox.Location = new System.Drawing.Point(452, 109);
            this.TipPercentageBox.Name = "TipPercentageBox";
            this.TipPercentageBox.Size = new System.Drawing.Size(195, 31);
            this.TipPercentageBox.TabIndex = 5;
            this.TipPercentageBox.TextChanged += new System.EventHandler(this.TipPercentageBox_TextChanged);
            // 
            // TotalTip
            // 
            this.TotalTip.AutoSize = true;
            this.TotalTip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.TotalTip.Location = new System.Drawing.Point(68, 247);
            this.TotalTip.Name = "TotalTip";
            this.TotalTip.Size = new System.Drawing.Size(96, 25);
            this.TotalTip.TabIndex = 6;
            this.TotalTip.Text = "Total Tip";
            // 
            // TotalBillBox
            // 
            this.TotalBillBox.Location = new System.Drawing.Point(452, 314);
            this.TotalBillBox.Name = "TotalBillBox";
            this.TotalBillBox.Size = new System.Drawing.Size(195, 31);
            this.TotalBillBox.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 25);
            this.label2.TabIndex = 8;
            this.label2.Text = "label2";
            // 
            // TotalBill
            // 
            this.TotalBill.AutoSize = true;
            this.TotalBill.BackColor = System.Drawing.Color.Yellow;
            this.TotalBill.Location = new System.Drawing.Point(68, 317);
            this.TotalBill.Name = "TotalBill";
            this.TotalBill.Size = new System.Drawing.Size(95, 25);
            this.TotalBill.TabIndex = 9;
            this.TotalBill.Text = "Total Bill";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TotalBill);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TotalBillBox);
            this.Controls.Add(this.TotalTip);
            this.Controls.Add(this.TipPercentageBox);
            this.Controls.Add(this.TipPercentLabel);
            this.Controls.Add(this.TotalTipBox);
            this.Controls.Add(this.TotalBillText);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TotalBillText;
        private System.Windows.Forms.TextBox TotalTipBox;
        private System.Windows.Forms.Label TipPercentLabel;
        private System.Windows.Forms.TextBox TipPercentageBox;
        private System.Windows.Forms.Label TotalTip;
        private System.Windows.Forms.TextBox TotalBillBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label TotalBill;
    }
}

