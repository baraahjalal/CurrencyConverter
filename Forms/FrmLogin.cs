using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CurrencyConverter
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void FrmLogin_Load(object sender, EventArgs e)
        {
            cmbUserName.Focus();
            cmbUserName.Items.Add("Baraah");
            cmbUserName.Items.Add("Danyah");
            cmbUserName.Items.Add("Fatima");
            cmbUserName.Items.Add("Amina");
        }

        private void cmbUserName_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtPassword.Focus();
            txtPassword.Text = "";
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // Passwords array (same order as ComboBox)
            string[] passwords = { "111", "222", "333", "444" };

            int index = cmbUserName.SelectedIndex;

            // Check if a user is selected and password matches
            if (index >= 0 && txtPassword.Text == passwords[index])
            {
                MessageBox.Show("Login successful ✅ "+ Environment.NewLine + "Weclome to the Currency Converter !", "Success");
                FrmMain frm = new FrmMain();
                frm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password ❌", "Error");
                // txtPassword.Clear();
                txtPassword.SelectAll();
                txtPassword.Focus();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOk_Click(sender, e);
            }
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
