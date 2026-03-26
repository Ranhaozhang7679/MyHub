#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StationTime
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       StationTime.cs
* 创建时间:       2022/8/24 10:07:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      90964ef5-3247-407f-bf1a-ac428270028e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/24 10:07:29
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 工站时间
    /// </summary>
    public struct StationTime
    {
        /// <summary>
        /// 工站名称
        /// </summary>
        public string Station { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 二维码
        /// </summary>
        public string SN { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 基元耗时 ms
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// 标准CT
        /// </summary>
        public float CT { get; set; }

        /// <summary>
        /// 扩展参数
        /// </summary>
        public Dictionary<string, object> ExtParams { get; set; }

        /// <summary>
        /// 结构体构造函数
        /// </summary>
        /// <param name="station"></param>
        /// <param name="module"></param>
        /// <param name="time"></param>
        public StationTime(string station, string module, float time, float ct, DateTime sTime, string sn = "", Dictionary<string, object> extParams = null)
        {
            Station = station;
            Module = module;
            Time = time;
            CT = ct;
            if (string.IsNullOrEmpty(sn))
            {
                sn = "None";
            }
            SN = sn;
            StartTime = sTime;
            EndTime = DateTime.Now;

            ExtParams = new Dictionary<string, object>()
                {
                    { "X_Spd", 0 },{ "X_Spd_Target", 0 },{ "X_RotateSpd", 0 }, { "X_Acc_Target", 0 },{ "X_Path", 0 }, { "X_Distance", 0 },
                    { "Y_Spd", 0 },{ "Y_Spd_Target", 0 }, { "Y_RotateSpd", 0 },{ "Y_Acc_Target", 0 },{ "Y_Path", 0 }, { "Y_Distance", 0 },
                    { "Z_Spd", 0 },{ "Z_Spd_Target", 0 }, { "Z_RotateSpd", 0 },{ "Z_Acc_Target", 0 }, { "Z_Path", 0 }, { "Z_Distance", 0 },
                    { "U_Spd", 0 },{ "U_Spd_Target", 0 },{ "U_RotateSpd", 0 }, { "U_Acc_Target", 0 }, { "U_Path", 0 }, { "U_Distance", 0 },
                    { "R_Spd", 0 },{ "R_Spd_Target", 0 },{ "R_RotateSpd", 0 }, { "R_Acc_Target", 0 }, { "R_Path", 0 }, { "R_Distance", 0 },
                    { "Delayed", 0}

                };
            if (extParams != null)
            {
                foreach (var item in extParams)
                {
                    try
                    {
                        ExtParams[item.Key] = item.Value;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}