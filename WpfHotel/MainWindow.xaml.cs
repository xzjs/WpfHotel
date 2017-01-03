﻿using System;
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
using System.Windows.Threading;

namespace WpfHotel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer ShowTimer;

        public MainWindow()
        {
            InitializeComponent();

            MainPage.Content = new MainPage();

            ShowTimer = new System.Windows.Threading.DispatcherTimer();
            ShowTimer.Tick += new EventHandler(ShowCurrentTimer);
            ShowTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            ShowTimer.Start();
        }

        private void ShowCurrentTimer(object sender, EventArgs e)
        {
            TimeTextBlock.Text = DateTime.Now.ToString();
        }

        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;
            switch (tvi.Header.ToString())
            {
                case "预约办理":
                    MainPage.Content = new OrderPage();
                    break;
                case "入住办理":
                    Window w = new CheckWindow();
                    w.ShowDialog();
                    break;
                case "预约/入住办理":
                case "房态图":
                    MainPage.Content = new MainPage();
                    break;
                case "房态统计":
                    MainPage.Content = new RoomStatisticsPage();
                    break;
                case "订单中心":
                    MainPage.Content = new BillPage();
                    break;
                default:
                    break;
            }
            e.Handled = true;
            
        }
    }
}
