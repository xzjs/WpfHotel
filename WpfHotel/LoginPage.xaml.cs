using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;

namespace WpfHotel
{
    /// <summary>
    /// LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        private LoginWindow _parentWindow;
        public LoginPage()
        {
            InitializeComponent();
        }

        public LoginWindow ParentWindow
        {
            get
            {
                return _parentWindow;
            }

            set
            {
                _parentWindow = value;
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetUpPage sup = new SetUpPage();
            sup.ParentWindow = ParentWindow;
           _parentWindow.PageFrame.Content = sup;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    var config = db.Config.FirstOrDefault();
                    if (config == null)
                    {
                        throw new Exception("请先配置相关参数");
                    }
                    var name = NameTextbox.Text.Trim();
                    var p = Marshal.SecureStringToBSTR(PwdBox.SecurePassword);
                    var password = Marshal.PtrToStringBSTR(p);

                    using (var client = new WebClient())
                    {
                        var values = new NameValueCollection
                        {
                            ["account"] = name,
                            ["password"] = password
                        };

                        var response = client.UploadValues("http://" + config.Http + "/hotelLogin/login.nd", values);

                        var responseString = Encoding.Default.GetString(response);
                        var jo = JObject.Parse(responseString);
                        if (jo["id"] != null)
                        {
                            var i = db.Information.FirstOrDefault() ?? new Information();
                            i.HotelId = (int)jo["id"];
                            if (i.Id == 0)
                            {
                                db.Information.Add(i);
                            }
                            db.SaveChanges();
                            MessageBox.Show("登陆成功");
                        }
                        else
                            MessageBox.Show("登录失败");
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateData(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    Config config = db.Config.FirstOrDefault();
                    if (config == null)
                    {
                        throw new Exception("请填写配置信息");
                    }
                    Information information = db.Information.FirstOrDefault();
                    if (information == null)
                    {
                        throw new Exception("请先登录");
                    }

                    int count = db.Order.Count(x => x.Finish == 0);
                    if (count > 0)
                    {
                        throw new Exception("有未完成的订单,无法更新数据");
                    }
                    MessageBoxResult messageBoxResult = MessageBox.Show("更新数据将会清空订单和历史数据，是否更新", "是否更新数据",
                        MessageBoxButton.OKCancel);
                    if (messageBoxResult == MessageBoxResult.OK)
                    {
                        using (var client = new WebClient())
                        {
                            client.Encoding = Encoding.UTF8;
                            var responseString =
                                client.DownloadString("http://" + config.Http + "/hotelClient/getThemeList.nd?hotelId=" +
                                                      information.HotelId);
                            var jo = JObject.Parse(responseString);
                            if (jo["themeRoomList"] != null)
                            {
                                db.Database.ExecuteSqlCommand("DELETE FROM [Type]");
                                foreach (var item in jo["themeRoomList"])
                                {
                                    var type = new Type()
                                    {
                                        ServerId = (long)item["id"],
                                        Name = (string)item["name"]
                                    };
                                    db.Type.Add(type);
                                }
                            }
                            db.SaveChanges();
                            responseString =
                                client.DownloadString("http://" + config.Http + "/hotelClient/roomInfoById.nd?id=" +
                                                      information.HotelId);
                            jo = JObject.Parse(responseString);
                            if (jo["roomList"] != null)
                            {
                                db.Database.ExecuteSqlCommand("DELETE FROM Room");
                                db.Database.ExecuteSqlCommand("DELETE FROM [Order]");
                                db.Database.ExecuteSqlCommand("DELETE FROM [Account]");
                                db.Database.ExecuteSqlCommand("DELETE FROM [Invoice]");
                                db.Database.ExecuteSqlCommand("DELETE FROM [User]");
                                foreach (var item in jo["roomList"])
                                {
                                    var room = new Room()
                                    {
                                        No = (int)item["roomNum"],
                                        ServerId = (long)item["id"],
                                        Status = (int)item["roomStatus"],
                                        Price = (decimal)item["price"],
                                        Limit = (int)item["numberLimit"],
                                        Details = (string)item["roomDetails"],
                                        Square = (double)item["roomSquare"],
                                    };
                                    long typeId = (long)item["roomThemeId"];
                                    var type = db.Type.FirstOrDefault(x => x.ServerId == typeId);
                                    if (type != null)
                                        room.TypeId = type.Id;
                                    db.Room.Add(room);
                                }
                            }
                            db.SaveChanges();
                            MessageBox.Show("更新数据成功后");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
