#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbProductInfo
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Tables
* 文 件 名:       TbProductInfo.cs
* 创建时间:       2022/10/14 10:16:25
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      7c59a73b-23bc-4752-a21e-933cbc2282c1
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:16:25
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using FreeSql.DataAnnotations;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// 生产信息表
    /// </summary>
    [Table(Name = "ProductInfoTable")]
    public class TbProductInfo : BaseTable
    {
        /// <summary>
        /// 条码信息
        /// </summary>
        [PropSort(1), DisplayName("SN编码"), StringLength(100)]
        public string SNCode { get; set; }

        /// <summary>
        /// 治具号
        /// </summary>
        [PropSort(2), DisplayName("治具号"), StringLength(100)]
        public string Jig { get; set; }

        /// <summary>
        /// 最终结果
        /// </summary>
        [PropSort(3), DisplayName("结果")]
        public string Result { get; set; }

        /// <summary>
        /// 是否抛料
        /// </summary>
        [PropSort(4), DisplayName("是否抛料")]
        public bool IsToss { get; set; }


        [PropSort(5), DisplayName("NGCode")]
        public string NgCode { get; set; }

        /// <summary>
        /// 是否抛料
        /// </summary>
        [PropSort(6), DisplayName("NG原因")]
        public string NgReason { get; set; }


        /// <summary>
        /// 图片存储路劲
        /// </summary>
        [PropSort(7), DisplayName("图片存储路劲")]
        public string ImagePath { get; set; }

        [Ignore]
        public string Data { get; set; }

        /// <summary>
        /// 扫码时间
        /// </summary>
        [PropSort(8), DisplayName("入站时间")]
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 出站时间
        /// </summary>
        [PropSort(9), DisplayName("出站时间")]
        public DateTime OutTime { get; set; }

        /// <summary>
        /// 机台运行模式
        /// </summary>
        [PropSort(10), DisplayName("模式")]
        public int Mode { get; set; }

        /// <summary>
        /// 出站时间
        /// </summary>
        [PropSort(11), DisplayName("CT")]
        public double CT { get; set; }

        /// <summary>
        /// 出站时间
        /// </summary>
        [PropSort(12), DisplayName("2次出料时间差")]
        public double NewCT { get; set; }

        /// <summary>
        /// WIP工单
        /// </summary>
        [PropSort(13), DisplayName("工单号"), StringLength(100)]
        public string Wip { get; set; }

        /// <summary>
        /// 运行时间
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetDataStr()
        {
            var newDict = new Dictionary<string, object>();

            //有测量数据  进入
            if (!string.IsNullOrEmpty(Data))
            {
                var splitGroup = Data.Split('|');
                foreach (var item in splitGroup)
                {
                    var tempValue = item.Split(':');
                    if (tempValue.Length == 2)
                    {
                        string key = tempValue[0];
                        string val = tempValue[1];
                        if (newDict.ContainsKey(key))
                        {
                            newDict[key] = val;
                        }
                        else
                        {
                            newDict.Add(key, val);
                        }
                    }
                }
            }

            return newDict;
        }
    }

    // 动态构建数据信息
}
