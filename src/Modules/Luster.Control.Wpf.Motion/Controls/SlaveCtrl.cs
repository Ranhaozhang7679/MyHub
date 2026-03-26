using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
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
    public class SlaveCtrl : System.Windows.Controls.Control
    {
        /// <summary>
        /// 值发生改变
        /// </summary>
        public event Action<ParameterAttribute> ValueChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SlaveCtrl()
        {
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
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SlaveCtrl), new PropertyMetadata(false));


        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SlaveCtrl),
                new PropertyMetadata(default, new PropertyChangedCallback(PropertyChangedCallback)));

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SlaveCtrl ctrl = (SlaveCtrl)d;
            if (ctrl != null)
            {
                if (e.NewValue == null)
                {
                    ctrl.Text = "";
                }
                else
                {
                    var vSlave = e.NewValue as SocketAction;
                    if (vSlave != null)
                    {
                        ctrl.Text = vSlave.Name;
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
            DependencyProperty.Register("Text", typeof(string), typeof(SlaveCtrl), new PropertyMetadata(""));

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
                    var vSlave = value.Value as SocketAction;
                    if (vSlave != null)
                    {
                        Text = vSlave.Name;
                    }
                }

                ValueChanged?.Invoke(parameter);
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
