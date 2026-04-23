using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Interactivity;
using HandyControl.Tools.Extension;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Luster.Control.Wpf.Motion
{
    public class ParamGrid : ListBox
    {
        private const string ElementItemsControl = "PART_ItemsControl";

        private const string ElementSearchBar = "PART_SearchBar";

        /// <summary>
        /// 列表组件
        /// </summary>
        private ItemsControl _itemsControl;

        /// <summary>
        /// 视图组件
        /// </summary>
        private ICollectionView _dataView;

        /// <summary>
        /// 模块
        /// </summary>
        private IModule module = null;

        /// <summary>
        /// 控件
        /// </summary>
        private List<ParamItem> paramItems = null;

        /// <summary>
        /// 默认是中文显示
        /// </summary>
        private bool isChinese = true;

        public ParamGrid()
        {
            DefaultStyleKey = typeof(ParamGrid);

            // 多语言事件
            HandyControl.Tools.ConfigHelper.Instance.PropertyChanged -= Instance_PropertyChanged;
            HandyControl.Tools.ConfigHelper.Instance.PropertyChanged += Instance_PropertyChanged;

            // 设置默认语言
            isChinese = Application.Current.Dispatcher.Thread.CurrentUICulture.Name == "zh-CN";
        }

        /// <summary>
        /// 多语言切换功能
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HandyControl.Tools.ConfigHelper cHelper = (HandyControl.Tools.ConfigHelper)sender;

            isChinese = cHelper.Lang.IetfLanguageTag != "en";

            UpdateItems(module);
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        public virtual ParamResolver ParamResolver => new ParamResolver();

        #region 点选事件
        /// <summary>
        /// 双击事件
        /// </summary>
        public static readonly RoutedEvent ItemConfigEvent =
            EventManager.RegisterRoutedEvent("ItemConfig", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ParamGrid));

        public event RoutedEventHandler ItemConfig
        {
            add => AddHandler(ItemConfigEvent, value);
            remove => RemoveHandler(ItemConfigEvent, value);
        }

        /// <summary>
        /// 点击触发
        /// </summary>
        /// <param name="pItem"></param>
        private void OnItemConfig(ParameterAttribute pItem)
        {
            this.RaiseEvent(new ItemConfigArgs(ItemConfigEvent, pItem) { Source = this });
        }
        #endregion

        #region SelectObject
        public static readonly RoutedEvent SelectedObjectChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedObjectChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<object>), typeof(ParamGrid));

        public event RoutedPropertyChangedEventHandler<object> SelectedObjectChanged
        {
            add => AddHandler(SelectedObjectChangedEvent, value);
            remove => RemoveHandler(SelectedObjectChangedEvent, value);
        }

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(object), typeof(ParamGrid), new PropertyMetadata(default(object), OnSelectedObjectChanged));

        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (ParamGrid)d;
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
        #endregion

        #region 属性
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool HasPermission
        {
            get { return (bool)GetValue(HasPermissionProperty); }
            set { SetValue(HasPermissionProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasPermissionProperty =
            DependencyProperty.Register("HasPermission", typeof(bool), typeof(ParamGrid), new PropertyMetadata(false));


        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(string), typeof(ParamGrid), new PropertyMetadata(default(string)));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty MaxTitleWidthProperty = DependencyProperty.Register(
            "MaxTitleWidth", typeof(double), typeof(ParamGrid), new PropertyMetadata(0.0));

        public double MaxTitleWidth
        {
            get => (double)GetValue(MaxTitleWidthProperty);
            set => SetValue(MaxTitleWidthProperty, value);
        }

        public static readonly DependencyProperty MinTitleWidthProperty = DependencyProperty.Register(
            "MinTitleWidth", typeof(double), typeof(ParamGrid), new PropertyMetadata(0.0));

        public double MinTitleWidth
        {
            get => (double)GetValue(MinTitleWidthProperty);
            set => SetValue(MinTitleWidthProperty, value);
        }
        #endregion

        /// <summary>
        /// 模板应用
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _itemsControl = GetTemplateChild(ElementItemsControl) as ItemsControl;

            UpdateItems(SelectedObject);
        }

        /// <summary>
        /// 更新界面每个Items
        /// </summary>
        /// <param name="obj">IModule</param>
        private void UpdateItems(object obj)
        {
            if (obj == null || _itemsControl == null) return;

            module = obj as IModule;
            if (module == null) return;

            // 清除历史记录
            if (paramItems != null)
            {
                foreach (var item in paramItems)
                {
                    if (item.Value is ParameterAttribute p)
                    {
                        p.IsBindUI = false;
                        p.IsSensitive = false;
                    }
                }

                paramItems?.Clear();
            }

            paramItems = new List<ParamItem>();
            foreach (var item in module.Parameters)
            {
                if (!item.Value.Visible) continue;

                var pItem = CreatePropertyItem(item.Value);


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
            if (module == null) return;

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
            bool isChanged = false;
            foreach (var item in paramItems)
            {
                var pAttr = item.Value as ParameterAttribute;

                // 依赖参数
                if (pAttr == null || pAttr.DependOns == null || pAttr.DependOns.Count() == 0) continue;
                bool uiVisible = item.UIVisible;

                // 对变化的值有依赖
                foreach (var dItem in pAttr.DependOns)
                {
                    string dependKey = changeItem.Name;
                    if (dItem.Key != dependKey) continue;

                    var dependProp = pAttr.Owner.Parameters[dependKey];
                    uiVisible = dependProp.Value.Equals(dItem.Value);
                    if (uiVisible)
                    {
                        break;
                    }
                }

                item.UIVisible = uiVisible;
                isChanged = true;
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
                //_dataView.SortDescriptions.Add(new SortDescription(ParamItem.CategoryProperty.Name, ListSortDirection.Ascending));
                _dataView.SortDescriptions.Add(new SortDescription(ParamItem.SortProperty.Name, ListSortDirection.Ascending));
                _dataView.GroupDescriptions.Add(new PropertyGroupDescription(ParamItem.CategoryProperty.Name));
            }
        }

        /// <summary>
        /// 构造属性的Item
        /// </summary>
        /// <param name="p">参数</param>
        /// <returns>参数对象</returns>
        protected virtual ParamItem CreatePropertyItem(ParameterAttribute p)
        {
            p.IsBindUI = true;

            if (p.ParamType == ParamType.IN)
            {
                p.IsSensitive = true;
            }

            // 注册切换事件
            p.RefChangedEvent -= P_RefChangedEvent;
            p.RefChangedEvent += P_RefChangedEvent;

            // 外部弹窗事件
            p.ConfigEvent -= P_ConfigEvent;
            p.ConfigEvent += P_ConfigEvent;

            p.DataChanged -= P_DataChanged;
            p.DataChanged += P_DataChanged;

            var uiVisible = true;
            if (p.DependOns != null)
            {
                foreach (var dItem in p.DependOns)
                {
                    if (p.Owner.Parameters.ContainsKey(dItem.Key))
                    {
                        var dependProp = p.Owner.Parameters[dItem.Key];
                        if (dependProp.Value != null)
                        {
                            uiVisible = dependProp.Value.Equals(dItem.Value);
                        }

                        if (uiVisible)
                        {
                            break;
                        }
                    }
                }
            }

            // 给编辑器赋值EngineUI
            var editor = ParamResolver.ResolveEditor(p);

            var pItem = new ParamItem()
            {
                ID = module.ID,
                Name = p.Name,
                Category = p.Group,
                DisplayName = (isChinese ? p.CN : p.Name),
                Description = p.Tips,
                IsReadOnly = p.IsReadOnly,
                Editor = editor,
                Value = p,
                PropertyName = GetPropertyName(p),
                PropertyType = p.EditorType ?? p.Type,
                Filter = p.Filter,
                IsRef = p.RefOut != null,
                ForeColor = p.RefOut != null ? "Green" : "Red",
                Sort = p.Sort,
                ParamType = ParamResolver.ResolveRef(p),
                IsExpanded = true,
                UIVisible = uiVisible,
            };

            if (!HasPermission)
            {
                pItem.IsReadOnly = true;
            }
            else
            {
                // 动态绑定 IsReadOnly，使 ParameterAttribute.IsReadOnly 变更时自动同步到 UI
                BindingOperations.SetBinding(pItem, ParamItem.IsReadOnlyProperty,
                    new Binding("IsReadOnly")
                    {
                        Source = p,
                        Mode = BindingMode.OneWay
                    });
            }

            BuildRelyDataSource(pItem, out var isRely);
            return pItem;
        }

        /// <summary>
        /// 数据源更新
        /// </summary>
        /// <param name="obj"></param>
        private void P_DataChanged(ParameterAttribute p, IEnumerable<object> obj)
        {
            foreach (var item in paramItems)
            {
                var pAttr = item.Value as ParameterAttribute;
                if (pAttr == p)
                {
                    item.Editor = ParamResolver.ResolveEditor(pAttr);
                    item.InitElement();
                }
            }
        }

        /// <summary>
        /// 需要外部进行交互
        /// </summary>
        /// <param name="obj">对象</param>
        private void P_ConfigEvent(ParameterAttribute obj)
        {
            OnItemConfig(obj);
        }

        /// <summary>
        /// 获取对应的属性名称
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected virtual string GetPropertyName(ParameterAttribute p)
        {
            if (p.EditorType == null) return nameof(p.Value);

            if (p.Type.IsAssignableFrom(p.EditorType) || p.Type == p.EditorType) return nameof(p.Value);

            return nameof(p.EditorValue);
        }

        /// <summary>
        /// 构建RelyDataSource的数据源
        /// </summary>
        /// <param name="pItem"></param>
        private void BuildRelyDataSource(ParamItem pItem, out bool isRely)
        {
            isRely = false;
            ParameterAttribute pAttr = pItem.Value as ParameterAttribute;
            if (pAttr.Type != typeof(LRelyType)) return;
            var relyVal = pAttr.Value as LRelyType;
            if (relyVal != null && pAttr.Owner.Parameters.ContainsKey(relyVal.ParamName))
            {
                List<KeyValue> items = new List<KeyValue>();

                // 搜索对象，构造数据源
                var relyAttr = pAttr.Owner.Parameters[relyVal.ParamName];
                if (relyAttr != null && relyAttr.RefOut != null)
                {
                    var realType = relyAttr.RefOut.Type;
                    var properties = realType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
                    foreach (var prop in properties)
                    {
                        if (prop.PropertyType.Name == relyVal.PropertyType)
                        {
                            var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                            items.Add(new KeyValue()
                            {
                                Value = prop.Name,
                                Desc = descAttr == null ? prop.Name : descAttr.Description,
                            });
                        }
                    }
                }

                // 构造数据源
                relyVal.DataSource = items;
                isRely = true;
            }
        }

        /// <summary>
        /// 参数连接发生变更
        /// </summary>
        /// <param name="obj"></param>
        private void P_RefChangedEvent(ParameterAttribute changeItem)
        {
            if (paramItems == null || paramItems.Count == 0) return;
            foreach (var item in paramItems)
            {
                var pAttr = item.Value as ParameterAttribute;

                if (pAttr == changeItem)
                {
                    item.IsRef = changeItem.RefOut != null;
                    item.ForeColor = changeItem.RefOut != null ? "Green" : "Red";
                    item.Editor = ParamResolver.ResolveEditor(pAttr);
                    item.InitElement();
                    continue;
                }

                // 依赖类型
                BuildRelyDataSource(item, out var isRely);
                if (isRely)
                {
                    item.InitElement();
                }
            }
        }

        /// <summary>
        /// 界面尺寸发生变化
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            TitleElement.SetTitleWidth(this, new GridLength(Math.Max(MinTitleWidth, Math.Min(MaxTitleWidth, ActualWidth / 3))));
        }
    }
}