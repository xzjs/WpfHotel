using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
    /// SetUpPage.xaml 的交互逻辑
    /// </summary>
    public partial class SetUpPage : Page
    {
        private Config _config;
        public SetUpPage()
        {
            InitializeComponent();
            using (var db =new hotelEntities())
            {
                _config = db.Config.FirstOrDefault() ?? new Config();
                ConfigStackPanel.DataContext = _config;
            }
        }

        public LoginWindow ParentWindow { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var db=new hotelEntities())
            {
                if (_config.Id == 0)
                {
                    db.Config.Add(_config);
                }
                else
                {
                    db.Entry(_config).State=EntityState.Modified;
                }
                db.SaveChanges();
            }
            LoginPage lp = new LoginPage {ParentWindow = ParentWindow};
            ParentWindow.PageFrame.Content = lp;
        }
    }
}
