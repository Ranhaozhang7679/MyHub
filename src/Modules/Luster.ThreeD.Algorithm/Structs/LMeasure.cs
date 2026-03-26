#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LMeasure
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Structs
* 文 件 名:       LMeasure.cs
* 创建时间:       2022/4/12 13:44:01
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1740811b-ca77-43e8-9fe5-e6f9aa120f50
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/12 13:44:01
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LMeasure
    {
        // 标准值
        public double Standard { get; set; }

        //实际值
        public double Value { get; set; }

        //最大值
        public double Max { get; set; }

        //最小值
        public double Min { get; set; }

        //结果  true:pass  
        public bool IsPass { get; set; }
    }
}