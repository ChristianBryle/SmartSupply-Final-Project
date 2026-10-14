using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SmartSupply
{
    internal static class StockMovement
    {
        public static void Setup(ComboBox product, NumericUpDown quantity)
        {
            product.DropDownStyle = ComboBoxStyle.DropDownList;
            product.DisplayMember = "label";
            product.ValueMember = "product_id";
            product.DataSource = Database.Query(
                "SELECT product_id, product_name + ' (Stock: ' + CAST(quantity AS VARCHAR(20)) + ')' AS label " +
                "FROM products ORDER BY product_name");
            quantity.Minimum = 1;
            quantity.Maximum = 1000000;
        }

        public static bool Record(int productId, string type, int quantity, string remarks)
        {
            int delta = type == "Stock In" ? quantity : -quantity;

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    var update = new SqlCommand(
                        "UPDATE products SET quantity = quantity + @delta " +
                        "WHERE product_id = @id AND quantity + @delta >= 0", conn, tx);
                    update.Parameters.AddWithValue("@delta", delta);
                    update.Parameters.AddWithValue("@id", productId);
                    if (update.ExecuteNonQuery() == 0)
                    {
                        tx.Rollback();
                        return false;
                    }

                    var log = new SqlCommand(
                        "INSERT INTO Transactions_log (product_id, transaction_type, quantity, remarks) " +
                        "VALUES (@id, @type, @quantity, @remarks)", conn, tx);
                    log.Parameters.AddWithValue("@id", productId);
                    log.Parameters.AddWithValue("@type", type);
                    log.Parameters.AddWithValue("@quantity", quantity);
                    log.Parameters.AddWithValue("@remarks", string.IsNullOrWhiteSpace(remarks) ? (object)DBNull.Value : remarks.Trim());
                    log.ExecuteNonQuery();

                    tx.Commit();
                    return true;
                }
            }
        }

        public static bool Save(Form form, ComboBox product, NumericUpDown quantity, TextBox remarks, string type)
        {
            if (product.SelectedValue == null)
            {
                MessageBox.Show("Please select a product.", type, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                if (!Record(Convert.ToInt32(product.SelectedValue), type, (int)quantity.Value, remarks.Text))
                {
                    MessageBox.Show("Insufficient stock for this product.", type, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Failed to record movement.\n" + ex.Message, type, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            form.DialogResult = DialogResult.OK;
            form.Close();
            return true;
        }
    }
}
