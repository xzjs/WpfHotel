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
        private ObservableCollection<Type> _types;
        public MainWindow()
        {
            InitializeComponent();

            //开启时间线程
            var showTimer = new DispatcherTimer();
            showTimer.Tick += ShowCurrentTimer;
            showTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            showTimer.Start();

            //开启队列线程
            var uploadQueue=new DispatcherTimer();
            uploadQueue.Tick += UploadQueue;
            uploadQueue.Interval=new TimeSpan(1,0,0);
            uploadQueue.Start();

            _types = new ObservableCollection<Type>();
            LoadThemeData();
            ThemeComboBox.ItemsSource = _types;

            LoadRoomData();

            using (var db=new hotelEntities())
            {
                Config = db.Config.FirstOrDefault();
                Information = db.Information.FirstOrDefault();
                if (Config == null || Information == null)
                {
                    LoginWindow loginWindow=new LoginWindow();
                    loginWindow.ShowDialog();
                    return;
                }
            }
            ListenOrderTcp();
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
                var roomItems = rooms.Select(room => new RoomItem { Room = room }).ToList();
                foreach (var roomItem in roomItems)
                {
                    //判断预抵
                    var order = roomItem.Room.Order.Where(o => o.Finish == 0).FirstOrDefault(o => o.Status == 1);
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
                    ((App)Application.Current).Config = config;
                    ((App)Application.Current).Information = information;
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

        /// <summary>
        ///     通过tcp获取数据
        /// </summary>
        private void ListenOrderTcp()
        {
            try
            {
                if (Config == null || Information == null)
                {
                    LoginWindow loginWindow=new LoginWindow();
                    loginWindow.ShowDialog();
                    return;
                }
                IConnectionFactory factory = new ConnectionFactory("tcp://" + Config.Tcp + ":" + Config.Port);
                var connection = factory.CreateConnection();
                connection.ClientId = Information.HotelId.ToString();
                connection.Start();

                var session = connection.CreateSession();
                var consumer = session.CreateConsumer(new ActiveMQQueue("hotelOrder" + Information.HotelId));
                consumer.Listener += consumer_Listener;

                var session1 = connection.CreateSession();
                var consumer1 = session1.CreateConsumer(new ActiveMQQueue("hotelModifyModify" + Information.HotelId));
                consumer1.Listener += Modify;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Modify(IMessage message)
        {
            try
            {
                var msg = (ITextMessage) message;
                Console.WriteLine(msg.Text);
                using (var db = new hotelEntities())
                {
                    var jo = JArray.Parse(msg.Text);
                    foreach (var item in jo)
                    {
                        int modifyType = (int) item["modifyType"];
                        if ((int) item["modifyItem"] == 0) //修改房间
                        {
                            if (modifyType == 0) //添加
                            {
                                var room = new Room
                                {
                                    ServerId = (long)item["roomInfo"]["id"],
                                    Limit = (int)item["roomInfo"]["numberLimit"],
                                    Price = (decimal)item["roomInfo"]["price"],
                                    Details = (string)item["roomInfo"]["roomDetails"],
                                    No = (int)item["roomInfo"]["roomNum"],
                                    Square = (double)item["roomInfo"]["roomSquare"],
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
                                    room.Limit = (int) item["roomInfo"]["numberLimit"];
                                    room.Price = (decimal) item["roomInfo"]["price"];
                                    room.Details = (string) item["roomInfo"]["roomDetails"];
                                    room.No = (int) item["roomInfo"]["roomNum"];
                                    room.Square = (double) item["roomInfo"]["roomSquare"];
                                    long typeId = (long) item["roomInfo"]["roomThemeId"];
                                    Type type = db.Type.First(t => t.ServerId == typeId);
                                    room.TypeId = type.Id;
                                }
                            }
                        }
                        else//修改主题
                        {
                            if (modifyType == 0) //添加
                            {
                                Type type=new Type
                                {
                                    ServerId = (long)item["themeInfo"]["id"],
                                    Name = (string)item["themeInfo"]["name"]
                                };
                                db.Type.Add(type);
                            }
                            else if (modifyType == 1) //删除
                            {
                                long typeId = (long) item["themeInfo"]["id"];
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
                                    type.Name = (string) item["themeInfo"]["name"];
                                }
                            }
                        }
                        db.SaveChanges();
                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            LoadThemeData();
                            LoadRoomData();
                        }));
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("修改数据错误"+exception.Message);

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
                            Status = 1,//已预订
                            ServerId = (long)item["orderId"]
                        };
                        long roomId = (long) item["roomId"];
                        Room room = db.Room.First(r => r.ServerId == roomId);
                        order.RoomId = room.Id;
                        db.Order.Add(order);
                        db.SaveChanges();
                        foreach (var _user in item["userList"])
                        {
                            User user = new User
                            {
                                Code = (string)_user["cardCode"],
                                Name = (string)_user["name"],
                                OrderId = order.Id,
                                Phone = 0,
                                Sex = "男"
                            };
                            db.User.Add(user);
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("预订订单数据错误"+exception.Message);

            }
        }

        private void UploadQueue(object sender, EventArgs e)
        {
            using (var db = new hotelEntities())
            {
                List<Queue> queues = db.Queue.OrderBy(q => q.Id).ToList();
                if (queues.Count > 0)
                {
                    using (var client = new WebClient())
                    {
                        foreach (var queue in queues)
                        {
                            try
                            {
                                Dictionary<string, string> dictionary =
                                    JsonConvert.DeserializeObject<Dictionary<string, string>>(queue.Parameter);
                                NameValueCollection values = new NameValueCollection();
                                foreach (var item in dictionary)
                                {
                                    values[item.Key] = item.Value;
                                }
                                var response = client.UploadValues(queue.Url, values);

                                var responseString = Encoding.Default.GetString(response);
                                var jo = JObject.Parse(responseString);
                                if ((string) jo["errorFlag"] == "false")
                                {
                                    db.Queue.Remove(queue);
                                    db.SaveChanges();
                                }
                            }
                            catch (WebException webException)
                            {
                                break;
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show(exception.Message);
                            }
                            
                        }
                    }
                }
                
            }
        }
    }
}