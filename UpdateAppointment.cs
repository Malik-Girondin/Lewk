﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace C969
{
    public partial class UpdateAppointment : Form
    {
        private DataGridViewRow _selectedRow;
        private string selectedTimeZoneId;
        private DateTime selectedDate;

        public UpdateAppointment(DataGridViewRow selectedRow)
        {
            InitializeComponent();
            _selectedRow = selectedRow;
        }

        private async void UpdateAppointment_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.TextBox.CheckForIllegalCrossThreadCalls = false;
            Label.CheckForIllegalCrossThreadCalls = false;

            await Task.Run(() =>
            {
                try
                {
                    PopulateTimeZones();
                    PopulateAppointmentTimes();
                    LoadAppointmentDetails();
                    CheckAppointmentsWithin15Minutes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            });
        }

        private void PopulateTimeZones()
        {
            comboBox2.Items.Clear();
            foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                comboBox2.Items.Add(timeZone.Id);
            }

            if (comboBox2.Items.Count > 0)
            {
                selectedTimeZoneId = comboBox2.Items[0].ToString();
            }
        }

        private void PopulateAppointmentTimes()
        {
            comboBox1.Items.Clear();
            DateTime startTime = new DateTime(2024, 1, 1, 9, 0, 0); // Start at 9:00 AM
            DateTime endTime = new DateTime(2024, 1, 1, 17, 0, 0); // End at 5:00 PM

            while (startTime <= endTime)
            {
                comboBox1.Items.Add(startTime.ToString("hh:mm tt"));
                startTime = startTime.AddMinutes(15);
            }
        }

        private void LoadAppointmentDetails()
        {
            textBox1.Text = Convert.ToDateTime(_selectedRow.Cells["Start"].Value).ToShortDateString();
            comboBox1.SelectedItem = Convert.ToDateTime(_selectedRow.Cells["Start"].Value).ToString("hh:mm tt");
            textBox4.Text = _selectedRow.Cells["Title"].Value.ToString();
            textBox5.Text = _selectedRow.Cells["Description"].Value.ToString();
            textBox3.Text = GetCustomerNameById(Convert.ToInt32(_selectedRow.Cells["CustomerID"].Value)); // Fetch customer name

            // Retrieve and set the stored time zone
            int appointmentId = Convert.ToInt32(_selectedRow.Cells["appointmentId"].Value);
            string timeZoneValue = TimeZoneStorage.GetTimeZone(appointmentId);
            comboBox2.SelectedItem = timeZoneValue;
        }

        private string GetCustomerNameById(int customerId)
        {
            string query = "SELECT CustomerName FROM customer WHERE CustomerID = @CustomerID";
            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID", customerId);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : string.Empty;
            }
        }

        private void CheckAppointmentsWithin15Minutes()
        {
            AddAppointment form = new AddAppointment();
            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
                        SELECT COUNT(*) 
                        FROM appointment 
                        WHERE CustomerId = @CustomerId 
                        AND start BETWEEN NOW() AND DATE_ADD(NOW(), INTERVAL 15 MINUTE)
                    ";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@CustomerId", GetCustomerIdByName(form.textBox3.Text));
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show("You have an appointment within the next 15 minutes!");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private int GetCustomerIdByName(string customerName)
        {
            string query = "SELECT CustomerID FROM customer WHERE CustomerName = @CustomerName";
            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@CustomerName", customerName);
                    con.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    return -1;
                }
            }
        }

        private int? GetUserIdByName(string userName)
        {
            string query = "SELECT UserID FROM user WHERE UserName = @UserName";
            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    con.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : (int?)null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    return null;
                }
            }
        }

        private int InsertNewUser(string userName, MySqlConnection con)
        {
            string userQuery = @"
        INSERT INTO user (UserName, Password, Active, CreateDate, CreatedBy, LastUpdate, LastUpdateBy)
        VALUES (@UserName, @Password, @Active, @CreateDate, @CreatedBy, @LastUpdate, @LastUpdateBy);
        SELECT LAST_INSERT_ID();
    ";

            MySqlCommand userCmd = new MySqlCommand(userQuery, con);
            userCmd.Parameters.AddWithValue("@UserName", userName);
            userCmd.Parameters.AddWithValue("@Password", "");
            userCmd.Parameters.AddWithValue("@Active", 1);
            userCmd.Parameters.AddWithValue("@CreateDate", DateTime.Now);
            userCmd.Parameters.AddWithValue("@CreatedBy", "sqlUser");
            userCmd.Parameters.AddWithValue("@LastUpdate", DateTime.Now);
            userCmd.Parameters.AddWithValue("@LastUpdateBy", "sqlUser");

            int userId = Convert.ToInt32(userCmd.ExecuteScalar());
            return userId;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    int appointmentId = GetAppointmentIdFromSomeSource();

                    UpdateAppointmentDetails(con, appointmentId);

                    // Store the updated time zone
                    string timeZone = comboBox2.SelectedItem.ToString();
                    TimeZoneStorage.SetTimeZone(appointmentId, timeZone);

                    Main form = (Main)Application.OpenForms["Main"];
                    if (form != null)
                    {
                        form.UpdateDataGridView2();
                        form.dataGridView2.Refresh();
                    }

                    this.Close();
                    form.dataGridView2.Refresh();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public int GetPhoneByCustomerId(string customerName)
        {
            string query = "SELECT phone FROM customer WHERE CustomerName = @CustomerName";
            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@CustomerName", customerName);
                    con.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    return -1;
                }
            }
        }

        private bool IsTimeConflicted(DateTime start, DateTime end)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;
            string query = @"
                SELECT COUNT(*) 
                FROM appointment 
                WHERE start < @End 
                AND end > @Start;
            ";

            using (MySqlConnection con = new MySqlConnection(connectionString))
            using (MySqlCommand cmd = new MySqlCommand(query, con))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);
                    con.Open();

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    return true;
                }
            }
        }

        private void UpdateAppointmentDetails(MySqlConnection con, int appointmentId)
        {
            try
            {
                DateTime selectedDateTime;
                if (!DateTime.TryParse(textBox1.Text, out selectedDateTime))
                {
                    MessageBox.Show("Invalid date format. Please enter a valid date.");
                    return;
                }

                DateTime selectedDate = monthCalendar1.SelectionStart.Date;
                DateTime selectedTime = DateTime.ParseExact(comboBox1.Text, "hh:mm tt", CultureInfo.InvariantCulture);
                DateTime start = selectedDate.Add(selectedTime.TimeOfDay);
                DateTime end = start.AddHours(0.25);
                string type = textBox4.Text;
                string description = textBox5.Text;
                string timeZone = comboBox2.SelectedItem.ToString(); // Capture time zone

                // Update query to exclude non-existing columns
                string query = "UPDATE Appointment SET Start = @Start, End = @End, Title = @Type, Description = @Description WHERE appointmentId = @AppointmentId";
                using (MySqlCommand cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Start", start);
                    cmd.Parameters.AddWithValue("@End", end);
                    cmd.Parameters.AddWithValue("@Type", type); // Use "Title" column for the type
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // Save the time zone to the dictionary
                        TimeZoneStorage.SetTimeZone(appointmentId, comboBox2.SelectedItem.ToString());

                        MessageBox.Show("Appointment details updated successfully.");
                    }
                    else
                    {
                        MessageBox.Show("No appointment found with the given ID.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private int GetAppointmentIdFromSomeSource()
        {
            int appointmentId = -1;

            string connectionString = ConfigurationManager.ConnectionStrings["localdb"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    string customerName = textBox3.Text;

                    con.Open();
                    string query = "SELECT appointmentId FROM Appointment WHERE CustomerId = @CustomerId";
                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@CustomerId", GetCustomerIdByName(customerName));

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            appointmentId = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("No appointment found for the customer.");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

            return appointmentId;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            try
            {
                string selectedTimeZoneId = string.Empty;
                comboBox2.Invoke((Action)(() =>
                {
                    selectedTimeZoneId = comboBox2.SelectedItem?.ToString();
                }));

                TimeZoneInfo selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(selectedTimeZoneId);

                DateTime selectedDate = monthCalendar1.SelectionStart;
                DateTimeOffset selectedDateTimeOffset = new DateTimeOffset(selectedDate, selectedTimeZone.GetUtcOffset(selectedDate));

                DateTime localDateTime = selectedDateTimeOffset.LocalDateTime;

                textBox1.Invoke((Action)(() =>
                {
                    textBox1.Text = localDateTime.ToShortDateString();
                }));

                List<string> itemsToAdd = new List<string>();

                DateTime startTime = new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, 9, 0, 0);
                DateTime endTime = new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, 17, 0, 0);

                while (startTime < endTime)
                {
                    itemsToAdd.Add(startTime.ToString("hh:mm tt"));
                    startTime = startTime.AddMinutes(15);
                }

                comboBox1.Invoke((Action)(() =>
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(itemsToAdd.ToArray());
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}
