using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartSupply
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            pnlDashboard.Show();
            pnlDashboard.BringToFront();
        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            pnlProducts.Show();
            pnlProducts.BringToFront();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPOS_Click(object sender, EventArgs e)
        {
            pnlPOSTerminal.Show();
            pnlPOSTerminal.BringToFront();
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            pnlCategories.Show();
            pnlCategories.BringToFront();
        }

        private void btnInventoryMovements_Click(object sender, EventArgs e)
        {
            pnlInventoryMovements.Show();
            pnlInventoryMovements.BringToFront();
        }

        private void brtnPriceCalculator_Click(object sender, EventArgs e)
        {
            pnlPriceCalculator.Show();
            pnlPriceCalculator.BringToFront();
        }

        private void panel21_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            AddProduct addProduct = new AddProduct();
            addProduct.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stockin stockin = new Stockin();
            stockin.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SmartMargin smartMargin = new SmartMargin();
            smartMargin.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            EditProduct editProduct = new EditProduct();
            editProduct.Show();

        }

        private void btnStockOut_Click(object sender, EventArgs e)
        {
            StockOut stockin = new StockOut();
            stockin.Show();
        }

        private void btnMarkUp_Click(object sender, EventArgs e)
        {
            MarkUp markUp = new MarkUp();
            markUp.Show();
        }
    }
}
