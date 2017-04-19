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
    ///     LoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class LoginPage : Page
    {
        private readonly LoginWindow _loginWindow;
        private Config _config;
        public LoginPage(LoginWindow loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
            using (var db = new hotelEntities())
            {
                _config = db.Config.First();
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var sup = new SetUpPage(_loginWindow);
            _loginWindow.PageFrame.Content = sup;
        }

        /// <summary>
        ///     登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    _config = db.Config.First();
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

                        var response = client.UploadValues("http://" + _config.Http + "/hotelLogin/login.nd", values);

                        var responseString = Encoding.UTF8.GetString(response);
                        var jo = JObject.Parse(responseString);
                        var str = (string)jo["id"];
                        if (str != null)
                        {
                            var i = new Information()
                            {
                                HotelId = (int)jo["id"]
                            };

                            db.Information.Add(i);

                            db.SaveChanges();
                            Button.Content = "登录成功，正在初始化数据";
                            ProgressRing.IsActive = true;
                            Init();
                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();
                            _loginWindow.Close();
                        }
                        else
                            MessageBox.Show("登录失败,账号或密码错误");
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void Init()
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        Information information = db.Information.First();
                        var responseString =
                                client.DownloadString("http://" + _config.Http + "/hotelClient/getThemeList.nd?hotelId=" +
                                                      information.HotelId);
                        var jo = JObject.Parse(responseString);
                        if (jo["themeRoomList"] != null)
                        {
                            foreach (var item in jo["themeRoomList"])
                            {
                                var type = new Type
                                {
                                    ServerId = (long)item["id"],
                                    Name = (string)item["name"]
                                };
                                db.Type.Add(type);
                            }
                        }
                        db.SaveChanges();
                        responseString =
                            client.DownloadString("http://" + _config.Http + "/hotelClient/roomInfoById.nd?id=" +
                                                  information.HotelId);
                        jo = JObject.Parse(responseString);
                        if (jo["roomList"] != null)
                            foreach (var item in jo["roomList"])
                            {
                                try
                                {
                                    var room = new Room
                                    {
                                        No = (int)item["roomNum"],
                                        ServerId = (long)item["id"],
                                        Status = 1,
                                        Price = (decimal)item["price"],
                                  
                                        Details = (string)item["roomDetails"]
                                      
                                    };
                                    var typeId = (long)item["roomThemeId"];
                                    var type = db.Type.FirstOrDefault(x => x.ServerId == typeId);
                                    if (type != null)
                                        room.TypeId = type.Id;
                                    db.Room.Add(room);
                                }
                                catch (Exception exception)
                                {
                                    MessageBox.Show("房间" + (string)item["roomNum"] + "数据有误," + exception.Message);
                                }

                            }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //Console.WriteLine(ex.Message);
            }
        }
    }
}