#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PostRunEventArgs
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Events
* 文 件 名:       PostRunEventArgs.cs
* 创建时间:       2022/5/19 15:36:44
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      037aa69e-c80f-40db-953b-a56e6c4eaf62
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/19 15:36:44
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Events
{
    public class PostRunEventArgs
    {
        /// <summary>
        /// 执行动作
        /// </summary>
        public RunStatus RunType { get; set; }

        /// <summary>
        /// 是否运行成功
        /// </summary>
        public bool IsSucess { get; set; }

        /// <summary>
        /// 执行错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 耗时,单位ms
        /// </summary>
        public float Time { get; set; }

        ///// <summary>
        ///// 输出
        ///// </summary>
        //public List<ParameterInfo> Outputs { get; set; }

        /// <summary>
        /// 外部信息
        /// </summary>
        public Dictionary<string, object> ExternalInfo { get; set; }

        /// <summary>
        /// 引擎对象
        /// </summary>
        public IMotionEngine MotionEngine { get; set; }
    }
}