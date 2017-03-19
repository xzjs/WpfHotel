using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfHotel
{
    /// <summary>
    /// OrderWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OrderWindow : Window
    {
        public OrderWindow()
        {
            InitializeComponent();
            using (var db=new hotelEntities())
            {
                DataGrid.ItemsSource = db.Order.Include("Room").Where(o => o.Finish == 0).Where(o => o.Status == 1).ToList();
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

        private void Show_Order(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap=new BitmapImage(new Uri("pack://application:,,,/WpfHotel;component/img/menu.png"));
            ImageBrush image=new ImageBrush {ImageSource = bitmap};
            Button1.Background = image;
            Button2.Background = image;
            Button3.Background = image;
            bitmap=new BitmapImage(new Uri("pack://application:,,,/WpfHotel;component/img/menu_click.png"));
            image = new ImageBrush { ImageSource = bitmap };
            Button button=sender as Button;
            button.Background = image;
            using (var db=new hotelEntities())
            {
                switch (button.Content.ToString())
                {
                    case "预订单":
                        DataGrid.ItemsSource = db.Order.Include("Room").Where(o => o.Finish == 0).Where(o => o.Status == 1).ToList();
                        break;
                    case "历史订单":
                        DataGrid.ItemsSource = db.Order.Include("Room").Where(o => o.Finish == 1).ToList();
                        break;
                    case "正在入住订单":
                        DataGrid.ItemsSource = db.Order.Include("Room").Where(o => o.Finish == 0).Where(o => o.Status == 2).ToList();
                        break;
                }
            }
            
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
