#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LDouble
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LDouble.cs
* 创建时间:       2022/6/26 17:59:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      99111ca8-8e7c-4731-a9bd-f8c87a5f2bbb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/26 17:59:21
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.DataModels
{
    public class LDouble : IGetProperty
    {
        /// <summary>
        /// 属性值
        /// </summary>
        public double Value { get; set; } = 0;

        public LDouble()
        {

        }

        public LDouble(double v)
        {
            Value = v;
        }

        public object GetProperty(string propName)
        {
            var prop = GetType().GetProperty(propName);
            if (prop != null)
            {
                return prop.GetValue(this, null);
            }

            return null;
        }

        public override string ToString()
        {
            return Math.Round(Value, 4).ToString();
        }
    }
}