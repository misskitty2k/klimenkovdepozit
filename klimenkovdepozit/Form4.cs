using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace klimenkovdepozit
{
    public partial class Form4 : Form
    {
        private decimal currentBalance = 0;
        private string connectionString = "Server=localhost;Database=dep;Uid=root;Pwd=a0#$f43JF@Ejk#_cwp[!323tFGed4%$%@z67cmrg-+356G^;"; // Заменили строку подключения

        public Form4()
        {
            InitializeComponent();
            LoadBalance();  // Загружаем текущий баланс
            LoadClients();  // Загружаем список клиентов в ComboBox

            this.FormClosing += Form4_FormClosing;

            // Добавление столбцов в DataGridView для возвратов
            dataGridViewReturns.Columns.Add("DepositID", "ID Депозита");
            dataGridViewReturns.Columns.Add("DepositAmount", "Сумма возврата");
            dataGridViewReturns.Columns.Add("InterestAmount", "Сумма с процентами");
            dataGridViewReturns.Columns.Add("EndDate", "Дата окончания");

            // Добавление столбцов в DataGridView для пополнений
            dataGridViewDeposits.Columns.Add("DepositID", "ID Депозита");
            dataGridViewDeposits.Columns.Add("DepositAmount", "Сумма пополнения");
            dataGridViewDeposits.Columns.Add("StartDate", "Дата начала");
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Завершаем приложение, когда форма закрывается
            Application.Exit();
        }

        // Загрузка текущего баланса
        private void LoadBalance()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT SUM(DepositAmount) FROM Deposits";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                object result = cmd.ExecuteScalar();
                currentBalance = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                label1.Text = $"Текущий баланс: {currentBalance} бун";
            }
        }

        // Загрузка списка клиентов в ComboBox
        private void LoadClients()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT FullName FROM Users";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string clientName = reader.GetString("FullName");
                        comboBoxClients.Items.Add(clientName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке списка клиентов: " + ex.Message);
                }
            }
        }

        // Метод для загрузки депозитов
        private void LoadDeposits_Click(object sender, EventArgs e)
        {
            string selectedClient = comboBoxClients.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedClient))
            {
                MessageBox.Show("Выберите клиента.");
                return;
            }

            // Очищаем DataGridView перед добавлением новых данных
            dataGridViewDeposits.Rows.Clear();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Получаем UserID клиента по его имени
                    string userQuery = "SELECT UserID FROM Users WHERE FullName = @FullName";
                    MySqlCommand userCmd = new MySqlCommand(userQuery, connection);
                    userCmd.Parameters.AddWithValue("@FullName", selectedClient);
                    object userIdObj = userCmd.ExecuteScalar();

                    if (userIdObj == null)
                    {
                        MessageBox.Show("Клиент не найден.");
                        return;
                    }

                    int userId = Convert.ToInt32(userIdObj);

                    // Запрос для пополнений
                    string depositQuery = @"
                        SELECT DepositID, DepositAmount, StartDate
                        FROM Deposits
                        WHERE UserID = @UserID";
                    MySqlCommand depositCmd = new MySqlCommand(depositQuery, connection);
                    depositCmd.Parameters.AddWithValue("@UserID", userId);

                    // Чтение данных по пополнениям
                    MySqlDataReader depositReader = depositCmd.ExecuteReader();
                    while (depositReader.Read())
                    {
                        int depositId = depositReader.GetInt32("DepositID");
                        decimal depositAmount = depositReader.GetDecimal("DepositAmount");
                        DateTime startDate = depositReader.GetDateTime("StartDate");

                        // Добавляем данные о пополнении в DataGridView
                        dataGridViewDeposits.Rows.Add(depositId, depositAmount.ToString("0.00"), startDate.ToShortDateString());
                    }
                    depositReader.Close(); // Закрываем DataReader
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке депозитов: " + ex.Message);
                }
            }
        }

        // Метод для загрузки возвратов
        private void LoadReturns_Click(object sender, EventArgs e)
        {
            string selectedClient = comboBoxClients.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedClient))
            {
                MessageBox.Show("Выберите клиента.");
                return;
            }

            // Очищаем DataGridView перед добавлением новых данных
            dataGridViewReturns.Rows.Clear();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Получаем UserID клиента по его имени
                    string userQuery = "SELECT UserID FROM Users WHERE FullName = @FullName";
                    MySqlCommand userCmd = new MySqlCommand(userQuery, connection);
                    userCmd.Parameters.AddWithValue("@FullName", selectedClient);
                    object userIdObj = userCmd.ExecuteScalar();

                    if (userIdObj == null)
                    {
                        MessageBox.Show("Клиент не найден.");
                        return;
                    }

                    int userId = Convert.ToInt32(userIdObj);

                    // Запрос для получения депозитов клиента
                    string depositQuery = @"
                SELECT DepositID, DepositAmount, InterestRate, StartDate, EndDate
                FROM Deposits
                WHERE UserID = @UserID";
                    MySqlCommand depositCmd = new MySqlCommand(depositQuery, connection);
                    depositCmd.Parameters.AddWithValue("@UserID", userId);

                    // Чтение данных по депозитам
                    MySqlDataReader depositReader = depositCmd.ExecuteReader();
                    while (depositReader.Read())
                    {
                        int depositId = depositReader.GetInt32("DepositID");
                        decimal depositAmount = depositReader.GetDecimal("DepositAmount");
                        decimal interestRate = depositReader.GetDecimal("InterestRate");
                        DateTime startDate = depositReader.GetDateTime("StartDate");
                        DateTime endDate = depositReader.GetDateTime("EndDate");

                        // Вычисляем период в годах между StartDate и EndDate
                        double years = (endDate - startDate).TotalDays / 365.25;

                        // Вычисляем сумму с процентами
                        decimal interestAmount = depositAmount * (interestRate / 100) * (decimal)years;
                        decimal finalAmount = depositAmount + interestAmount;

                        // Добавляем данные о расчете в DataGridView
                        dataGridViewReturns.Rows.Add(depositId, depositAmount.ToString("0.00"), finalAmount.ToString("0.00"), endDate.ToShortDateString());

                        // Логирование данных для отладки
                        Console.WriteLine($"DepositID: {depositId}, DepositAmount: {depositAmount}, InterestAmount: {interestAmount}, FinalAmount: {finalAmount}, EndDate: {endDate}");
                    }

                    depositReader.Close(); // Закрываем DataReader
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных для возврата: " + ex.Message);
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
