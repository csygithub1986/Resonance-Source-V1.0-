using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Resonance
{
    /// <summary>
    /// 噪声信息
    /// </summary>
    public class NoiseInfo
    {
        /// <summary>
        /// 量程
        /// </summary>
        public int RangeIndex;

        /// <summary>
        /// 噪声数据
        /// </summary>
        public short[] NoiseData;

        /// <summary>
        /// 从文件中读入噪声数据
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static NoiseInfo ReadFile(FileInfo fileInfo)
        {
            NoiseInfo cd = new NoiseInfo();
            using (BinaryReader bw = new BinaryReader(fileInfo.OpenRead()))
            {
                cd.RangeIndex = bw.ReadInt32();//量程

                int showLen = bw.ReadInt32();
                byte[] temp = bw.ReadBytes(showLen * 2);
                cd.NoiseData = new short[showLen];
                for (int i = 0; i < showLen; i++)
                {
                    cd.NoiseData[i] = BitConverter.ToInt16(temp, i * 2);
                }
            }
            return cd;
        }

        /// <summary>
        /// 将噪声数据写入文件
        /// </summary>
        /// <param name="noiseData"></param>
        /// <param name="fileInfo"></param>
        public static void WriteFile(NoiseInfo noiseData, FileInfo fileInfo)
        {
            using (BinaryWriter bw = new BinaryWriter(fileInfo.Create()))
            {
                bw.Write(noiseData.RangeIndex);
                //alldata
                bw.Write(noiseData.NoiseData.Length);
                byte[] temp = new byte[noiseData.NoiseData.Length * 2];
                for (int i = 0; i < noiseData.NoiseData.Length; i++)
                {
                    byte[] bs = BitConverter.GetBytes(noiseData.NoiseData[i]);
                    temp[2 * i] = bs[0];
                    temp[2 * i + 1] = bs[1];
                }
                bw.Write(temp);
            }

        }
    }
}
