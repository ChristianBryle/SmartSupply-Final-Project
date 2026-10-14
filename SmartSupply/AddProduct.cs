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
    public partial class AddProduct : Form
    {
        public AddProduct()
        {
            InitializeComponent();
            ProductFields.Setup(comboBox1, numericUpDown1, numericUpDown2, numericUpDown3);
            numericUpDown2.Value = 10;
            btnSaveProduct.Click += btnSaveProduct_Click;
        }

        private void btnSaveProduct_Click(object sender, EventArgs e)
        {
            if (!ProductFields.Validate(textBox1, comboBox1))
                return;

            try
            {
                DBconnection.Execute(
                    "INSERT INTO products (product_name, category_id, quantity, reorder_level, unit_price) " +
                    "VALUES (@name, @category, @quantity, @reorder, @price)",
                    new SqlParameter("@name", textBox1.Text.Trim()),
                    new SqlParameter("@category", comboBox1.SelectedValue),
                    new SqlParameter("@quantity", (int)numericUpDown1.Value),
                    new SqlParameter("@reorder", (int)numericUpDown2.Value),
                    new SqlParameter("@price", numericUpDown3.Value));

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Failed to save product.\n" + ex.Message, "Add Product", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label49_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
