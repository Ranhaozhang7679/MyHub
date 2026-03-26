using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Luster.SimDevice.SubSystem.ViewModel;

namespace Luster.SimDevice.SubSystem.Views
{
    /// <summary>
    /// FXTCPContent.xaml 的交互逻辑
    /// </summary>
    public partial class FXTCPContent : UserControl
    {
        public FXTCPContent()
        {
            InitializeComponent();
        }
    }
    public static class EnumBindingSourceExtension
    {
        public static List<KeyValuePair<AspectModeEnum, string>> AspectModeEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(AspectModeEnum))
                    .Cast<AspectModeEnum>()
                    .Select(e => new KeyValuePair<AspectModeEnum, string>(e, GetEnumDescription(e)))
                    .ToList();
            }
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }
    }
}
