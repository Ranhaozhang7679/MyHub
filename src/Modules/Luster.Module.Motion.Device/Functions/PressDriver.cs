#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WriteOutput
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       WriteOutput.cs
* 创建时间:       2022/5/30 9:36:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b0171b65-cb6e-4fd0-97fd-62a80d2a0632
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 9:36:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using TaiKeCommon;


namespace Luster.Module.Motion.Device.Functions
{
    public class PressDriver : MotionFunction
    {
        //[NotEmpty]
        //[Parameter("通信设备", 1, CN = "通信设备", EditorType = typeof(VCommuncation))]
        //public VDevice CommDevice { get; set; }

        //[Range(1, 8)]
        //[Parameter("站号", 2, CN = "压力传感器站号", DefaultV = 1)]
        //public int StationNum { get; set; }


        [Parameter("产品条码", 1, CN = "产品码", CanRef = ParamRef.Ref)]
        public string SNCode { get; set; }


        [NotEmpty]
        [Parameter("曲线显示控件名称", 2, CN = "曲线名称", DefaultV = "压力1")]
        public string Name { get; set; }


        [NotEmpty]
        [Parameter("保压头", 3, CN = "保压头", DefaultV = "ANT2")]
        public string CompressNo { get; set; }
         


        /// <summary>
        /// 压力值数组
        /// </summary>
        private List<double> lstPressVal = new List<double>();

        /// <summary>
        /// 时间
        /// </summary>
        private List<double> lstTimeVal = new List<double>();  

      



        bool connectedNet = false;
        bool connectedSerial = false;

        private TimePress timepress; 

        bool updated = false;

        /// <summary>
        /// 自动注释
        /// </summary>
        public override string[] NoteParams => new string[] { "设置", nameof(Device) };

        public PressDriver()
        {
            this.Tips = "压力曲线显示";
            this.Icon = "\xe652";
            timepress = new TimePress(); 
        }


        bool uiRegistered = false;

        public override bool DoExcute(out string errMsg)
        {
            try
            {
                DateTime timestart = DateTime.Now;
                errMsg = "";
                if (!uiRegistered && MyOwner != null)
                {
                    uiRegistered = true;
                    MyOwner.PressRegister(timepress, Name);
                }
                string dicroot = $"D:\\PressData\\{DateTime.Now:yyyyMMdd}\\{Name}\\{CompressNo}\\Data\\";
                if (!Directory.Exists(dicroot))
                {
                    Directory.CreateDirectory(dicroot);
                }

                string file = $"D:\\PressData\\{DateTime.Now:yyyyMMdd}\\{Name}\\{CompressNo}\\Data\\{SNCode}.csv"; 
                List<double> time = new List<double>();
                List<double> press = new List<double>();

                double length = 0;
                double interval = 0;
                List<TimePressModel> _pressModel = CSVTool.OpenCSV<TimePressModel>(file);
                if (_pressModel != null)
                {
                   
                    string aaa = _pressModel.First().Time.ToString();
                    DateTime start =Convert.ToDateTime(aaa);
                    DateTime end = Convert.ToDateTime(_pressModel.Last().Time);
                    length = _pressModel.Count();
                    double timespan = (end - start).TotalMilliseconds;
                    interval = timespan / (length - 1);
                }

                for(int i = 0;i<length;i++)
                {
                    time.Add(i * interval);
                }


                foreach (var item in _pressModel)
                {
                    //time.Add(item.Time);
                    press.Add(item.Press);
                }
                string Curveroot = $"D:\\PressData\\{DateTime.Now:yyyyMMdd}\\{Name}\\{CompressNo}\\";
                timepress.UpdatePress(Curveroot, SNCode, time.ToArray(), press.ToArray());
            }
            catch (Exception ex)
            {
            }



            errMsg = string.Empty;
            return true;
        }
    }
}