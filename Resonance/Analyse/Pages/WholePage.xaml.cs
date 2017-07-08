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
    /// 分析页面的总框架，承载所有全局数据
    /// </summary>
    public partial class WholePage : Page
    {
        public WholePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 返回菜单
        /// </summary>
        private void menuReturn_Click(object sender, RoutedEventArgs e)
        {
            PulsePair.WriteMapFile(new FileInfo(AnalyseState.Instance.Path.FullName + "/result.map"));
            ChooseFilePage cfp = new ChooseFilePage();
            NavigationService.Navigate(cfp);
        }

        /// <summary>
        /// 分析菜单
        /// </summary>
        private void menuAnalyse_Click(object sender, RoutedEventArgs e)
        {
            MapPage._this.StartAnalyse();
        }

        /// <summary>
        /// 报告菜单
        /// </summary>
        private void menuReport_Click(object sender, RoutedEventArgs e)
        {
            ReportWin w = new ReportWin();
            w.Owner = MainWindow.Instance;
            w.ShowDialog();
        }

        /// <summary>
        /// 帮助菜单
        /// </summary>
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 当前页面返回上一层是，自动保存当前分析结果
        /// </summary>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //如果是导入，此为一次无用的操作
            PulsePair.WriteMapFile(new FileInfo(AnalyseState.Instance.Path.FullName + "/result.map"));
        }

        /// <summary>
        /// 分析参数设置菜单
        /// </summary>
        private void menuParamSet_Click(object sender, RoutedEventArgs e)
        {
            LocParamWin lpw = new LocParamWin(false);
            lpw.Owner = MainWindow.Instance;
            lpw.ShowDialog();
            if (lpw.IsOk)
            {
                MapPage._this.StartAnalyse();
            }
        }

    }//end class

}
