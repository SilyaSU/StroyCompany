using System.Data;
using System.Windows;

namespace StroyCompany
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadObjects();
        }

        private void LoadObjects()
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
                objectsDataGrid.ItemsSource = dataTable.DefaultView;
                db.closeConnection();
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddObjectWindow addObjectWindow = new AddObjectWindow();
            addObjectWindow.ShowDialog();
            LoadObjects();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)objectsDataGrid.SelectedItem;
            if (selectedRow != null)
            {
                EditObjectWindow editObjectWindow = new EditObjectWindow(selectedRow);
                editObjectWindow.ShowDialog();
                LoadObjects();
            }
            else
            {
                MessageBox.Show("Please select an object to edit.");
            }
        }


    }
}
