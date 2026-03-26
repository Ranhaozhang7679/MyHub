#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SaveProductModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       SaveProductModel.cs
* 创建时间:       2022/9/14 9:56:22
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      1e147c5a-5618-4ba4-b0b1-8df35531b3bf
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/14 9:56:22
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows;
using Luster.Motion.DataStruct.DataModels;

namespace Luster.Motion.CommonUI.Models
{
    public class SaveProductModel
    {
        /// <summary>
        /// 产品码信息
        /// </summary>
        [PropSort(0), DisplayName("产品编码")]
        public string WIP_SN { get; set; } // WIP

        /// <summary>
        /// 条码信息
        /// </summary>
        [PropSort(1), DisplayName("SN编码")]
        public string MATERIAL_SN { get; set; } //SNCode

        /// <summary>
        /// 治具号
        /// </summary>
        [PropSort(2), DisplayName("治具号")]
        public string Jig { get; set; }

        /// <summary>
        /// 最终结果
        /// </summary>
        [PropSort(3), DisplayName("结果")]
        public string Result { get; set; }

        /// <summary>
        /// 扫码时间
        /// </summary>
        [PropSort(4), DisplayName("入站时间")]
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 出站时间
        /// </summary>
        [PropSort(5), DisplayName("出站时间")]
        public DateTime Time { get; set; } //OutTime

        /// <summary>
        /// 运行模式：生產模式：0 ；Audit模式：1；空跑模式：2
        /// </summary>
        [PropSort(6), DisplayName("模式")]
        public int Mode { get; set; }

        /// <summary>
        /// CT
        /// </summary>
        [PropSort(7), DisplayName("CT")]
        public Double CT { get; set; }

        /// <summary>
        /// CT
        /// </summary>
        [PropSort(8), DisplayName("2次出料时差")]
        public Double NewCT { get; set; }

        /// <summary>
        /// 图片存储路径
        /// </summary>
        [PropSort(9), DisplayName("Ng原因")]
        public string NgReason { get; set; }

        /// <summary>
        /// 图片存储路径
        /// </summary>
        [PropSort(10), DisplayName("图片存储路径")]
        public string ImagePath { get; set; }

        public Dictionary<string, object> Data { get; set; }


        public SaveProductModel(TbProductInfo productInfo, List<string> headers = null)
        {
            MATERIAL_SN = productInfo.SNCode;
            WIP_SN = productInfo.Wip;
            Jig = productInfo.Jig;
            ImagePath = productInfo.ImagePath;
            //Result = productInfo.IsToss ? "Toss" : productInfo.Result;
            Result = productInfo.Result;
            EnterTime = productInfo.EnterTime;
            Time = productInfo.OutTime;
            Mode = productInfo.Mode;
            CT = productInfo.CT;
            NewCT = productInfo.NewCT;
            NgReason=productInfo.NgReason;
            Data = new Dictionary<string, object>();

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    Data.Add(item, "");
                }
            }

            if (!string.IsNullOrEmpty(productInfo.Data))
            {
                var spitGroup = productInfo.Data.Split('|');
                foreach (var item in spitGroup)
                {
                    var tempValue = item.Split(':');
                    if (tempValue.Length == 2)
                    {
                        string key = tempValue[0];
                        if (Data.ContainsKey(key))
                        {
                            Data[key] = tempValue[1];
                        }
                        else
                        {
                            Data.Add(key, tempValue[1]);
                        }
                    }
                }
            }
        }
    }

    public class SaveTossingModel
    {
        /// <summary>
        /// 产品码信息
        /// </summary>
        [PropSort(0), DisplayName("产品编码")]
        public string WIP_SN { get; set; }

        /// <summary>
        /// 条码信息
        /// </summary>
        [PropSort(1), DisplayName("SN编码")]
        public string MATERIAL_SN { get; set; }

        /// <summary>
        /// 最终结果
        /// </summary>
        [PropSort(2), DisplayName("结果")]
        public string RESULT { get; set; }

        /// <summary>
        /// 抛料时间
        /// </summary>
        [PropSort(3), DisplayName("抛料时间")]
        public DateTime TIME { get; set; }

        /// <summary>
        /// 抛料原因
        /// </summary>
        [PropSort(4), DisplayName("抛料原因")]
        public string TOSS_REASON { get; set; }

        /// <summary>
        /// CT
        /// </summary>
        [PropSort(5), DisplayName("抛料类型")]
        public string TOSS_TYPE { get; set; }

        /// <summary>
        /// 抛料总数
        /// </summary>
        [PropSort(6), DisplayName("抛料个数")]
        public int TOSS_NUM { get; set; }

        /// <summary>
        /// 运行模式：生產模式：0 ；Audit模式：1；空跑模式：2
        /// </summary>
        [PropSort(7), DisplayName("模式")]
        public int Mode { get; set; }

        public SaveTossingModel(TbThrow tossInfo)
        {
            WIP_SN = tossInfo.Wip;
            MATERIAL_SN = tossInfo.SNCode;
            TIME = DateTime.Now;
            RESULT = "NG";
            TOSS_REASON = tossInfo.Reason;
            TOSS_TYPE = string.IsNullOrEmpty(MATERIAL_SN) ? "LotNo" : "KeyPart";
            TOSS_NUM = 1;
            Mode = tossInfo.Mode;
        }
        public SaveTossingModel(TbProductInfo productInfo)
        {
            WIP_SN = productInfo.Wip;
            MATERIAL_SN = productInfo.SNCode;
            RESULT = productInfo.Result; // "Toss";
            TIME = productInfo.OutTime;
            TOSS_REASON = productInfo.NgReason;
            TOSS_TYPE = "Keypart";
            TOSS_NUM = 1;
            Mode = productInfo.Mode;
        }
    }

}
