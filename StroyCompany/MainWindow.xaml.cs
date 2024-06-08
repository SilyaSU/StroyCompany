using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace StroyCompany
{
    public partial class MainWindow : Window
    {
        private DataBase database;
        private int currentUserId;

        public MainWindow(int userId)
        {
            InitializeComponent();
            database = new DataBase();
            currentUserId = userId;
            LoadAllObjects();
            LoadUserObjects(currentUserId);
            LoadTablesList();
        }

        private void LoadAllObjects()
        {
            using (var db = new DataBase())
            {
                db.openConnection();
                string query = @"
                SELECT 
                    Объект.Название AS Название,
                    Объект.Адрес AS Адрес,
                    Объект.Тип AS Тип,
                    Рабочая_группа.Название AS Рабочая_группа,
                    Объект.Стоимость AS Стоимость
                FROM Объект
                LEFT JOIN Рабочая_группа ON Объект.FK_Рабочая_группа = Рабочая_группа.id_Рабочая_группа";
                var reader = db.ExecuteQuery(query);
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                allObjectsDataGrid.ItemsSource = dataTable.DefaultView;
                db.closeConnection();
            }
        }

        private void LoadUserObjects(int userId)
        {
            using (var db = new DataBase())
            {
                db.openConnection();
                string query = @"
            SELECT 
                Объект.Название AS Название,
                Объект.Адрес AS Адрес,
                Объект.Тип AS Тип,
                Рабочая_группа.Название AS Рабочая_группа,
                Объект.Стоимость AS Стоимость
            FROM Объект
            LEFT JOIN Рабочая_группа ON Объект.FK_Рабочая_группа = Рабочая_группа.id_Рабочая_группа
            LEFT JOIN Право_собственности ON Объект.id_Объект = Право_собственности.FK_Объект
            WHERE Право_собственности.FK_Клиент = @userId";
                var cmd = new SqlCommand(query, db.sqlConnection);
                cmd.Parameters.AddWithValue("@userId", userId);
                var reader = cmd.ExecuteReader();
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                userObjectsDataGrid.ItemsSource = dataTable.DefaultView;
                db.closeConnection();
            }
        }

        private void LoadTablesList()
        {
            using (var db = new DataBase())
            {
                db.openConnection();
                var tables = db.GetTables();
                tablesComboBox.ItemsSource = tables;
                db.closeConnection();
            }
        }

        private void TablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tablesComboBox.SelectedItem != null)
            {
                string selectedTable = tablesComboBox.SelectedItem.ToString();
                LoadSelectedTable(selectedTable);
            }
        }

        private void LoadSelectedTable(string tableName)
        {
            using (var db = new DataBase())
            {
                db.openConnection();
                string query = $"SELECT * FROM {tableName}";
                var reader = db.ExecuteQuery(query);
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                selectedTableDataGrid.ItemsSource = dataTable.DefaultView;
                db.closeConnection();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddObjectWindow addObjectWindow = new AddObjectWindow();
            addObjectWindow.ShowDialog();
            LoadAllObjects();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (allObjectsDataGrid.SelectedItem is DataRowView selectedRow)
            {
                EditObjectWindow editObjectWindow = new EditObjectWindow(selectedRow);
                editObjectWindow.ShowDialog();
                LoadAllObjects();
            }
            else
            {
                MessageBox.Show("Please select an object to edit.");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (allObjectsDataGrid.SelectedItem is DataRowView selectedRow)
            {
                string name = selectedRow["Название"].ToString();

                MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить объект \"{name}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    string deleteQuery = "DELETE FROM Объект WHERE Название = @name";

                    database.openConnection();
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, database.sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@name", name);

                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show("Ошибка при удалении объекта: " + ex.Message);
                        }
                    }
                    database.closeConnection();
                    LoadAllObjects();
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите объект для удаления.");
            }
        }

        private void AddTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (tablesComboBox.SelectedItem != null)
            {
                string selectedTable = tablesComboBox.SelectedItem.ToString();
                var columns = GetTableColumns(selectedTable);
                AddRecordWindow addRecordWindow = new AddRecordWindow(selectedTable, columns);
                if (addRecordWindow.ShowDialog() == true)
                {
                    LoadSelectedTable(selectedTable);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите таблицу.");
            }
        }

        private void DeleteTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTableDataGrid.SelectedItem is DataRowView selectedRow)
            {
                string selectedTable = tablesComboBox.SelectedItem.ToString();
                string primaryKeyColumn = selectedRow.Row.Table.Columns[0].ColumnName;
                string primaryKeyValue = selectedRow[primaryKeyColumn].ToString();

                MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить запись с ключом \"{primaryKeyValue}\"?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    string deleteQuery = $"DELETE FROM {selectedTable} WHERE {primaryKeyColumn} = @primaryKeyValue";

                    using (var db = new DataBase())
                    {
                        db.openConnection();
                        using (SqlCommand cmd = new SqlCommand(deleteQuery, db.sqlConnection))
                        {
                            cmd.Parameters.AddWithValue("@primaryKeyValue", primaryKeyValue);

                            try
                            {
                                cmd.ExecuteNonQuery();
                            }
                            catch (SqlException ex)
                            {
                                MessageBox.Show("Ошибка при удалении записи: " + ex.Message);
                            }
                        }
                        db.closeConnection();
                    }
                    LoadSelectedTable(selectedTable);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.");
            }
        }

        private List<string> GetTableColumns(string tableName)
        {
            List<string> columns = new List<string>();

            using (var db = new DataBase())
            {
                db.openConnection();
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
                var reader = db.ExecuteQuery(query);

                while (reader.Read())
                {
                    columns.Add(reader["COLUMN_NAME"].ToString());
                }
                db.closeConnection();
            }

            return columns;
        }

    }


}
