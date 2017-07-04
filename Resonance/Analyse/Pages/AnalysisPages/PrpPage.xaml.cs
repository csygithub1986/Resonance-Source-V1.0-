using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;


namespace Resonance
{
    /// <summary>
    /// PRPD图显示页面
    /// </summary>
    public partial class PrpPage : Page
    {
        public PrpPage()
        {
            InitializeComponent();
            _this = this;
            Display(AnalyseState.Instance.Prp, lbPhase.SelectedIndex);
        }

        public static PrpPage _this;

        private void Page_Initialized(object sender, EventArgs e)
        {
            string[] pstr = { "A相", "B相", "C相" };
            for (int i = 0; i < 3; i++)
            {
                ListBoxItem lbi = new ListBoxItem();
                lbi.Content = pstr[i];
                lbi.Tag = i;
                lbi.Margin = new Thickness(0, 3, 3, 0);
                lbi.BorderBrush = Brushes.Gray;
                lbi.BorderThickness = new Thickness(1);
                lbi.Selected += new RoutedEventHandler(lbi_Selected);
                lbPhase.Items.Add(lbi);
            }
            ((ListBoxItem)lbPhase.Items[0]).IsSelected = true;
        }

        /// <summary>
        /// 选定特定相序，并绘制PRPD
        /// </summary>
        void lbi_Selected(object sender, RoutedEventArgs e)
        {
            Display(AnalyseState.Instance.Prp, (int)((ListBoxItem)sender).Tag);
        }

        /// <summary>
        /// 绘制PRPD
        /// </summary>
        /// <param name="prpPairs">PRPD数据</param>
        /// <param name="phase">相序</param>
        private void Display(List<Point>[] prpPairs, int phase)
        {
            List<Point> prpPair = prpPairs[phase];
            plotterPrp.Children.RemoveAll(typeof(RectangleHighlight));
            if (prpPair.Count == 0)
            {
                return;
            }
            foreach (var item in prpPair)
            {
                RectangleHighlight rh = new RectangleHighlight(FixPosition.Center, item, 3, 3);
                //Color temp = Params.Colors[phase];
                Color temp = Colors.Black;
                temp.A = 200;
                rh.Fill = new SolidColorBrush(temp);
                rh.StrokeThickness = 0;
                plotterPrp.Children.Add(rh);
            }
            double max = prpPair.Max(a => Math.Abs(a.Y));
            plotterPrp.Visible = new Rect(0, -max * 1.1, 360, 2.2 * max);
            DisplayHV(max);
        }

        /// <summary>
        /// 绘制参考电压
        /// </summary>
        /// <param name="max">prpd数据的最大值，用以绘制参考曲线</param>
        void DisplayHV(double max)
        {
            double[] xx = new double[361];
            double[] yy = new double[361];
            for (int i = 0; i < 361; i++)
            {
                xx[i] = i;
                yy[i] = max * 0.9 * Math.Sin(i / 360.0 * 2 * Math.PI);// +
            }
            CompositeDataSource cds = new CompositeDataSource();
            EnumerableDataSource<double> baseXData = new EnumerableDataSource<double>(xx);
            EnumerableDataSource<double> baseYData = new EnumerableDataSource<double>(yy);
            baseXData.SetXMapping(x => x);//(必须)
            baseYData.SetYMapping(y => y);
            cds = baseXData.Join(baseYData);
            lineGraphPrp.DataSource = cds;
            baseYData.RaiseDataChanged();
        }

    }
}
