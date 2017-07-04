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
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace Resonance
{
    /// <summary>
    /// 动态显示扫频过程窗口
    /// </summary>
    public partial class SweepWin : Window
    {
        ObservableDataSource<Point> source = null;

        /// <summary>
        /// 构造函数，初始化界面
        /// </summary>
        public SweepWin()
        {
            InitializeComponent();
            source = new ObservableDataSource<Point>();
            source.SetXYMapping(p => p);
            lineGraph.DataSource = source;
        }

        /// <summary>
        /// 动态添加数据
        /// </summary>
        /// <param name="p">数据点</param>
        public void AddData(Point p)
        {
            source.AppendAsync(Dispatcher, p);
        }
    }
}
