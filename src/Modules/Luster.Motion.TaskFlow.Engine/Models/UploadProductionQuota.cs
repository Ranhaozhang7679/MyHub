#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UploadProductionQuota
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       UploadProductionQuota.cs
* 创建时间:       2022/9/28 16:09:38
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      74b1eb96-b1f8-483c-96b0-acdc4dc0bb79
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/28 16:09:38
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class UploadProductionQuota
    {
        public int InputQty { get; set; } = 0;

        public int OutputQty { get; set; } = 0;

        public int PassQty { get; set; } = 0;

        public int FailQty { get; set; } = 0;

        public int TossingQty { get; set; } = 0;

        public int RetestQty { get; set; } = 0;

        public int TargetUph { get; set; } = 0;

        public int RealUph { get; set; } = 0;

        public double Yield { get; set; } = 0;

        public double TossingRate { get; set; } = 0;

        public double RetestRate { get; set; } = 0;

        public Dictionary<string, int> throwDic = new Dictionary<string, int>();

        // 入料时间列表
        public List<DateTime> ProductInTimes = new List<DateTime>();

        // 出料时间列表
        public List<DateTime> ProductOutTimes = new List<DateTime>();

        // Ok产品时间列表
        public List<DateTime> ProductOKTimes = new List<DateTime>();

        // NG产品时间列表
        public List<DateTime> ProductNGTimes = new List<DateTime>();

        // 抛料时间列表
        public List<DateTime> TossingTimes = new List<DateTime>();

        // 重试时间列表
        public List<DateTime> RetestTimes = new List<DateTime>();

        //抛料产品对应时间列表
        public Dictionary<string,List<DateTime>> throwDicTime=new Dictionary<string,List<DateTime>>();

        public UploadProductionQuota()
        {

        }

        public UploadProductionQuota(UploadProductionQuota data)
        {
            if (data != null)
            {
                ProductInTimes = new List<DateTime>(data.ProductInTimes);
                ProductOutTimes = new List<DateTime>(data.ProductInTimes);
                ProductOKTimes = new List<DateTime>(data.ProductOKTimes);
                ProductNGTimes = new List<DateTime>(data.ProductNGTimes);
                TossingTimes = new List<DateTime>(data.TossingTimes);
                RetestTimes = new List<DateTime>(data.RetestTimes);
                throwDicTime = new Dictionary<string, List<DateTime>>();
            }
        }

        public UploadProductionQuota GetUploadProductionQuotaByTime(DateTime startTime, DateTime endTime)
        {
            var data = new UploadProductionQuota(this);
            data.InputQty = data.ProductInTimes.Count(x => x > startTime && x < endTime);
            data.OutputQty = data.ProductOutTimes.Count(x => x > startTime && x <= endTime);
            data.PassQty = data.ProductOKTimes.Count(x => x > startTime && x <= endTime);
            data.FailQty = data.ProductNGTimes.Count(x => x > startTime && x <= endTime);
            data.TossingQty = data.TossingTimes.Count(x => x > startTime && x < endTime);
            data.RetestQty = data.RetestTimes.Count(x => x > startTime && x < endTime);
            data.Yield = data.InputQty == 0 ? 100 : Math.Round((double)data.PassQty / data.InputQty, 4);
            data.TossingRate = data.InputQty == 0 ? 0 : Math.Round((double)data.TossingQty / data.InputQty, 4);
            data.RetestRate = data.InputQty == 0 ? 0 : Math.Round((double)data.RetestQty / data.InputQty, 4);
            TossingCalc(data, startTime,endTime);
            return data;
        }


        public void AddProductInTime(DateTime time)
        {
            ProductInTimes.Add(time);
        }

        public void AddProductOutTime(DateTime time)
        {
            ProductOutTimes.Add(time);
        }

        public void AddOKProductTime(DateTime time)
        {
            ProductOKTimes.Add(time);
            AddProductOutTime(time);
        }

        public void AddNGProductTime(DateTime time)
        {
            ProductNGTimes.Add(time);
            AddProductOutTime(time);
        }

        public void AddTossingProductTime(DateTime time,string material)
        {
            TossingTimes.Add(time);
            if (throwDicTime.Keys.Contains(material))
            {
                throwDicTime[material].Add(time);
            }
            else
            {
                List<DateTime> throwtimeList=new List<DateTime>();
                throwtimeList.Add(time);
                throwDicTime.Add(material, throwtimeList);
            }
        }

        public void AddRetestProductTime(DateTime time)
        {
            RetestTimes.Add(time);
        }

        /// <summary>
        /// 计算对应时间内的出料数量
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public void TossingCalc(UploadProductionQuota data, DateTime startTime, DateTime endTime)
        {
            Dictionary<string, int> Dicthrow = new Dictionary<string, int>();
            foreach (var item in throwDicTime)
            {
                if (item.Value != null)
                {
                    int count = item.Value.Count(x => x > startTime && x <= endTime);
                    Dicthrow.Add(item.Key,count);
                }
            }
            data.throwDic = Dicthrow;
        }

    }
}
