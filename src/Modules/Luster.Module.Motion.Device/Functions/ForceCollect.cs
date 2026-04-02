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
using Luster.TaskFlow.Motion.Logic;
using System.Data.Common;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using System.Drawing.Imaging;
using System.Runtime.Remoting.Messaging;
using TaiKeCommon;
using static System.Net.Mime.MediaTypeNames;

namespace Luster.Module.Motion.Device.Functions
{
    public class ForceCollect : MotionFunction
    {

        [NotEmpty]
        [Parameter("模拟量", 1, CN = "模拟量X", EditorType = typeof(VIO))]
        public VDevice ADIX { get; set; }

        [NotEmpty]
        [Parameter("模拟量", 2, CN = "模拟量Y", EditorType = typeof(VIO))]
        public VDevice ADIY { get; set; }

        [NotEmpty]
        [Parameter("模拟量", 3, CN = "模拟量Z", EditorType = typeof(VIO))]
        public VDevice ADIZ { get; set; }

        [Parameter("比例", 4, CN = "比例X", DefaultV = 1.0)]
        public double RatioX { get; set; }

        [Parameter("比例", 5, CN = "比例Y", DefaultV = 1.0)]
        public double RatioY { get; set; }


        [Parameter("比例", 6, CN = "比例Z", DefaultV = 1.0)]
        public double RatioZ { get; set; }


        [Parameter("终止条件", 7, CN = "终止条件", EditorType = typeof(IGlobal))]
        public string EndCondition { get; set; }

        [Parameter("产品条码", 8, CN = "产品码", CanRef = ParamRef.Ref)]
        public string SNCode { get; set; }

        /// <summary>
        /// 压力值数组
        /// </summary>
        private List<double> lstPressVal = new List<double>();

        /// <summary>
        /// 时间
        /// </summary>
        private List<double> lstTimeVal = new List<double>();


        private ElectricScrewDrivers electricScrewDriver;

        /// <summary>
        /// 全局变量
        /// </summary>
        private IMotionModule gModule = null;

        /// <summary>
        /// 参数
        /// </summary>
        private ParameterAttribute gParameter = null;

        /// <summary>
        /// 自动注释
        /// </summary>
        public override string[] NoteParams => new string[] { "设置", nameof(Device) };

        public ForceCollect()
        {
            this.Tips = "压力采集";
            this.Icon = "\xe652";
            electricScrewDriver = new ElectricScrewDrivers();
        }


        bool uiRegistered = false;

        public override bool DoExcute(out string errMsg)
        {
            //获取全局模块
            if (gModule == null)
            {
                gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
            }

            GetVDevice<VIO>(ADIX, out var adix);
            GetVDevice<VIO>(ADIY, out var adiy);
            GetVDevice<VIO>(ADIZ, out var adiz);
            errMsg = "";
            if (!uiRegistered && MyOwner != null)
            {
                uiRegistered = true;
                MyOwner.ToeinForceRegister(electricScrewDriver, Name);
            }
            List<double> time = new List<double>();
            List<double> xforce = new List<double>();
            List<double> yforce = new List<double>();
            List<double> zforce = new List<double>();

            int i = 0;
            while (true)
            {

                //获取全局变量

                if (gParameter == null)
                {
                    if (!gModule.Parameters.ContainsKey(EndCondition))
                    {
                        MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"全局变量:{EndCondition}不存在!");
                    }
                    gParameter = gModule.Parameters[EndCondition];
                }


                //实时拿到全局对象的值
                object pVal = gParameter?.Value;
                if (pVal == null)
                {
                    pVal = false;
                }
                i = i + 1;
                double currentXforce = Math.Abs(Math.Round(adix.GetAnglogIn() / RatioX, 3));
                if (currentXforce <= 0) currentXforce = 0;
                xforce.Add(currentXforce);
                double currentYforce = Math.Abs(Math.Round(adiy.GetAnglogIn() / RatioX, 3));
                if (currentYforce <= 0) currentYforce = 0;
                yforce.Add(currentYforce);
                double currentZforce = Math.Abs(Math.Round(adiz.GetAnglogIn() / RatioX, 3));
                if (currentZforce <= 0) currentZforce = 0;
                zforce.Add(currentZforce);
                time.Add(i * 50);


                #region  写入表格
                #region X方向
                try
                {
                    string date = DateTime.Now.ToString("yyyyMMdd");
                    string directoryName_x = Path.GetDirectoryName($"D:\\TaiKeScrewDatas\\CowlingForceData\\XForce\\{date}\\");
                    if (!Directory.Exists(directoryName_x))
                    {
                        Directory.CreateDirectory(directoryName_x);
                    }

                    string filepath = $"D:\\TaiKeScrewDatas\\CowlingForceData\\XForce\\{date}\\{SNCode}.csv";

                    using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        var RowDatas = string.Empty;
                        string text = sr.ReadToEnd();
                        string NewHeader = "Time,Force\r\n";
                        ///表头为空
                        if (!text.Contains(NewHeader))
                        {
                            fs.Write(Encoding.UTF8.GetBytes(NewHeader), 0, Encoding.UTF8.GetBytes(NewHeader).Length);
                        }
                        RowDatas = $"{50 * i},{currentXforce}\r\n";
                        fs.Write(Encoding.UTF8.GetBytes(RowDatas), 0, Encoding.UTF8.GetBytes(RowDatas).Length);
                    }
                }
                catch (IOException ex)
                {
                    // 文件被占用，跳过本次写入
                    LogTool.Warn($"文件被占用，跳过本次X方向压力采集写入: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // 其他异常也跳过
                    LogTool.Warn($"其他异常，跳过本次X方向压力采集写入: {ex.Message}");
                }
                #endregion X方向

                #region Y方向
                try
                {
                    string date = DateTime.Now.ToString("yyyyMMdd");
                    string directoryName_y = Path.GetDirectoryName($"D:\\TaiKeScrewDatas\\CowlingForceData\\YForce\\{date}\\");
                    if (!Directory.Exists(directoryName_y))
                    {
                        Directory.CreateDirectory(directoryName_y);
                    }
                    string filepath_y = $"D:\\TaiKeScrewDatas\\CowlingForceData\\YForce\\{date}\\{SNCode}.csv";
                    using (FileStream fs_y = new FileStream(filepath_y, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    using (StreamReader sr_y = new StreamReader(fs_y))
                    {
                        string text = sr_y.ReadToEnd();
                        var RowDatas = string.Empty;
                        string NewHeader = "Time,Force\r\n";
                        ///表头为空
                        if (!text.Contains(NewHeader))
                        {
                            fs_y.Write(Encoding.UTF8.GetBytes(NewHeader), 0, Encoding.UTF8.GetBytes(NewHeader).Length);
                        }
                        RowDatas = $"{50 * i},{currentYforce}\r\n";
                        fs_y.Write(Encoding.UTF8.GetBytes(RowDatas), 0, Encoding.UTF8.GetBytes(RowDatas).Length);
                    }
                }
                catch (IOException ex)
                {
                    // 文件被占用，跳过本次写入
                    LogTool.Warn($"文件被占用，跳过本次Y方向压力采集写入: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // 其他异常也跳过
                    LogTool.Warn($"其他异常，跳过本次Y方向压力采集写入: {ex.Message}");
                }

                #endregion

                #region Z方向
                try
                {
                    string date = DateTime.Now.ToString("yyyyMMdd");
                    string directoryName_z = Path.GetDirectoryName($"D:\\TaiKeScrewDatas\\CowlingForceData\\ZForce\\{date}\\");
                    if (!Directory.Exists(directoryName_z))
                    {
                        Directory.CreateDirectory(directoryName_z);
                    }
                    string filepath_z = $"D:\\TaiKeScrewDatas\\CowlingForceData\\ZForce\\{date}\\{SNCode}.csv";
                    using (FileStream fs_z = new FileStream(filepath_z, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    using (StreamReader sr_z = new StreamReader(fs_z))
                    {
                        string text = sr_z.ReadToEnd();
                        var RowDatas = string.Empty;
                        string NewHeader = "Time,Force\r\n";
                        ///表头为空
                        if (!text.Contains(NewHeader))
                        {
                            fs_z.Write(Encoding.UTF8.GetBytes(NewHeader), 0, Encoding.UTF8.GetBytes(NewHeader).Length);
                        }
                        RowDatas = $"{50 * i},{currentZforce}\r\n";
                        fs_z.Write(Encoding.UTF8.GetBytes(RowDatas), 0, Encoding.UTF8.GetBytes(RowDatas).Length);
                    }
                }
                catch (IOException ex)
                {
                    // 文件被占用，跳过本次写入
                    LogTool.Warn($"文件被占用，跳过本次Z方向压力采集写入: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // 其他异常也跳过
                    LogTool.Warn($"其他异常，跳过本次Z方向压力采集写入: {ex.Message}");
                }

                #endregion
                #endregion

                //xforce.add(0.1 * i);
                //yforce.add(0.2 * i);
                //zforce.add(0.3 * i);
                //press.Add(i * 0.1);
                Thread.Sleep(45);
                if (pVal.Equals(true)) break;
            }
            electricScrewDriver.UpdateToeinCurve(xforce.ToArray(), yforce.ToArray(), zforce.ToArray(), time.ToArray(),SNCode);
            errMsg = string.Empty;
            return true;
        }
    }
}