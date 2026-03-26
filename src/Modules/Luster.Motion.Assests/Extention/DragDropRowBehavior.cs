#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DragDropRowBehavior
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.Assests.Extention
* 文 件 名:       DragDropRowBehavior.cs
* 创建时间:       2022/8/8 15:17:14
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      35bac7df-6b6c-4810-a73e-856f43467edf
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/8 15:17:14
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Luster.Motion.Assests.Extention
{
    public static class DragDropRowBehavior
    {
        private static DataGrid dataGrid;

        private static Popup popup;

        private static bool enable;

        private static bool edit;

        private static object draggedItem;

        public static object DraggedItem
        {
            get { return DragDropRowBehavior.draggedItem; }
            set { DragDropRowBehavior.draggedItem = value; }
        }

        public static Popup GetPopupControl(DependencyObject obj)
        {
            return (Popup)obj.GetValue(PopupControlProperty);
        }

        public static void SetPopupControl(DependencyObject obj, Popup value)
        {
            obj.SetValue(PopupControlProperty, value);
        }

        // Using a DependencyProperty as the backing store for PopupControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupControlProperty =
            DependencyProperty.RegisterAttached("PopupControl", typeof(Popup), typeof(DragDropRowBehavior), new UIPropertyMetadata(null, OnPopupControlChanged));

        private static void OnPopupControlChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || !(e.NewValue is Popup))
            {
                throw new ArgumentException("Popup Control should be set", "PopupControl");
            }
            popup = e.NewValue as Popup;

            dataGrid = depObject as DataGrid;
            // Check if DataGrid
            if (dataGrid == null)
                return;


            if (enable && popup != null)
            {
                if (edit)
                {
                    dataGrid.BeginningEdit += new EventHandler<DataGridBeginningEditEventArgs>(OnBeginEdit);
                    dataGrid.CellEditEnding += new EventHandler<DataGridCellEditEndingEventArgs>(OnEndEdit);
                }
                dataGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(OnPreviewMouseLeftButtonUp);
                dataGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
                dataGrid.MouseMove += new MouseEventHandler(OnMouseMove);
            }
            else
            {
                if (edit)
                {
                    dataGrid.BeginningEdit -= new EventHandler<DataGridBeginningEditEventArgs>(OnBeginEdit);
                    dataGrid.CellEditEnding -= new EventHandler<DataGridCellEditEndingEventArgs>(OnEndEdit);
                }
                dataGrid.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(OnPreviewMouseLeftButtonUp);
                dataGrid.MouseLeftButtonDown -= new MouseButtonEventHandler(OnMouseLeftButtonDown);
                dataGrid.MouseMove -= new MouseEventHandler(OnMouseMove);

                dataGrid = null;
                popup = null;
                draggedItem = null;
                IsEditing = false;
                IsDragging = false;
            }
        }

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(DragDropRowBehavior), new UIPropertyMetadata(false, OnEnabledChanged));

        private static void OnEnabledChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs e)
        {
            //Check if value is a Boolean Type
            if (e.NewValue is bool == false)
                throw new ArgumentException("Value should be of bool type", "Enabled");

            enable = (bool)e.NewValue;

        }


        public static bool GetEdit(DependencyObject obj)
        {
            return (bool)obj.GetValue(EditProperty);
        }

        public static void SetEdit(DependencyObject obj, bool value)
        {
            obj.SetValue(EditProperty, value);
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditProperty =
            DependencyProperty.RegisterAttached("Edit", typeof(bool), typeof(DragDropRowBehavior), new UIPropertyMetadata(false, OnEditChanged));

        private static void OnEditChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs e)
        {
            //Check if value is a Boolean Type
            if (e.NewValue is bool == false)
                throw new ArgumentException("Value should be of bool type", "Edit");

            edit = (bool)e.NewValue;

        }

        public static bool IsEditing { get; set; }

        public static bool IsDragging { get; set; }

        private static void OnBeginEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            IsEditing = true;
            //in case we are in the middle of a drag/drop operation, cancel it...
            if (IsDragging) ResetDragDrop();
        }

        private static void OnEndEdit(object sender, DataGridCellEditEndingEventArgs e)
        {
            IsEditing = false;
        }


        /// <summary>
        /// Initiates a drag action if the grid is not in edit mode.
        /// </summary>
        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEditing) return;
            var column = UIHelpers.TryFindFromPoint<DataGridTextColumn>((UIElement)sender, e.GetPosition(dataGrid));
            var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(dataGrid));
            if (row == null || row.IsEditing ) return;
            
            //set flag that indicates we're capturing mouse movements
            IsDragging = true;
            DraggedItem = row.Item;
        }

        /// <summary>
        /// Completes a drag/drop operation.
        /// </summary>
        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsDragging || IsEditing)
            {
                return;
            }
            dataGrid.Cursor = Cursors.Arrow;

            //get the target item
            var targetItem = dataGrid.SelectedItem;

            if (targetItem == null || !ReferenceEquals(DraggedItem, targetItem))
            {
                //get target index
                var targetIndex = ((dataGrid).ItemsSource as IList).IndexOf(targetItem);
                if (targetIndex < 0) return;
                //remove the source from the list
                ((dataGrid).ItemsSource as IList).Remove(DraggedItem);

                //move source at the target's location
                ((dataGrid).ItemsSource as IList).Insert(targetIndex, DraggedItem);

                //select the dropped item
                dataGrid.SelectedItem = DraggedItem;
            }

            //reset
            ResetDragDrop();
        }

        /// <summary>
        /// Completes a drag/drop operation.
        /// </summary>
        private static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsDragging || IsEditing)
            {
                return;
            }
            dataGrid.Cursor = Cursors.Arrow;

            //get the target item
            var targetItem = dataGrid.SelectedItem;

            if (targetItem == null || !ReferenceEquals(DraggedItem, targetItem))
            {
                //get target index
                var targetIndex = ((dataGrid).ItemsSource as IList).IndexOf(targetItem);
                if (targetIndex < 0) return;
                //remove the source from the list
                ((dataGrid).ItemsSource as IList).Remove(DraggedItem);

                //move source at the target's location
                ((dataGrid).ItemsSource as IList).Insert(targetIndex, DraggedItem);

                //select the dropped item
                dataGrid.SelectedItem = DraggedItem;
            }

            //reset
            ResetDragDrop();
        }

        /// <summary>
        /// Closes the popup and resets the
        /// grid to read-enabled mode.
        /// </summary>
        private static void ResetDragDrop()
        {
            IsDragging = false;
            popup.IsOpen = false;
            dataGrid.IsReadOnly = false;
        }

        /// <summary>
        /// Updates the popup's position in case of a drag/drop operation.
        /// </summary>
        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging || e.LeftButton != MouseButtonState.Pressed) return;
            if (dataGrid.Cursor != Cursors.SizeAll) dataGrid.Cursor = Cursors.SizeAll;
            popup.DataContext = DraggedItem;
            //display the popup if it hasn't been opened yet
            if (!popup.IsOpen)
            {
                //switch to read-only mode
                dataGrid.IsReadOnly = true;

                //make sure the popup is visible
                popup.IsOpen = true;
            }


            Size popupSize = new Size(popup.ActualWidth, popup.ActualHeight);
            popup.PlacementRectangle = new Rect(e.GetPosition(dataGrid), popupSize);

            //make sure the row under the grid is being selected
            Point position = e.GetPosition(dataGrid);
            var row = UIHelpers.TryFindFromPoint<DataGridRow>(dataGrid, position);
            if (row != null) dataGrid.SelectedItem = row.Item;
        }

    }
}
