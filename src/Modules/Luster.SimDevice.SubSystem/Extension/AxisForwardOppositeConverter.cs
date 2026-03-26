using Luster.Motion.DataStruct.Enums;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace Luster.SimDevice.SubSystem.Extension
{
    public class AxisForwardOppositeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AxisForward dir)
            {
                var opposite = GetOpposite(dir);
                return GetDescription(opposite);
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static AxisForward GetOpposite(AxisForward dir)
        {
            switch (dir)
            {
                case AxisForward.Left: return AxisForward.Right;
                case AxisForward.Right: return AxisForward.Left;
                case AxisForward.Front: return AxisForward.Behind;
                case AxisForward.Behind: return AxisForward.Front;
                case AxisForward.Up: return AxisForward.Down;
                case AxisForward.Down: return AxisForward.Up;
                default: return dir;
            }
        }

        private static string GetDescription(Enum e)
        {
            var fi = e.GetType().GetField(e.ToString());
            var attr = fi?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? e.ToString();
        }
    }
}