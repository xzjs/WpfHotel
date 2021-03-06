﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfHotel
{
    /// <summary>
    ///     BillWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BillWindow : Window
    {
        private readonly ObservableCollection<Account> _accounts;
        private readonly Order _order;

        public BillWindow(Order order)
        {
            _order = order;
            InitializeComponent();
            using (var db= new hotelEntities())
            {
                List<Account> accounts = db.Account.Where(a => a.OrderId == order.Id).ToList();
                _accounts = new ObservableCollection<Account>(accounts);
                AccountDataGrid.ItemsSource = _accounts;
            }
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
                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        ///     入账函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Account(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var accountWindow = new AccountWindow(_accounts, _order.Id, button.Content.ToString());
            accountWindow.ShowDialog();
        }

        /// <summary>
        ///     结账
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckOut(object sender, RoutedEventArgs e)
        {
            var checkOutWindow = new CheckOutWindow(_order);
            checkOutWindow.Show();
            Close();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            decimal consumeTotal = 0, balanceTotal = 0;
            double remain = 0;
            foreach (var account in _accounts)
            {
                consumeTotal += account.Consume.Value;
                balanceTotal += account.Balance.Value;
            }
            remain = Convert.ToDouble(balanceTotal - consumeTotal);
            ConsumeTotalTextBlock.Text = consumeTotal.ToString(CultureInfo.InvariantCulture);
            BalanceTotalTextBlock.Text = balanceTotal.ToString(CultureInfo.InvariantCulture);
            RemainTextBlock.Text = remain.ToString(CultureInfo.InvariantCulture);
        }

        private void ShowOrder(object sender, RoutedEventArgs e)
        {
            CheckInWindow checkInWindow=new CheckInWindow(_order);
            checkInWindow.Show();
            Close();
        }
    }
}