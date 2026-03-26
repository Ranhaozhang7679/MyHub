#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CommResult
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       CommResult.cs
* 创建时间:       2022/7/20 17:31:49
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d6fe4ec1-da98-4d0e-b27a-0135be640206
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/20 17:31:49
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
    /// 通信结果
    /// </summary>
    public class CommResult<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 数据结果
        /// </summary>
        public List<T> Datas { get; set; }

        /// <summary>
        /// 空构造函数
        /// </summary>
        public CommResult()
        {
            IsSuccess = true;
            ErrorMsg = "";
            Datas = new List<T>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="errorMsg"></param>
        public CommResult(bool isSuccess, string errorMsg)
        {
            IsSuccess = isSuccess;
            ErrorMsg = errorMsg;
            Datas = new List<T>();
        }
    }

    public class CommResult : CommResult<bool>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="errorMsg"></param>
        public CommResult(bool isSuccess, string errorMsg = "")
        {
            IsSuccess = isSuccess;
            ErrorMsg = errorMsg;
            Datas = new List<bool>();
        }
    }
}