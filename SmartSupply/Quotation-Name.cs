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
    public partial class Quotation_Name : Form
    {
        public string QuotationName => textBox1.Text.Trim();

        public Quotation_Name()
        {
            InitializeComponent();
            AcceptButton = button1;
            button1.Click += (s, e) =>
            {
                if (QuotationName.Length == 0)
                {
                    MessageBox.Show("Please enter a quotation name.", "Quotation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                DialogResult = DialogResult.OK;
            };
        }
    }
}
