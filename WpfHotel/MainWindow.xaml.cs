using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfHotel
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var showTimer = new DispatcherTimer();
            showTimer.Tick += ShowCurrentTimer;
            showTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            showTimer.Start();

            using (var db=new hotelEntities())
            {
                List<Type> types = db.Type.ToList();
                types.Insert(0,new Type {Id = 0,Name = "全部"});
                ComboBox.ItemsSource = types;
            }

            LoadRoomData();
        }

        private void ShowCurrentTimer(object sender, EventArgs e)
        {
            TimeTextBlock.Text = DateTime.Today.ToShortDateString();
            DateTextBlock.Text = DateTime.Now.ToShortTimeString();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Login_CLick(object sender, RoutedEventArgs e)
        {
            var lw = new LoginWindow();
            lw.ShowDialog();
        }

        /// <summary>
        ///     加载房间列表
        /// </summary>
        public void LoadRoomData()
        {
            using (var db = new hotelEntities())
            {
                List<Room> rooms;
                if (ComboBox.SelectedIndex == 0)
                {
                    rooms = db.Room.Include(r => r.Order).ToList();
                }
                else
                {
                    var type = ComboBox.SelectedItem as Type;
                    rooms = db.Room.Include(r => r.Order).Where(r => r.TypeId == type.Id).ToList();
                }
                TotalTextBlock.Text = rooms.Count().ToString();
                CheckInTextBlock.Text = db.Room.Count(r => r.Status == 3 || r.Status == 7).ToString();
                var roomItems = rooms.Select(room => new RoomItem {Room = room}).ToList();
                foreach (var roomItem in roomItems)
                {
                    //判断预抵
                    var order = roomItem.Room.Order.Where(o=>o.Finish==0).FirstOrDefault(o => o.Status==1);
                    if (order == null) continue;
                    var inDateTime = order.InDate.Value.Date;
                    if (inDateTime == DateTime.Today && roomItem.Room.Status != 2)
                    {
                        roomItem.SetRoomStatus(2);
                        roomItem.Room = db.Room.Find(roomItem.Room.Id);
                    }
                    //判断预离
                    var leaveDateTime = order.LeaveDate.Value.Date;
                    if (leaveDateTime == DateTime.Today && roomItem.Room.Status != 7)
                    {
                        roomItem.SetRoomStatus(7);
                        roomItem.Room = db.Room.Find(roomItem.Room.Id);
                    }
                }
                RoomList.ItemsSource = roomItems;
            }
        }

        /// <summary>
        ///     关闭程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowMenu(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            var roomItem = button.Tag as RoomItem;
            button.ContextMenu = roomItem.ContextMenu;
        }

        private void CheckIn(object sender, RoutedEventArgs e)
        {
            var checkInWindow = new CheckInWindow();
            checkInWindow.ShowDialog();
        }

        /// <summary>
        ///     检查是否设置相关的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated(object sender, EventArgs e)
        {
            using (var db = new hotelEntities())
            {
                var config = db.Config.FirstOrDefault();
                var information = db.Information.FirstOrDefault();
                if (config == null || information == null)
                {
                    var login = new LoginWindow();
                    login.ShowDialog();
                }
                else
                {
                    ((App) Application.Current).Config = config;
                    ((App) Application.Current).Information = information;
                }
            }
        }

        /// <summary>
        ///     预约
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Appointment(object sender, RoutedEventArgs e)
        {
            var appointmentWindow = new AppointmentWindow();
            appointmentWindow.ShowDialog();
        }

        /// <summary>
        ///     显示订单中心
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowOrderWindow(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderWindow();
            orderWindow.ShowDialog();
        }

        /// <summary>
        ///     显示实时房态报表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowRoomStatusWindow(object sender, RoutedEventArgs e)
        {
            var roomStatusWindow = new RoomStatusWindow();
            roomStatusWindow.ShowDialog();
        }

        private void ShowRealTimeWindow(object sender, RoutedEventArgs e)
        {
            var realTimeWindow = new RealTimeWindow();
            realTimeWindow.ShowDialog();
        }

        private void ShowMoneyWindow(object sender, RoutedEventArgs e)
        {
            var moneyReportWindow = new MoneyReportWindow();
            moneyReportWindow.ShowDialog();
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var button = sender as Button;
            var roomItem = button.Tag as RoomItem;
            roomItem.DoubleClick();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadRoomData();
        }
    }
}