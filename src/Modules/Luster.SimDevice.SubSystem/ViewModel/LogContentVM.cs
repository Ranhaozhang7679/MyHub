#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogContent
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       LogContent.cs
* 创建时间:       2022/5/12 9:33:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      61c74b9d-2724-4ebe-ab4f-03d0d9d4d6b5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/12 9:33:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class LogContentVM : BaseVM
    {
        protected LogContentVM(ISimDeviceEngineUI engineUI) : base(engineUI)
        {
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="engineUI"></param>
        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            base.Subscribe(engineUI);
            engineUI.Subscribe<LogEvent, LogInfo>((logInfo) =>
            {
                LogMessage = logInfo.LogMessage;
                if (logInfo.LogType == LogType.Error)
                {
                    LogMessage = logInfo.LogMessage;
                    ForeBrush = "Red";
                }
                else if (logInfo.LogType == LogType.Info)
                {
                    LogMessage = logInfo.LogMessage;
                    ForeBrush = "Blue";
                }
                else if (logInfo.LogType == LogType.Warning)
                {
                    LogMessage = logInfo.LogMessage;
                    ForeBrush = "Orange";
                }
                else
                {
                    LogMessage = String.Empty;
                }
            });
        }


        /// <summary>
        /// 消息
        /// </summary>
        private string _logMessage;
        public string LogMessage
        {
            get { return _logMessage; }
            set { SetProperty(ref _logMessage, value); }
        }

        /// <summary>
        /// 前景色
        /// </summary>
        private string _foreBrush;
        public string ForeBrush
        {
            get { return _foreBrush; }
            set { SetProperty(ref _foreBrush, value); }
        }

    }
}