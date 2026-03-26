using AvalonDock.Controls;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Control.Wpf.Motion
{
    public class ParamItemsControl : ListBox
    {
        protected override bool IsItemItsOwnContainerOverride(object item) => item is ParamItem;

        ScrollViewer _scroll;
        public ParamItemsControl()
        {
            VirtualizingPanel.SetIsVirtualizingWhenGrouping(this, true);
            VirtualizingPanel.SetScrollUnit(this, ScrollUnit.Pixel);

        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = this;
            this.RaiseEvent(eventArg);
        }
    }
}