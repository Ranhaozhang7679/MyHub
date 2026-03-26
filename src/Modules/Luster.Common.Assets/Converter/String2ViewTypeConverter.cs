using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HandyControl.Data;
using HandyControl.Tools;

//using MoonPdfLib;

namespace Luster.Common.Assets.Converter
{
    public class String2ViewTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //string path = (string)value;
            //if (!string.IsNullOrEmpty(path))
            //{
            //    switch (path)
            //    {
            //        case "SinglePage":
            //            {
            //                return ViewType.SinglePage;
            //            }

            //        case "Facing":
            //            {
            //                return ViewType.Facing;
            //            }

            //        case "BookView":
            //            {
            //                return ViewType.BookView;
            //            }

            //        default:
            //            {
            //                return ViewType.SinglePage;
            //            }

            //    }

            //}
            //else
            //{
            //    return null;
            //}

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}