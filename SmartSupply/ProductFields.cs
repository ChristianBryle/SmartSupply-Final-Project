using System.Windows.Forms;

namespace SmartSupply
{
    internal static class ProductFields
    {
        public static void Setup(ComboBox category, NumericUpDown stock, NumericUpDown reorder, NumericUpDown price)
        {
            category.DropDownStyle = ComboBoxStyle.DropDownList;
            category.DisplayMember = "category_name";
            category.ValueMember = "category_id";
            category.DataSource = DBconnection.Query("SELECT category_id, category_name FROM categories ORDER BY category_name");

            if (stock != null)
                stock.Maximum = 1000000;
            reorder.Maximum = 1000000;
            price.Maximum = 99999999;
            price.DecimalPlaces = 2;
        }

        public static bool Validate(TextBox name, ComboBox category)
        {
            if (name.Text.Trim().Length == 0)
            {
                MessageBox.Show("Product name is required.", "Product", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (category.SelectedValue == null)
            {
                MessageBox.Show("Please select a category. Add one in the Categories page first.", "Product", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
