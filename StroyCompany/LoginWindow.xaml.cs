using System.Windows;

namespace StroyCompany
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordBox.Password;

            using (var db = new DataBase())
            {
                db.openConnection();
                string query = $"SELECT * FROM Люди WHERE Фамилия='{username}' AND Пароль='{password}'";
                var reader = db.ExecuteQuery(query);

                if (reader.Read())
                {
                    string role = reader["Роль"].ToString();
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password");
                }

                db.closeConnection();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
        }
    }
}
