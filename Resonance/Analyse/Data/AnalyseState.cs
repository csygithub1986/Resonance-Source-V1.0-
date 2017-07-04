using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Windows;

namespace Resonance
{
    /// <summary>
    /// 管理全局分析结果
    /// </summary>
    public class AnalyseState
    {
        /// <summary>
        /// 当前目录
        /// </summary>
        public DirectoryInfo Path { get; set; }

        /// <summary>
        /// 文件名—数据键值对
        /// </summary>
        public Dictionary<string, DataInfo> DataInfos { get; set; }

        /// <summary>
        /// 高压电缆参数
        /// </summary>
        public HvParam HvParam { get; set; }

        /// <summary>
        /// 每次更新分析结果时更新
        /// </summary>
        public List<Point>[] Prp { get; set; } 

        /// <summary>
        /// 全局分析结果
        /// </summary>
        public Dictionary<string, List<PulsePair>> AllMapResults { get; set; }

        /// <summary>
        /// 标定信息
        /// </summary>
        public CalibrationInfo[] CalibrationInfos;

        #region 可新建实例的单例

        public static AnalyseState Instance;

        public AnalyseState()
        {
            DataInfos = new Dictionary<string, DataInfo>();
            AllMapResults = new Dictionary<string, List<PulsePair>>();
            Prp = new List<Point>[3];
            for (int i = 0; i < 3; i++)
            {
                Prp[i] = new List<Point>();
            }
            CalibrationInfos = new CalibrationInfo[3];
            Instance = this;
        }
        #endregion
    }
}
