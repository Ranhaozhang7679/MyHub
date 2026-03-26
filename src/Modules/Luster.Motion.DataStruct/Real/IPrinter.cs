#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IPrinter
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Real
* 文 件 名:       IPrinter.cs
* 创建时间:       2022/6/17 11:20:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7aeac1f6-d965-478d-9d07-c5e8ae328e3d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/17 11:20:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 调码打印
    /// </summary>
    public interface IPrinter:IDevice
    {
        /// <summary>
        /// 打开打印机
        /// </summary>
        void Open();

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="content"></param>
        void Print(string content);

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void SetPageSize(int width,int height);

        /// <summary>
        /// 断开打印机
        /// </summary>
        void Close();
    }
}