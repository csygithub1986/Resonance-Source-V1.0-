using Resonance.Properties;
using System;
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
using System.Windows.Shapes;

namespace Resonance
{
    /// <summary>
    /// SystemSetWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SystemSetWindow : Window
    {
        public SystemSetWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //加压等级
            foreach (var item in Settings.Default.VoltageList)
            {
                listboxVoltage.Items.Add(item);
            }
        }

        #region 加压等级
        private void btnDeleteVoltage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddVoltage_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

    }
}
