using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartSupply
{
    public partial class SmartMargin : Form
    {
        private decimal costPrice;
        private int quantity;

        public decimal SellingPrice { get; private set; }
        public decimal Profit { get; private set; }
        public decimal TotalCost { get; private set; }
        public decimal TotalProfit { get; private set; }

        public SmartMargin()
        {
            InitializeComponent();
          
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }


private void btnCalculate_Click(object sender, EventArgs e)
        {
            decimal percentage;

            if (!decimal.TryParse(smartpercentage.Text, out percentage)
                || percentage <= 0 || percentage >= 100)
            {
                MessageBox.Show("Enter a margin between 0 and 100.");
                return;
            }

            SellingPrice = costPrice / (1 - percentage / 100);
            Profit = SellingPrice - costPrice;
            TotalCost = costPrice * quantity;
            TotalProfit = Profit * quantity;

            

        }

    }
}

