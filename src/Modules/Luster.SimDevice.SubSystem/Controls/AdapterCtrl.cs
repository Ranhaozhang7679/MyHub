#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AdapterCtrl
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.Controls
* 文 件 名:       AdapterCtrl.cs
* 创建时间:       2022/4/18 15:19:13
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      0db1d83a-2218-4a7f-9c82-040ecd9e4495
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 15:19:13
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.SimDevice.SubSystem.Controls
{
    /// <summary>
    /// 连接配置UI
    /// 1.加载方式
    /// </summary>
    public class AdapterCtrl : System.Windows.Controls.Control
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        public AdapterCtrl()
        {

        }

        public object Adapter
        {
            get { return (object)GetValue(AdapterProperty); }
            set { SetValue(AdapterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Adapter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdapterProperty =
            DependencyProperty.Register("Adapter", typeof(object), typeof(AdapterCtrl), new PropertyMetadata(0));



        /// <summary>
        /// 连接方式
        /// </summary>
        public List<string> AdapterItems
        {
            get { return (List<string>)GetValue(AdapterItemsProperty); }
            set { SetValue(AdapterItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdapterItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdapterItemsProperty =
            DependencyProperty.Register("AdapterItems", typeof(List<string>), typeof(AdapterCtrl), new PropertyMetadata(default));



        public object AdapterObj
        {
            get { return (object)GetValue(AdapterObjProperty); }
            set { SetValue(AdapterObjProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AdapterObj.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AdapterObjProperty =
            DependencyProperty.Register("AdapterObj", typeof(object), typeof(AdapterCtrl), new PropertyMetadata(0));

        /// <summary>
        /// Button 按钮
        /// </summary>
        protected const string PART_Items = nameof(PART_Items);
        private ComboBox combItems = null;
        public ComboBox ViewCommbox
        {
            get
            {
                return combItems;
            }
            set
            {
                // 先反注册事件
                if (combItems != null)
                {
                    combItems.SelectionChanged -= CombItems_SelectionChanged;
                }

                combItems = value;

                if (combItems != null)
                {
                    combItems.SelectionChanged += CombItems_SelectionChanged;
                }
            }
        }

        /// <summary>
        /// 事件切换工
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void CombItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0) return;
            var selectObj = e.AddedItems[0];
            if (selectObj is Type type)
            {
                AdapterObj = Activator.CreateInstance(type);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ViewCommbox = GetTemplateChild(PART_Items) as ComboBox;
            ViewCommbox.DisplayMemberPath = "Name";
            ViewCommbox.ItemsSource = LoadAdapters();
        }

        /// <summary>
        /// 加载所有连接方式
        /// </summary>
        /// <returns></returns>
        private List<Type> LoadAdapters()
        {
            List<Type> adapterTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            {
                if (item.GetName().Name != "Luster.SimDevice") continue;

                foreach (var type in item.GetTypes())
                {
                    if (!type.IsInterface && !type.IsAbstract && typeof(IAdapter).IsAssignableFrom(type))
                    {
                        adapterTypes.Add(type);
                    }
                }
            }

            return adapterTypes;
        }
    }
}