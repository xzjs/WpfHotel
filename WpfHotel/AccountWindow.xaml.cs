using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfHotel
{
    /// <summary>
    /// AccountWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AccountWindow : Window
    {
        private ObservableCollection<Account> _accounts;
        private readonly string _type;
        private Account _account;
        public AccountWindow(ObservableCollection<Account> accounts, long orderId, string type)
        {
            _accounts = accounts;
            _type = type;
            InitializeComponent();
            _account=new Account
            {
                Type = type,
                OrderId = orderId,
                Consume = 0,
                Balance = 0
            };
            StackPanel.DataContext = _account;
            using (var db=new hotelEntities())
            {
                Order order = db.Order.Find(orderId);
                string orderText = "房间号：" + order.Room.No + " 客户姓名：" + order.User.First().Name;
                OrderTextBlock.Text = orderText;
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
                Console.WriteLine(exception);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddAccount(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new hotelEntities())
                {
                    if (string.IsNullOrWhiteSpace(MoneyTextBox.Text))
                    {
                        throw new Exception("请输入金额");
                    }
                    if (_type == "入住押金")
                    {
                        _account.Balance = Convert.ToDecimal(MoneyTextBox.Text);
                    }
                    else
                    {
                        _account.Consume = Convert.ToDecimal(MoneyTextBox.Text);
                    }
                    _account.Time=DateTime.Now;
                    db.Account.Add(_account);
                    db.SaveChanges();
                    _accounts.Add(_account);
                    Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            
        }
    }
}
