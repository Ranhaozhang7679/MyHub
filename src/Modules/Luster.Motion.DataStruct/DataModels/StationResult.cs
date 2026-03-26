#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VResult
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       VResult.cs
* 创建时间:       2022/6/29 13:33:55
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d6eb6081-0018-4043-ad2b-97139d67471c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/29 13:33:55
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
    /// 工站结果
    /// </summary>
    public class StationResult : ICloneable
    {
        /// <summary>
        /// 工站默认运行都是成功的
        /// </summary>
        public bool Result { get; set; } = true;

        /// <summary>
        /// 失败错误消息
        /// </summary>
        public string ErrMsg { get; set; }

        /// <summary>
        /// 当前站名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 治具条码
        /// </summary>
        public string JigCode { get; set; } = "";

        /// <summary>
        /// 产品编码
        /// </summary>
        public string ProCode { get; set; } = "";

        /// <summary>
        /// 真实入料时间
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// SN对应的图像路径
        /// </summary>
        public string ImagePath { get; set; } = "";

        /// <summary>
        /// 是否发生抛料
        /// </summary>
        public bool IsToss { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string NgCode { get; set; }

        public string WIP { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public Dictionary<string, object> Datas { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public List<(string,string)> TossDatas { get; set; }


        /// <summary>
        /// 是否发生抛料
        /// </summary>
        public bool IsPreviousStationUndo { get; set; }
        public StationResult()
        {
            Datas = new Dictionary<string, object>();
        }

        /// <summary>
        /// 对象克隆
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new StationResult()
            {
                Result = Result,
                ErrMsg = ErrMsg,
                StationName = StationName,
                JigCode = JigCode,
                EnterTime = EnterTime,
                ProCode = ProCode,
                Datas = new Dictionary<string, object>(Datas),
                ImagePath = ImagePath,
                NgCode = NgCode,
                IsPreviousStationUndo= IsPreviousStationUndo,
                WIP = WIP,
            };
        }
    }
}