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
    ///     AppointmentWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AppointmentWindow : Window
    {
        private readonly Order _order;
        private List<long?> roomIdList;

        public AppointmentWindow()
        {
            InitializeComponent();
            Start.DisplayDateStart = DateTime.Today;
            _order = new Order
            {
                InDate = DateTime.Today,
                LeaveDate = DateTime.Today.AddDays(1),
                Day = 1,
                Status = 1, //已预定
                Finish = 0
            };
            _order.User = new ObservableCollection<User>();
            DockPanel.DataContext = _order;
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
        ///     日期变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //计算住宿天数
            _order.Day = _order.LeaveDate.Value.Subtract(_order.InDate.Value).Days;
            //计算可以预定的房间
            using (var db = new hotelEntities())
            {
                var orders = db.Order.Where(x => x.InDate <= _order.InDate).Where(x => x.LeaveDate > _order.LeaveDate);
                roomIdList = orders.Select(o => o.RoomId).ToList();
                var types = db.Type.Where(t => t.Room.Any(r => !roomIdList.Contains(r.Id))).ToList();
                TypeList.ItemsSource = types;
                TypeList.SelectedIndex = 0;
            }
        }

        /// <summary>
        ///     房间类别更换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var db = new hotelEntities())
            {
                var type = TypeList.SelectedItem as Type;
                if (type != null)
                {
                    var rooms =
                        db.Room.Where(r => r.TypeId == type.Id).Where(r => !roomIdList.Contains(r.Id)).ToList();
                    RoomList.ItemsSource = rooms;
                    RoomList.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        ///     预定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MakeAppointment(object sender, RoutedEventArgs e)
        {
            try
            {
                var room = RoomList.SelectedItem as Room;
                if (room == null)
                    throw new Exception("指定日期内没有合适的房间");
                using (var db = new hotelEntities())
                {
                    _order.RoomId = room.Id;
                    _order.Price = room.Price.Value * _order.Day.Value;
                    
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
                                clStatus = 1,
                                users = new List<UserItem>()
                            };
                            foreach (var user in _order.User)
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

                            var response =
                                client.UploadValues("http://" + config.Http + "/hotelClient/buildOrder.nd", values);

                            var responseString = Encoding.UTF8.GetString(response);
                            var jo = JObject.Parse(responseString);
                            if ((string)jo["errorFlag"] != "false")
                                MessageBox.Show("上传订单失败");
                            else
                                _order.ServerId = (long)jo["orderId"];
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                    db.Order.Add(_order);
                    db.SaveChanges();
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.LoadRoomData();
                    Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        ///     关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}