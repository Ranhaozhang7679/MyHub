#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProtocolType
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       ProtocolType.cs
* 创建时间:       2022/7/20 19:27:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9d735ecc-0999-464a-a19c-f6424f100836
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/20 19:27:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    /// <summary>
    /// 协议类型
    /// </summary>
    public enum ProtocolType
    {
        [Description("字符串默认")]
        StringDefault,

        [Description("字符串Utf8")]
        StringUtf8,

        [Description("字符串Hex")]
        StringHex,

        [Description("ModbusRTU")]
        ModbusRTU,

        [Description("ModbusASCII")]
        ModbusASCII,

        [Description("ModbusTCP")]
        ModbusTCP,

        [Description("MCBinary")]
        MCBinary,

        [Description("FinsTCP")]
        FinsTCP,
        //[Description("HslModbusTcp")]
        //HslModbusTcp
    }


    /// <summary>
    /// 动作类型
    /// </summary>
    public enum ActionType
    {
        [Description("读取")]
        Read,

        [Description("写入")]
        Write
    }
}