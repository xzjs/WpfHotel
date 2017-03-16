using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MahApps.Metro.Controls;

namespace WpfHotel
{
    /// <summary>
    /// CheckInWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CheckInWindow : Window
    {
        private ObservableCollection<User> _users;
        private User _user;
        private Order _order;
        public CheckInWindow()
        {
            InitializeComponent();

            _users = new ObservableCollection<User>();
            UserDataGrid.ItemsSource = _users;
            using (var db = new hotelEntities())
            {
                var roomIDs = db.Room.Where(x => x.Status == 1).Select(x => x.TypeId);
                List<Type> types = db.Type.Include("Room").Where(x => roomIDs.Contains(x.Id)).ToList();
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
        /// 关闭当前窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 添加用户
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
        /// 创建用户
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
        /// 删除用户
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
        /// 计算住宿日期及金额
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            _order.Day = _order.LeaveDate.Value.Subtract(_order.InDate.Value).Days;
        }

        /// <summary>
        /// 完成入住
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishCheckIn(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db=new hotelEntities())
                {
                    //保存订单
                    Room room = RoomList.SelectedItem as Room;
                    _order.RoomId = room.Id;
                    _order.Price = room.Price * _order.Day;
                    db.Order.Add(_order);
                    db.SaveChanges();
                    //保存用户
                    foreach (var user in _users)
                    {
                        user.OrderId = _order.Id;
                        db.User.Add(user);
                    }
                    db.SaveChanges();
                    //更改房间状态
                    RoomItem roomItem=new RoomItem {Room = room};
                    roomItem.SetRoomStatus(3);
                    MainWindow main = Application.Current.MainWindow as MainWindow;
                    main.LoadRoomData();
                    Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
