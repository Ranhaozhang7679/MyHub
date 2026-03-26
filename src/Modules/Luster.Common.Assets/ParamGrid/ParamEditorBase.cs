#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ParamEditorBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.ParamGrid
* 文 件 名:       ParamEditorBase.cs
* 创建时间:       2022/4/18 16:49:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      88bc3fbb-66e7-4a17-9caa-bc1893012293
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 16:49:21
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Luster.Common.Assets.ParamGrid
{
    public abstract class ParamEditorBase : DependencyObject
    {
        public abstract FrameworkElement CreateElement(ParamItem propertyItem);

        /// <summary>
        /// 创建绑定
        /// </summary>
        /// <param name="propertyItem"></param>
        /// <param name="element"></param>
        public virtual void CreateBinding(ParamItem propertyItem, DependencyObject element)
        {
            var dependencyProp = GetDependencyProperty();
           
            BindingOperations.SetBinding(element, GetDependencyProperty(),
               new Binding($"{propertyItem.PropertyName}")
               {
                   Source = propertyItem.Value,
                   Mode = GetBindingMode(propertyItem),
                   UpdateSourceTrigger = GetUpdateSourceTrigger(propertyItem),
                   Converter = GetConverter(propertyItem),
               });

        }

        /// <summary>
        /// 对应的依赖属性
        /// </summary>
        /// <returns></returns>
        public abstract DependencyProperty GetDependencyProperty();

        /// <summary>
        /// 绑定模式
        /// </summary>
        /// <param name="propertyItem"></param>
        /// <returns></returns>
        public virtual BindingMode GetBindingMode(ParamItem propertyItem) => propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;

        /// <summary>
        /// 获取更新方式
        /// </summary>
        /// <param name="propertyItem"></param>
        /// <returns></returns>
        public virtual UpdateSourceTrigger GetUpdateSourceTrigger(ParamItem propertyItem) => UpdateSourceTrigger.PropertyChanged;

        /// <summary>
        /// 获取转换方法
        /// </summary>
        /// <param name="propertyItem"></param>
        /// <returns></returns>
        protected virtual IValueConverter GetConverter(ParamItem propertyItem) => null;

        /// <summary>
        /// 参数变更事件
        /// </summary>
        public event Action<object> ParamItemValueChanged;

        protected void OnParamValueChanged(object newV) => ParamItemValueChanged?.Invoke(newV);
    }
}