using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TourFirm
{
    public partial class TouristsForm : Form
    {
        private string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=Tourfirm";

        public TouristsForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                DataTable schema = connection.GetSchema("Tables");

                foreach (DataRow row in schema.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    comboBoxTables.Items.Add(tableName);
                }
                comboBoxTables.SelectedIndex = 0;
            }
        }


        private void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = comboBoxTables.SelectedItem.ToString();
            
            LoadFields(selectedTable);
        }

        private void updateData(string tableName, NpgsqlConnection connection)
        {
            DataTable dt1 = new DataTable();
            NpgsqlDataAdapter adap1 = new NpgsqlDataAdapter("SELECT * FROM " + tableName, connection);
            adap1.Fill(dt1);
            dataGridView1.DataSource = dt1;
        }

        private void LoadFields(string tableName)
        {
            flowLayoutPanelFields.Controls.Clear();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                updateData(tableName, connection);

                string query = $"SELECT column_name FROM information_schema.columns WHERE table_name = '{tableName}'";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        while (reader.Read())
                        {
                            string columnName = reader.GetString(0);

                            Label label = new Label();
                            label.Text = columnName + ":";
                            label.AutoSize = true;

                            TextBox textBox = new TextBox();
                            textBox.Name = columnName;

                            flowLayoutPanelFields.Controls.Add(label);
                            flowLayoutPanelFields.Controls.Add(textBox);
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedTable = comboBoxTables.SelectedItem.ToString();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string columns = ""; // Строка для имен столбцов
                    string values = ""; // Строка для значений

                    // Генерация строк для столбцов и значений на основе введенных данных
                    foreach (Control control in flowLayoutPanelFields.Controls)
                    {
                        if (control is TextBox)
                        {
                            columns += ((TextBox)control).Name + ",";
                            values += "'" + ((TextBox)control).Text + "',";
                        }
                    }
                    columns = columns.TrimEnd(',');
                    values = values.TrimEnd(',');

                    // Формирование SQL-запроса на вставку данных в выбранную таблицу
                    string query = $"INSERT INTO {selectedTable} ({columns}) VALUES ({values});";
                    //Console.WriteLine(query);
                    // Выполнение запроса
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Data saved successfully!");
                    updateData(selectedTable, conn);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        // Далее можно добавить код для сохранения данных в базу данных
    }
}
