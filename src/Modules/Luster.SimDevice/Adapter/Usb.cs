#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Usb
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Adapter
* 文 件 名:       Usb.cs
* 创建时间:       2022/4/11 15:23:22
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      3f3d883c-257c-46a4-a77e-be0fefb7bb38
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 15:23:22
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Adapter
{

    /// <summary>
    /// Usb
    /// </summary>
    public class Usb : AdapterBase
    {
        public string Name { get; set; }

        public override string GetMethod()
        {
            return Name;
        }

        public override void SetMethod(string name)
        {
            Name = name;
        }
    }
}