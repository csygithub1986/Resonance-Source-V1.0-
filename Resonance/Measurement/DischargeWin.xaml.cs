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
    /// DischargeWin.xaml 的交互逻辑
    /// </summary>
    public partial class DischargeWin : Window
    {
        public bool IsOk;
        public int Discharge;

        public DischargeWin()
        {
            InitializeComponent();
            cbDischarge.Items.Add(5);
            cbDischarge.Items.Add(10);
            cbDischarge.Items.Add(50);
            cbDischarge.Items.Add(100);
            cbDischarge.Items.Add(500);
            cbDischarge.Items.Add(1000);
            cbDischarge.SelectedIndex = 4;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(cbDischarge.Text, out Discharge);
            if (Discharge==0|| Discharge>20000)
            {
                MessageBox.Show("标定电荷量请输入大于0，小于20000的正整数", "格式错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            IsOk = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsOk = false;
            this.Close();
        }
    }
}
