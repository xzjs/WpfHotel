﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                string[] images = { "", "kong.png", "di.png", "zhu.png", "zang.png", "xiu.png", "ting,png", "li.png" };
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
                switch (Room.Status)
                {
                    case 1:
                        MenuItem checkInMenuItem = new MenuItem { Header = "办理入住" };
                        checkInMenuItem.Click += CheeckIn;
                        contextMenu.Items.Add(checkInMenuItem);
                        break;
                }
                return contextMenu;
            }
        }

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
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
