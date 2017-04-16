using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfHotel
{
    /// <summary>
    /// LogoutPage.xaml 的交互逻辑
    /// </summary>
    public partial class LogoutPage : Page
    {
        private readonly LoginWindow _loginWindow;
        public LogoutPage(LoginWindow loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _loginWindow.PageFrame.Content = new SetUpPage(_loginWindow);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("注销会清空本地所有数据，是否注销", "注销", MessageBoxButton.OKCancel) != MessageBoxResult.OK) return;
                using (var db = new hotelEntities())
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM [Queue]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Account]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Invoice]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [User]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Order]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Room]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Type]");
                    db.Database.ExecuteSqlCommand("DELETE FROM [Information]");
                    _loginWindow.PageFrame.Content = new LoginPage(_loginWindow);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Enter(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            _loginWindow.Close();
        }
    }
}
