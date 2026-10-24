using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartSupply
{
    public partial class  frmMain : Form
    {

        public  frmMain()
        {
            InitializeComponent();
        }
        
        private void cmbProduct_SelectedIndexChanged( object sender, EventArgs e)
        {
            
        }
        

        private void cmbProduct_KeyDown( object sender, KeyEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (DataGridView grid in new[] { dataGridView1, dataGridView2, dataGridView3, dataGridView4, dataGridView5, dataGridView6 })
            {
                grid.ReadOnly = true;
                grid.AllowUserToAddRows = false;
                grid.AllowUserToDeleteRows = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.MultiSelect = false;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }

            button2.Click += btnDeleteCategory_Click;
            button7.Click += btnDeleteProduct_Click;
            button9.Click += btnSearchProduct_Click;
            textBox2.KeyDown += (s, args) => { if (args.KeyCode == Keys.Enter) LoadProducts(); };

            RefreshAll();
        }

        private void RefreshAll()
        {
            try
            {
                LoadProducts();
                LoadCategories();
                LoadMovements();
                LoadDashboard();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error.\n" + ex.Message, "SmartSupply", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static int? SelectedId(DataGridView grid, string column)
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells[column].Value == null)
                return null;
            return Convert.ToInt32(grid.CurrentRow.Cells[column].Value);
        }

        private void LoadProducts()
        {
            dataGridView3.DataSource = DBconnection.Query(
                "SELECT p.product_id AS [ID], p.product_name AS [Product], c.category_name AS [Category], " +
                "p.quantity AS [Stock], p.reorder_level AS [Reorder Level], p.unit_price AS [Price] " +
                "FROM products p LEFT JOIN categories c ON c.category_id = p.category_id " +
                "WHERE p.product_name LIKE @search OR c.category_name LIKE @search ORDER BY p.product_name",
                new SqlParameter("@search", "%" + textBox2.Text.Trim() + "%"));
        }

        private void LoadCategories()
        {
            dataGridView2.DataSource = DBconnection.Query(
                "SELECT c.category_id AS [ID], c.category_name AS [Category], " +
                "(SELECT COUNT(*) FROM products p WHERE p.category_id = c.category_id) AS [Products] " +
                "FROM categories c ORDER BY c.category_name");
        }

        private void LoadMovements()
        {
            dataGridView1.DataSource = DBconnection.Query(
                "SELECT t.transaction_id AS [ID], t.transaction_date AS [Date], p.product_name AS [Product], " +
                "t.transaction_type AS [Type], t.quantity AS [Quantity], t.remarks AS [Remarks] " +
                "FROM Transactions_log t JOIN products p ON p.product_id = t.product_id " +
                "ORDER BY t.transaction_date DESC, t.transaction_id DESC");
        }

        private void LoadDashboard()
        {
            label47.Text = Convert.ToInt32(DBconnection.Scalar("SELECT COUNT(*) FROM products")).ToString();
            label50.Text = Convert.ToInt32(DBconnection.Scalar("SELECT COUNT(*) FROM products WHERE quantity <= reorder_level")).ToString();

            dataGridView6.DataSource = DBconnection.Query(
                "SELECT p.product_name AS [Product], c.category_name AS [Category], " +
                "p.quantity AS [Stock], p.reorder_level AS [Reorder Level] " +
                "FROM products p LEFT JOIN categories c ON c.category_id = p.category_id " +
                "WHERE p.quantity <= p.reorder_level ORDER BY p.quantity, p.product_name");
        }

        private void btnSearchProduct_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            int? id = SelectedId(dataGridView3, "ID");
            if (id == null)
            {
                MessageBox.Show("Please select a product.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Delete the selected product and its transaction history?", "Products", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DBconnection.Execute(
                "DELETE FROM Transactions_log WHERE product_id = @id; DELETE FROM products WHERE product_id = @id;",
                new SqlParameter("@id", id.Value));
            RefreshAll();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Please enter a category name.", "Categories", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            object exists = DBconnection.Scalar("SELECT COUNT(*) FROM categories WHERE category_name = @name", new SqlParameter("@name", name));
            if (Convert.ToInt32(exists) > 0)
            {
                MessageBox.Show("Category already exists.", "Categories", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DBconnection.Execute("INSERT INTO categories (category_name) VALUES (@name)", new SqlParameter("@name", name));
            textBox1.Clear();
            RefreshAll();
        }

        private void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            int? id = SelectedId(dataGridView2, "ID");
            if (id == null)
            {
                MessageBox.Show("Please select a category.", "Categories", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Delete the selected category? Products in it will become uncategorized.", "Categories", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DBconnection.Execute(
                "UPDATE products SET category_id = NULL WHERE category_id = @id; DELETE FROM categories WHERE category_id = @id;",
                new SqlParameter("@id", id.Value));
            RefreshAll();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            pnlDashboard.Show();
            pnlDashboard.BringToFront();
            try
            {
                LoadDashboard();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error.\n" + ex.Message, "SmartSupply", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (addProduct.ShowDialog(this) == DialogResult.OK)
                RefreshAll();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stockin stockin = new Stockin();
            if (stockin.ShowDialog(this) == DialogResult.OK)
                RefreshAll();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SmartMargin smartMargin = new SmartMargin();
            smartMargin.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int? id = SelectedId(dataGridView3, "ID");
            if (id == null)
            {
                MessageBox.Show("Please select a product.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            EditProduct editProduct = new EditProduct(id.Value);
            if (editProduct.ShowDialog(this) == DialogResult.OK)
                RefreshAll();

        }

        private void btnStockOut_Click(object sender, EventArgs e)
        {
            StockOut stockOut = new StockOut();
            if (stockOut.ShowDialog(this) == DialogResult.OK)
                RefreshAll();
        }

        private void btnMarkUp_Click(object sender, EventArgs e)
        {
            MarkUp markUp = new MarkUp();
            markUp.Show();
        }

        private void label55_Click(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }
    }
}
