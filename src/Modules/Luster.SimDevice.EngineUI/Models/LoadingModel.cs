#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LoadingModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       LoadingModel.cs
* 创建时间:       2022/5/13 13:44:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      12d41ca4-b019-4db2-be67-a9d8f67343dd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/13 13:44:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class LoadingModel
    {
        /// <summary>
        /// 最大值
        /// </summary>
        public int MaxValue { get; set; }

        public int Value { get; set; }

        public int LoadingVal => (int)Math.Round(Value * 100.0 / MaxValue, 0);

        public string LoadingMsg { get; set; }

        public bool IsLoading => Value < MaxValue;
    }
}