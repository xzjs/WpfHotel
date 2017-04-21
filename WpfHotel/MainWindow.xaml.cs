using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfHotel
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public Config Config;
        public Information Information;
        public ObservableCollection<MessageWindow> MessageWindows;

        private readonly ObservableCollection<Type> _types;
        private string _str = "";
        private IConnection _connection;

        public MainWindow()
        {
            InitializeComponent();

            MyApp.RoomItems = new ObservableCollection<RoomItem>();
            RoomList.ItemsSource = MyApp.RoomItems;
            //开启时间线程
            var showTimer = new DispatcherTimer();
            showTimer.Tick += ShowCurrentTimer;
            showTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            showTimer.Start();

            //开启队列线程
            var uploadQueue = new DispatcherTimer();
            uploadQueue.Tick += UploadQueue;
            uploadQueue.Interval = new TimeSpan(1, 0, 0);
            uploadQueue.Start();

            _types = new ObservableCollection<Type>();
            LoadThemeData();
            ThemeComboBox.ItemsSource = _types;

            MessageWindows = new ObservableCollection<MessageWindow>();
            MessageWindows.CollectionChanged += ArrageWindow;

            using (var db = new hotelEntities())
            {
                Config = db.Config.FirstOrDefault();
                Information = db.Information.FirstOrDefault();
                MyApp.Config = Config;
                MyApp.Information = Information;
            }
            ListenOrderTcp();
        }

        /// <summary>
        /// 排列显示消息窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrageWindow(object sender, NotifyCollectionChangedEventArgs e)
        {
            double height = SystemParameters.WorkArea.Height;
            double width = SystemParameters.WorkArea.Width;
            int max = Convert.ToInt32(Math.Floor(height / 200));

            for (int i = 0; i < MessageWindows.Count; i++)
            {
                MessageWindows[i].Top = height - 200 * (i + 1);
                int n = i / max;
                int m = i % max;
                MessageWindows[i].Left = width - 300 * (m + 1);
            }
        }

        /// <summary>
        /// 加载主题
        /// </summary>
        public void LoadThemeData()
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    List<Type> types = db.Type.ToList();
                    types.Insert(0, new Type { Id = 0, Name = "全部" });
                    ThemeComboBox.SelectedIndex = -1;
                    _types.Clear();
                    foreach (var type in types)
                    {
                        _types.Add(type);
                    }
                    ThemeComboBox.SelectedIndex = 0;
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

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
                switch (ThemeComboBox.SelectedIndex)
                {
                    case -1:
                        return;
                    case 0:
                        rooms = db.Room.Include(r => r.Order).ToList();
                        break;
                    default:
                        var type = ThemeComboBox.SelectedItem as Type;
                        rooms = db.Room.Include(r => r.Order).Where(r => r.TypeId == type.Id).ToList();
                        break;
                }
                TotalTextBlock.Text = rooms.Count().ToString();
                CheckInTextBlock.Text = db.Room.Count(r => r.Status == 3 || r.Status == 7).ToString();

                foreach (RoomItem roomItem in MyApp.RoomItems)
                {
                    //判断预抵
                    var order = roomItem.Room.Order.Where(o => o.Finish == 0).FirstOrDefault(o => o.Status == 1);
                    if (order != null)
                    {
                        var inDateTime = order.InDate.Value.Date;
                        if (inDateTime == DateTime.Today && roomItem.Room.Status != 2)
                        {
                            roomItem.SetRoomStatus(2);
                        }
                    }
                    //判断预离
                    order = roomItem.Room.Order.Where(o => o.Finish == 0).FirstOrDefault(o => o.Status == 2);
                    // ReSharper disable once InvertIf
                    if (order != null)
                    {
                        var leaveDateTime = order.LeaveDate.Value.Date;
                        if (leaveDateTime == DateTime.Today && roomItem.Room.Status != 7)
                        {
                            roomItem.SetRoomStatus(7);
                        }
                    }
                }
                MyApp.ReloadRoomItems();
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
            switch (ThemeComboBox.SelectedIndex)
            {
                case -1:
                    return;
                case 0:
                    MyApp.TypeId = 0;
                    break;
                default:
                    var type = ThemeComboBox.SelectedItem as Type;
                    MyApp.TypeId = type.Id;
                    break;
            }
            LoadRoomData();
        }

        /// <summary>
        ///     通过tcp获取数据
        /// </summary>
        private void ListenOrderTcp()
        {
            try
            {
                IConnectionFactory factory = new ConnectionFactory("tcp://" + Config.Tcp + ":" + Config.Port);
                _connection = factory.CreateConnection();
                _connection.ClientId = Information.HotelId.ToString();
                _connection.Start();

                var session = _connection.CreateSession();
                var consumer = session.CreateConsumer(new ActiveMQQueue("hotelOrder" + Information.HotelId));
                consumer.Listener += consumer_Listener;

                var session1 = _connection.CreateSession();
                var consumer1 = session1.CreateConsumer(new ActiveMQQueue("hotelModifyModify" + Information.HotelId));
                consumer1.Listener += Modify;

                var cancelConsumer =
                    session.CreateConsumer(new ActiveMQQueue("hotelOrderCancel" + MyApp.Information.HotelId));
                cancelConsumer.Listener += OrderCancel;

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// 取消订单接口
        /// </summary>
        /// <param name="message"></param>
        private void OrderCancel(IMessage message)
        {
            try
            {
                var msg = (ITextMessage)message;
                Console.WriteLine(msg.Text);
                var jo = JArray.Parse(msg.Text);
                foreach (var item in jo)
                {
                    long orderId = (long)item["orderId"];
                    int status = (int)item["status"];
                    using (var db = new hotelEntities())
                    {
                        Order order = db.Order.FirstOrDefault(o => o.ServerId == orderId);
                        if (order == null)
                            continue;
                        if (status != 4) continue;
                        
                        if (order.InDate.Value.Date == DateTime.Today)
                        {
                            if (order.Room.Status.Value == 2)
                            {
                                RoomItem roomItem = new RoomItem { Room = order.Room };
                                roomItem.SetRoomStatus(1);
                            }
                        }
                        _str = "房间" + order.Room.No + "订单取消";
                        db.Order.Remove(order);
                        db.SaveChanges();
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(RefreshRoomData));
                    }
                }
            }
            catch (Exception exception)
            {
                _str = exception.Message;
            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ShowMessageWindow));
            }
        }

        private void Modify(IMessage message)
        {
            try
            {
                var msg = (ITextMessage)message;
                Console.WriteLine(msg.Text);
                using (var db = new hotelEntities())
                {
                    var jo = JArray.Parse(msg.Text);
                    foreach (var item in jo)
                    {
                        int modifyType = (int)item["modifyType"];
                        if ((int)item["modifyItem"] == 0) //修改房间
                        {
                            if (modifyType == 0) //添加
                            {
                                var room = new Room
                                {
                                    ServerId = (long)item["roomInfo"]["id"],

                                    Price = (decimal)item["roomInfo"]["price"],
                                    Details = (string)item["roomInfo"]["roomDetails"],
                                    No = (int)item["roomInfo"]["roomNum"],

                                    Status = 1
                                };
                                long typeId = (long)item["roomInfo"]["roomThemeId"];
                                Type type = db.Type.First(t => t.ServerId == typeId);
                                room.TypeId = type.Id;
                                db.Room.Add(room);
                            }
                            else if (modifyType == 1) //删除
                            {
                                long roomId = (long)item["roomInfo"]["id"];
                                Room room = db.Room.FirstOrDefault(t => t.ServerId == roomId);
                                if (room != null)
                                {
                                    db.Room.Remove(room);
                                }
                            }
                            else //修改
                            {
                                long roomId = (long)item["roomInfo"]["id"];
                                Room room = db.Room.FirstOrDefault(t => t.ServerId == roomId);
                                if (room != null)
                                {

                                    room.Price = (decimal)item["roomInfo"]["price"];
                                    room.Details = (string)item["roomInfo"]["roomDetails"];
                                    room.No = (int)item["roomInfo"]["roomNum"];
                                    room.Status = (int)item["roomInfo"]["roomStatus"];

                                    long typeId = (long)item["roomInfo"]["roomThemeId"];
                                    Type type = db.Type.First(t => t.ServerId == typeId);
                                    room.TypeId = type.Id;
                                }
                            }
                        }
                        else//修改主题
                        {
                            if (modifyType == 0) //添加
                            {
                                Type type = new Type
                                {
                                    ServerId = (long)item["themeInfo"]["id"],
                                    Name = (string)item["themeInfo"]["name"]
                                };
                                db.Type.Add(type);
                            }
                            else if (modifyType == 1) //删除
                            {
                                long typeId = (long)item["themeInfo"]["id"];
                                Type type = db.Type.FirstOrDefault(t => t.ServerId == typeId);
                                if (type != null)
                                {
                                    db.Type.Remove(type);
                                }
                            }
                            else //修改
                            {
                                long typeId = (long)item["themeInfo"]["id"];
                                Type type = db.Type.FirstOrDefault(t => t.ServerId == typeId);
                                if (type != null)
                                {
                                    type.Name = (string)item["themeInfo"]["name"];
                                }
                            }
                        }
                        db.SaveChanges();
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(LoadThemeData));
                        _str = "修改数据成功\n";
                        switch (modifyType)
                        {
                            case 0:
                                _str += "添加";
                                break;
                            case 1:
                                _str += "删除";
                                break;
                            default:
                                _str += "修改";
                                break;
                        }
                        if ((int)item["modifyItem"] == 0)
                        {
                            _str += "房间" + (int)item["roomInfo"]["roomNum"];
                        }
                        else
                        {
                            _str += "主题" + (string)item["themeInfo"]["name"];
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show("修改数据错误" + exception.Message);
                _str = "修改数据错误" + exception.Message;
            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ShowMessageWindow));
            }
        }

        private void consumer_Listener(IMessage message)
        {
            try
            {
                var msg = (ITextMessage)message;
                Console.WriteLine(msg.Text);
                using (var db = new hotelEntities())
                {
                    var jo = JArray.Parse(msg.Text);
                    foreach (var item in jo)
                    {
                        Order order = new Order
                        {
                            InDate = Convert.ToDateTime((string)item["inDateStr"]),
                            Day = (int)item["inDays"],
                            LeaveDate = Convert.ToDateTime((string)item["leaveDateStr"]),
                            Price = (decimal)item["price"],
                            Remark = (string)item["remark"],
                            Finish = 0,
                            Status = 1, //已预订
                            ServerId = (long)item["orderId"]
                        };
                        long roomId = (long)item["roomId"];
                        Room room = db.Room.First(r => r.ServerId == roomId);
                        order.RoomId = room.Id;
                        db.Order.Add(order);
                        db.SaveChanges();

                        User user = new User
                        {
                            Code = "0",
                            Name = (string)item["linkManName"],
                            OrderId = order.Id,
                            Phone = (long)item["linkMobile"],
                            Sex = "男"
                        };
                        db.User.Add(user);

                        db.SaveChanges();
                        if (order.InDate.Value.Date == DateTime.Today)
                        {
                            RoomItem roomItem = new RoomItem { Room = room };
                            roomItem.SetRoomStatus(2);
                        }
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(RefreshRoomData));
                        _str = "收到新订单\n用户名称：" + user.Name + "\n联系方式" + user.Phone;
                    }
                }
            }
            catch (Exception exception)
            {
                //MessageBox.Show("预订订单数据错误" + exception.Message);
                _str = "接收订单失败，" + exception.Message;
            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ShowMessageWindow));
            }
        }

        private void RefreshRoomData()
        {
            MyApp.ReloadRoomItems();
        }

        private void ShowMessageWindow()
        {
            MessageWindow messageWindow = new MessageWindow(_str, this);
            messageWindow.Show();
            MessageWindows.Add(messageWindow);
        }

        /// <summary>
        /// 上传队列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadQueue(object sender, EventArgs e)
        {
            using (var db = new hotelEntities())
            {
                List<Queue> queues = db.Queue.ToList();
                if (queues.Count == 0) return;
                foreach (var queue in queues)
                {
                    Dictionary<string, string> dictionary =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(queue.Parameter);
                    NameValueCollection values = new NameValueCollection();
                    foreach (var item in dictionary)
                    {
                        values[item.Key] = item.Value;
                    }
                    string result = MyApp.Upload(queue.Url, queue.Type, values, true);
                    if (result == null) continue;
                    db.Queue.Remove(queue);
                    db.SaveChanges();
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _connection.Close();
        }
    }
}