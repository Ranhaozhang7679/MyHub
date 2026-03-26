#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RangeCtrl
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.Controls.Components
* 文 件 名:       RangeCtrl.cs
* 创建时间:       2022/2/8 13:39:32
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      3e66a994-9308-479e-bdc5-9942410c41c6
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/8 13:39:32
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using HandyControl.Interactivity;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Control.Wpf.Motion.Controls
{
    public class RangeCtrl : System.Windows.Controls.Control
    {
        public bool IsInit = true;
        

        /// <summary>
        /// 构造函数
        /// </summary>
        public RangeCtrl()
        {
            CommandBindings.Add(new CommandBinding(ControlCommands.Switch, (s, e) =>
            {
                ShowPop = !ShowPop;
            }, (s, e) => e.CanExecute = true));
        }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RangeCtrl), new PropertyMetadata(false));

        public double MinVal
        {
            get { return (double)GetValue(MinValProperty); }
            set { SetValue(MinValProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinVal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValProperty =
            DependencyProperty.Register("MinVal", typeof(double), typeof(RangeCtrl), new PropertyMetadata(0.0));


        public double MaxVal
        {
            get { return (double)GetValue(MaxValProperty); }
            set { SetValue(MaxValProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxVal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValProperty =
            DependencyProperty.Register("MaxVal", typeof(double), typeof(RangeCtrl), new PropertyMetadata(10000.0));


        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(double), typeof(RangeCtrl), new PropertyMetadata(-0.1, new PropertyChangedCallback(ValueChangeCallback)));

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(double), typeof(RangeCtrl), new PropertyMetadata(1.0, new PropertyChangedCallback(ValueChangeCallback)));

        private static void ValueChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RangeCtrl ctrl && !ctrl.IsInit)
            {
                ctrl.DisplayValue = new LRange()
                {
                    Min = ctrl.Min,
                    Max = ctrl.Max,
                };
            }
        }

        /// <summary>
        /// 显示值
        /// </summary>
        public object DisplayValue
        {
            get { return (object)GetValue(DisplayValueProperty); }
            set { SetValue(DisplayValueProperty, value); }
        }

        public static readonly DependencyProperty DisplayValueProperty =
            DependencyProperty.Register("DisplayValue", typeof(object), typeof(RangeCtrl), new PropertyMetadata(default, new PropertyChangedCallback(DisplayValueChangedCallback)));

        private static void DisplayValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RangeCtrl ctrl)
            {
                if (ctrl.DisplayValue == null)
                {
                    ctrl.Min = -0.1;
                    ctrl.Max = 0.1;
                }
                else
                {
                    LRange p = (LRange)ctrl.DisplayValue;
                    ctrl.Min = p.Min;
                    ctrl.Max = p.Max;
                }

                ctrl.IsInit = false;
            }
        }

        /// <summary>
        /// 显示弹出
        /// </summary>
        public bool ShowPop
        {
            get { return (bool)GetValue(ShowPopProperty); }
            set { SetValue(ShowPopProperty, value); }
        }

        public static readonly DependencyProperty ShowPopProperty =
            DependencyProperty.Register("ShowPop", typeof(bool), typeof(RangeCtrl), new PropertyMetadata(false));
    }
}