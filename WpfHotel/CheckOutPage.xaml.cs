using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OfficeOpenXml;

namespace WpfHotel
{
    /// <summary>
    ///     CheckOutPage.xaml 的交互逻辑
    /// </summary>
    public partial class CheckOutPage : Page
    {
        private List<CheckOutItem> _checkOutItems;

        public CheckOutPage()
        {
            InitializeComponent();
            Start.SelectedDate = DateTime.Today.AddDays(-1);
            End.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (End.SelectedDate == null)
                return;
            using (var db = new hotelEntities())
            {
                var orders =
                    db.Order.Include(o => o.Room)
                        .Include(o => o.User)
                        .Include(o => o.Account)
                        .Where(o => o.LeaveDate >= Start.SelectedDate)
                        .Where(o => o.LeaveDate <= End.SelectedDate).Where(o => o.Finish == 1)
                        .ToList();
                _checkOutItems = new List<CheckOutItem>();
                foreach (var order in orders)
                    _checkOutItems.Add(new CheckOutItem {Order = order});
                DataGrid.ItemsSource = _checkOutItems;
            }
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            try
            {
                var package = new ExcelPackage(new MemoryStream());
                var ws1 = package.Workbook.Worksheets.Add("Worksheet1");
                for (var row = 2; row < _checkOutItems.Count + 2; row++)
                {
                    ws1.Cells[row, 1].Value = _checkOutItems[row - 2].Order.Room.No;
                    ws1.Cells[row, 2].Value = _checkOutItems[row - 2].Order.Room.Price;
                    ws1.Cells[row, 3].Value = _checkOutItems[row - 2].Order.InDate;
                    ws1.Cells[row, 4].Value = _checkOutItems[row - 2].Order.LeaveDate;
                    ws1.Cells[row, 5].Value = _checkOutItems[row - 2].Order.Price;
                    ws1.Cells[row, 6].Value = _checkOutItems[row - 2].Other;
                    ws1.Cells[row, 7].Value = _checkOutItems[row - 2].Deposit;
                }
                ws1.Cells[1, 1].Value = "房号";
                ws1.Cells[1, 2].Value = "房价";
                ws1.Cells[1, 3].Value = "入住时间";
                ws1.Cells[1, 4].Value = "结账时间";
                ws1.Cells[1, 5].Value = "房费";
                ws1.Cells[1, 6].Value = "其他";
                ws1.Cells[1, 7].Value = "押金";
                var saveFileDialog = new SaveFileDialog {Filter = "Excel files (*.xlsx)|*.xlsx"};
                var dialogResult = saveFileDialog.ShowDialog();
                if (dialogResult.Value)
                    package.SaveAs(new FileInfo(saveFileDialog.FileName));
                MessageBox.Show("导出成功");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}