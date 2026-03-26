#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LTolerance
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LTolerance.cs
* 创建时间:       2021/12/22 14:57:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      ba7b16bd-b03b-4a48-b62c-45b4cded61a7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/22 14:57:45
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.DataModels
{
    /// <summary>
    /// 公差对象
    /// </summary>
    public class LTolerance 
    {
        /// <summary>
        /// 对象类型
        /// </summary>
        public ToleranceType TolerenceType { get; set; }

        /// <summary>
        /// 中文名称
        /// </summary>
        public string Name
        {
            get => TolerenceType.GetDescription();
            set
            {
            }
        }

        /// <summary>
        /// 标准值
        /// </summary>
        public double Standard { get; set; } = 0;

        /// <summary>
        /// 测量值
        /// </summary>
        public double Measure { get; set; } = 0;

        /// <summary>
        /// 公差下限
        /// </summary>
        public double ToleranceMin { get; set; } = -0.1;

        /// <summary>
        /// 公差下限
        /// </summary>
        public double ToleranceMax { get; set; } = 1.0;

        /// <summary>
        /// 误差值
        /// </summary>
        public double Error => Measure + Compensate - Standard;

        /// <summary>
        /// 是否通过
        /// </summary>
        public bool IsPass => Error >= ToleranceMin && Error <= ToleranceMax;

        /// <summary>
        /// 补偿值
        /// </summary>
        public double Compensate { get; set; } = 0;


        public override string ToString()
        {
            return $"{Math.Round(Measure, 5)},Pass={IsPass}";
        }
    }
}