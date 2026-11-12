using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartSupply
{
    public partial class frmMain : Form
    {
        private readonly int currentUserId;
        private DataTable _markupProducts;
        private DataTable _marginProducts;
        private bool _loadingProducts = false;

        private int? viewingQuotationId;

        public frmMain(int userId)
        {
            InitializeComponent();
            currentUserId = userId;

            InitializePriceCalculator();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (DataGridView grid in new[] { dataGridView1, dataGridView2, dataGridView3, dataGridView4, dataGridView5, dataGridView6, dataGridView7 })
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
            button14.Click += btnAddToCart_Click;
            button11.Click += btnClearCart_Click;
            button12.Click += btnRemoveFromCart_Click;
            btnView.Click += btnSaveQuotation_Click;
            button1.Click += btnViewQuotation_Click;
            btnDelete.Click += btnDeleteQuotation_Click;
            button13.Click += btnCompleteSale_Click;
            textBox3.TextChanged += (s, args) => UpdateChange();

            InitCart();
            RefreshAll();
        }
        private void InitializePriceCalculator()
        {
            ConfigureSearchCombo(cboProductMarkUp);
            ConfigureSearchCombo(cboProductMargin);

            ConfigureNumeric(nudQuantityMarkUp, 0);
            ConfigureNumeric(nudQuantityMargin, 0);
            ConfigureNumeric(nudTotalCostMarkUp, 2);
            ConfigureNumeric(nudTotalCostMargin, 2);
            ConfigurePercentTextBox(cboMarkUpPercent);
            ConfigurePercentTextBox(textBox4);

            nudQuantityMarkUp.ReadOnly = false;
            nudQuantityMarkUp.Increment = 1;

            cboProductMarkUp.SelectedIndexChanged -= cboProductMarkUp_Changed;
            cboProductMarkUp.SelectedIndexChanged += cboProductMarkUp_Changed;
            cboProductMarkUp.TextChanged -= cboProductMarkUp_Changed;
            cboProductMarkUp.TextChanged += cboProductMarkUp_Changed;

            cboProductMargin.SelectedIndexChanged -= cboProductMargin_Changed;
            cboProductMargin.SelectedIndexChanged += cboProductMargin_Changed;
            cboProductMargin.TextChanged -= cboProductMargin_Changed;
            cboProductMargin.TextChanged += cboProductMargin_Changed    ;

            nudTotalCostMarkUp.ValueChanged -= MarkupInput_Changed;
            nudTotalCostMarkUp.ValueChanged += MarkupInput_Changed;
            nudQuantityMarkUp.ValueChanged -= MarkupInput_Changed;
            nudQuantityMarkUp.ValueChanged += MarkupInput_Changed;
            cboMarkUpPercent.TextChanged -= MarkupInput_Changed;
            cboMarkUpPercent.TextChanged += MarkupInput_Changed;

            nudTotalCostMargin.ValueChanged -= MarginInput_Changed;
            nudTotalCostMargin.ValueChanged += MarginInput_Changed;
            nudQuantityMargin.ValueChanged -= MarginInput_Changed;
            nudQuantityMargin.ValueChanged += MarginInput_Changed;
            textBox4.TextChanged -= MarginInput_Changed;
            textBox4.TextChanged += MarginInput_Changed;

            btnCalculateMarkUp.Click -= btnCalculateMarkUp_Click;
            btnCalculateMarkUp.Click += btnCalculateMarkUp_Click;
            smartmarginbutton.Click -= btnCalculateMargin_Click;
            smartmarginbutton.Click += btnCalculateMargin_Click;

            LoadPriceProducts();
            ResetMarkupResults();
            ResetMarginResults();
        }

        private void cboProductMarkUp_Changed(object sender, EventArgs e)
        {
            if (_loadingProducts) return;
            ResetMarkupResults();
        }

        private void cboProductMargin_Changed(object sender, EventArgs e)
        {
            if (_loadingProducts) return;
            ResetMarginResults();
        }

        private void MarkupInput_Changed(object sender, EventArgs e)
        {
            if (_loadingProducts) return;
            ResetMarkupResults();
        }

        private void MarginInput_Changed(object sender, EventArgs e)
        {
            if (_loadingProducts) return;
            ResetMarginResults();
        }

        private void btnCalculateMarkUp_Click(object sender, EventArgs e)
        {
            DataRow row = FindProductRow(_markupProducts, cboProductMarkUp.Text);
            if (row == null)
            {
                ShowInputError("Please select a product from the list.");
                return;
            }

            int productId = Convert.ToInt32(row["product_id"]);
            decimal quantity = nudQuantityMarkUp.Value;
            decimal totalCost = nudTotalCostMarkUp.Value;

            if (quantity <= 0)
            {
                ShowInputError("This product has no available stock, so unit cost cannot be computed.");
                return;
            }
            if (totalCost <= 0)
            {
                ShowInputError("Please enter a total cost greater than zero.");
                return;
            }

            decimal markup;
            if (string.IsNullOrWhiteSpace(cboMarkUpPercent.Text) || !TryGetPercent(cboMarkUpPercent, out markup))
            {
                ShowInputError("Markup percentage must be a whole number from 1 to 99.");
                return;
            }

            decimal unitCost = totalCost / quantity;
            decimal sellingPrice = Math.Round(unitCost * (1m + markup), 2, MidpointRounding.AwayFromZero);
            decimal profit = Math.Round(sellingPrice - unitCost, 2, MidpointRounding.AwayFromZero);
            decimal totalProfit = Math.Round(profit * quantity, 2, MidpointRounding.AwayFromZero);
            unitCost = Math.Round(unitCost, 2, MidpointRounding.AwayFromZero);

            if (!SaveMarkUpSellingPrice(productId, sellingPrice)) return;

            RefreshProductsAfterUpdate();

            lblSellingPrice.Text = Peso(sellingPrice);
            lblUnitCostMarkUp.Text = Peso(unitCost);
            lblProfitMarkUp.Text = Peso(profit);
            MessageBox.Show("Product selling price has been successfully updated!", "Price Calculator",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCalculateMargin_Click(object sender, EventArgs e)
        {
            DataRow row = FindProductRow(_marginProducts, cboProductMargin.Text);
            if (row == null)
            {
                ShowInputError("Please select a product from the list.");
                return;
            }

            decimal quantity = nudQuantityMargin.Value;
            decimal totalCost = nudTotalCostMargin.Value;

            if (quantity <= 0)
            {
                ShowInputError("Please enter a quantity greater than zero.");
                return;
            }
            if (totalCost <= 0)
            {
                ShowInputError("Please enter a total cost greater than zero.");
                return;
            }

            decimal margin;
            if (string.IsNullOrWhiteSpace(textBox4.Text) || !TryGetPercent(textBox4, out margin))
            {
                ShowInputError("Margin percentage must be a whole number from 1 to 99.");
                return;
            }

            decimal unitCost = totalCost / quantity;
            decimal sellingPrice = Math.Round(unitCost / (1m - margin), 2, MidpointRounding.AwayFromZero);
            decimal profit = Math.Round(sellingPrice - unitCost, 2, MidpointRounding.AwayFromZero);
            unitCost = Math.Round(unitCost, 2, MidpointRounding.AwayFromZero);

            lblSuggestedPrice.Text = Peso(sellingPrice);
            lblUnitCostMargin.Text = Peso(unitCost);
            lblProfitMargin.Text = Peso(profit);
        }

        private void RefreshProductsAfterUpdate()
        {
            RefreshProducts();
        }

        public void RefreshProducts()
        {
            DataRow markupRow = FindProductRow(_markupProducts, cboProductMarkUp.Text);
            DataRow marginRow = FindProductRow(_marginProducts, cboProductMargin.Text);
            object markupId = markupRow != null ? markupRow["product_id"] : null;
            object marginId = marginRow != null ? marginRow["product_id"] : null;

            LoadPriceProducts();
            LoadProducts();

            _loadingProducts = true;
            try
            {
                if (markupId != null)
                {
                    cboProductMarkUp.SelectedValue = markupId;
                }
                if (marginId != null)
                {
                    cboProductMargin.SelectedValue = marginId;
                }
            }
            finally
            {
                _loadingProducts = false;
            }
        }

        // Separate tables for each dropdown
        private void LoadPriceProducts()
        {
            _loadingProducts = true;

            try
            {
                _markupProducts = GetProductsTable();
                _marginProducts = GetProductsTable();

                BindCombo(cboProductMarkUp, _markupProducts);
                BindCombo(cboProductMargin, _marginProducts);
            }
            finally
            {
                _loadingProducts = false;
            }
        }

        private void BindCombo(ComboBox cbo, DataTable table)
        {
            cbo.DataSource = null;
            cbo.DisplayMember = "product_name";
            cbo.ValueMember = "product_id";
            cbo.DataSource = table;
            cbo.SelectedIndex = -1;

            ConfigureSearchCombo(cbo);
        }

        private static void ConfigureSearchCombo(ComboBox combo)
        {
            combo.DropDownStyle = ComboBoxStyle.DropDown;
            combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            combo.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        private static void ConfigurePercentTextBox(TextBox textBox)
        {
            textBox.Text = string.Empty;
            textBox.KeyPress -= PercentTextBox_KeyPress;
            textBox.KeyPress += PercentTextBox_KeyPress;
        }

        private static void PercentTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (char.IsControl(e.KeyChar))
                return;

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            string value = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, e.KeyChar.ToString());
            if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int wholeNumber) ||
                wholeNumber < 1 || wholeNumber > 99)
                e.Handled = true;
        }

        private static void ConfigureNumeric(NumericUpDown numeric, int decimalPlaces)
        {
            numeric.Minimum = 0;
            numeric.Maximum = 1000000000;
            numeric.DecimalPlaces = decimalPlaces;
            numeric.ThousandsSeparator = true;
        }

        private static DataRow FindProductRow(DataTable products, string productName)
        {
            if (products == null || string.IsNullOrWhiteSpace(productName))
                return null;

            foreach (DataRow row in products.Rows)
            {
                if (string.Equals(Convert.ToString(row["product_name"]), productName.Trim(), StringComparison.OrdinalIgnoreCase))
                    return row;
            }

            return null;
        }

        private static DataTable GetProductsTable()
        {
            return DBconnection.Query(
                "SELECT product_id, product_name, quantity, unit_price FROM products ORDER BY product_name");
        }

        private static bool TryGetPercent(TextBox textBox, out decimal percent)
        {
            percent = 0m;
            string value = (textBox.Text ?? string.Empty).Trim();
            decimal numericValue;

            if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int wholeNumber) ||
                wholeNumber < 1 || wholeNumber > 99)
                return false;

            numericValue = wholeNumber;
            percent = numericValue / 100m;
            return true;
        }

        private bool SaveMarkUpSellingPrice(int productId, decimal sellingPrice)
        {
            try
            {
                int affectedRows = DBconnection.Execute(
                    "UPDATE products SET unit_price = @price WHERE product_id = @id",
                    new SqlParameter("@price", sellingPrice),
                    new SqlParameter("@id", productId));
                if (affectedRows != 1)
                {
                    MessageBox.Show("The product selling price was not saved.",
                        "Price Calculator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Failed to update the product price.\n" + ex.Message,
                    "Price Calculator", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void ResetMarkupResults()
        {
            lblSellingPrice.Text = Peso(0m);
            lblUnitCostMarkUp.Text = Peso(0m);
            lblProfitMarkUp.Text = Peso(0m);
            lblMarkUpUsed.Text = string.Empty;
        }

        private void ResetMarginResults()
        {
            lblSuggestedPrice.Text = Peso(0m);
            lblUnitCostMargin.Text = Peso(0m);
            lblProfitMargin.Text = Peso(0m);
            lblMarginUsed.Text = string.Empty;
        }

        private static string Peso(decimal value)
        {
            return "₱ " + value.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static void ShowInputError(string message)
        {
            MessageBox.Show(message, "Price Calculator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private readonly DataTable cart = new DataTable();

        private void InitCart()
        {
            cart.Columns.Add("ID", typeof(int));
            cart.Columns.Add("Product", typeof(string));
            cart.Columns.Add("Qty", typeof(int));
            cart.Columns.Add("Price", typeof(decimal));
            dataGridView4.DataSource = cart;
            dataGridView4.Columns["ID"].Visible = false;
            dataGridView4.Columns["Price"].DefaultCellStyle.Format = "N2";

            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = 100000;
            numericUpDown1.Value = 1;
        }

        private void RefreshAll()
        {
            try
            {
                LoadProducts();
                LoadPosProducts();
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

        private void LoadPosProducts()
        {
            dataGridView5.DataSource = DBconnection.Query(
                "SELECT product_id AS [ID], product_name AS [Product], quantity AS [Stock], unit_price AS [Price] " +
                "FROM products WHERE quantity > 0 ORDER BY product_name");
            dataGridView5.Columns["ID"].Visible = false;
            dataGridView5.Columns["Price"].DefaultCellStyle.Format = "N2";
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
                "ORDER BY t.transaction_date ASC, t.transaction_id ASC");
        }

        private void LoadDashboard()
        {
            label47.Text = Convert.ToInt32(DBconnection.Scalar("SELECT COUNT(*) FROM products")).ToString();
            label50.Text = Convert.ToInt32(DBconnection.Scalar("SELECT COUNT(*) FROM products WHERE quantity <= reorder_level")).ToString();
            label54.Text = Convert.ToInt32(DBconnection.Scalar("SELECT COUNT(*) FROM sales")).ToString();

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

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            int? id = SelectedId(dataGridView5, "ID");
            if (id == null)
            {
                MessageBox.Show("Please select a product.", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataGridViewRow row = dataGridView5.CurrentRow;
            string name = Convert.ToString(row.Cells["Product"].Value);
            int stock = Convert.ToInt32(row.Cells["Stock"].Value);
            decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
            int qty = (int)numericUpDown1.Value;

            DataRow existing = cart.AsEnumerable().FirstOrDefault(r => r.Field<int>("ID") == id.Value);
            int inCart = existing == null ? 0 : existing.Field<int>("Qty");

            if (inCart + qty > stock)
            {
                MessageBox.Show($"Not enough stock for {name}.\nAvailable: {stock}, already in cart: {inCart}.", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (existing == null)
            {
                cart.Rows.Add(id.Value, name, qty, price);
            }
            else
            {
                existing["Qty"] = inCart + qty;
            }

            numericUpDown1.Value = 1;
            UpdateCartTotal();
        }
        private void UpdateCartTotal()
        {
            decimal total = cart.AsEnumerable().Sum(r => r.Field<decimal>("Price") * r.Field<int>("Qty"));
            label37.Text = "\u20B1 " + total.ToString("N2");
            UpdateChange();
        }

        private decimal CartTotal()
        {
            return cart.AsEnumerable().Sum(r => r.Field<decimal>("Price") * r.Field<int>("Qty"));
        }

        private void UpdateChange()
        {
            decimal cash;
            decimal change = decimal.TryParse(textBox3.Text.Trim(), out cash) ? cash - CartTotal() : 0m;
            label35.Text = "\u20B1 " + Math.Max(0m, change).ToString("N2");
        }

        private void btnRemoveFromCart_Click(object sender, EventArgs e)
        {
            int? id = SelectedId(dataGridView4, "ID");
            if (id == null)
            {
                MessageBox.Show("Please select an item in the cart.", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRow row = cart.AsEnumerable().FirstOrDefault(r => r.Field<int>("ID") == id.Value);
            if (row != null)
                cart.Rows.Remove(row);
            UpdateCartTotal();
        }

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            if (cart.Rows.Count == 0)
                return;
            if (MessageBox.Show("Clear all items from the cart?", "POS Terminal", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            cart.Rows.Clear();
            UpdateCartTotal();
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
                "DELETE FROM Transactions_log WHERE product_id = @id; DELETE FROM sale_items WHERE product_id = @id; DELETE FROM products WHERE product_id = @id;",
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

        private void button10_Click(object sender, EventArgs e)
        {
            AddProduct addProduct = new AddProduct();
            if (addProduct.ShowDialog(this) == DialogResult.OK)
                RefreshProducts();
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
                RefreshProducts();

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

        private void btnQuatation_Click(object sender, EventArgs e)
        {
            pnlQuotation.Show();
            pnlQuotation.BringToFront();
            try
            {
                LoadQuotations();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error.\n" + ex.Message, "SmartSupply", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadQuotations()
        {
            viewingQuotationId = null;
            button1.Text = "View";
            label68.Text = "Saved Quotation";
            dataGridView7.DataSource = DBconnection.Query(
                "SELECT q.quotation_id AS [ID], q.quotation_number AS [Quotation No.], q.quotation_name AS [Name], " +
                "q.quotation_date AS [Date], u.username AS [Created By], " +
                "ISNULL((SELECT SUM(i.total_price) FROM quotation_items i WHERE i.quotation_id = q.quotation_id), 0) AS [Total] " +
                "FROM quotations q LEFT JOIN users u ON u.user_id = q.created_by " +
                "ORDER BY q.quotation_id DESC");
            dataGridView7.Columns["ID"].Visible = false;
            dataGridView7.Columns["Total"].DefaultCellStyle.Format = "N2";
            dataGridView7.Columns["Date"].DefaultCellStyle.Format = "d";
        }

        private void btnViewQuotation_Click(object sender, EventArgs e)
        {
            try
            {
                int? id = SelectedId(dataGridView7, "ID");
                if (id == null)
                {
                    MessageBox.Show("Please select a quotation.", "Quotation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DataGridViewRow row = dataGridView7.CurrentRow;
                string quotationNo = row.Cells["Quotation No."].Value.ToString();
                string quotationName = row.Cells["Name"].Value.ToString();
                string title = quotationNo + " - " + quotationName;

                Qoutation_items form = new Qoutation_items(id.Value, title);
                form.ShowDialog(this);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error.\n" + ex.Message, "SmartSupply", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteQuotation_Click(object sender, EventArgs e)
        {
            int? id = viewingQuotationId ?? SelectedId(dataGridView7, "ID");
            if (id == null)
            {
                MessageBox.Show("Please select a quotation.", "Quotation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Delete the selected quotation and all its items?", "Quotation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                DBconnection.Execute(
                    "DELETE FROM quotation_items WHERE quotation_id = @id; DELETE FROM quotations WHERE quotation_id = @id;",
                    new SqlParameter("@id", id.Value));
                LoadQuotations();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error.\n" + ex.Message, "SmartSupply", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCompleteSale_Click(object sender, EventArgs e)
        {
            if (cart.Rows.Count == 0)
            {
                MessageBox.Show("The cart is empty.", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal total = CartTotal();
            decimal cash;
            if (!decimal.TryParse(textBox3.Text.Trim(), out cash) || cash < total)
            {
                MessageBox.Show($"Cash received must be at least \u20B1 {total:N2}.", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox3.Focus();
                return;
            }
            decimal change = cash - total;

            string saleNumber;
            DateTime saleDate;
            int saleId;
            try
            {
                using (SqlConnection conn = DBconnection.Open())
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO sales (total_amount, cash_received, change_amount, created_by) VALUES (@total, @cash, @change, @user); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@total", total);
                        cmd.Parameters.AddWithValue("@cash", cash);
                        cmd.Parameters.AddWithValue("@change", change);
                        cmd.Parameters.AddWithValue("@user", currentUserId);
                        saleId = (int)cmd.ExecuteScalar();
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT sale_number, sale_date FROM sales WHERE sale_id = @id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", saleId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            saleNumber = reader.GetString(0);
                            saleDate = reader.GetDateTime(1);
                        }
                    }

                    foreach (DataRow r in cart.Rows)
                    {
                        int productId = r.Field<int>("ID");
                        int qty = r.Field<int>("Qty");

                        using (SqlCommand cmd = new SqlCommand(
                            "UPDATE products SET quantity = quantity - @qty WHERE product_id = @pid AND quantity >= @qty", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@qty", qty);
                            cmd.Parameters.AddWithValue("@pid", productId);
                            if (cmd.ExecuteNonQuery() == 0)
                            {
                                tx.Rollback();
                                MessageBox.Show($"Not enough stock for {r.Field<string>("Product")}. Sale cancelled.", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                RefreshAll();
                                return;
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO sale_items (sale_id, product_id, quantity, unit_price) VALUES (@sid, @pid, @qty, @price); " +
                            "INSERT INTO Transactions_log (product_id, transaction_type, quantity, remarks) VALUES (@pid, 'Stock Out', @qty, @remarks);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@sid", saleId);
                            cmd.Parameters.AddWithValue("@pid", productId);
                            cmd.Parameters.AddWithValue("@qty", qty);
                            cmd.Parameters.AddWithValue("@price", r.Field<decimal>("Price"));
                            cmd.Parameters.AddWithValue("@remarks", "POS Sale " + saleNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error.\n" + ex.Message, "SmartSupply", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string receiptPath = null;
            try
            {
                receiptPath = ReceiptPdf.Save(saleNumber, saleDate, cart, total, cash, change);
                DBconnection.Execute("UPDATE sales SET receipt_path = @path WHERE sale_id = @id",
                    new SqlParameter("@path", receiptPath), new SqlParameter("@id", saleId));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sale saved, but the receipt PDF could not be created.\n" + ex.Message, "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (receiptPath != null)
                MessageBox.Show($"Sale {saleNumber} completed.\nChange: \u20B1 {change:N2}\nReceipt saved to:\n{receiptPath}", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Information);

            cart.Rows.Clear();
            textBox3.Clear();
            UpdateCartTotal();
            RefreshAll();
        }

        private void btnSaveQuotation_Click(object sender, EventArgs e)
        {
            if (cart.Rows.Count == 0)
            {
                MessageBox.Show("The cart is empty.", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name;
            using (Quotation_Name dialog = new Quotation_Name())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                name = dialog.QuotationName;
            }

            try
            {
                string number;
                using (SqlConnection conn = DBconnection.Open())
                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    int quotationId;
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO quotations (quotation_name, created_by) VALUES (@name, @user); SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@user", currentUserId);
                        quotationId = (int)cmd.ExecuteScalar();
                    }

                    foreach (DataRow r in cart.Rows)
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO quotation_items (quotation_id, product_id, quantity, unit_price) VALUES (@qid, @pid, @qty, @price)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@qid", quotationId);
                            cmd.Parameters.AddWithValue("@pid", r.Field<int>("ID"));
                            cmd.Parameters.AddWithValue("@qty", r.Field<int>("Qty"));
                            cmd.Parameters.AddWithValue("@price", r.Field<decimal>("Price"));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT quotation_number FROM quotations WHERE quotation_id = @id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", quotationId);
                        number = Convert.ToString(cmd.ExecuteScalar());
                    }

                    tx.Commit();
                }

                MessageBox.Show($"Quotation {number} saved.", "POS Terminal", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error.\n" + ex.Message, "SmartSupply", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblProfitMargin_Click(object sender, EventArgs e)
        {

        }
    }
}
