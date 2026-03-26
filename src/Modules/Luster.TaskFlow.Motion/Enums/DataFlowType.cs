#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DataFlowType
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Enums
* 文 件 名:       DataFlowType.cs
* 创建时间:       2022/7/4 15:26:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f06fd051-5fa1-412e-a530-53c7fe37826e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/4 15:26:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Enums
{
    /// <summary>
    /// 站别位置
    /// </summary>
    public enum DataFlowType
    {
        [Description("数据缓存")]
        Cache,

        [Description("数据获取")]
        Obtain,

        [Description("最后一站")]
        Last
    }
}