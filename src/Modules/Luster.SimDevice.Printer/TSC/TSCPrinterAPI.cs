#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TSCPrinterAPI
* 机器名称:       Z05150
* 命名空间:       Luster.SimDevice.Printer.TSC
* 文 件 名:       TSCPrinterAPI.cs
* 创建时间:       2022/6/20 16:09:14
* 作    者:       张宇
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yuzhang5150@lusterinc.com 
* 唯一标识：      0cb4c904-41e7-418b-aea1-fc9d9220cc94
* 登录用户:       zhangyu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/20 16:09:14
* 修 改 人:		  Z05150
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.Printer.TSC
{
    public class TSCPrinterAPI
    {
        const string PrinterDll = @"TSCLIB.dll";
        [DllImport(PrinterDll, EntryPoint = "about")]
        public static extern int about();

        [DllImport(PrinterDll, EntryPoint = "openport")]
        public static extern int openport(string printername);

        [DllImport(PrinterDll, EntryPoint = "barcode")]
        public static extern int barcode(string x, string y, string type,
                    string height, string readable, string rotation,
                    string narrow, string wide, string code);

        [DllImport(PrinterDll, EntryPoint = "clearbuffer")]
        public static extern int clearbuffer();

        [DllImport(PrinterDll, EntryPoint = "closeport")]
        public static extern int closeport();

        [DllImport(PrinterDll, EntryPoint = "downloadpcx")]
        public static extern int downloadpcx(string filename, string image_name);

        [DllImport(PrinterDll, EntryPoint = "formfeed")]
        public static extern int formfeed();

        [DllImport(PrinterDll, EntryPoint = "nobackfeed")]
        public static extern int nobackfeed();

        [DllImport(PrinterDll, EntryPoint = "printerfont")]
        public static extern int printerfont(string x, string y, string fonttype,
                        string rotation, string xmul, string ymul,
                        string text);

        [DllImport(PrinterDll, EntryPoint = "printlabel")]
        public static extern int printlabel(string set, string copy);

        [DllImport(PrinterDll, EntryPoint = "sendcommand")]
        public static extern int sendcommand(string printercommand);

        [DllImport(PrinterDll, EntryPoint = "setup")]
        public static extern int setup(string width, string height,
                  string speed, string density,
                  string sensor, string vertical,
                  string offset);

        [DllImport(PrinterDll, EntryPoint = "windowsfont")]
        public static extern int windowsfont(int x, int y, int fontheight,
                        int rotation, int fontstyle, int fontunderline,
                        string szFaceName, string content);


        /// <summary>
        /// 连接打印机
        /// </summary>
        /// <returns></returns>
        public unsafe int ConnectPrinter()
        {
            return openport("USB");
        }

        /// <summary>
        /// 设置打印参数
        /// </summary>
        /// <param name="Width">标签宽度，单位mm</param>
        /// <param name="Height">标签高度，单位mm</param>
        /// <param name="Speed">打印速度，1.0：每秒1.0列印速度</param>
        /// <param name="Density">設定列印濃度，0~15，數字愈大列印結果愈黑</param>
        /// <param name="Senor">感應器類別，0 表示使用垂直間距感測器(gap sensor)；1 表示使用黑標感測器(black mark sensor)</param>
        /// <param name="Vertical">设定 gap/black mark 垂直间距高度，单位: mm</param>
        /// <param name="Offset">设定 gap/black mark 偏移距离，单位: mm</param>
        /// <returns></returns>
        public unsafe int SetUpConfig(string Width, string Height, string Speed, string Density, string Senor, string Vertical, string Offset)
        {
            return setup(Width, Height, Speed, Density, Senor, Vertical, Offset);
        }

        public unsafe int ClearBuffer()
        {
            return clearbuffer();
        }

        /// <summary>
        /// 创建条码
        /// </summary>
        /// <param name="x">条码X方向起始点</param>
        /// <param name="y">条码Y方向起始点</param>
        /// <param name="type">字符串类别</param>
        /// <param name="height">条码高度</param>
        /// <param name="readable">是否打印条码马文，0不打印，1打印</param>
        /// <param name="rotation">设定条码旋转角度</param>
        /// <param name="narrow">设定条码窄比例因子</param>
        /// <param name="wide">设定条码宽比例因子</param>
        /// <param name="code">条码内容</param>
        /// <returns></returns>
        public unsafe int BarCode(string x, string y, string type, string height, string readable, string rotation, string narrow, string wide, string code)
        {
            return barcode(x, y, type, height, readable, rotation, narrow, wide, code);
        }

        /// <summary>
        /// 打印标签内容
        /// </summary>
        /// <param name="set">标签式数</param>
        /// <param name="copy">标签份数</param>
        /// <returns></returns>
        public unsafe int PrintLabel(string set, string copy)
        {
            return printlabel(set,copy);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        public unsafe int ClosePrinter()
        {
            return closeport();
        }
    }
}