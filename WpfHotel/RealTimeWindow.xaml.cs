using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OfficeOpenXml;

namespace WpfHotel
{
    /// <summary>
    ///     RealTimeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RealTimeWindow : Window
    {
        private readonly List<RoomItem> _roomItems = new List<RoomItem>();

        public RealTimeWindow()
        {
            InitializeComponent();
            using (var db = new hotelEntities())
            {
                var rooms = db.Room.Include("Type").ToList();
                foreach (var room in rooms)
                    _roomItems.Add(new RoomItem {Room = room});
                DataGrid.ItemsSource = _roomItems;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
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

        private void Export(object sender, RoutedEventArgs e)
        {
            try
            {
                var package = new ExcelPackage(new MemoryStream());
                var ws1 = package.Workbook.Worksheets.Add("Worksheet1");
                for (var row = 2; row < _roomItems.Count + 2; row++)
                {
                    ws1.Cells[row, 1].Value = _roomItems[row - 2].Room.No;
                    ws1.Cells[row, 2].Value = _roomItems[row - 2].Room.Type.Name;
                    ws1.Cells[row, 3].Value = _roomItems[row - 2].Status;
                    ws1.Cells[row, 4].Value = _roomItems[row - 2].HasPeople;
                    ws1.Cells[row, 5].Value = _roomItems[row - 2].UserName;
                    ws1.Cells[row, 6].Value = _roomItems[row - 2].IsReadyLeave;
                }
                ws1.Cells[1, 1].Value = "房间号";
                ws1.Cells[1, 2].Value = "主题";
                ws1.Cells[1, 3].Value = "房态";
                ws1.Cells[1, 4].Value = "有无客人(0-无 1-有)";
                ws1.Cells[1, 5].Value = "客人姓名";
                ws1.Cells[1, 6].Value = "预离(0-否 1-是)";
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