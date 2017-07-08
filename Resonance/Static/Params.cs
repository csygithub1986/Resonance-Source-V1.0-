using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;

namespace Resonance
{
    /// <summary>
    /// 全局参数类
    /// </summary>
    public class Params
    {
        public static string IP = "192.168.0.105";
        public const int Port = 14519;

        public const double SamRateHv = 125; // s/ms
        public const double SamRatePd = 125000;// s/ms
        public const int Multi = 1000;//PD和HV采样率比值

        public const int CalibPack = 1;

        //幅值换算比例
        public const double HV_Coeffi = 0.00152;
        public const double PD_Coeffi = 0.122;

        //颜色相关，需要界面线程添加
        public static Color Color1;// = (Color)ColorConverter.ConvertFromString("#DFA705");
        public static Color Color2;//= (Color)ColorConverter.ConvertFromString("#058205");
        public static Color Color3;// = (Color)ColorConverter.ConvertFromString("#FF0000");
        public static Color Color4;//= (Color)ColorConverter.ConvertFromString("#E46C0A");
        public static SolidColorBrush Brush1;//= new SolidColorBrush(Color1);
        public static SolidColorBrush Brush2;// = new SolidColorBrush(Color2);
        public static SolidColorBrush Brush3;// = new SolidColorBrush(Color3);
        public static SolidColorBrush Brush4;//= new SolidColorBrush(Color4);
        public static Color[] Colors;// = new Color[] { Color1, Color2, Color3, Color4 };
        public static SolidColorBrush[] Brushes;// = new SolidColorBrush[] { Brush1, Brush2, Brush3, Brush4 };

        public const string FolderExName = "振荡波测试";
        public const int RetainPeriod = 2;//谐振保留周期
        public const double R = 25;//电阻
        public const double L = 1;//电感

        public static double AF_L = 1;//us 计算脉宽时的窗口长度
        public static double AF_TCondition = 0.2;//脉宽常数
        public static double GlobleThresh = 20;
        public static double[] Range = { 0.1, 0.2, 1, 10 };//四种量程

        //DischargeUnit
        static Params()
        {
            DischargeUnit = Properties.Settings.Default.DischargeUnit == 0 ? "放电幅值 (mV)" : "放电量 (pC)";
            MaxDischarge = Properties.Settings.Default.DischargeUnit == 0 ? "最大放电幅值" : "最大放电量";
            UnitChar = Properties.Settings.Default.DischargeUnit == 0 ? "mV" : "pC";
            mVTopC = new double[3];
        }
        public static string DischargeUnit { get; set; }
        public static string MaxDischarge { get; set; }
        public static string UnitChar { get; set; }
        public static double[] mVTopC;
    }
}
