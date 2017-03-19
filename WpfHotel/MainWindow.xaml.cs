using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var showTimer = new System.Windows.Threading.DispatcherTimer();
            showTimer.Tick += new EventHandler(ShowCurrentTimer);
            showTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            showTimer.Start();

            LoadRoomData();
        }

        private void ShowCurrentTimer(object sender, EventArgs e)
        {
            //TimeTextBlock.Text = DateTime.Now.ToString();
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
            LoginWindow lw = new LoginWindow();
            lw.ShowDialog();
        }

        /// <summary>
        /// 加载房间列表
        /// </summary>
        public void LoadRoomData()
        {
            using (var db = new hotelEntities())
            {
                List<Room> rooms = db.Room.Include(r => r.Order).ToList();
                List<RoomItem> roomItems = rooms.Select(room => new RoomItem { Room = room }).ToList();
                foreach (var roomItem in roomItems)
                {
                    //判断预抵
                    Order order = roomItem.Room.Order.FirstOrDefault(o => o.Finish == 0);
                    if (order == null) continue;
                    DateTime inDateTime = order.InDate.Value.Date;
                    if (inDateTime == DateTime.Today && roomItem.Room.Status != 2)
                    {
                        roomItem.SetRoomStatus(2);
                        roomItem.Room = db.Room.Find(roomItem.Room.Id);
                    }
                    //判断预离
                    DateTime leaveDateTime = order.LeaveDate.Value.Date;
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
        /// 关闭程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowMenu(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            RoomItem roomItem = button.Tag as RoomItem;
            button.ContextMenu = roomItem.ContextMenu;
        }

        private void CheckIn(object sender, RoutedEventArgs e)
        {
            CheckInWindow checkInWindow = new CheckInWindow();
            checkInWindow.ShowDialog();
        }

        /// <summary>
        /// 检查是否设置相关的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated(object sender, EventArgs e)
        {
            using (var db = new hotelEntities())
            {
                Config config = db.Config.FirstOrDefault();
                Information information = db.Information.FirstOrDefault();
                if (config == null || information == null)
                {
                    LoginWindow login = new LoginWindow();
                    login.ShowDialog();
                }
                else
                {
                    ((App)Application.Current).Config = config;
                    ((App)Application.Current).Information = information;
                }
            }
        }

        /// <summary>
        /// 预约
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Appointment(object sender, RoutedEventArgs e)
        {
            AppointmentWindow appointmentWindow = new AppointmentWindow();
            appointmentWindow.ShowDialog();
        }

        /// <summary>
        /// 显示订单中心
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowOrderWindow(object sender, RoutedEventArgs e)
        {
            OrderWindow orderWindow = new OrderWindow();
            orderWindow.ShowDialog();
        }

        /// <summary>
        /// 显示实时房态报表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowRoomStatusWindow(object sender, RoutedEventArgs e)
        {
            RoomStatusWindow roomStatusWindow = new RoomStatusWindow();
            roomStatusWindow.ShowDialog();
        }

        private void ShowRealTimeWindow(object sender, RoutedEventArgs e)
        {
            RealTimeWindow realTimeWindow = new RealTimeWindow();
            realTimeWindow.ShowDialog();
        }

        private void ShowMoneyWindow(object sender, RoutedEventArgs e)
        {
            MoneyReportWindow moneyReportWindow = new MoneyReportWindow();
            moneyReportWindow.ShowDialog();
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button button=sender as Button;
            RoomItem roomItem=button.Tag as RoomItem;
            roomItem.DoubleClick();
        }
    }
}
