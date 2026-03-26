#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LStatus
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LStatus.cs
* 创建时间:       2022/10/25 15:16:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1ec0278e-f8a2-4125-acf5-eac3a64cfdb2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/25 15:16:21
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.DataModels
{
    /// <summary>
    /// 是否运行完成
    /// </summary>
    public class LStatus
    {
        /// <summary>
        /// 是否运行完成
        /// </summary>
        public bool IsRunEnd { get; set; } = false;

        /// <summary>
        /// 隶属对象
        /// </summary>
        public object Owner { get; set; }

        public override string ToString()
        {
            return IsRunEnd ? "Success" : "Default";
        }

        /// <summary>
        /// 获取状态，如果状态成功,那么更新状态值
        /// </summary>
        /// <returns></returns>
        public bool GetStatusAndDefault()
        {
            lock (this)
            {
                if (IsRunEnd)
                {
                    IsRunEnd = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void SetDefault()
        {
            lock (this)
            {
                IsRunEnd = false;
            }
        }

        public void SetEnd()
        {
            lock (this)
            {
                IsRunEnd = true;
            }
        }
    }
}