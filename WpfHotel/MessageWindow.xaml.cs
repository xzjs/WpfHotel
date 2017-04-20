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
        private readonly MainWindow _mainWindow;
        public MessageWindow(string message, MainWindow mainWindow, string title="通知")
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            TextBlock.Text = message;
            Title = title;
            //var dispatcherTimer = new DispatcherTimer();
            //dispatcherTimer.Tick += CloseWindow;
            //dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            //dispatcherTimer.Start();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            Close();
        }

        private void MessageWindow_OnClosed(object sender, EventArgs e)
        {
            _mainWindow.MessageWindows.Remove(this);
        }
    }
}
