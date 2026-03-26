#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NetworkCtrl
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Controls
* 文 件 名:       NetworkCtrl.cs
* 创建时间:       2022/7/4 21:29:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a870ada2-d4bc-40a6-8603-39f9f8bce0c4
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/4 21:29:51
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Luster.Control.Wpf.Motion.Controls
{
    public class NetworkCtrl : System.Windows.Controls.Control
    {
        public NetworkCtrl()
        {
            IsInit = true;
        }

        static bool IsInit = true;

        public LNetwork Network
        {
            get { return (LNetwork)GetValue(NetworkProperty); }
            set { SetValue(NetworkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Network.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NetworkProperty =
            DependencyProperty.Register("Network", typeof(LNetwork), typeof(NetworkCtrl), new PropertyMetadata(default, new PropertyChangedCallback(NetPropertyChangedCallback)));

        /// <summary>
        /// ip 地址回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void NetPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkCtrl ctrl = (NetworkCtrl)d;
            if (ctrl != null && IsInit)
            {
                if (ctrl.Network == null) return;

                ctrl.Ip = ctrl.Network.Ip;
                ctrl.Port = ctrl.Network.Port.ToString();

                IsInit = false;
            }
        }

        /// <summary>
        /// ip地址
        /// </summary>
        public string Ip
        {
            get { return (string)GetValue(IpProperty); }
            set { SetValue(IpProperty, value); }
        }
        public static readonly DependencyProperty IpProperty =
            DependencyProperty.Register("Ip", typeof(string), typeof(NetworkCtrl), new PropertyMetadata("127.0.0.1", new PropertyChangedCallback(PropertyChangedCallback)));

        /// <summary>
        /// 端口
        /// </summary>
        public string Port
        {
            get { return (string)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }
        public static readonly DependencyProperty PortProperty =
            DependencyProperty.Register("Port", typeof(string), typeof(NetworkCtrl), new PropertyMetadata("9999", new PropertyChangedCallback(PropertyChangedCallback)));

        /// <summary>
        /// ip 地址回调
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkCtrl ctrl = (NetworkCtrl)d;
            if (ctrl != null)
            {
                int port = 9999;
                int.TryParse(ctrl.Port, out port);
                ctrl.Network = new LNetwork() { Ip = ctrl.Ip, Port = port, Network = ctrl.Gateway, Submask = ctrl.Submask };
            }
        }
        /// <summary>
        /// 子网掩码
        /// </summary>
        public string Submask
        {
            get { return (string)GetValue(SubmaskProperty); }
            set { SetValue(SubmaskProperty, value); }
        }
        public static readonly DependencyProperty SubmaskProperty =
            DependencyProperty.Register("Submask", typeof(string), typeof(NetworkCtrl), new PropertyMetadata("255.255.0.0"));


        public string Gateway
        {
            get { return (string)GetValue(GatewayProperty); }
            set { SetValue(GatewayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Gateway.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GatewayProperty =
            DependencyProperty.Register("Gateway", typeof(string), typeof(NetworkCtrl), new PropertyMetadata(""));

        /// <summary>
        /// 显示详情
        /// </summary>
        public bool ShowPinPop
        {
            get { return (bool)GetValue(ShowPinPopProperty); }
            set { SetValue(ShowPinPopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowPipPop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowPinPopProperty =
            DependencyProperty.Register("ShowPinPop", typeof(bool), typeof(NetworkCtrl), new PropertyMetadata(false));



        private Button btn;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            btn = GetTemplateChild("PART_ViewButtonPOINT") as Button;
            btn.Click -= Btn_Click;
            btn.Click += Btn_Click;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            ShowPinPop = !ShowPinPop;
        }
    }
}