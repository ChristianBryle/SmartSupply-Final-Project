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
    public partial class Qoutation_items : Form
    {
        private int quotationId;
        private string quotationTitle;

        public Qoutation_items()
        {
            InitializeComponent();
        }

        public Qoutation_items(int quotationId, string quotationTitle) : this()
        {
            this.quotationId = quotationId;
            this.quotationTitle = quotationTitle;
        }

        private void Qoutation_items_Load(object sender, EventArgs e)
        {
            LoadQuotationItems();
            button1.Click += btnExit_Click;
            button2.Click += btnDownloadQuotation_Click;
        }

        private void LoadQuotationItems()
        {
            try
            {
                label1.Text = quotationTitle;

                DataTable items = DBconnection.Query(
                    "SELECT p.product_name AS [Product], i.quantity AS [Qty], i.unit_price AS [Price], i.total_price AS [Total] " +
                    "FROM quotation_items i JOIN products p ON p.product_id = i.product_id " +
                    "WHERE i.quotation_id = @id ORDER BY p.product_name",
                    new SqlParameter("@id", quotationId));

                dataGridView1.DataSource = items;
                dataGridView1.ReadOnly = true;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.Columns["Price"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["Total"].DefaultCellStyle.Format = "N2";
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error.\n" + ex.Message, "Quotation Items", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDownloadQuotation_Click(object sender, EventArgs e)
        {
            try
            {
                // Get quotation details from database
                DataTable quotationData = DBconnection.Query(
                    "SELECT q.quotation_number, q.quotation_name, q.quotation_date, u.username " +
                    "FROM quotations q LEFT JOIN users u ON u.user_id = q.created_by " +
                    "WHERE q.quotation_id = @id",
                    new SqlParameter("@id", quotationId));

                if (quotationData.Rows.Count == 0)
                {
                    MessageBox.Show("Quotation not found.", "Download Quotation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataRow row = quotationData.Rows[0];
                string quotationNumber = row["quotation_number"].ToString();
                string quotationName = row["quotation_name"].ToString();
                DateTime quotationDate = Convert.ToDateTime(row["quotation_date"]);
                string createdBy = row["username"] != DBNull.Value ? row["username"].ToString() : "Unknown";

                // Generate PDF
                string pdfPath = QuotationPdf.Save(quotationId, quotationNumber, quotationName, quotationDate, createdBy);

                MessageBox.Show($"Quotation saved successfully.\nFile: {pdfPath}", "Download Quotation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to download quotation.\n" + ex.Message, "Download Quotation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
