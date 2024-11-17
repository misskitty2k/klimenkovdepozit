using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace klimenkovdepozit
{
    public partial class Form2 : Form
    {
        private string connectionString = "Server=localhost;Database=dep;Uid=root;Pwd=a0#$f43JF@Ejk#_cwp[!323tFGed4%$%@z67cmrg-+356G^;";

        public Form2()
        {
            InitializeComponent();
            LoadUserData();
            this.FormClosing += Form2_FormClosing;
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Завершаем приложение, когда форма закрывается
            Application.Exit();
        }

        private void LoadUserData()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL-запрос для получения данных
                    string query = "SELECT UserID, FullName, PassportNumber, PhoneNumber, CreatedAt FROM Users";

                    // Создаем команду для выполнения запроса
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Создаем адаптер для заполнения данных
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Присваиваем источник данных для DataGridView
                            DataTable userTable = new DataTable();
                            userTable.Load(reader);
                            dataGridView1.DataSource = userTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL-запрос для вставки нового пользователя
                    string query = "INSERT INTO Users (FullName, PassportNumber, PhoneNumber, CreatedAt) " +
                                   "VALUES (@FullName, @PassportNumber, @PhoneNumber, @CreatedAt)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Добавляем параметры в запрос
                        command.Parameters.AddWithValue("@FullName", textBox1.Text);
                        command.Parameters.AddWithValue("@PassportNumber", textBox2.Text);
                        command.Parameters.AddWithValue("@PhoneNumber", textBox3.Text);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        // Выполняем команду
                        command.ExecuteNonQuery();
                    }
                }

                // Обновляем данные в DataGridView
                LoadUserData();

                // Очищаем текстовые поля
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();

                MessageBox.Show("Клиент успешно добавлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Проверяем, что выбрана хотя бы одна строка
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранных клиентов?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return; // Если пользователь не подтвердил удаление
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Выполняем удаление для каждой выбранной строки
                    foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                    {
                        int userId = Convert.ToInt32(row.Cells["UserID"].Value);

                        // Удаляем все записи из таблицы Contracts, которые ссылаются на депозиты этого клиента
                        string deleteContractsQuery = "DELETE FROM Contracts WHERE DepositID IN (SELECT DepositID FROM Deposits WHERE UserID = @UserID)";
                        using (MySqlCommand deleteContractsCommand = new MySqlCommand(deleteContractsQuery, connection))
                        {
                            deleteContractsCommand.Parameters.AddWithValue("@UserID", userId);
                            deleteContractsCommand.ExecuteNonQuery();
                        }

                        // Удаляем все транзакции, связанные с депозитами клиента
                        string deleteTransactionsQuery = "DELETE FROM Transactions WHERE DepositID IN (SELECT DepositID FROM Deposits WHERE UserID = @UserID)";
                        using (MySqlCommand deleteTransactionsCommand = new MySqlCommand(deleteTransactionsQuery, connection))
                        {
                            deleteTransactionsCommand.Parameters.AddWithValue("@UserID", userId);
                            deleteTransactionsCommand.ExecuteNonQuery();
                        }

                        // Удаляем все депозиты клиента
                        string deleteDepositsQuery = "DELETE FROM Deposits WHERE UserID = @UserID";
                        using (MySqlCommand deleteDepositsCommand = new MySqlCommand(deleteDepositsQuery, connection))
                        {
                            deleteDepositsCommand.Parameters.AddWithValue("@UserID", userId);
                            deleteDepositsCommand.ExecuteNonQuery();
                        }

                        // После этого удаляем самого клиента
                        string deleteUserQuery = "DELETE FROM Users WHERE UserID = @UserID";
                        using (MySqlCommand deleteUserCommand = new MySqlCommand(deleteUserQuery, connection))
                        {
                            deleteUserCommand.Parameters.AddWithValue("@UserID", userId);
                            deleteUserCommand.ExecuteNonQuery();
                        }
                    }
                }

                // Обновляем данные в DataGridView
                LoadUserData();

                MessageBox.Show("Клиенты и их депозиты успешно удалены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
