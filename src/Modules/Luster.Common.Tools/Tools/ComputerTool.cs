using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools.Tools
{
    /// <summary>
    /// 电脑工具
    /// </summary>
    public class ComputerTool
    {
        private string _processName = "";

        /// <summary>
        /// CPU 对象
        /// </summary>
        private PerformanceCounter Cpu { get; set; }

        private ComputerInfo ComputerInfo { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="processName"></param>
        public ComputerTool(string processName, string disk = "")
        {
            _processName = processName;

            Cpu = new PerformanceCounter("Processor", "% Processor Time", processName);
            if (!string.IsNullOrEmpty(processName))
            {
                Cpu.InstanceName = processName;
            }

            ComputerInfo = new ComputerInfo();
        }

        /// <summary>
        /// 获取CPU 使用率
        /// </summary>
        /// <returns></returns>
        public double GetCpu()
        {
            return Math.Round(Cpu.NextValue(), 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 本机内存使用率
        /// </summary>
        /// <returns></returns>
        public double GetMemory()
        {
            var useMem = this.ComputerInfo.TotalPhysicalMemory - this.ComputerInfo.AvailablePhysicalMemory * 1.0;
            return Math.Round((useMem * 100 / this.ComputerInfo.TotalPhysicalMemory), 2, MidpointRounding.AwayFromZero);
        }
    }
}
