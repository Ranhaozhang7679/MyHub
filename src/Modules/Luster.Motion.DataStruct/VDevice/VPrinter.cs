#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VPrinter
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.DataModels
* 文 件 名:       VPrinter.cs
* 创建时间:       2022/6/17 11:27:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a2e7d155-5e80-4a7c-b5d6-d94e9628db78
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/17 11:27:00
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    public class VPrinter : VirtualDeviceBase
    {
        /// <summary>
        /// 真实打印机名称
        /// </summary>
        public string PrinterName { get; set; }

        /// <summary>
        /// 标签宽度
        /// </summary>
        public int PageWidth { get; set; }

        /// <summary>
        /// 标签长度
        /// </summary>
        public int PageHeight { get; set; }

        [Ignore]
        /// <summary>
        /// 当前设备
        /// </summary>
        public IPrinter printer { get; set; }

        public override string[] GetRefProps()
        {
            return new string[] {

            };
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="content"></param>
        /// <param name="timeOut"></param>
        public void Print(string content, int timeOut)
        {
            ProcessAction(
               () =>
               {
                   // 设置标签宽度和高度
                   printer.SetPageSize(PageWidth, PageHeight);

                   // 打印
                   printer.Print(content);
               });
        }

        /// <summary>
        /// 设置设备
        /// </summary>
        /// <param name="hardDevice"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool SetDevice(IDevice hardDevice, out string errMsg)
        {
            base.SetDevice(hardDevice, out errMsg);
            printer = hardDevice as IPrinter;

            // 连接
            ProcessAction(() =>
            {
                printer.Open();
            });

            return true;
        }
    }
}