#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Calculator
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       Calculator.cs
* 创建时间:       2022/9/30 19:45:30
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b6327b94-d48d-405d-93c0-891852d50913
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/30 19:45:30
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.Integration.Web;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public class WebConfigRead : MotionFunction
    {
        [Parameter("Result", 2, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        [Parameter("设备SN", 2, CN = "设备SN", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string MachineSN { get; set; }

        [Parameter("QPLNum", 2, CN = "多工站编号", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string QPLNum { get; set; }

        public WebConfigRead()
        {
            this.Icon = "\xe629";
            this.Tips = "设备参数读取";
        }
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            Result = true;
            QPLNum = MyOwner?.ConfigManager?.GetWebConfig("UniteCode") ?? string.Empty;
            MachineSN = MyOwner?.ConfigManager?.GetWebConfig("MachineSN") ?? string.Empty;
            if (string.IsNullOrEmpty(QPLNum))
            {
                //errMsg = "读取QPLNum设备参数失败，请检查配置文件";
                MyOwner.OnLog(LogType.Warning, $"QPLNum设备参数取值为空，请检查配置文件");
                Result = false;
            }
            if (string.IsNullOrEmpty(MachineSN))
            {
                MyOwner.OnLog(LogType.Warning, $"MachineSN设备参数取值为空，请检查配置文件");
                Result = false;
            }

            return string.IsNullOrEmpty(errMsg);
        }

    }
}