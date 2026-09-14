using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TipCalculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void TotalBillText_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(TotalBillText.Text, out double totalBill) && double.TryParse(TipPercentageBox.Text, out double tipPercentage))
            {
                ComputeTipButton_Click();
            }
        }

        private void TipPercentageBox_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(TotalBillText.Text, out double totalBill) && double.TryParse(TipPercentageBox.Text, out double tipPercentage))
            {
                ComputeTipButton_Click();
            }
        }

        private void ComputeTipButton_Click()
        {
            double totalBill;
            double.TryParse(TotalBillText.Text, out totalBill);
            double tipPercent;
            double.TryParse(TipPercentageBox.Text, out tipPercent);
            tipPercent = tipPercent * 0.01;
            double computedTip = totalBill * tipPercent;
            TotalTipBox.Text = "" + computedTip;
            double billWithTip = totalBill + computedTip;
            TotalBillBox.Text = "" + billWithTip;
        }
    }
}
