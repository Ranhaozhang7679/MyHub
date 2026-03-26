#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DisplayElementModel
* 机器名称:       L05014-NB
* 命名空间:       Luster.SubSystem.ThreeD.Model
* 文 件 名:       DisplayElementModel.cs
* 创建时间:       2022/6/7 10:15:01
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      b1bab3e4-8edf-4092-93f8-281e2670bf54
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/7 10:15:01
* 修 改 人:		  L05014
************************************************************************************/

#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Luster.Controls.Wpf.ListEx
{
    public class CusListBox : ListBox   
    {
        /// <summary>
        /// Identifies the <see cref="IsSelectedPath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedPathProperty = DependencyProperty.Register(
            nameof(IsSelectedPath),
            typeof(string),
            typeof(CusListBox),
            new UIPropertyMetadata("IsSelected"));

        /// <summary>
        /// Identifies the <see cref="HierarchySource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HierarchySourceProperty = DependencyProperty.Register(
            nameof(HierarchySource),
            typeof(IEnumerable),
            typeof(CusListBox),
            new UIPropertyMetadata(null, (s, e) => ((CusListBox)s).HierarchySourceChanged(e)));

        static CusListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CusListBox), new FrameworkPropertyMetadata(typeof(CusListBox)));
        }

        /// <summary>
        /// Gets or sets the hierarchy source.
        /// </summary>
        /// <value>The hierarchy source.</value>
        public IEnumerable HierarchySource
        {
            get
            {
                return (IEnumerable)this.GetValue(HierarchySourceProperty);
            }

            set
            {
                this.SetValue(HierarchySourceProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the binding path to the IsSelected property of the items.
        /// </summary>
        /// <value>A binding path.</value>
        public string IsSelectedPath
        {
            get
            {
                return (string)this.GetValue(IsSelectedPathProperty);
            }

            set
            {
                this.SetValue(IsSelectedPathProperty, value);
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see
        /// cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> .
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        /// <summary>
        /// Handles changes in the HierarchySource.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private void HierarchySourceChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldTreeSource = e.OldValue as IEnumerable;              
            if (oldTreeSource != null)
            {
                this.SelectedItems.Clear();
            }
            this.ClearItems();
            var hierarchySource = this.HierarchySource as IList ?? this.HierarchySource?.Cast<object>().ToList();
            if (hierarchySource != null)
            {
                this.Items.Clear();
                this.UnsubscribeCollectionChanges(hierarchySource);
                this.SubscribeForCollectionChanges(hierarchySource);
                foreach (var item in hierarchySource)
                {
                    this.AddItem(item);
                }
            }          
        }

        /// <summary>
        /// Subscribes for changes on the specified collection.
        /// </summary>
        /// <param name="collection">The collection to observe.</param>
        private void SubscribeForCollectionChanges(IList collection)
        {
            var cc = collection as INotifyCollectionChanged;
            if (cc != null)
            {
                cc.CollectionChanged += this.ChildCollectionChanged;
            }
        }

        /// <summary>
        /// Removes the change subscription for the specified collection.
        /// </summary>
        /// <param name="collection">The collection to stop observing.</param>
        private void UnsubscribeCollectionChanges(IList collection)
        {
            var cc = collection as INotifyCollectionChanged;
            if (cc != null)
            {
                cc.CollectionChanged -= this.ChildCollectionChanged;
            }
        }

        /// <summary>
        /// Handles child collection changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        internal void ChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    this.RemoveItem(item);
                }
            }

            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    this.AddItem(item);
                }
            }
        }

        /// <summary>
        /// Gets the container from the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The container.
        /// </returns>
        internal CusListBoxItem ContainerFromIndex(int index)
        {
            return this.ItemContainerGenerator.ContainerFromIndex(index) as CusListBoxItem;
        }

        /// <summary>
        /// Collapses the specified item.
        /// </summary>
        /// <param name="item">The item to collapse.</param>
        internal void Collapse(object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            this.RemoveItem(item);
        }

        /// <summary>
        /// Gets the container from an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// The container.
        /// </returns>
        internal CusListBoxItem GetContainerFromItem(object item)
        {
            return (CusListBoxItem)this.ItemContainerGenerator.ContainerFromItem(item);
        }

        /// <summary>
        /// Creates or identifies the element used to display a specified item.
        /// </summary>
        /// <returns>
        /// A <see cref="CusListBoxItem" /> .
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CusListBoxItem();
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own ItemContainer.
        /// </summary>
        /// <param name="item">Specified item.</param>
        /// <returns>
        /// <c>true</c> if the item is its own ItemContainer; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CusListBoxItem;
        }

        /// <summary>
        /// Responds to the <see cref="E:System.Windows.UIElement.KeyDown" /> event.
        /// </summary>
        /// <param name="e">Provides data for <see cref="T:System.Windows.Input.KeyEventArgs" /> .</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            var control = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            var item = this.SelectedIndex >= 0 ? this.Items[this.SelectedIndex] : null;
            //快捷键可以先不加
            switch (e.Key)
            {
                //case Key.Left:
                //    if (control)
                //    {
                       
                //    }
                //    else
                //    {
                //        this.Collapse(item);
                //    }

                //    e.Handled = true;
                //    break;
                //case Key.Right:
                //    if (control)
                //    {
                //        foreach (var i in this.Items.Cast<object>().ToArray())
                //        {
                            
                //        }
                //    }
                //    e.Handled = true;
                //    break;
            }
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">Container used to display the specified item.</param>
        /// <param name="item">The item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var container = (CusListBoxItem)element;
            if (container == null)
            {
                throw new InvalidOperationException("Container not created.");
            }
            if (item == null)
            {
                throw new InvalidOperationException("Item not specified.");
            }
            if (!string.IsNullOrEmpty(this.IsSelectedPath))
            {
                container.SetBinding(ListBoxItem.IsSelectedProperty, new Binding(this.IsSelectedPath));
            }
        }

        /// <summary>
        /// Provides an appropriate <see cref="T:System.Windows.Automation.Peers.ListBoxAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.
        /// </summary>
        /// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CusListBoxAutomationPeer(this);
        }

        /// <summary>
        /// 移除所有项
        /// </summary>
        /// <param name="itemsToRemove"></param>
        private void RemoveItems(IList itemsToRemove)
        {
            var queue = new Queue(itemsToRemove);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (this.Items.Contains(item))
                {
                    this.RemoveItem(item);
                }
            }
        }

        /// <summary>
        /// 清除所有项
        /// </summary>
        private void ClearItems()
        {
            this.Items.Clear();
        }

       /// <summary>
       /// 移除单元
       /// </summary>
       /// <param name="item"></param>
        private void RemoveItem(object item)
        {
            var container = this.GetContainerFromItem(item);
            if (container != null)
            {
                container.IsSelected = false;
            }

            this.Items.Remove(item);           
        }

        /// <summary>
        /// 插入单元
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void InsertItem(int index, object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            #if DEBUG
            if (this.Items.Contains(item))
            {
                throw new ArgumentNullException(nameof(item));
            }
            #endif
            try
            {
                this.Items.Insert(index, item);
            }
            catch (ArgumentException e)
            {             
                if (e.Message == "Height must be non-negative.")
                {
                    return;
                }
                throw;
            }
        }

        /// <summary>
        /// 增加单元
        /// </summary>
        /// <param name="item"></param>
        private void AddItem(object item)
        {
            this.InsertItem(this.Items.Count, item);
        }

    }
}
