using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace klimenkovdepozit
{
    public partial class Form3 : Form
    {
        private string connectionString = "Server=localhost;Database=dep;Uid=root;Pwd=a0#$f43JF@Ejk#_cwp[!323tFGed4%$%@z67cmrg-+356G^;";

        public Form3()
        {
            InitializeComponent();
            LoadDepositsData();
            LoadInterestRates();  // Загружаем проценты
            LoadClients();  // Загружаем клиентов
            this.FormClosing += Form3_FormClosing;
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Завершаем приложение, когда форма закрывается
            Application.Exit();
        }

        // Метод для загрузки данных депозитов
        private void LoadDepositsData()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT d.DepositID, u.FullName AS ClientName, d.DepositAmount, d.InterestRate, 
                               d.StartDate, d.EndDate
                        FROM Deposits d
                        INNER JOIN Users u ON d.UserID = u.UserID";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }

        // Метод для загрузки процентных ставок в ComboBox
        private void LoadInterestRates()
        {
            // Добавляем процентные ставки в ComboBox с длительностью
            comboBox1.Items.Add("5% - 12 месяцев");
            comboBox1.Items.Add("10% - 24 месяца");
            comboBox1.Items.Add("15% - 36 месяцев");
            comboBox1.Items.Add("20% - 48 месяцев");
        }

        // Метод для загрузки списка клиентов в ComboBox
        private void LoadClients()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT UserID, FullName FROM Users";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    comboBox2.DisplayMember = "FullName";  // Отображаем ФИО
                    comboBox2.ValueMember = "UserID";  // Значение будет ID клиента
                    comboBox2.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке клиентов: " + ex.Message);
                }
            }
        }

        // Метод для обработки нажатия кнопки "Добавить депозит"
        private void button2_Click(object sender, EventArgs e)
        {
            decimal depositAmount = Convert.ToDecimal(textBox1.Text);
            string selectedRate = comboBox1.SelectedItem.ToString();

            // Проверяем сумму вклада
            if (depositAmount < 100 || depositAmount > 10000)
            {
                MessageBox.Show("Сумма вклада должна быть от 100 до 10000.");
                return;
            }

            // Разбираем выбранную процентную ставку и длительность
            decimal interestRate = 0;
            int depositDuration = 0;
            if (selectedRate.Contains("5%"))
            {
                interestRate = 5;
                depositDuration = 12;
            }
            else if (selectedRate.Contains("10%"))
            {
                interestRate = 10;
                depositDuration = 24;
            }
            else if (selectedRate.Contains("15%"))
            {
                interestRate = 15;
                depositDuration = 36;
            }
            else if (selectedRate.Contains("20%"))
            {
                interestRate = 20;
                depositDuration = 48;
            }
            else
            {
                MessageBox.Show("Выберите правильную процентную ставку.");
                return;
            }

            DateTime startDate = DateTime.Now;
            DateTime endDate = startDate.AddMonths(depositDuration);

            string query = @"
                INSERT INTO Deposits (UserID, DepositAmount, InterestRate, StartDate, EndDate) 
                VALUES (@UserID, @DepositAmount, @InterestRate, @StartDate, @EndDate)";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    cmd.Parameters.AddWithValue("@UserID", comboBox2.SelectedValue);
                    cmd.Parameters.AddWithValue("@DepositAmount", depositAmount);
                    cmd.Parameters.AddWithValue("@InterestRate", interestRate);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    cmd.ExecuteNonQuery();
                    LoadDepositsData();

                    // Очистка полей
                    textBox1.Clear();
                    comboBox1.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении депозита: " + ex.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
