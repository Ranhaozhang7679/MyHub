#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IntExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Extensions
* 文 件 名:       IntExtension.cs
* 创建时间:       2022/6/21 13:02:25
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      17e4ef6a-8fe3-497a-92e5-0814df4e3621
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/21 13:02:25
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Extensions
{
    public static class IntExtension
    {
        /// <summary>
        /// 创建一个Int 数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="start"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static List<int> GenArray<T>(this T array, int start, int len) where T : struct
        {
            List<int> list = new List<int>();
            for (int i = start; i < len; i++)
            {
                list.Add(i);
            }

            return list;
        }
    }
}