﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Resonance
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 测试按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            CableInfoWin w = new CableInfoWin();
            w.Owner = MainWindow.Instance;
            w.ShowDialog();

            if (!w.Ok)
            {
                return;
            }
            MeasureState.CableInfo = w.Info;

            WlanPage wp = new WlanPage();
            NavigationService.Navigate(wp);

            MainWindow.Instance.SettingBtn.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 分析按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResult_Click(object sender, RoutedEventArgs e)
        {
            ChooseFilePage p = new ChooseFilePage();
            NavigationService.Navigate(p);
            MainWindow.Instance.SettingBtn.Visibility = Visibility.Collapsed;
        }
    }
}
