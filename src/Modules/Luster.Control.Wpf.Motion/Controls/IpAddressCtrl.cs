#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IpAddressCtrl
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Controls
* 文 件 名:       IpAddressCtrl.cs
* 创建时间:       2022/7/5 12:41:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      80013f0b-2841-4743-b833-ff456a032884
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/5 12:41:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Control.Wpf.Motion.Controls
{
    public class IpAddressCtrl : System.Windows.Controls.Control
    {
        static bool IsInit = true;

        public IpAddressCtrl()
        {
            IsInit = true;
        }


        public string IpAddress
        {
            get { return (string)GetValue(IpAddressProperty); }
            set { SetValue(IpAddressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IpAddress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IpAddressProperty =
            DependencyProperty.Register("IpAddress", typeof(string), typeof(IpAddressCtrl), new PropertyMetadata("", new PropertyChangedCallback(IpPropertyChangedCallback)));

        /// <summary>
        /// ip 地址回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void IpPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IpAddressCtrl ctrl = (IpAddressCtrl)d;
            if (ctrl != null && IsInit)
            {
                if (string.IsNullOrEmpty(ctrl.IpAddress)) return;

                string[] addr = ctrl.IpAddress.Split('.');
                if (addr.Length != 4) return;
                ctrl.Value1 = addr[0];
                ctrl.Value2 = addr[1];
                ctrl.Value3 = addr[2];
                ctrl.Value4 = addr[3];

                IsInit = false;
            }
        }

        public string Value1
        {
            get { return (string)GetValue(Value1Property); }
            set { SetValue(Value1Property, value); }
        }

        // Using a DependencyProperty as the backing store for Value1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Value1Property =
            DependencyProperty.Register("Value1", typeof(string), typeof(IpAddressCtrl), new PropertyMetadata("", new PropertyChangedCallback(ValuePropertyChangedCallback)));


        public string Value2
        {
            get { return (string)GetValue(Value2Property); }
            set { SetValue(Value2Property, value); }
        }

        // Using a DependencyProperty as the backing store for Value2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Value2Property =
            DependencyProperty.Register("Value2", typeof(string), typeof(IpAddressCtrl), new PropertyMetadata("", new PropertyChangedCallback(ValuePropertyChangedCallback)));



        public string Value3
        {
            get { return (string)GetValue(Value3Property); }
            set { SetValue(Value3Property, value); }
        }

        // Using a DependencyProperty as the backing store for Value3.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Value3Property =
            DependencyProperty.Register("Value3", typeof(string), typeof(IpAddressCtrl), new PropertyMetadata("", new PropertyChangedCallback(ValuePropertyChangedCallback)));


        public string Value4
        {
            get { return (string)GetValue(Value4Property); }
            set { SetValue(Value4Property, value); }
        }

        // Using a DependencyProperty as the backing store for Value4.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Value4Property =
            DependencyProperty.Register("Value4", typeof(string), typeof(IpAddressCtrl), new PropertyMetadata("", new PropertyChangedCallback(ValuePropertyChangedCallback)));

        /// <summary>
        /// ip 地址回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void ValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IpAddressCtrl ctrl = (IpAddressCtrl)d;
            if (ctrl != null)
            {
                ctrl.IpAddress = $"{ctrl.Value1}.{ctrl.Value2}.{ctrl.Value3}.{ctrl.Value4}";
            }
        }

        private TextBox TextBox1;
        private TextBox TextBox2;
        private TextBox TextBox3;
        private TextBox TextBox4;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBox1 = GetTemplateChild("PART_TextBox1") as TextBox;
            TextBox2 = GetTemplateChild("PART_TextBox2") as TextBox;
            TextBox3 = GetTemplateChild("PART_TextBox3") as TextBox;
            TextBox4 = GetTemplateChild("PART_TextBox4") as TextBox;
            TextBox1.GotFocus -= TextBox_GotFocus;
            TextBox1.GotFocus += TextBox_GotFocus;

            TextBox2.GotFocus -= TextBox_GotFocus;
            TextBox2.GotFocus += TextBox_GotFocus;

            TextBox3.GotFocus -= TextBox_GotFocus;
            TextBox3.GotFocus += TextBox_GotFocus;

            TextBox4.GotFocus -= TextBox_GotFocus;
            TextBox4.GotFocus += TextBox_GotFocus;
        }

        /// <summary>
        /// 获取焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
                textBox.Focus();
            }
        }
    }
}