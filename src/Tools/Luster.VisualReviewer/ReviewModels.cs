using System.Collections.Generic;

namespace Luster.VisualReviewer
{
    /// <summary>视觉评审发现的单条问题</summary>
    public sealed class ReviewIssue
    {
        /// <summary>严重度: high / medium / low</summary>
        public string Severity;

        /// <summary>问题分类: overlap / spacing / layout / font(像素可见维度;源码级维度由 XamlLinter 覆盖)</summary>
        public string Category;

        /// <summary>问题描述</summary>
        public string Description;

        /// <summary>问题位置(控件名 / 区域)</summary>
        public string Location;
    }

    /// <summary>视觉评审报告(由 VisualReviewClient 产出)</summary>
    public sealed class ReviewReport
    {
        /// <summary>被评审的 View 名</summary>
        public string View;

        /// <summary>截图路径(由调用方落盘,此处仅记录)</summary>
        public string Screenshot;

        /// <summary>评审总结</summary>
        public string Summary;

        /// <summary>评分 0-10;降级时为 -1</summary>
        public int Score;

        /// <summary>设计时数据(mock VM)是否就绪: present / missing。由 PreviewHost sidecar 透传,
        /// Reviewer 从像素无法判断,无 sidecar 时默认 present(向后兼容)。</summary>
        public string DesignData = "present";

        /// <summary>降级标记:网络/key 失败为 true</summary>
        public bool Degraded;

        /// <summary>发现的问题清单</summary>
        public List<ReviewIssue> Issues = new List<ReviewIssue>();
    }

    /// <summary>视觉评审客户端契约(便于测试桩注入)</summary>
    public interface IVisualReviewClient
    {
        /// <summary>评审截图,返回结构化报告;失败降级不抛</summary>
        ReviewReport Review(byte[] png, string viewName);
    }
}
