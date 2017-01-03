using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// CheckWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CheckWindow : Window
    {
        public CheckWindow()
        {
            InitializeComponent();

            ObservableCollection<Customer> memberData = new ObservableCollection<Customer>();
            memberData.Add(new Customer()
            {
                name = "张三",
                sex = "男",
                type = "身份证",
                idCard = "37028411111",
                phone = "131225225225"
            });
            memberData.Add(new Customer()
            {
                name = "张三",
                sex = "男",
                type = "身份证",
                idCard = "37028411111",
                phone = "131225225225"
            });
            memberData.Add(new Customer()
            {
                name = "张三",
                sex = "男",
                type = "身份证",
                idCard = "37028411111",
                phone = "131225225225"
            });
            CustomerDataGrid.DataContext = memberData;
        }
    }
}
