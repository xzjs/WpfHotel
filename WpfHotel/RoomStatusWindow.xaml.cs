using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using OfficeOpenXml;


namespace WpfHotel
{
    /// <summary>
    /// RoomStatusWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RoomStatusWindow : Window
    {
        private List<TypeItem> _typeItems;
        public RoomStatusWindow()
        {
            InitializeComponent();
            DatePicker.DisplayDateStart = DateTime.Today;
            DatePicker.SelectedDate = DateTime.Today;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var db = new hotelEntities())
            {
                List<Type> types = db.Type.ToList();
                _typeItems = types.Select(t => new TypeItem { Type = t, Date = DatePicker.SelectedDate.Value }).ToList();
                DataGrid.ItemsSource = _typeItems;
                int total1 = 0, total2 = 0, total3 = 0, total4 = 0;
                foreach (var typeItem in _typeItems)
                {
                    total1 += typeItem.TotalRoomNum;
                    total2 += typeItem.CanUseRoom;
                    total3 += typeItem.UnUseRoom;
                    total4 += typeItem.OrderRoomNum;
                }
                TextBlock1.Text = total1.ToString();
                TextBlock2.Text = total2.ToString();
                TextBlock3.Text = total3.ToString();
                TextBlock4.Text = total4.ToString();
            }
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            try
            {
                  ExcelPackage package = new ExcelPackage(new MemoryStream());
                var ws1 = package.Workbook.Worksheets.Add("Worksheet1");
                for (int row = 2; row < _typeItems.Count + 2; row++)
                {
                    ws1.Cells[row, 1].Value = _typeItems[row - 2].Type.Name;
                    ws1.Cells[row, 2].Value = _typeItems[row - 2].TotalRoomNum;
                    ws1.Cells[row, 3].Value = _typeItems[row - 2].CanUseRoom;
                    ws1.Cells[row, 4].Value = _typeItems[row - 2].UnUseRoom;
                    ws1.Cells[row, 5].Value = _typeItems[row - 2].OrderRoomNum;
                }
                ws1.Cells[1, 1].Value = "主题类型";
                ws1.Cells[1, 2].Value = "总房数";
                ws1.Cells[1, 3].Value = "可售房数";
                ws1.Cells[1, 4].Value = "不可用房数";
                ws1.Cells[1, 5].Value = "已经预订";
                ws1.Cells[_typeItems.Count + 2, 1].Value = "小计";
                ws1.Cells[_typeItems.Count + 2, 2].Value = TextBlock1.Text;
                ws1.Cells[_typeItems.Count + 2, 3].Value = TextBlock2.Text;
                ws1.Cells[_typeItems.Count + 2, 4].Value = TextBlock3.Text;
                ws1.Cells[_typeItems.Count + 2, 5].Value = TextBlock4.Text;
                SaveFileDialog saveFileDialog = new SaveFileDialog {Filter = "Excel files (*.xlsx)|*.xlsx"};
                var dialogResult = saveFileDialog.ShowDialog();
                if (dialogResult.Value)
                {
                    package.SaveAs(new FileInfo(saveFileDialog.FileName));
                }
                MessageBox.Show("导出成功");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);     
                
            }
            
        }
    }
}
