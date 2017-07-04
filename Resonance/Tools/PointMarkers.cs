using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace Resonance
{
    /// <summary>
    /// 矩形标记
    /// </summary>
    public class RectPointMarker : Microsoft.Research.DynamicDataDisplay.PointMarkers.ShapePointMarker
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public override void Render(DrawingContext dc, Point screenPoint)
        {
            dc.DrawRectangle(Fill, Pen, new Rect(screenPoint.X - Width / 2, screenPoint.Y - Height / 2, Width, Height));
        }
    }

    /// <summary>
    /// 多角星标记
    /// </summary>
    public class StarPointMarker : Microsoft.Research.DynamicDataDisplay.PointMarkers.ShapePointMarker
    {
        public override void Render(System.Windows.Media.DrawingContext dc, System.Windows.Point screenPoint)
        {
            dc.DrawGeometry(Fill, Pen, getGeometry(5, Size, screenPoint));
        }

        private Geometry getGeometry(int angles, double size, System.Windows.Point center)
        {
            //五角星外围的点角度的一个数组
            double[] angles1 = GetAngles(-Math.PI / 2, angles);
            //五角星内围的点角度的一个数组
            double[] angles2 = GetAngles(-Math.PI / 2 + Math.PI / angles, angles);
            //五角星外围的点的一个数组
            System.Windows.Point[] points1 = GetPoints(center, size, angles1);
            //五角星内围的点的一个数组
            System.Windows.Point[] points2 = GetPoints(center, size * 0.382, angles2);
            //最终合成多边形所有点的数组
            System.Windows.Point[] points = new System.Windows.Point[points1.Length + points2.Length];
            for (int i = 0, j = 0; i < points.Length; i += 2, j++)
            {
                points[i] = points1[j];
                points[i + 1] = points2[j];
            }

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = points[points.Length - 1];
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment myLineSegment = new LineSegment() { Point = points[i] };
                myPathSegmentCollection.Add(myLineSegment);
            }

            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;

            return myPathGeometry;
        }

        private System.Windows.Point GetPoint(System.Windows.Point ptCenter, double length, double angle)
        {
            return new System.Windows.Point(
                (ptCenter.X + length * Math.Cos(angle)),
                (ptCenter.Y + length * Math.Sin(angle)));
        }

        private System.Windows.Point[] GetPoints(System.Windows.Point ptCenter, double length, params double[] angles)
        {
            System.Windows.Point[] points = new System.Windows.Point[angles.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = GetPoint(ptCenter, length, angles[i]);
            }
            return points;
        }

        private double[] GetAngles(double startAngle, int pointed)
        {
            double[] angles = new double[pointed];
            angles[0] = startAngle;
            for (int i = 1; i < angles.Length; i++)
            {
                angles[i] = angles[i - 1] + GetAngleLength(pointed);
            }
            return angles;
        }

        private double GetAngleLength(int pointed)
        {
            return 2 * Math.PI / pointed;
        }

    }

    /// <summary>
    /// 正多边形
    /// </summary>
    public class PolygonPointMarker : Microsoft.Research.DynamicDataDisplay.PointMarkers.ShapePointMarker
    {
        public override void Render(System.Windows.Media.DrawingContext dc, System.Windows.Point screenPoint)
        {
            dc.DrawGeometry(Fill, Pen, getGeometry(3, Size, screenPoint));
        }

        private Geometry getGeometry(int angleNum, double size, System.Windows.Point center)
        {
            double[] angles = GetAngles(-Math.PI / 2, angleNum);
            System.Windows.Point[] points = GetPoints(center, size, angles);

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = points[points.Length - 1];
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment myLineSegment = new LineSegment() { Point = points[i] };
                myPathSegmentCollection.Add(myLineSegment);
            }

            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;

            return myPathGeometry;
        }

        private System.Windows.Point GetPoint(System.Windows.Point ptCenter, double length, double angle)
        {
            return new System.Windows.Point(
                (ptCenter.X + length * Math.Cos(angle)),
                (ptCenter.Y + length * Math.Sin(angle)));
        }

        private System.Windows.Point[] GetPoints(System.Windows.Point ptCenter, double length, params double[] angles)
        {
            System.Windows.Point[] points = new System.Windows.Point[angles.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = GetPoint(ptCenter, length, angles[i]);
            }
            return points;
        }

        private double[] GetAngles(double startAngle, int pointed)
        {
            double[] angles = new double[pointed];
            angles[0] = startAngle;
            for (int i = 1; i < angles.Length; i++)
            {
                angles[i] = angles[i - 1] + GetAngleLength(pointed);
            }
            return angles;
        }

        private double GetAngleLength(int pointed)
        {
            return 2 * Math.PI / pointed;
        }

    }

    /// <summary>
    /// 三角，且中心在顶点
    /// </summary>
    public class TrianglePointMarker : Microsoft.Research.DynamicDataDisplay.PointMarkers.ShapePointMarker
    {
        private int _direction = -1;
        /// <summary>
        /// 1为正三角，-1为倒三角
        /// </summary>
        public int Direction
        {
            get
            { return _direction; }
            set
            {
                if (value > 0)
                    _direction = 1;
                else
                    _direction = -1;
            }
        }

        public override void Render(System.Windows.Media.DrawingContext dc, System.Windows.Point screenPoint)
        {
            dc.DrawGeometry(Fill, Pen, getGeometry(3, Size, screenPoint));
        }

        private Geometry getGeometry(int angleNum, double size, System.Windows.Point center)
        {
            Point[] points = new Point[] { center, new Point(center.X + size / 2, center.Y + size * Direction), new Point(center.X - size / 2, center.Y + size * Direction) };

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = points[points.Length - 1];
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment myLineSegment = new LineSegment() { Point = points[i] };
                myPathSegmentCollection.Add(myLineSegment);
            }

            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;

            return myPathGeometry;
        }
    }

    /// <summary>
    /// 三角形，带文字
    /// </summary>
    public class TrianglePointMarkerAndText : Microsoft.Research.DynamicDataDisplay.PointMarkers.ShapePointMarker
    {
        private int _direction = -1;
        public string Text;
        /// <summary>
        /// 1为正三角，-1为倒三角
        /// </summary>
        public int Direction
        {
            get
            { return _direction; }
            set
            {
                if (value > 0)
                    _direction = 1;
                else
                    _direction = -1;
            }
        }

        public override void Render(System.Windows.Media.DrawingContext dc, System.Windows.Point screenPoint)
        {
            dc.DrawGeometry(Fill, Pen, getGeometry(3, Size, screenPoint));
            dc.DrawText(new FormattedText(Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 20, Brushes.Black), new Point(screenPoint.X-7, screenPoint.Y - Size-24));
        }

        private Geometry getGeometry(int angleNum, double size, System.Windows.Point center)
        {
            Point[] points = new Point[] { center, new Point(center.X + size / 2, center.Y + size * Direction), new Point(center.X - size / 2, center.Y + size * Direction) };

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = points[points.Length - 1];
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment myLineSegment = new LineSegment() { Point = points[i] };
                myPathSegmentCollection.Add(myLineSegment);
            }

            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;

            return myPathGeometry;
        }
    }


    /// <summary>
    /// 十字标记
    /// </summary>
    public class CrossPointMarker : Microsoft.Research.DynamicDataDisplay.PointMarkers.ShapePointMarker
    {
        public override void Render(System.Windows.Media.DrawingContext dc, System.Windows.Point screenPoint)
        {
            dc.DrawGeometry(Fill, Pen, getGeometry(Size, screenPoint));
        }

        private Geometry getGeometry(double size, System.Windows.Point center)
        {
            Vector v = new Vector(center.X, center.Y);
            Point[] points = new Point[12];
            points[0] = new Point(size / 2, 0) + v;
            points[1] = new Point(size, size / 2) + v;
            points[2] = new Point(size / 2, size) + v;
            points[3] = new Point(0, size / 2) + v;
            points[4] = new Point(-size / 2, size) + v;
            points[5] = new Point(-size, size / 2) + v;
            points[6] = new Point(-size / 2, 0) + v;
            points[7] = new Point(-size, -size / 2) + v;
            points[8] = new Point(-size / 2, -size) + v;
            points[9] = new Point(0, -size / 2) + v;
            points[10] = new Point(size / 2, -size) + v;
            points[11] = new Point(size, -size / 2) + v;


            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = points[points.Length - 1];
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            for (int i = 0; i < points.Length; i++)
            {
                LineSegment myLineSegment = new LineSegment() { Point = points[i] };
                myPathSegmentCollection.Add(myLineSegment);
            }

            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;

            return myPathGeometry;
        }
    }

}
