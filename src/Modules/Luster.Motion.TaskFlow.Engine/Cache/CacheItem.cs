#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PlcConfig
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       PlcConfig.cs
* 创建时间:       2022/9/1 14:37:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      22b038f0-c587-4bc6-a16b-ec454a8e21d1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/1 14:37:58
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.Motion.TaskFlow.Engine
{
    /// <summary>
    /// 缓存数据
    /// </summary>
    public class CacheItem : List<LColumn>, ICloneable
    {
        /// <summary>
        /// 具有唯一标识，缓存产品信息
        /// </summary>
        public string DataID { get; set; }

        /// <summary>
        /// 创建事件
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 入料时间
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// 标记为上传
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dataID"></param>
        public CacheItem(string dataID)
        {
            DataID = dataID;
            CreateTime = DateTime.Now;
            //EnterTime = DateTime.Now;//初始化时赋值EnterTime
        }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Clone()
        {
            var item = new CacheItem(DataID);
            item.CreateTime = CreateTime;
            item.EnterTime = EnterTime;
            foreach (var cItem in this)
            {
                item.Add(cItem);
            }

            return item;
        }
    }
}