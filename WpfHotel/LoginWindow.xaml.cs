using MahApps.Metro.Controls;

namespace WpfHotel
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            LoginPage lg = new LoginPage();
            lg.ParentWindow = this;
            PageFrame.Content = lg;
        }
    }
}
