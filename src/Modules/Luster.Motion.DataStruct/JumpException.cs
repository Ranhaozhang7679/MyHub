#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       JumpException
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct
* 文 件 名:       JumpException.cs
* 创建时间:       2022/8/5 12:47:54
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5fda243b-68f7-454f-b042-1d6b597fa40e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/5 12:47:54
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct
{
    /// <summary>
    /// 跳出循环，终止方法继续运行
    /// </summary>
    public class JumpException : Exception
    {
        public JumpException(string message)
        {

        }
    }
}