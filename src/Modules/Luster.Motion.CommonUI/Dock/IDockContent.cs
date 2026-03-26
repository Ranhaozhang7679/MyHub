#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IDockCotent
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.SubSystem.Dock
* 文 件 名:       IDockCotent.cs
* 创建时间:       2023/2/21 10:18:18
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d717950f-696e-45ad-b020-ccb24c9b5027
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/21 10:18:18
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Dock
{
    public interface IDockContent
    {
        /// <summary>
        /// Dock的唯一标识
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 对应的Content名称
        /// </summary>
        string RegionName { get; set; }
    }
}