using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartSupply
{
    public partial class EditProduct : Form
    {
        private readonly int productId;

        public EditProduct()
        {
            InitializeComponent();
        }

        public EditProduct(int productId) : this()
        {
            this.productId = productId;
            ProductFields.Setup(comboBox1, null, numericUpDown2, numericUpDown3);
            btnSaveProduct.Click += btnSaveProduct_Click;
            LoadProduct();
        }

        private void LoadProduct()
        {
            DataTable table = DBconnection.Query(
                "SELECT product_name, category_id, reorder_level, unit_price FROM products WHERE product_id = @id",
                new SqlParameter("@id", productId));
            if (table.Rows.Count == 0)
                return;

            DataRow row = table.Rows[0];
            textBox1.Text = row["product_name"].ToString();
            if (row["category_id"] != DBNull.Value)
                comboBox1.SelectedValue = row["category_id"];
            numericUpDown2.Value = Convert.ToDecimal(row["reorder_level"]);
            numericUpDown3.Value = Convert.ToDecimal(row["unit_price"]);
        }

        private void btnSaveProduct_Click(object sender, EventArgs e)
        {
            if (!ProductFields.Validate(textBox1, comboBox1))
                return;

            try
            {
                DBconnection.Execute(
                    "UPDATE products SET product_name = @name, category_id = @category, " +
                    "reorder_level = @reorder, unit_price = @price WHERE product_id = @id",
                    new SqlParameter("@name", textBox1.Text.Trim()),
                    new SqlParameter("@category", comboBox1.SelectedValue),
                    new SqlParameter("@reorder", (int)numericUpDown2.Value),
                    new SqlParameter("@price", numericUpDown3.Value),
                    new SqlParameter("@id", productId));

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Failed to update product.\n" + ex.Message, "Edit Product", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
