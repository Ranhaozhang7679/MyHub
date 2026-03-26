#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LPointCloud
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LPointCloud.cs
* 创建时间:       2022/3/16 18:31:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      5ba28659-e251-4bba-a73a-53a1efc564cf
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/16 18:31:40
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.DataModels
{
    /// <summary>
    /// 点云结构
    /// </summary>
    public struct LineLaser
    {
        /// <summary>
        /// 行数量
        /// </summary>
        public int Row
        {
            get;
            set;
        }

        /// <summary>
        /// 行间距
        /// </summary>
        public double RowDis
        {
            get;
            set;
        }

        /// <summary>
        /// 列数量
        /// </summary>
        public int Column
        {
            get;
            set;
        }

        /// <summary>
        /// 列间距
        /// </summary>
        public double ColDis
        {
            get;
            set;
        }

        /// <summary>
        /// Z向高度值
        /// </summary>
        public IntPtr ZPointer
        {
            get;
            set;
        }

        /// <summary>
        /// Z向数据指针类型
        /// </summary>
        public DataType ZDataType
        {
            get;
            set;
        } 

        /// <summary>
        /// 分辨率
        /// </summary>
        public double Resolution
        {
            get;
            set;
        }

        /// <summary>
        /// Z值最小值 单位mm
        /// </summary>
        public double ValidMin { get; set; }

        /// <summary>
        /// Z值最大值 单位mm
        /// </summary>
        public double ValidMax { get; set; }

        /// <summary>
        /// X方向偏移
        /// </summary>
        public double OffsetX { get; set; }

        /// <summary>
        /// Y方向偏移
        /// </summary>
        public double OffsetY { get; set; }

        /// <summary>
        /// Z方向偏移
        /// </summary>
        public double OffsetZ { get; set; }

        /// <summary>
        /// 是否行列反序
        /// </summary>
        public bool Invert { get; set; }
    }
}