using DC.Authorization;
using DC.Authorization.WPF.Helper;
using Luster.Motion.CommonUI.Interfaces;
using Prism.Ioc;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// ToolBarContent.xaml 的交互逻辑
    /// </summary>
    public partial class ToolBarContent : IToolbarContent
    {
        public ToolBarContent()
        {
            InitializeComponent();
        }


        private void btnMode_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            popup.IsOpen = !popup.IsOpen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            popupAlarm.IsOpen = false;
        }

        private void TxtStatus_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var txt = sender as TextBlock;
            if (txt != null && txt.Text == "报警中")
            {
                popupAlarm.IsOpen = !popupAlarm.IsOpen;
            }
        }
    }

    public class PopupHelper
    {
        /// <summary>
        /// 附加属性：跟随放置控件而移动
        /// </summary>
        public static readonly DependencyProperty PopupPlacementTargetProperty = DependencyProperty.RegisterAttached("PopupPlacementTarget", typeof(DependencyObject), typeof(PopupHelper), new PropertyMetadata(null, OnPopupPlacementTargetChanged));

        public static DependencyObject GetPopupPlacementTarget(DependencyObject obj)
        {
            return (DependencyObject)obj.GetValue(PopupPlacementTargetProperty);
        }
        public static void SetPopupPlacementTarget(DependencyObject obj, DependencyObject value)
        {
            obj.SetValue(PopupPlacementTargetProperty, value);
        }

        private static void OnPopupPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.Primitives.Popup pop && e.NewValue is DependencyObject placementTarget)
            {
                //var window = Window.GetWindow(placementTarget);
                var window = Application.Current.MainWindow;
                if (window != null)
                {
                    window.LocationChanged += delegate
                    {
                        if (pop.IsOpen)
                        {
                            var mi = typeof(System.Windows.Controls.Primitives.Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            mi?.Invoke(pop, null);
                        }
                    };
                }
            }
        }
    }
}


