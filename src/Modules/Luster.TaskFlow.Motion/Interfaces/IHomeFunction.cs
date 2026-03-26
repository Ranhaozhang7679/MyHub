#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ISuccess
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.interfaces
* 文 件 名:       ISuccess.cs
* 创建时间:       2022/6/29 16:33:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f07c93ac-7d77-4db6-a122-b9381132614e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/29 16:33:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.interfaces
{
    /// <summary>
    /// 模块会输出成功与否
    /// </summary>
    public interface IHomeFunction
    {
        void Home();
    }
}