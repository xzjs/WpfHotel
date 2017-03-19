using System.Collections.Generic;

namespace WpfHotel
{
    class OrderItem
    {
        public long hotelId;
        public long roomId;
        public string inDateStr;
        public string leaveDateStr;
        public int inDays;
        public int clStatus;
        public string remark;
        public List<UserItem> users;
    }
}
