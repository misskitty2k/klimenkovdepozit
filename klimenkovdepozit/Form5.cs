using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace klimenkovdepozit
{
    public partial class Form5 : Form
    {
        // Строка подключения к базе данных
        private string connectionString = "Server=localhost;Database=dep;Uid=root;Pwd=a0#$f43JF@Ejk#_cwp[!323tFGed4%$%@z67cmrg-+356G^;";

        public Form5()
        {
            InitializeComponent();
            this.FormClosing += Form4_FormClosing;
            InitializeDataGridView();  // Инициализируем столбцы DataGridView
            LoadClients();
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        // Структура данных клиента
        public class Client
        {
            public string FullName { get; set; }
            public string PassportNumber { get; set; }
            public string PhoneNumber { get; set; }
            public List<Deposit> Deposits { get; set; }

            // Группировка вкладов по клиенту
            public Client()
            {
                Deposits = new List<Deposit>();
            }

            // Рассчитываем итоговую сумму по каждому вкладу
            public decimal CalculateFinalAmount(decimal depositAmount, decimal interestRate, DateTime startDate, DateTime endDate)
            {
                int years = (endDate.Year - startDate.Year);
                return depositAmount * (1 + (interestRate / 100) * years);
            }
        }

        // Структура данных вклада
        public class Deposit
        {
            public decimal DepositAmount { get; set; }
            public decimal InterestRate { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal FinalAmount { get; set; }

            public Deposit(decimal depositAmount, decimal interestRate, DateTime startDate, DateTime endDate)
            {
                DepositAmount = depositAmount;
                InterestRate = interestRate;
                StartDate = startDate;
                EndDate = endDate;
                FinalAmount = DepositAmount * (1 + (InterestRate / 100) * (EndDate.Year - StartDate.Year));
            }
        }

        // Инициализация столбцов DataGridView
        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Clear(); // Очищаем любые существующие столбцы

            dataGridView1.Columns.Add("FullName", "ФИО");
            dataGridView1.Columns.Add("PassportNumber", "Номер паспорта");
            dataGridView1.Columns.Add("PhoneNumber", "Номер телефона");
            dataGridView1.Columns.Add("DepositAmount", "Сумма вклада");
            dataGridView1.Columns.Add("InterestRate", "Процентная ставка");
            dataGridView1.Columns.Add("StartDate", "Дата открытия");
            dataGridView1.Columns.Add("EndDate", "Дата закрытия");
            dataGridView1.Columns.Add("FinalAmount", "Конечная сумма");
        }

        // Загрузка данных клиентов из базы данных
        private void LoadClients()
        {
            List<Client> clients = new List<Client>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT u.FullName, u.PassportNumber, u.PhoneNumber, d.DepositAmount, d.InterestRate, d.StartDate, d.EndDate
                        FROM Users u
                        JOIN Deposits d ON u.UserID = d.UserID";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string fullName = reader.GetString("FullName");
                        string passportNumber = reader.GetString("PassportNumber");
                        string phoneNumber = reader.GetString("PhoneNumber");
                        decimal depositAmount = reader.GetDecimal("DepositAmount");
                        decimal interestRate = reader.GetDecimal("InterestRate");
                        DateTime startDate = reader.GetDateTime("StartDate");
                        DateTime endDate = reader.GetDateTime("EndDate");

                        Client client = clients.FirstOrDefault(c => c.FullName == fullName);
                        if (client == null)
                        {
                            client = new Client
                            {
                                FullName = fullName,
                                PassportNumber = passportNumber,
                                PhoneNumber = phoneNumber
                            };
                            clients.Add(client);
                        }

                        client.Deposits.Add(new Deposit(depositAmount, interestRate, startDate, endDate));
                    }

                    // Подключаем список клиентов в ComboBox
                    comboBoxClients.DataSource = clients;
                    comboBoxClients.DisplayMember = "FullName"; // Отображаем ФИО
                    comboBoxClients.ValueMember = "FullName";   // Значение тоже ФИО
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }

        // Обработка выбора клиента в ComboBox
        private void comboBoxClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            Client selectedClient = comboBoxClients.SelectedItem as Client;

            if (selectedClient != null)
            {
                // Заполняем DataGridView данными клиента
                dataGridView1.Rows.Clear();

                foreach (var deposit in selectedClient.Deposits)
                {
                    // В DataGridView будем записывать все вклады для выбранного клиента
                    dataGridView1.Rows.Add(
                        selectedClient.FullName,
                        selectedClient.PassportNumber,
                        selectedClient.PhoneNumber,
                        deposit.DepositAmount,
                        deposit.InterestRate,
                        deposit.StartDate.ToShortDateString(),
                        deposit.EndDate.ToShortDateString(),
                        deposit.FinalAmount
                    );
                }
            }
        }

        // Сохранение отчета в файл
        // Сохранение отчета в файл в формате CSV
        private void SaveReportToHtml(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Начинаем HTML-документ
                writer.WriteLine("<html>");
                writer.WriteLine("<head><style>table {border-collapse: collapse; width: 100%;} th, td {border: 1px solid black; padding: 8px; text-align: left;}</style></head>");
                writer.WriteLine("<body>");

                // Добавляем заголовок таблицы
                writer.WriteLine("<table>");
                writer.WriteLine("<tr>");
                writer.WriteLine("<th>ФИО</th>");
                writer.WriteLine("<th>Номер паспорта</th>");
                writer.WriteLine("<th>Номер телефона</th>");
                writer.WriteLine("<th>Сумма вклада</th>");
                writer.WriteLine("<th>Процентная ставка</th>");
                writer.WriteLine("<th>Дата открытия</th>");
                writer.WriteLine("<th>Дата закрытия</th>");
                writer.WriteLine("<th>Конечная сумма</th>");
                writer.WriteLine("</tr>");

                // Заполняем таблицу данными
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        writer.WriteLine("<tr>");
                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            // Для каждого значения ячейки пишем его в таблицу
                            writer.WriteLine($"<td>{row.Cells[i].Value}</td>");
                        }
                        writer.WriteLine("</tr>");
                    }
                }

                // Закрываем таблицу и HTML
                writer.WriteLine("</table>");
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }

            MessageBox.Show("Отчет сохранен в формате HTML!");
        }



        // Кнопка для выбора директории и сохранения отчета
        private void btnSaveReport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "HTML Files (*.html)|*.html"; // Фильтр для HTML файлов
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveReportToHtml(saveFileDialog.FileName); // Вызываем метод сохранения в HTML
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
