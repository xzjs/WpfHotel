using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            using (var db=new hotelEntities())
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
    }
}
