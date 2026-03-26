using Luster.SimDevice.EngineUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Luster.SimDevice.SubSystem.Views
{
    /// <summary>
    /// VAxisMContent.xaml 的交互逻辑
    /// </summary>
    public partial class AxisIODebugContent
    {
        public AxisIODebugContent()
        {
            InitializeComponent();
        }

    }


#if false
    /// <summary>
    /// IO 模板
    /// </summary>
    public class ValueTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// 数字模板
        /// </summary>
        public DataTemplate DigInTemplate { get; set; }

        /// <summary>
        /// 模拟量模板
        /// </summary>
        public DataTemplate DigOutTemplate { get; set; }

        /// <summary>
        /// 数字模板
        /// </summary>
        public DataTemplate AnaInTemplate { get; set; }

        /// <summary>
        /// 模拟量模板
        /// </summary>
        public DataTemplate AnaOutTemplate { get; set; }

        /// <summary>
        /// 方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null && item is IOModel model)
            {
                if (model.IsDigital)
                {
                    if (model.IsIn)
                    {
                        return DigInTemplate;
                    }
                    else
                    {
                        return DigOutTemplate;
                    }
                }
                else
                {
                    if (model.IsIn)
                    {
                        return AnaInTemplate;
                    }
                    else
                    {
                        return AnaOutTemplate;
                    }
                }

            }

            return DigInTemplate;
        }
    }
#endif
}
