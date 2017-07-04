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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;


namespace Resonance
{
    /// <summary>
    /// 承载MapPage和PrpPage的主窗体
    /// </summary>
    public partial class AnalysisPage : Page
    {
        public AnalysisPage()
        {
            InitializeComponent();
            _this = this;
        }

        private static AnalysisPage _this;

        //public static void GotoTab(int tabIndex)
        //{
        //    _this.analysisTab.SelectedIndex = tabIndex;
        //}

    }
}
