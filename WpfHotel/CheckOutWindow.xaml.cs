using System;
using System.Collections.Generic;
using System.Data.Entity;
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


            using (var db = new hotelEntities())
            {
                _order = db.Order.Find(order.Id);
                //设置离开日期为当日日期
                _order.LeaveDate = DateTime.Today;
                _order.Day = _order.LeaveDate.Value.Subtract(_order.InDate.Value).Days;
                _order.Price = _order.Room.Price * _order.Day;
                OrderStackPanel.DataContext = _order;

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
                    Money = _order.Price.Value + _otherMoney,
                    Orderid = _order.Id
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
                    db.Entry(_order).State=EntityState.Modified;
                    
                    _order.Status = 3;
                    _order.Finish = 1;
                    db.SaveChanges();
                    RoomItem roomItem = new RoomItem { Room = _order.Room };
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
