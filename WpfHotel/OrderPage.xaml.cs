using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfHotel
{
    /// <summary>
    /// OrderPage.xaml 的交互逻辑
    /// </summary>
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            TextBlock nameTextBlock = new TextBlock();
            nameTextBlock.Text = "姓名：";
            nameTextBlock.Width = 70;
            nameTextBlock.TextAlignment = TextAlignment.Center;
            sp.Children.Add(nameTextBlock);

            TextBox nameTextBox = new TextBox();
            nameTextBox.Width = 130;
            sp.Children.Add(nameTextBox);

            TextBlock phoneTextBlock = new TextBlock();
            phoneTextBlock.Text = "手机号码：";
            phoneTextBlock.Width = 70;
            phoneTextBlock.TextAlignment = TextAlignment.Center;
            sp.Children.Add(phoneTextBlock);

            TextBox phoneTextBox = new TextBox();
            phoneTextBox.Width = 130;
            sp.Children.Add(phoneTextBox);

            CustomerStackPanel.Children.Add(sp);
        }
    }
}
