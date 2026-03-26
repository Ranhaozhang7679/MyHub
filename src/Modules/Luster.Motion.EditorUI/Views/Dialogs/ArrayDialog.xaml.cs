using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.EditorUI.Models;
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

namespace Luster.Motion.EditorUI.Views.Dialogs
{
    /// <summary>
    /// ArrayDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ArrayDialog : UserControl
    {
        public ArrayDialog()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// 数据类型模板
    /// </summary>
    public class DataTypeTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// 数字模板
        /// </summary>
        public DataTemplate IntTemplate { get; set; }

        /// <summary>
        /// 模拟量模板
        /// </summary>
        public DataTemplate BoolTemplate { get; set; }

        /// <summary>
        /// 字符串模块
        /// </summary>
        public DataTemplate StringTemplate { get; set; }

        /// <summary>
        /// 方法
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var iModel = item as ValueModel;  
            if (iModel == null) return BoolTemplate;

            var vType = iModel.Value.GetType();
            if (vType == typeof(bool))
            {
                return BoolTemplate;
            }
            else if (vType == typeof(string))
            {
                return StringTemplate;
            }
            else if (vType == typeof(int))
            {
                return IntTemplate;
            }
            else
            {
                throw new FriendlyException($"类型不支持:{item.GetType()}");
            }
        }
    }
}
