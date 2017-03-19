using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace WpfHotel
{
    public class RoomItem
    {
        public Room Room { get; set; }

        /// <summary>
        /// 背景图
        /// </summary>
        public ImageSource Image
        {
            get
            {
                string[] images = { "", "kong.png", "di.png", "zhu.png", "zang.png", "xiu.png", "ting.png", "li.png" };
                return new BitmapImage(new Uri("pack://application:,,,/WpfHotel;component/img/" + images[Room.Status.Value]));
            }
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        public ContextMenu ContextMenu
        {
            get
            {
                ContextMenu contextMenu = new ContextMenu();
                MenuItem menuItem1 = new MenuItem
                {
                    Header = "转干净房",
                    Tag = 1
                };
                menuItem1.Click += ChangeRoomStatus;
                MenuItem menuItem4 = new MenuItem
                {
                    Header = "转为脏房",
                    Tag = 4
                };
                menuItem4.Click += ChangeRoomStatus;
                MenuItem menuItem5 = new MenuItem
                {
                    Header = "转维修房",
                    Tag = 5
                };
                menuItem5.Click += ChangeRoomStatus;
    
                MenuItem menuItem6 = new MenuItem
                {
                    Header = "转停用房",
                    Tag = 6
                };
                menuItem6.Click += ChangeRoomStatus;
         
                switch (Room.Status)
                {
                    case 1:
                        MenuItem checkInMenuItem = new MenuItem { Header = "办理入住" };
                        checkInMenuItem.Click += CheeckIn;
                        contextMenu.Items.Add(checkInMenuItem);
                        contextMenu.Items.Add(menuItem4);
                        contextMenu.Items.Add(menuItem5);
                        contextMenu.Items.Add(menuItem6);
                        break;
                    case 3:
                        MenuItem renewMenuItem = new MenuItem { Header = "办理续住" };
                        renewMenuItem.Click += Renew;
                        contextMenu.Items.Add(renewMenuItem);
                        MenuItem checkOutMenuItem = new MenuItem { Header = "结账退房" };
                        checkOutMenuItem.Click += CheckOut;
                        contextMenu.Items.Add(checkOutMenuItem);
                        break;
                    case 4:
                        contextMenu.ItemsSource=new List<MenuItem> {menuItem1,menuItem5,menuItem6};
                        break;
                    case 5:
                        contextMenu.ItemsSource = new List<MenuItem> { menuItem1, menuItem4, menuItem6 };
                        break;
                    case 6:
                        contextMenu.ItemsSource = new List<MenuItem> { menuItem1, menuItem4, menuItem5 };
                        break;
                }
                return contextMenu;
            }
        }

        /// <summary>
        /// 更改房间状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeRoomStatus(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            int status = Convert.ToInt32(menuItem.Tag);
            SetRoomStatus(status);
        }

        /// <summary>
        /// 房态
        /// </summary>
        public string Status
        {
            get
            {
                string[] status = { "", "空房", "预抵房", "在住房", "脏房", "维修房", "停用房", "预离房" };
                return status[Room.Status.Value];
            }
        }

        /// <summary>
        /// 是否有人
        /// </summary>
        public int HasPeople
        {
            get
            {
                //在住或者预离算有人
                if (Room.Status == 3 || Room.Status == 7)
                {
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// 客人姓名
        /// </summary>
        public string UserName
        {
            get
            {
                if (Room.Status != 3)
                    return "";
                using (var db = new hotelEntities())
                {
                    Order order = db.Order.Include("User").First(o => o.Status == 2);
                    return order.User.First().Name;
                }
            }
        }

        /// <summary>
        /// 是否预离
        /// </summary>
        public int IsReadyLeave => Room.Status.Value == 7 ? 1 : 0;

        /// <summary>
        /// 结账退房
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckOut(object sender, RoutedEventArgs e)
        {
            CheckOutWindow checkOutWindow = new CheckOutWindow(Room.Order.First(o => o.Finish == 0));
            checkOutWindow.ShowDialog();
        }

        /// <summary>
        /// 办理续住
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Renew(object sender, RoutedEventArgs e)
        {
            using (var db = new hotelEntities())
            {
                Order order = db.Order.Where(x => x.Finish == 0).FirstOrDefault(x => x.RoomId == Room.Id);
                CheckInWindow checkIn = new CheckInWindow(order);
                checkIn.ShowDialog();
            }
        }

        /// <summary>
        /// 办理入住
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheeckIn(object sender, RoutedEventArgs e)
        {
            CheckInWindow checkInWindow = new CheckInWindow();
            checkInWindow.ShowDialog();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="status"></param>
        public void SetRoomStatus(int status)
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    //更新数据库
                    Room room = db.Room.Find(Room.Id);
                    room.Status = status;
                    db.SaveChanges();
                    //更新服务器
                    Config config = ((App)Application.Current).Config;
                    using (var client = new WebClient())
                    {
                        var values = new NameValueCollection
                        {
                            ["roomId"] = Room.ServerId.ToString(),
                            ["status"] = status.ToString()
                        };

                        var response = client.UploadValues("http://" + config.Http + "/hotelClient/setRoomStatus.nd", values);

                        var responseString = Encoding.Default.GetString(response);
                        var jo = JObject.Parse(responseString);
                        if ((string)jo["errorFlag"] != "false")
                            MessageBox.Show("设置房间状态失败");
                    }
                    MainWindow main = Application.Current.MainWindow as MainWindow;
                    main.LoadRoomData();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public void DoubleClick()
        {
            CheckInWindow checkInWindow = new CheckInWindow();
            switch (Room.Status)
            {
                case 1:

                    checkInWindow.ShowDialog();
                    break;
                case 7:
                case 3:
                case 2:
                    checkInWindow = new CheckInWindow(Room.Order.FirstOrDefault(o => o.Finish == 0));
                    checkInWindow.ShowDialog();
                    break;
            }
        }
    }
}
