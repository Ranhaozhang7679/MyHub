#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PathToProgressBehavior
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Flow
* 文 件 名:       PathToProgressBehavior.cs
* 创建时间:       2022/6/2 11:29:22
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      798b5334-9c50-47f6-82af-593548a4a241
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/2 11:29:22
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Luster.Control.Wpf.Motion.Flow
{
    public class PathProgressBehavior : Behavior<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
        }

        /// <summary>
        /// 获取或设置Progress的值
        /// </summary>
        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        /// <summary>
        /// 标识 Progress 依赖属性。
        /// </summary>
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(PathProgressBehavior), new PropertyMetadata(0d, OnProgressChanged));

        private static void OnProgressChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var target = obj as PathProgressBehavior;
            var oldValue = (double)args.OldValue;
            var newValue = (double)args.NewValue;
            if (oldValue != newValue)
                target.OnProgressChanged(oldValue, newValue);
        }

        protected virtual void OnProgressChanged(double oldValue, double newValue)
        {
            UpdateStrokeDashArray();
        }

        protected virtual double GetTotalLength(Shape shape)
        {
            var path = shape.RenderedGeometry.GetFlattenedPathGeometry();

            var length = 0.0;

            foreach (var pf in path.Figures)
            {
                var start = pf.StartPoint;

                foreach (PolyLineSegment seg in pf.Segments)
                {
                    foreach (var point in seg.Points)
                    {
                        length += (start - point).Length;
                        start = point;
                    }
                }
            }

            return length;
        }

        private void UpdateStrokeDashArray()
        {
            var target = AssociatedObject as Shape;
            if (target == null)
                return;

            var progress = Progress;
            //if (target.ActualHeight == 0 || target.ActualWidth == 0)
            //    return;

            if (target.StrokeThickness == 0)
                return;

            var totalLength = GetTotalLength(target);
            var firstSection = progress * totalLength / 100 / target.StrokeThickness;
            if (progress == 100)
                firstSection = Math.Ceiling(firstSection);

            var result = new DoubleCollection { firstSection, double.MaxValue };
            target.StrokeDashArray = result;
        }
    }
}