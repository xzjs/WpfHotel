using System.Collections.Generic;

namespace WpfHotel
{
    internal class OrderItem
    {
        public int clStatus;
        public long hotelId;
        public string inDateStr;
        public int inDays;
        public string leaveDateStr;
        public string remark;
        public long roomId;
        public List<UserItem> users;
    }
}