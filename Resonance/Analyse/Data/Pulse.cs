using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Resonance
{
    /// <summary>
    /// 表示一个脉冲
    /// </summary>
    [Serializable]
    public class Pulse
    {
        /// <summary>
        /// 峰值
        /// </summary>
        public double Amplitude;

        /// <summary>
        /// 相位
        /// </summary>
        public double Phase;

        /// <summary>
        /// 入射峰时间
        /// </summary>
        public int Index;

        /// <summary>
        /// 归一化脉宽，越窄脉冲越陡峭
        /// </summary>
        public double T;

        /// <summary>
        /// 归一化带宽，越宽说明脉冲越原始
        /// </summary>
        public double F;
    }
}
