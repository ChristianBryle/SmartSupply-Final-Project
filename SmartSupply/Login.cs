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
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            textBox2.UseSystemPasswordChar = true;
            AcceptButton = btnLogin;
        }

        private void label57_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text;

            if (username.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("Please enter your username and password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId;
            try
            {
                object found = DBconnection.Scalar(
                    "SELECT user_id FROM users WHERE username = @username AND password_hash = @hash",
                    new SqlParameter("@username", username),
                    new SqlParameter("@hash", DBconnection.HashPassword(password)));

                if (found == null)
                {
                    MessageBox.Show("Invalid username or password.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textBox2.Clear();
                    textBox2.Focus();
                    return;
                }
                userId = Convert.ToInt32(found);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Unable to connect to the database.\n" + ex.Message, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmMain Main = new frmMain(userId);
            Main.FormClosed += (s, args) => Close();
            Main.Show();
            Hide();
        }
    }
}
