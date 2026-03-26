#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VAxisMCtrl
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Controls
* 文 件 名:       VAxisMCtrl.cs
* 创建时间:       2022/7/7 13:28:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d23982a1-82c9-42b8-96ba-ca7e35e00e4e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/7 13:28:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Control.Wpf.Motion.Controls
{
    public class VAxisMCtrl : System.Windows.Controls.Control
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public VAxisMCtrl()
        {

        }



        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(VAxisMCtrl), new PropertyMetadata(false));



        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(VAxisMCtrl), 
                new PropertyMetadata(default, new PropertyChangedCallback(PropertyChangedCallback)));

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VAxisMCtrl ctrl = (VAxisMCtrl)d;
            if (ctrl != null)
            {
                if (e.NewValue == null)
                {
                    ctrl.Text = "";
                }
                else
                {
                    var vDevice = e.NewValue as VDevice;
                    if (vDevice != null)
                    {
                        ctrl.Text = vDevice.Name;
                    }
                }
            }
        }

        /// <summary>
        /// 显示文本信息
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(VAxisMCtrl), new PropertyMetadata(""));

        /// <summary>
        /// 参数对象
        /// </summary>
        private ParameterAttribute parameter;
        public ParameterAttribute Parameter
        {
            get
            {
                return parameter;
            }
            set
            {
                parameter = value;
                if (parameter.Value == null)
                {
                    Text = "";
                }
                else
                {
                    var vAxis = value.Value as VDevice;
                    if (vAxis != null)
                    {
                        Text = $"{vAxis.Name}";
                    }
                }
            }
        }

        /// <summary>
        /// Button 按钮
        /// </summary>
        protected const string PART_BtnConfig = nameof(PART_BtnConfig);

        private Button btnView = null;
        public Button ViewButton
        {
            get
            {
                return btnView;
            }
            set
            {
                // 先反注册事件
                if (value != null)
                {
                    value.Click -= BtnView_Click;
                }

                btnView = value;

                if (btnView != null)
                {
                    btnView.Click += BtnView_Click;
                }
            }
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            Parameter.OnConfig(Parameter);
        }

        /// <summary>
        /// 点选
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ViewButton = GetTemplateChild(PART_BtnConfig) as Button;
        }
    }
}