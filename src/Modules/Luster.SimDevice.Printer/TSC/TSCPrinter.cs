#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TSCPrinter
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Printer.TSC
* 文 件 名:       TSCPrinter.cs
* 创建时间:       2022/6/17 11:26:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      665ef6fd-2922-4b04-a079-59ad9ff61b4a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/17 11:26:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Printer.TSC
{
    public class TSCPrinter : PrinterBase, IPrinter
    {
        public override string Brand => "TSC";

        public TSCPrinterAPI _tSCPrinterAPI;

        private bool _isConnected;

        public TSCPrinter()
        {
            _tSCPrinterAPI = new TSCPrinterAPI();
            _isConnected = false;
        }

        /// <summary>
        /// 连接打印机
        /// </summary>
        public override void Open()
        {
            if (_isConnected)
            {
                return;
            }
            SafeNativeMethod(() =>
            {
                if (_tSCPrinterAPI.ConnectPrinter() == 1)
                {
                    _isConnected = true;
                    return true;
                }
                return false;
            }, $"连接打印机失败");
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="content"></param>
        public void Print(string content)
        {
            if (!_isConnected)
            {
                return;
            }
            SafeNativeMethod(() =>
            {
                _tSCPrinterAPI.BarCode("100", "100", "128", "100", "1", "0", "2", "2", content);
                if (_tSCPrinterAPI.PrintLabel("1","1") == 1)
                {
                    _isConnected = true;
                    return true;
                }
                return false;
            }, $"打印机打印标签失败");
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void SetPageSize(int width, int height)
        {
            if (!_isConnected)
            {
                return;
            }
            SafeNativeMethod(() =>
            {
                _tSCPrinterAPI.SetUpConfig(width.ToString(), height.ToString(), "4", "8", "0", "0", "0");
                if (_tSCPrinterAPI.ClearBuffer() == 1)
                {
                    return true;
                }
                return false;
            }, $"打印机设置标签参数失败");
        }

        /// <summary>
        /// 断开打印机
        /// </summary>
        public override void Close()
        {
            if (!_isConnected)
            {
                return;
            }
            SafeNativeMethod(() =>
            {
                if (_tSCPrinterAPI.ClosePrinter() == 1)
                {
                    _isConnected = false;
                    return true;
                }
                return false;
            }, $"断开打印机失败");
        }
    }
}