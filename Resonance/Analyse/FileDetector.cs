using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Resonance
{
    /// <summary>
    /// 检测目录是否是存放合法数据的文件夹
    /// </summary>
    public static class FileDetector
    {
        /// <summary>
        /// 获得文件夹内的合法数据信息，为null则不合法
        /// </summary>
        /// <param name="dire"></param>
        /// <returns></returns>
        public static CableInfo GetValid(DirectoryInfo dire)
        {
            CableInfo cableinfo = null;
            try
            {
                FileInfo[] cal = dire.GetFiles("start.info");
                if (cal.Length != 1)
                {
                    return null;
                }
                cableinfo = CableInfo.ReadFile(new FileInfo(dire.FullName + "\\start.info"));
            }
            catch (Exception)
            {
                return null;
            }
            cableinfo.Path = dire;
            return cableinfo;
        }

    }
}
