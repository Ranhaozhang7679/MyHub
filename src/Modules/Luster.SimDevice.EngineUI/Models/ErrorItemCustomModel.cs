#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MaintainItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       MaintainItemModel.cs
* 创建时间:       2022/12/9 10:55:36
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      19f317a9-8d7b-4435-80a5-4bbfaa5dc9cb
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/9 10:55:36
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Virtual;
using Luster.SimDevice.Real;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.SimDevice.EngineUI.Models
{
    public class ErrorItemCustomModel : BindableBase
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        private string _alarmCode;
        public string AlarmCode
        {
            get { return _alarmCode; }
            set 
            { 
                if (_alarmCode != value) // 当值未发生变化时，防止设置OldAlarmCode
                {
                    OldAlarmCode = _alarmCode;
                }
                SetProperty(ref _alarmCode, value); 
            }
        }

        public string OldAlarmCode { get; set; }

        /// <summary>
        /// 报警内容
        /// </summary>
        private string _alarmContent;
        public string AlarmContent
        {
            get { return _alarmContent; }
            set { SetProperty(ref _alarmContent, value); }
        }

        /// <summary>
        /// 报警英文
        /// </summary>
        private string _alarmEnglish;
        public string AlarmEnglish
        {
            get { return _alarmEnglish; }
            set { SetProperty(ref _alarmEnglish, value); }
        }

        public ErrorItemCustomModel(string alarmCode, string alarmContent, string alarmEnglish)
        {
            AlarmCode = alarmCode;
            OldAlarmCode = alarmCode;
            AlarmContent = alarmContent;
            AlarmEnglish = alarmEnglish;
        }
    }
}
