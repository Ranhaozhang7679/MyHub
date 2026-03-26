using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.EngineUI.Models;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Luster.SimDevice.SubSystem.Extension
{
    public class AxisMoveDirectionIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] : AxisModel
            // values[1] : "Positive" or "Negative"

            if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
            { 
                return DependencyProperty.UnsetValue;
            }
            var axis = values[0] as AxisModel;
            string direction = values[1] as string;

            try
            {
                string typeStr;
                if ((axis.AxisType == AxisType.U) || (axis.AxisType == AxisType.U2))
                {
                    typeStr = "Rotary";
                }
                else
                {
                    typeStr = "Linear";
                }

                string dir = GetDirection(direction, axis.AxisForward).ToString();
                string key = $"{typeStr}{dir}";

                return Application.Current.TryFindResource(GetGeometryIconName(key, direction));
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        private AxisForward GetDirection(string direction, AxisForward dir)
        {
            if (direction == "Positive")
            {
                switch (dir)
                {
                    case AxisForward.Left: return AxisForward.Left;
                    case AxisForward.Right: return AxisForward.Right;
                    case AxisForward.Front: return AxisForward.Up;
                    case AxisForward.Behind: return AxisForward.Down;
                    case AxisForward.Up: return AxisForward.Up;
                    case AxisForward.Down: return AxisForward.Down;
                    default: return dir;
                }
            }
            else
            {
                switch (dir)
                {
                    case AxisForward.Left: return AxisForward.Right;
                    case AxisForward.Right: return AxisForward.Left;
                    case AxisForward.Front: return AxisForward.Down;
                    case AxisForward.Behind: return AxisForward.Up;
                    case AxisForward.Up: return AxisForward.Down;
                    case AxisForward.Down: return AxisForward.Up;
                    default: return dir;
                }
            }
        }
        private string GetGeometryIconName(string iconKey, string direction)
        {
            switch (iconKey)
            {
                case "LinearLeft": return "LeftGeometry";
                case "LinearRight": return "RightGeometry";
                case "LinearUp": return "UpGeometry";
                case "LinearDown": return "DownGeometry";
                case "RotaryLeft": return "RotateLeftGeometry";
                case "RotaryRight": return "RotateRightGeometry";
                default:
                    if (direction == "Positive")
                    {
                        return "AddGeometry";
                    }
                    else
                    { 
                        return "SubGeometry";
                    }
            }
        }

    }

}