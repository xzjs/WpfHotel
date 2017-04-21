using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WpfHotel
{
    public class MyApp
    {
        public static Config Config;
        public static Information Information;

        public static ObservableCollection<RoomItem> RoomItems;
        public static long TypeId;

        /// <summary>
        /// 重新加载房间数据
        /// </summary>
        public static void ReloadRoomItems()
        {
            using (var db = new hotelEntities())
            {
                List<Room> rooms;
                switch (TypeId)
                {
                    case -1:
                        return;
                    case 0:
                        rooms = db.Room.Include(r => r.Order).ToList();
                        break;
                    default:
                        rooms = db.Room.Include(r => r.Order).Where(r => r.TypeId == TypeId).ToList();
                        break;
                }
                RoomItems.Clear();
                foreach (var room in rooms)
                {
                    RoomItems.Add(new RoomItem { Room = room });
                }
            }
        }

        /// <summary>
        /// 往服务器上传数据
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="type">method</param>
        /// <param name="value">值</param>
        /// <param name="retry">是否是重试</param>
        /// <returns>服务器返回的字符串</returns>
        public static string Upload(string path, string type, NameValueCollection value, bool retry = false)
        {
            string responseString = null;
            try
            {
                using (var client = new WebClient())
                {
                    if (type == "POST")
                    {
                        var response =
                            client.UploadValues("http://" + Config.Http + path, value);

                        responseString = Encoding.UTF8.GetString(response);
                    }

                    
                }
            }
            catch (WebException webException)
            {
                Dictionary<string, string> dictionary = value.AllKeys.ToDictionary(key => key, key => value[key]);
                string parameter = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
                Queue queue = new Queue()
                {
                    Url = path,
                    Type = "POST",
                    Time = DateTime.Now,
                    Parameter = parameter
                };
                if (retry) return responseString;
                using (var db = new hotelEntities())
                {
                    db.Queue.Add(queue);
                    db.SaveChanges();
                }
            }
            return responseString;
        }
    }


}