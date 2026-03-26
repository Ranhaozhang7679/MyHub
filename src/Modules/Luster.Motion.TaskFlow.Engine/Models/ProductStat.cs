#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProductStat
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       ProductStat.cs
* 创建时间:       2022/7/27 9:37:22
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      44663ae2-20f2-41cf-8e60-2a26ebe2a7ee
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/27 9:37:22
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 产品信息统计
    /// </summary>
    public class ProductStat
    {
        /// <summary>
        /// 最终的产品结果
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        public int AllCount { get; set; }

        /// <summary>
        /// OK数量
        /// </summary>
        public int OKCount { get; set; }

        /// <summary>
        /// NG数量
        /// </summary>
        public int NGCount { get; set; }

        /// <summary>
        /// 产品抛料数量
        /// </summary>
        public int Toss { get; set; } = 0;

        public string NGCode { get; set; } = "";

        public string SN { get; set; }
        /// <summary>
        /// 抛料信息
        /// </summary>
        public Dictionary<string, int> ThrowMaterialDic { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// 抛料信息详细
        /// </summary>
        public Dictionary<string, List<(string, string)>> ThrowMaterialDicDetail { get; set; } = new Dictionary<string, List<(string, string)>>();

        /// <summary>
        /// 每小时产能数据
        /// </summary>
        public int UPH { get; set; } = 0;

        /// <summary>
        /// CT 时间
        /// </summary>
        public double RealCT { get; set; } = 0;


        /// <summary>
        /// 良率
        /// </summary>
        /// <returns></returns>
        public double GetYield()
        {
            if (AllCount == 0)
            {
                return 100;
            }
            else
            {
                return Math.Round((((float)OKCount / AllCount) * 100), 2);
            }
        }

        /// <summary>
        /// 抛料率
        /// </summary>
        /// <returns></returns>
        public double GetThrowRate(int count)
        {
            if (AllCount == 0)
            {
                return 0;
            }
            else
            {
                return Math.Round((((float)count / AllCount) * 100), 2);
            }
        }


        public void SetRealCT(double value)
        {
            RealCT = value;
            //if (RealCT > 0)
            //{
            //    UPH = (int)(3600 / RealCT);
            //}
        }

        private DateTime uphStart = DateTime.MinValue;

        /// <summary>
        /// 配置UPH
        /// </summary>
        /// <param name="uph"></param>
        public void SetUPH(int uph = 0)
        {
            #region 保留
            //// 第一次初始化，从数据库中加载数据
            //if (uph > 0)
            //{
            //    uphStart = DateTime.Now;
            //    UPH = uph;
            //    return;
            //}

            //// 第一次开始统计
            //if (uphStart == DateTime.MinValue)
            //{
            //    uphStart = DateTime.Now;
            //    UPH = 1;
            //}
            //else
            //{
            //    // 判断当前时间和上一次计时时间
            //    if (Math.Abs((DateTime.Now.Hour - uphStart.Hour)) > 0)
            //    {
            //        uphStart = DateTime.Now;
            //        UPH = 1;
            //    }
            //    else
            //    {
            //        UPH++;
            //    }
            //}

            #endregion

            //目前采用3600/ct_*#
            if (RealCT > 0)
            {
                UPH = (int)(3600 / RealCT);
            }
        }
    }
}