using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfHotel
{
    /// <summary>
    /// MoneyReportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MoneyReportWindow : Window
    {
        public MoneyReportWindow()
        {
            InitializeComponent();
            CollectPage collectPage=new CollectPage();
            Frame.Content = collectPage;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap = new BitmapImage(new Uri("pack://application:,,,/WpfHotel;component/img/menu.png"));
            ImageBrush image = new ImageBrush { ImageSource = bitmap };
            Button1.Background = image;
            Button2.Background = image;
            bitmap = new BitmapImage(new Uri("pack://application:,,,/WpfHotel;component/img/menu_click.png"));
            image = new ImageBrush { ImageSource = bitmap };
            Button button = sender as Button;
            button.Background = image;
            using (var db = new hotelEntities())
            {
                switch (button.Content.ToString())
                {
                    case "收银汇总报表":
                        Frame.Content=new CollectPage();
                        break;
                    case "结账明细报表":
                        Frame.Content=new CheckOutPage();
                        break;
                }
            }
        }
    }
}
