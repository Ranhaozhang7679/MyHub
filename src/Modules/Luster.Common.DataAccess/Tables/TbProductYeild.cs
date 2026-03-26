#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbProductYeild
* 机器名称:       L05123-02
* 命名空间:       Luster.Common.DataAccess.Tables
* 文 件 名:       TbProductYeild.cs
* 创建时间:       2023/1/6 21:51:58
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7dd506d9-07df-49d7-969a-55cf8085a593
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/1/6 21:51:58
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using FreeSql.DataAnnotations;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    [Table(Name = "ProductYeild")]
    public class TbProductYeild : BaseTable
    {
        /// <summary>
        /// 总数
        /// </summary>
        [PropSort(1), DisplayName("总数")]
        public int AllCount { get; set; }

        /// <summary>
        /// OK数量
        /// </summary>
        [PropSort(2), DisplayName("OK数量")]
        public int OKCount { get; set; }

        /// <summary>
        /// NG数量
        /// </summary>
        [PropSort(3), DisplayName("NG数量")]
        public int NGCount { get; set; }

        /// <summary>
        /// 抛料
        /// </summary>
        [PropSort(3), DisplayName("抛料数")]
        public int Toss { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [PropSort(4), DisplayName("数据")]
        public string Data { get; set; }

        /// <summary>
        /// 良率
        /// </summary>
        /// <returns></returns>
        public double GetYield()
        {
            var total = AllCount;
            if (total == 0)
            {
                return 100;
            }
            else
            {
                return Math.Round((((float)OKCount / total) * 100), 2);
            }
        }

        /// <summary>
        /// 抛料率
        /// </summary>
        /// <returns></returns>
        public double GetThrowRate(int count)
        {
            var total = AllCount;
            if (total == 0)
            {
                return 0;
            }
            else
            {
                return Math.Round((((float)count / total) * 100), 2);
            }
        }

        public Dictionary<string, int> GetThrowMaterial()
        {
            Dictionary<string, int> mData = new Dictionary<string, int>();
            if (string.IsNullOrEmpty(Data))
            {
                return mData;
            }

            foreach (var item in Data.Split('|'))
            {
                var items = item.Split('_');
                if (items.Length == 2 && int.TryParse(items[1], out var mNum))
                {
                    mData.Add(items[0], mNum);
                }
            }

            return mData;
        }

        public Dictionary<string, int> ThrowMaterial { get; set; } = new Dictionary<string, int>();
    }
}