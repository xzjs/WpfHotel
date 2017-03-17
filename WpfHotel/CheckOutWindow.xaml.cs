using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfHotel
{
    /// <summary>
    /// CheckOutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CheckOutWindow : Window
    {
        private Order _order;
        private decimal _otherMoney = 0, _collection = 0;
        private Invoice _invoice;
        public CheckOutWindow(Order order)
        {
            InitializeComponent();
            _order = order;
            OrderStackPanel.DataContext = _order;
            using (var db = new hotelEntities())
            {
                Room room = db.Room.Find(_order.RoomId);
                RoomList.ItemsSource = new List<Room> { room };
                Type type = db.Type.Find(room.TypeId);
                TypeList.ItemsSource = new List<Type> { type };
                List<Account> accounts = db.Account.Where(x => x.OrderId == order.Id).ToList();
                foreach (var account in accounts)
                {
                    _otherMoney += account.Consume.Value;
                    _collection += account.Balance.Value;
                }
                //设置其他金额
                MoneyTextBlock.Text = _otherMoney.ToString(CultureInfo.InvariantCulture);
                _invoice = new Invoice
                {
                    Money = order.Price.Value + _otherMoney,
                    Orderid = order.Id
                };
                //绑定总消费数据
                DockPanel.DataContext = _invoice;

                //设置收银
                CollectTextBlock.Text = "已收取：￥" + _collection.ToString(CultureInfo.InvariantCulture);
                //设置余额
                RemainTextBlock.Text = "需再收：￥" + (_invoice.Money - _collection);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 结账
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckOut(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    if (checkBox.IsChecked.Value)
                    {
                        if (string.IsNullOrEmpty(_invoice.Title))
                        {
                            throw new Exception("请填写发票抬头");
                        }
                        db.Invoice.Add(_invoice);

                    }
                    Order order = db.Order.Find(_order.Id);
                    order.Finish = 1;
                    db.SaveChanges();
                    RoomItem roomItem=new RoomItem {Room = order.Room};
                    roomItem.SetRoomStatus(1);
                    Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
