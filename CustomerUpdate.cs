using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace C969
{
    public partial class CustomerUpdate : Form
    {
        private DataGridViewRow _selectedRow;
        public CustomerUpdate(DataGridViewRow selectedRow)
        {
            InitializeComponent();
            _selectedRow = selectedRow;
        }

        private void CustomerUpdate_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string customerName = textBox2.Text.Trim();
            string address = textBox4.Text.Trim();
            string phoneNumber = textBox5.Text.Trim();

            // Check for empty fields
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(phoneNumber))
            {
                MessageBox.Show("All fields must be filled out.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if phone number contains only digits and dashes
            foreach (char c in phoneNumber)
            {
                if (!char.IsDigit(c) && c != '-')
                {
                    MessageBox.Show("Phone number can only contain digits and dashes.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            _selectedRow.Cells["CustomerName"].Value = customerName;
            _selectedRow.Cells["Address"].Value = address;
            _selectedRow.Cells["Phone"].Value = phoneNumber;

            string query = "UPDATE address SET Address = @Address, Phone = @Phone WHERE AddressID = @AddressID;" +
                           "UPDATE customer SET CustomerName = @CustomerName WHERE AddressID = @AddressID;";

            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    MySqlCommand updateCMD = new MySqlCommand(query, con);
                    updateCMD.Parameters.AddWithValue("@AddressID", _selectedRow.Cells["AddressID"].Value);
                    updateCMD.Parameters.AddWithValue("@Phone", phoneNumber);
                    updateCMD.Parameters.AddWithValue("@CustomerName", customerName);
                    updateCMD.Parameters.AddWithValue("@Address", address);
                    updateCMD.ExecuteNonQuery();

                    Main form = new Main();
                    form.UpdateDataGridView();

                    MessageBox.Show("Customer updated successfully");
                    this.Close();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error" + ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
