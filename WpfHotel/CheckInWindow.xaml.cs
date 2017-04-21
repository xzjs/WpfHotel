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
                if (order == null)
                {
                    var roomIDs = db.Room.Where(x => x.Status == 1).Select(x => x.TypeId);
                    var types = db.Type.Include("Room").Where(x => roomIDs.Contains(x.Id)).ToList();
                    TypeList.ItemsSource = types;
                    
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
                else //续住或预约到店
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
                    Start.DisplayDate = order.InDate.Value;
                    Start.IsEnabled = false;
                    if (order.Status == 2)
                        End.DisplayDateStart = order.LeaveDate;
                    else
                    {
                        End.DisplayDate = order.LeaveDate.Value;
                        End.IsEnabled = false;
                    }

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
            if (UserDataGrid.SelectedIndex != -1) return;
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
                    if (_order.Id == 0)//现场入住
                    {
                        List<Order> orders = db.Order.Where(o => o.RoomId == room.Id).Where(o => o.Finish == 0).ToList();
                        if (orders.Any(order => order.InDate < _order.LeaveDate))
                        {
                            throw new Exception("该房间在指定日期里有预定，请重新选择房间或日期");
                        }

                        _order.Price = room.Price * _order.Day;

                        //保存订单
                        _order.RoomId = room.Id;

                        #region 上传订单
                        var orderItem = new OrderItem
                        {
                            hotelId = MyApp.Information.HotelId.Value,
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
                        string result=MyApp.Upload("/hotelClient/buildOrder.nd", "POST", values);
                        var jo = JObject.Parse(result);
                        if ((string)jo["errorFlag"] != "false")
                            MessageBox.Show("上传订单失败");
                        else
                            _order.ServerId = (long)jo["orderId"];
                        #endregion

                        #region 更新用户
                        List<User> users = _order.User.ToList();
                        db.User.RemoveRange(users);
                        foreach (var user in _users)
                        {
                            _order.User.Add(user);
                        }
                        #endregion

                        db.Order.Add(_order);
                    }
                    else//续住或预约到店
                    {
                        var order = db.Order.Find(_order.Id);

                        if (order.Status.Value == 1) //预约到店
                        {
                            #region 更新用户

                            List<User> users = order.User.ToList();
                            db.User.RemoveRange(users);
                            foreach (var user in _users)
                            {
                                User u = new User
                                {
                                    Name = user.Name,
                                    Sex = user.Sex,
                                    Code = user.Code,
                                    Phone = user.Phone,
                                    CardType = user.CardType
                                };
                                order.User.Add(u);
                            }
                            order.Status = 2;

                            #endregion

                            #region 更新服务器订单状态

                            var values = new NameValueCollection
                            {
                                ["orderId"] = order.ServerId.ToString(),
                                ["status"] = "2"
                            };
                            var responseString = MyApp.Upload("/hotelClient/setOrderStatus.nd", "POST", values);
                            var jo = JObject.Parse(responseString);
                            if ((string)jo["errorFlag"] != "false")
                                MessageBox.Show("更新服务器订单状态失败");

                            #endregion

                        }
                        else//续住
                        {
                            order.LeaveDate = _order.LeaveDate;
                            order.Day = _order.Day;
                            order.Price = _order.Price;
                        }

                    }
                    db.SaveChanges();
                    //更改房间状态
                    var roomItem = new RoomItem { Room = room };
                    roomItem.SetRoomStatus(3);
                    MyApp.ReloadRoomItems();

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