#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FriendlyException
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Exceptions
* 文 件 名:       FriendlyException
* 创建时间:       2021/10/30 18:37:56
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      98e9cec3-aa69-431b-9549-8f22870e783b
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/30 18:37:56
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct
{
    public class FriendlyException : Exception
    {
        /// <summary>
        /// 异常信息
        /// </summary>
        /// <param name="errMsg">名称</param>
        public FriendlyException(string errMsg) : base(errMsg)
        {
        }

        /// <summary>
        /// 异常
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <param name="ex">异常</param>
        public FriendlyException(string errMsg, Exception ex) : base(errMsg, ex)
        {
        }
    }
}
