using System;
using System.Data;
using System.Windows;

namespace StroyCompany
{
    public partial class EditObjectWindow : Window
    {
        private DataRowView selectedRow;
        private DataBase database;

        public EditObjectWindow(DataRowView row)
        {
            InitializeComponent();
            selectedRow = row;
            PopulateFields();
            database = new DataBase();
        }

        private void PopulateFields()
        {
            nameTextBox.Text = selectedRow["Название"].ToString();
            addressTextBox.Text = selectedRow["Адрес"].ToString();
            typeTextBox.Text = selectedRow["Тип"].ToString();
            groupNameTextBox.Text = selectedRow["Рабочая_группа"].ToString();
            costTextBox.Text = selectedRow["Стоимость"].ToString();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTextBox.Text;
            string address = addressTextBox.Text;
            string type = typeTextBox.Text;
            string groupName = groupNameTextBox.Text;
            string cost = costTextBox.Text;

            string updateQuery = $"UPDATE Объект SET Название = '{name}', Адрес = '{address}', Тип = '{type}', Стоимость = {cost}, FK_Рабочая_группа = (SELECT id_Рабочая_группа FROM Рабочая_группа WHERE Название = '{groupName}'";

            database.openConnection();
            database.ExecuteQuery(updateQuery);
            database.closeConnection();

            MessageBox.Show("Object updated successfully!");
            this.Close();
        }
    }
}
