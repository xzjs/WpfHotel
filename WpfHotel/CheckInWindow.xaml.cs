using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfHotel
{
    /// <summary>
    ///     CheckInWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CheckInWindow : Window
    {
        private readonly Order _order;
        private readonly ObservableCollection<User> _users;
        private User _user;

        public CheckInWindow(Order order = null)
        {
            InitializeComponent();

            _users = new ObservableCollection<User>();
            UserDataGrid.ItemsSource = _users;
            using (var db = new hotelEntities())
            {
                if (order == null || order.Id == 0)
                {
                    var roomIDs = db.Room.Where(x => x.Status == 1).Select(x => x.TypeId);
                    var types = db.Type.Include("Room").Where(x => roomIDs.Contains(x.Id)).ToList();
                    TypeList.ItemsSource = types;
                    if (order != null)
                    {
                        foreach (var type in types)
                        {
                            foreach (var room in type.Room)
                            {
                                if (room.Id != order.RoomId.Value) continue;
                                RoomList.SelectedItem = room;
                                TypeList.SelectedItem = type;
                            }
                        }
                    }
                    _order = new Order
                    {
                        InDate = DateTime.Today,
                        LeaveDate = DateTime.Today.AddDays(1),
                        Day = 1,
                        Finish = 0,
                        Price = 0,
                        Status = 2
                    };
                }
                else
                {
                    _order = order;
                    var room = db.Room.Find(order.RoomId);
                    var type = room.Type;
                    TypeList.ItemsSource = new List<Type> { type };
                    RoomList.ItemsSource = new List<Room> { room };

                    TypeList.IsEnabled = false;
                    RoomList.IsEnabled = false;
                    var users = db.User.Where(x => x.OrderId == order.Id).ToList();
                    foreach (var user in users)
                        _users.Add(user);

                    End.DisplayDateStart = order.LeaveDate;

                }
                OrderStackPanel.DataContext = _order;
            }

            _user = new User { Sex = "男", CardType = "二代身份证" };
            UserInformation.DataContext = _user;
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

        /// <summary>
        ///     关闭当前窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     添加用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_User(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid.SelectedIndex == -1)
            {
                if (string.IsNullOrEmpty(_user.Name) || string.IsNullOrEmpty(_user.Code) ||
                    string.IsNullOrEmpty(_user.Phone.ToString()))
                {
                    MessageBox.Show("请填写相关信息");
                    return;
                }
                _users.Add(_user);
                _user = new User { Sex = "男", CardType = "二代身份证" };
                UserInformation.DataContext = _user;
            }
        }

        /// <summary>
        ///     创建用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Create_User(object sender, RoutedEventArgs e)
        {
            UserDataGrid.SelectedIndex = -1;
            _user = new User { Sex = "男", CardType = "二代身份证" };
            UserInformation.DataContext = _user;
        }

        private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _user = (sender as DataGrid).SelectedItem as User;
            UserInformation.DataContext = _user;
        }

        /// <summary>
        ///     删除用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_User(object sender, RoutedEventArgs e)
        {
            _users.Remove(_user);
            _user = new User { Sex = "男", CardType = "二代身份证" };
            UserInformation.DataContext = _user;
        }

        /// <summary>
        ///     计算住宿日期及金额
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _order.Day = _order.LeaveDate.Value.Subtract(_order.InDate.Value).Days;
        }

        /// <summary>
        ///     完成入住
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishCheckIn(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    var room = RoomList.SelectedItem as Room;
                    _order.Price = room.Price * _order.Day;
                    if (_order.Id == 0)
                    {
                        //保存订单
                        _order.RoomId = room.Id;
                        //上传订单
                        var config = ((App)Application.Current).Config;
                        var information = ((App)Application.Current).Information;
                        try
                        {
                            using (var client = new WebClient())
                            {
                                var orderItem = new OrderItem
                                {
                                    hotelId = information.HotelId.Value,
                                    roomId = room.ServerId.Value,
                                    inDateStr = _order.InDate.Value.Date.ToString("yyyy-MM-dd"),
                                    leaveDateStr = _order.LeaveDate.Value.Date.ToString("yyyy-MM-dd"),
                                    inDays = _order.Day.Value,
                                    remark = _order.Remark,
                                    clStatus = 2,
                                    users = new List<UserItem>()
                                };
                                foreach (var user in _users)
                                {
                                    var userItem = new UserItem
                                    {
                                        name = user.Name,
                                        sex = user.Sex == "男" ? "male" : "female",
                                        cardCode = user.Code,
                                        mobile = user.Phone.Value.ToString()
                                    };
                                    orderItem.users.Add(userItem);
                                }
                                var json = JsonConvert.SerializeObject(orderItem);
                                var values = new NameValueCollection
                                {
                                    ["details"] = json
                                };
                                try
                                {
                                    var response =
                                    client.UploadValues("http://" + config.Http + "/hotelClient/buildOrder.nd", values);

                                    var responseString = Encoding.UTF8.GetString(response);
                                    var jo = JObject.Parse(responseString);
                                    if ((string)jo["errorFlag"] != "false")
                                        MessageBox.Show("上传订单失败");
                                    else
                                        _order.ServerId = (long)jo["orderId"];
                                }
                                catch (WebException webException)
                                {
                                    string parameter = JsonConvert.SerializeObject(new Dictionary<string, string>
                                    {
                                        ["details"] = json
                                    }, Formatting.Indented);
                                    Queue queue = new Queue()
                                    {
                                        Url = "http://" + config.Http + "/hotelClient/buildOrder.nd",
                                        Type = "POST",
                                        Time = DateTime.Now,
                                        Parameter = parameter
                                    };
                                    db.Queue.Add(queue);
                                    db.SaveChanges();
                                }
                                
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message);
                        }

                        db.Order.Add(_order);
                        db.SaveChanges();
                        //保存用户
                        foreach (var user in _users)
                        {
                            user.OrderId = _order.Id;
                            db.User.Add(user);
                        }
                    }
                    else
                    {
                        //续住
                        var order = db.Order.Find(_order.Id);
                        order.LeaveDate = _order.LeaveDate;
                        order.Day = _order.Day;
                        order.Price = _order.Price;
                        if (order.Status.Value == 1)
                        {
                            using (var client = new WebClient())
                            {
                                var values = new NameValueCollection
                                {
                                    ["orderId"] = order.ServerId.ToString(),
                                    ["status"] = "1"
                                };
                                var config = db.Config.First();
                                var response =
                                    client.UploadValues("http://" + config.Http + "/hotelClient/setOrderStatus.nd", values);

                                var responseString = Encoding.UTF8.GetString(response);
                                var jo = JObject.Parse(responseString);
                                if ((string)jo["errorFlag"] != "false")
                                    MessageBox.Show("上传订单失败");
                               
                            }
                        }
                        
                    }
                    db.SaveChanges();
                    //更改房间状态
                    var roomItem = new RoomItem { Room = room };
                    roomItem.SetRoomStatus(3);

                    Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void ShowBillWindow(object sender, RoutedEventArgs e)
        {
            if (_order.Id == 0)
            {
                MessageBox.Show("还未生成账单");
                return;
            }
            var bill = new BillWindow(_order);
            bill.Show();
            Close();
        }
    }
}