using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Resonance
{
    /// <summary>
    /// 原控件不支持左键框选缩放，该类主要用于扩展该效果
    /// </summary>
    public class PlotterWrap
    {
        private ChartPlotter plotter;
        FrameworkElement window;

        public PlotterWrap Wrap(ChartPlotter plotter, FrameworkElement window)
        {
            this.plotter = plotter;
            this.window = window;

            zoomRect.Fill = Brushes.Green;
            zoomRect.Opacity = 0.2;
            zoomRect.Visibility = Visibility.Hidden;
            plotter.Children.Add(zoomRect);

            plotter.Children.Remove(plotter.MouseNavigation);
            plotter.MouseLeftButtonDown += new MouseButtonEventHandler(plotter_MouseLeftButtonDown);
            plotter.MouseLeftButtonUp += new MouseButtonEventHandler(plotter_MouseLeftButtonUp);
            plotter.MouseMove += new MouseEventHandler(plotter_MouseMove);

            plotter.VerticalAxisNavigation.MouseEnter += new MouseEventHandler(VerticalAxisNavigation_MouseEnter);
            plotter.VerticalAxisNavigation.MouseLeave += new MouseEventHandler(AxisNavigation_MouseLeave);
            plotter.HorizontalAxisNavigation.MouseEnter += new MouseEventHandler(HorizontalAxisNavigation_MouseEnter);
            plotter.HorizontalAxisNavigation.MouseLeave += new MouseEventHandler(AxisNavigation_MouseLeave);

            return this;
        }

        RectangleHighlight zoomRect = new RectangleHighlight();
        bool plotterMouseDown = false;
        Point originP;

        private void plotter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            plotterMouseDown = true;
            originP = e.GetPosition(plotter.CentralGrid);
            originP = originP.ScreenToViewport(plotter.Transform);
        }

        private void plotter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            plotterMouseDown = false;
            zoomRect.Visibility = Visibility.Hidden;

            Point screenP1 = originP.ViewportToScreen(plotter.Transform);//原始鼠标的屏幕点
            Point p = e.GetPosition(plotter.CentralGrid);//结束屏幕点
            if ((screenP1.X - p.X <= 5 && screenP1.X - p.X >= -5) || (screenP1.Y - p.Y <= 5 && screenP1.Y - p.Y >= -5))//小于两个像素不动作
            {
                return;
            }

            p = p.ScreenToViewport(plotter.Transform);
            plotter.Viewport.Visible = new Rect(originP, p);
        }

        private void plotter_MouseMove(object sender, MouseEventArgs e)
        {
            if (plotterMouseDown)
            {
                Point p = e.GetPosition(plotter.CentralGrid);
                p = p.ScreenToViewport(plotter.Transform);

                zoomRect.Bounds = new Rect(originP, p);
                zoomRect.Visibility = Visibility.Visible;
            }
        }

        public void HorizontalAxisNavigation_MouseEnter(object sender, MouseEventArgs e)
        {
            window.Cursor = Cursors.ScrollWE;
        }

        public void VerticalAxisNavigation_MouseEnter(object sender, MouseEventArgs e)
        {
            window.Cursor = Cursors.ScrollNS;
        }

        public void AxisNavigation_MouseLeave(object sender, MouseEventArgs e)
        {
            window.Cursor = Cursors.Arrow;
        }

    }
}
