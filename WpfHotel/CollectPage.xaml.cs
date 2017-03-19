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
    /// CollectPage.xaml 的交互逻辑
    /// </summary>
    public partial class CollectPage : Page
    {
        private List<Account> _accounts;
        public CollectPage()
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
                _accounts =
                    db.Account.Include(a => a.Order.Room)
                        .Include(a => a.Order.User)
                        .Where(a => a.Time >= Start.SelectedDate)
                        .Where(a => a.Time <= End.SelectedDate)
                        .ToList();
                DataGrid.ItemsSource = _accounts;
            }
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelPackage package = new ExcelPackage(new MemoryStream());
                var ws1 = package.Workbook.Worksheets.Add("Worksheet1");
                for (int row = 2; row < _accounts.Count + 2; row++)
                {
                    ws1.Cells[row, 1].Value = _accounts[row - 2].OrderId;
                    ws1.Cells[row, 2].Value = _accounts[row - 2].Order.Room.No;
                    ws1.Cells[row, 3].Value = _accounts[row - 2].Consume;
                    ws1.Cells[row, 4].Value = _accounts[row - 2].Balance;
                    ws1.Cells[row, 5].Value = _accounts[row - 2].Time;
                    ws1.Cells[row, 6].Value = _accounts[row - 2].Remark;
                }
                ws1.Cells[1, 1].Value = "客单";
                ws1.Cells[1, 2].Value = "房号";
                ws1.Cells[1, 3].Value = "消费";
                ws1.Cells[1, 4].Value = "结算";
                ws1.Cells[1, 5].Value = "时间";
                ws1.Cells[1, 6].Value = "备注";
                SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "Excel files (*.xlsx)|*.xlsx" };
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
