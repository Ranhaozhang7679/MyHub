#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MsgTipDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SubSystem.ThreeD.ViewModel.Dialog
* 文 件 名:       MsgTipDialogVM.cs
* 创建时间:       2022/1/7 14:10:10
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      42fbe4da-3bc1-4ded-8bc6-4eae4a816f62
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/7 14:10:10
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Common.Assets.ViewModel
{
    /// <summary>
    /// 消息提醒类型
    /// </summary>
    public enum MessageType
    {
        Success,
        Error,
        Info,
        Confirm,
        ConfirmWithoutNo
    }

    /// <summary>
    /// 用于消息提醒功能
    /// </summary>
    public class MessageDialogVM : BaseDialogVM
    {
        /// <summary>
        /// 消息内容
        /// </summary>
        private string _msgInfo;

        public string MsgInfo
        {
            get { return _msgInfo; }
            set { SetProperty(ref _msgInfo, value); }
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        private string _msgIcon;

        public string MsgIcon
        {
            get { return _msgIcon; }
            set { SetProperty(ref _msgIcon, value); }
        }

        /// <summary>
        /// 颜色信息
        /// </summary>
        private SolidColorBrush _foreBrush;

        public SolidColorBrush Foreground
        {
            get { return _foreBrush; }
            set { SetProperty(ref _foreBrush, value); }
        }

        /// <summary>
        /// 消息类型
        /// </summary>
        private MessageType _messageType;

        public MessageType MessageType
        {
            get => _messageType; set
            {
                SetProperty(ref _messageType, value);
            }
        }

        /// <summary>
        /// 是否取消
        /// </summary>
        private bool _isCancelButton;

        public bool IsCancelButton
        {
            get { return _isCancelButton; }
            set { SetProperty(ref _isCancelButton, value); }
        }

        /// <summary>
        /// 是否没有NoButton
        /// </summary>
        private bool _isNoButton;

        public bool IsNoButton
        {
            get { return _isNoButton; }
            set { SetProperty(ref _isNoButton, value); }
        }


        /// <summary>
        /// Dialog的文本信息
        /// </summary>
        private string _btnOkText;
        public string BtnYesText
        {
            get { return _btnOkText; }
            set { SetProperty(ref _btnOkText, value); }
        }

        /// <summary>
        /// Dialog的文本信息
        /// </summary>
        private string _btnCancelText;
        public string BtnCancelText
        {
            get { return _btnCancelText; }
            set { SetProperty(ref _btnCancelText, value); }
        }

        /// <summary>
        /// Dialog的文本信息
        /// </summary>
        private string _btnNoText;
        public string BtnNoText
        {
            get { return _btnNoText; }
            set { SetProperty(ref _btnNoText, value); }
        }

        /// <summary>
        /// 消息提醒
        /// </summary>
        public MessageDialogVM()
        {
            _foreBrush = Brushes.Blue;
        }

        /// <summary>
        /// 自定义关闭方法
        /// </summary>
        /// <param name="parameter">xaml command传递过来的参数</param>
        protected override void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            // OK和Cancel是xaml按钮传递过来的值
            if (parameter?.ToLower() == "yes")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "no")
                result = ButtonResult.No;
            else if (parameter?.ToLower() == "cancel")
                result = ButtonResult.Cancel;

            RaiseRequestClose(new DialogResult(result));
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            BtnCancelText = L("Cancel");
            BtnYesText = L("Yes");
            BtnNoText = L("No");
            // 图标
            if (parameters.TryGetValue<string>("MsgIcon", out var icon))
            {
                MsgIcon = icon;
            }

            // 文本信息
            if (parameters.TryGetValue<string>("Message", out var text))
            {
                MsgInfo = text;
            }

            // 文本信息
            if (parameters.TryGetValue<MessageType>("MessageType", out var msgType))
            {
                MessageType = msgType;
                switch (msgType)
                {
                    case MessageType.Error:
                        Foreground = Brushes.Red;
                        break;

                    case MessageType.Confirm:
                        Foreground = Brushes.Orange;
                        IsCancelButton = true;
                        IsNoButton = true;
                        break;

                    case MessageType.ConfirmWithoutNo:
                        Foreground = Brushes.Orange;
                        IsCancelButton = true;
                        IsNoButton = false;
                        break;

                    case MessageType.Info:
                        Foreground = Brushes.Blue;
                        break;

                    case MessageType.Success:
                        Foreground = Brushes.Green;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}