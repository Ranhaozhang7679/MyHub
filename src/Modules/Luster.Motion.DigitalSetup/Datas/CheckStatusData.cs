using System;
using System.Collections.Generic;

namespace Luster.Motion.DigitalSetup.Datas
{
    /// <summary>
    /// 点检状态枚举
    /// </summary>
    public enum CheckStatus
    {
        /// <summary>未点检</summary>
        NotChecked = 0,
        /// <summary>点检OK</summary>
        CheckedOK = 1,
        /// <summary>点检Fail</summary>
        CheckedFail = 2
    }

    /// <summary>
    /// 单个页面点检记录
    /// </summary>
    public class PageCheckRecord
    {
        /// <summary>页面唯一标识 (格式: "ParentRegion_SubPageName")</summary>
        public string PageKey { get; set; }

        /// <summary>父页面Region</summary>
        public string ParentRegion { get; set; }

        /// <summary>子页面名称</summary>
        public string SubPageName { get; set; }

        /// <summary>点检状态</summary>
        public CheckStatus Status { get; set; }

        /// <summary>点检时间</summary>
        public DateTime? CheckTime { get; set; }

        /// <summary>点检人员</summary>
        public string Operator { get; set; }

        /// <summary>备注信息</summary>
        public string Remark { get; set; }
    }

    /// <summary>
    /// 点检状态持久化数据
    /// </summary>
    public class CheckStatusPersistenceData
    {
        /// <summary>所有页面的点检记录</summary>
        public Dictionary<string, PageCheckRecord> CheckRecords { get; set; } = new Dictionary<string, PageCheckRecord>();

        /// <summary>最后保存时间</summary>
        public DateTime LastSavedTime { get; set; }

        /// <summary>软件启动时的时间戳（用于判断是否需要重置）</summary>
        public DateTime? SessionStartTime { get; set; }
    }
}
