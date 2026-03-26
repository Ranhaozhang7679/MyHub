#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IReturn
* 机器名称:       L05123-02
* 命名空间:       Luster.TaskFlow.Motion.Interfaces
* 文 件 名:       IReturn.cs
* 创建时间:       2023/1/30 9:50:59
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ca2d5824-4e2b-4ae5-873b-9f388e66eb8d
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/1/30 9:50:59
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    public interface IReturn
    {
        /// <summary>
        /// 是否Return
        /// </summary>
        /// <returns></returns>
        bool IsReturn();
    }
}