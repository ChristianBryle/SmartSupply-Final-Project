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
    public partial class StockOut : Form
    {
        public StockOut()
        {
            InitializeComponent();
            StockMovement.Setup(comboBox1, numericUpDown1);
            btnSaveProduct.Click += (s, e) => StockMovement.Save(this, comboBox1, numericUpDown1, textBox1, "Stock Out");
        }
    }
}
