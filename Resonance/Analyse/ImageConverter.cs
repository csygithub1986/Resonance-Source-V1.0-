using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Resonance
{
    /// <summary>
    /// 图片转换类
    /// </summary>
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        /// <summary>
        /// 把对象转换为WPF程序中绑定目标（即控件的某个属性）可以使用的类型数据
        /// </summary>
        /// <param name="value">要转换的对象</param>
        /// <param name="targetType">标示所需要的类型</param>
        /// <param name="parameter">转换所需的参数</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //在载入数据的时候将数据转换为图片
            try
            {
                Uri uri = new Uri((string)value, UriKind.RelativeOrAbsolute);
                BitmapImage img = new BitmapImage(uri);
                return img;//返回BitmapImage对象
            }
            catch
            {
                return new BitmapImage();
            }
        }
        /// <summary>
        /// 与Convert相反，可以将控件的属性再转变成数据源类型的对象
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //将图片类型转成数据
            BitmapImage img = value as BitmapImage;
            return img.UriSource.AbsoluteUri;
        }
    }
}
