using System.Linq;

namespace WpfHotel
{
    public class CheckOutItem
    {
        public Order Order { get; set; }

        /// <summary>
        ///     姓名
        /// </summary>
        public string UserName => Order.User.First().Name;

        /// <summary>
        ///     其他
        /// </summary>
        public decimal Other
        {
            get { return Order.Account.Sum(account => account.Consume.Value); }
        }

        /// <summary>
        ///     押金
        /// </summary>
        public decimal Deposit
        {
            get { return Order.Account.Sum(account => account.Balance.Value); }
        }
    }
}