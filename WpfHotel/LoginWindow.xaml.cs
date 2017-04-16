using System.Linq;
using MahApps.Metro.Controls;

namespace WpfHotel
{
    /// <summary>
    ///     LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        public Information Information;
        public LoginWindow()
        {
            InitializeComponent();

            using (var db = new hotelEntities())
            {
                Information = db.Information.FirstOrDefault();
                if (Information != null)
                {
                    PageFrame.Content = new LogoutPage(this);
                }
                else
                {
                    PageFrame.Content = new LoginPage(this);
                }
            }
        }
    }
}