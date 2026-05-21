#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SystemConfig
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       SystemConfig.cs
* 创建时间:       2022/7/12 10:00:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f7a39ee0-76dd-4970-973f-357d4c803b15
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/12 10:00:57
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    public class FileConfig : IXMLParser
    {
        #region 软件配置

        /// <summary>
        /// 图像保存路径
        /// </summary>
        public string PicSavePath { get; set; }

        /// <summary>
        /// 数据库保存路径
        /// </summary>
        public string DBSavePath { get; set; }

        /// <summary>
        /// 日志存放路径
        /// </summary>
        public string LogsSavePath { get; set; }


        /// <summary>
        /// 数据库转储路径
        /// </summary>
        public string DumpDBPath { get; set; }

        /// <summary>
        /// 图像保存天数
        /// </summary>
        public int PicSaveDays { get; set; }

        /// <summary>
        /// 数据库保存天数
        /// </summary>
        public int DBSaveDays { get; set; } = 90;

        /// <summary>
        /// 日志备份天数
        /// </summary>
        public int LogBackUpDays { get; set; }

        /// <summary>
        /// 配方备份天数
        /// </summary>
        public int RecipeBackUpDays { get; set; }

        /// <summary>
        /// 数据库备份天数
        /// </summary>
        public int DBBackUpDays { get; set; } = 90;


        #region Hive 相关
        /// <summary>
        /// 最新周保养时间
        /// </summary>
        public DateTime LastWeekMaintenanceDate { get; set; }


        /// <summary>
        /// 最新月保养时间
        /// </summary>
        public DateTime LastMonthMaintenanceDate { get; set; }


        /// <summary>
        /// 宕机原因
        /// </summary>
        public string DownTimeCauses { get; set; }

        /// <summary>
        /// 消耗物品
        /// </summary>
        public string SparePartsUsed { get; set; }

        /// <summary>
        /// 检查步骤
        /// </summary>
        public string ActionLists { get; set; } = "";

        /// <summary>
        /// 用到工具
        /// </summary>
        public string ToolLists { get; set; } = "";

        /// <summary>
        /// 调整参数
        /// </summary>
        public string SettingLists { get; set; } = "";

        /// <summary>
        /// Hive Help Request Time
        /// </summary>
        public DateTime HiveHelpRequestTime { get; set; }

        /// <summary>
        /// Hive Repair Start Time
        /// </summary>
        public DateTime HiveRepairStartTime { get; set; }


        /// <summary>
        /// Hive Repair End Time
        /// </summary>
        public DateTime HiveRepairEndTime { get; set; }

        public bool IsCardEnd { get; set; }


        /// <summary>
        /// Hive 继续维修可视
        /// </summary>
        public bool IsHiveContinueMaintenanceVisible { get; set; }

        #endregion


        #endregion

        #region 导入导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            var root = this.ToXml("FileConfig");
            return root;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }
        #endregion

        /// <summary>
        /// 保存
        /// </summary>
        public void SaveFileConfig(string fileConfig = "")
        {
            if (!Directory.Exists(Path.GetDirectoryName(fileConfig)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileConfig));//创建路径
            }
            XElement xe = ExportXml();
            xe.Save(fileConfig);
        }

        /// <summary>
        /// 加载
        /// </summary>
        public void LoadFileConfig(string fileConfig = "")
        {
            if (fileConfig != "")
            {
                if (!File.Exists(fileConfig))
                {
                    SaveFileConfig(fileConfig);
                }

                try
                {
                    XElement xSysConfig = XElement.Load(fileConfig);
                    ParserXml(xSysConfig);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}