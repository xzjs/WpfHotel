using System;
using System.Linq;

namespace WpfHotel
{
    public class TypeItem
    {
        public Type Type { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        ///     总房数
        /// </summary>
        public int TotalRoomNum
        {
            get
            {
                using (var db = new hotelEntities())
                {
                    return db.Room.Count(r => r.TypeId == Type.Id);
                }
            }
        }

        /// <summary>
        ///     已经预订
        /// </summary>
        public int OrderRoomNum
        {
            get
            {
                using (var db = new hotelEntities())
                {
                    var orders =
                        db.Order.Where(o => o.InDate <= Date).Where(o => o.LeaveDate >= Date).Where(o => o.Status == 1);
                    var roomIDs = orders.Select(o => o.RoomId);
                    return db.Room.Where(r => r.TypeId == Type.Id).Count(r => roomIDs.Contains(r.Id));
                }
            }
        }

        /// <summary>
        ///     不可用房数
        /// </summary>
        public int UnUseRoom
        {
            get
            {
                using (var db = new hotelEntities())
                {
                    return
                        db.Room
                            .Where(r => r.TypeId == Type.Id)
                            .Where(r => r.Status > 2)
                            .Count(r => r.Status < 7);
                }
            }
        }

        /// <summary>
        ///     可用房数
        /// </summary>
        public int CanUseRoom => TotalRoomNum - OrderRoomNum - UnUseRoom;
    }
}