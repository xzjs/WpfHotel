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
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls;

namespace WpfHotel
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWindow : MetroWindow
    {
        public MessageWindow(string message)
        {
            InitializeComponent();
            TextBlock.Text = message;
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += CloseWindow;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            dispatcherTimer.Start();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            Close();
        }
    }
}
