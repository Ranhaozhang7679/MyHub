#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VRectangular
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Core
* 文 件 名:       VRectangular.cs
* 创建时间:       2022/1/25 9:25:35
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      42d3244c-0abc-4520-9e54-1523bd0989e2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/25 9:25:35
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm
{
    public class VRectangular : LDisposable
    {
        public VRectangular() : base(IntPtr.Zero)
        {

        }

        public VRectangular(IntPtr ptr) : base(ptr)
        {

        }

        [Description("中心点")]
        public VVector Center
        {
            get
            {
                NativeAPI.GetRectangularCenterPoint(LPtr, out var point, out _);
                return new VVector(point);
            }
        }
    }
}
