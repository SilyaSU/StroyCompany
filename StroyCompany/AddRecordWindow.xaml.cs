using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Data.SqlClient;

namespace StroyCompany
{
    public partial class AddRecordWindow : Window
    {
        private string tableName;
        private List<string> columns;

        public AddRecordWindow(string tableName, List<string> columns)
        {
            InitializeComponent();
            this.tableName = tableName;
            this.columns = columns;

            foreach (var column in columns)
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
                sp.Children.Add(new TextBlock { Text = column, Width = 120 });
                sp.Children.Add(new TextBox { Name = $"txt{column}", Width = 200 });
                inputStackPanel.Children.Add(sp);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (var column in columns)
            {
                TextBox textBox = (TextBox)inputStackPanel.FindName($"txt{column}");
                values[column] = textBox.Text;
            }

            using (var db = new DataBase())
            {
                db.openConnection();
                string columnsPart = string.Join(", ", values.Keys);
                string valuesPart = string.Join(", ", values.Values.Select(v => $"'{v}'"));
                string insertQuery = $"INSERT INTO {tableName} ({columnsPart}) VALUES ({valuesPart})";

                try
                {
                    db.ExecuteQuery(insertQuery);
                    DialogResult = true;
                    Close();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Ошибка при добавлении записи: " + ex.Message);
                }
                db.closeConnection();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
