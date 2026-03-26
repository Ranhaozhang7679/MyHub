#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PCIe
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Adapter
* 文 件 名:       PCIe.cs
* 创建时间:       2022/4/11 15:23:33
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      eebf30f8-adbf-47ba-9ac2-50af8e4a5989
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 15:23:33
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
    /// PCie卡
    /// </summary>
    public class PCIe : AdapterBase
    {
        /// <summary>
        /// 插槽索引
        /// </summary>
        public int Slot { get; set; }


        public override string GetMethod()
        {
            return $"Slot{Slot}";
        }

        public override void SetMethod(string slot)
        {
            if (!string.IsNullOrEmpty(slot))
            {
                Slot = Convert.ToInt32(slot.Substring(4, slot.Length - 4)) - 1;
            }
        }
    }
}