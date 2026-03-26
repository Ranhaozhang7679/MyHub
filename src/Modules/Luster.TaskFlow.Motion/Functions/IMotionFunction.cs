#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Motion
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Functions
* 文 件 名:       Motion.cs
* 创建时间:       2022/5/19 18:14:10
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      8a8e0965-86d6-4abd-bb08-3618f8b04f60
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/19 18:14:10
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion
{
    /// <summary>
    /// 对应的接口
    /// </summary>
    public interface IMotionFunction : IFunction
    {
        void Added();

        /// <summary>
        /// 注释变更
        /// </summary>
        event Action<string> NoteChangedEvent;

        /// <summary>
        /// 获取注释
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        string GetNote(INote note);

        /// <summary>
        /// 模块终止
        /// </summary>
        void Stop();
    }
}