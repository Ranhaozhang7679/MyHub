#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ParamGrid
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.ParamGrid
* 文 件 名:       ParamGrid.cs
* 创建时间:       2022/4/18 16:45:55
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      27598968-d679-4951-818c-01cd8b1d7251
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 16:45:55
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Luster.Common.Assets.ParamGrid
{
    /// <summary>
    /// 参数变更信息
    /// </summary>
    public class PropertyGrid : ListBox
    {
        /// <summary>
        /// 窗体
        /// </summary>
        private const string ElementItemsControl = "PART_ItemsControl";

        /// <summary>
        /// 列表组件
        /// </summary>
        private ItemsControl _itemsControl;

        /// <summary>
        /// 视图
        /// </summary>
        private ICollectionView _dataView;

        /// <summary>
        /// 控件
        /// </summary>
        private List<ParamItem> paramItems = null;

        public PropertyGrid()
        {
            DefaultStyleKey = typeof(PropertyGrid);
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        public virtual ParamResolver ParamResolver { get; } = new ParamResolver();

        public static readonly RoutedEvent SelectedObjectChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedObjectChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<object>), typeof(PropertyGrid));

        public event RoutedPropertyChangedEventHandler<object> SelectedObjectChanged
        {
            add => AddHandler(SelectedObjectChangedEvent, value);
            remove => RemoveHandler(SelectedObjectChangedEvent, value);
        }

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(object), typeof(PropertyGrid), new PropertyMetadata(default(object), OnSelectedObjectChanged));

        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (PropertyGrid)d;
            ctl.OnSelectedObjectChanged(e.OldValue, e.NewValue);
        }

        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        protected virtual void OnSelectedObjectChanged(object oldValue, object newValue)
        {
            UpdateItems(newValue);
            RaiseEvent(new RoutedPropertyChangedEventArgs<object>(oldValue, newValue, SelectedObjectChangedEvent));
        }

        /// <summary>
        /// 是否进行分组
        /// </summary>
        public bool IsGroup
        {
            get { return (bool)GetValue(IsGroupProperty); }
            set { SetValue(IsGroupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGroupProperty =
            DependencyProperty.Register("IsGroup", typeof(bool), typeof(PropertyGrid), new PropertyMetadata(true));


        /// <summary>
        /// 应用
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _itemsControl = GetTemplateChild(ElementItemsControl) as ItemsControl;

            UpdateItems(SelectedObject);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private List<PropItemAttribute> Parse(object obj)
        {
            List<PropItemAttribute> attrItems = new List<PropItemAttribute>();
            var props = obj.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in props)
            {
                PropItemAttribute attr = PropItemAttribute.CreateByProperty(prop, obj);
                if (attr != null)
                {
                    attrItems.Add(attr);

                    // 设置隶属集合
                    attr.OwnerCollection = attrItems;
                    attr.Owner = obj;
                }
            }

            return attrItems;
        }

        /// <summary>
        /// 更新界面每个Items
        /// </summary>
        /// <param name="obj">IModule</param>
        protected virtual void UpdateItems(object obj)
        {
            if (obj == null || _itemsControl == null) return;

            paramItems?.Clear();
            paramItems = new List<ParamItem>();

            // 1.检查对象是否是集合
            var pList = Parse(obj).OrderBy(u => u.Sort);

            // 2.检查对象是否是对象

            foreach (var item in pList)
            {
                var pItem = CreatePropertyItem(item);

                pItem.InitElement();

                // 事件回调,放到对象创建之后的目的是防止初始绑定也会触发方法
                pItem.ParamValueChangedEvent -= PItem_ParamValueChangedEvent;
                pItem.ParamValueChangedEvent += PItem_ParamValueChangedEvent;

                // 添加到集合中
                paramItems.Add(pItem);
            }

            // 函数构造
            _dataView = CollectionViewSource.GetDefaultView(paramItems);

            // 对分类进行排序
            SortByCategory(null, null);

            // ItemSource 属性赋值
            _itemsControl.ItemsSource = _dataView;
        }

        /// <summary>
        /// 事件方法
        /// </summary>
        /// <param name="pItem">属性变更</param>
        /// <param name="newV">新值</param>
        private void PItem_ParamValueChangedEvent(ParamItem pItem, object newV)
        {
            if (newV == null) return;

            // 更新依赖属性
            UpdateItemVisible(pItem);
        }

        /// <summary>
        /// 某个属性变更，同时也要改变其他参数的状态
        /// </summary>
        /// <param name="changeItem">变化对象</param>
        private void UpdateItemVisible(ParamItem changeItem)
        {
            if (paramItems == null || paramItems.Count == 0) return;

            foreach (var item in paramItems)
            {
                var pAttr = item.Value as PropItemAttribute;

                // 依赖参数
                if (pAttr == null || pAttr.DependOns == null || pAttr.DependOns.Count() == 0) continue;
                bool uiVisible = item.UIVisible;

                // 对变化的值有依赖
                foreach (var dItem in pAttr.DependOns)
                {
                    string dependKey = changeItem.Name;
                    if (dItem.Key != dependKey) continue;

                    var dependProp = pAttr.GetPropItemByKey(dependKey);
                    uiVisible = dependProp.Value.Equals(dItem.Value);
                    if (uiVisible)
                    {
                        break;
                    }
                }

                item.UIVisible = uiVisible;
            }
        }

        /// <summary>
        /// 对Category进行排序
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void SortByCategory(object sender, ExecutedRoutedEventArgs e)
        {
            if (_dataView == null) return;

            using (_dataView.DeferRefresh())
            {
                _dataView.GroupDescriptions.Clear();
                _dataView.SortDescriptions.Clear();

                // 通过Sort进行排序
                _dataView.SortDescriptions.Add(new SortDescription(ParamItem.SortProperty.Name, ListSortDirection.Ascending));

                if (IsGroup)
                {
                    _dataView.GroupDescriptions.Add(new PropertyGroupDescription(ParamItem.CategoryProperty.Name));
                }
            }
        }

        /// <summary>
        /// 构造属性的Item
        /// </summary>
        /// <param name="p">参数</param>
        /// <returns>参数对象</returns>
        protected virtual ParamItem CreatePropertyItem(PropItemAttribute p)
        {
            var uiVisible = true;
            if (p.DependOns != null)
            {
                foreach (var dItem in p.DependOns)
                {
                    var dependProp = p.GetPropItemByKey(dItem.Key);
                    uiVisible = dependProp.Value.Equals(dItem.Value);
                    if (uiVisible)
                    {
                        break;
                    }
                }
            }

            // 给编辑器赋值EngineUI
            var editor = ParamResolver.ResolveEditor(p);

            var pItem = new ParamItem()
            {
                Name = p.Name,
                Category = p.Group,
                DisplayName = p.DisplayName,
                Description = p.Tips,
                IsReadOnly = p.IsReadOnly,
                PropertyName = nameof(p.Value),
                Editor = editor,
                Value = p,
                PropertyType = p.Type,
                Sort = p.Sort,
                IsExpanded = true,
                UIVisible = uiVisible,
            };

            BuildRelyDataSource(pItem, out var isRely);
            return pItem;
        }

        /// <summary>
        /// 构建RelyDataSource的数据源
        /// </summary>
        /// <param name="pItem"></param>
        private void BuildRelyDataSource(ParamItem pItem, out bool isRely)
        {
            isRely = false;
            PropItemAttribute pAttr = pItem.Value as PropItemAttribute;
            if (pAttr == null || pAttr.Type != typeof(LRelyType)) return;
            var relyVal = pAttr.Value as LRelyType;
            if (relyVal != null && pAttr.ContainKey(relyVal.ParamName))
            {
                List<KeyValue> items = new List<KeyValue>();

                // 搜索对象，构造数据源
                var relyAttr = pAttr.GetPropItemByKey(relyVal.ParamName);

                // 构造数据源
                relyVal.DataSource = items;
                isRely = true;
            }
        }
    }
}