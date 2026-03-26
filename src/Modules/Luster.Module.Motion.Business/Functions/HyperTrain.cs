#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HyperTrain
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Business.Functions
* 文 件 名:       HyperTrain.cs
* 创建时间:       2022/9/27 21:28:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      28ede871-44b3-4024-8317-8aa37333921b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/27 21:28:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.Tools;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public enum HyperTrainType
    {
        [Description("注册")]
        Register,

        [Description("心跳")]
        Heartbeat,

        [Description("稼动")]
        Impact,

        [Description("生产报文")]
        Product,
    }

    public class HyperTrain : MotionFunction
    {
        [NotEmpty]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public string Url { get; set; }

        [Parameter("Http的请求地址", 0, CN = "API方法", DefaultV = HyperTrainType.Register)]
        public HyperTrainType TrainType { get; set; }

        [Parameter("设备铭牌信息资产 SN", 0, CN = "设备SN")]
        public string MachineSn { get; set; }

        [Parameter("示例：FXZZ_G01-4FT-02_1_AE - 34", 0, CN = "工站ID")]
        public string StationId { get; set; }


        #region 注册

        [DependOn("TrainType", HyperTrainType.Register)]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public string StationName { get; set; }
       
        [DependOn("TrainType", HyperTrainType.Register)]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public string vendorName { get; set; }
        #endregion

        #region 心跳报文 1 次/1 秒
        [DependOn("TrainType", HyperTrainType.Heartbeat)]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public string WorkMode { get; set; }

        [DependOn("TrainType", HyperTrainType.Heartbeat)]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public string NgCount { get; set; }

        [DependOn("TrainType", HyperTrainType.Heartbeat)]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public int BufferCount { get; set; }
        #endregion

        #region 稼动状态
        [DependOn("TrainType", HyperTrainType.Impact)]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public string ToStatus { get; set; }

        [DependOn("TrainType", HyperTrainType.Impact)]
        [Parameter("当前状态代码", 0, CN = "URL地址")]
        public string Status { get; set; }

        [DependOn("TrainType", HyperTrainType.Impact)]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public string ErrorCode { get; set; }

        [DependOn("TrainType", HyperTrainType.Impact)]
        [Parameter("Http的请求地址", 0, CN = "URL地址")]
        public int ErrorMessage { get; set; }
        #endregion


        #region 设备生成报文 上传频率 :1次/5秒
        [DependOn("TrainType", HyperTrainType.Product)]
        [Parameter("物料SN", 0, CN = "物料SN")]
        public string MaterialSn { get; set; }

        [DependOn("TrainType", HyperTrainType.Product)]
        [Parameter("载具SN", 0, CN = "载具SN")]
        public string CarrierSn { get; set; }

        [DependOn("TrainType", HyperTrainType.Product)]
        [Parameter("加工CT", 0, CN = "加工CT")]
        public int CycleTime { get; set; }

        [DependOn("TrainType", HyperTrainType.Product)]
        [Parameter("加工结果", 0, CN = "加工结果")]
        public int Result { get; set; }
        #endregion

        /// <summary>
        /// web 通信组件
        /// </summary>
        private WebTool webTool;

        public HyperTrain()
        {
            this.Tips = "驾驶仓";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            if (webTool == null)
            {
                webTool = new WebTool() { Encoding = Encoding.UTF8 };
            }

            return base.DoExcute(out errMsg);
        }
    }
}