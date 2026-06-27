using System.IO;
using System.Text;

namespace Luster.VisualReviewer
{
    /// <summary>向工作区 index.md 追加迭代历史(View 级 + 根级)</summary>
    public static class WorkspaceIndexer
    {
        /// <summary>根索引 workspace/wpf-preview/index.md 追加一行</summary>
        public static void AppendRoot(string workspaceRoot, string viewName, ReviewReport report)
        {
            string path = Path.Combine(workspaceRoot, "index.md");
            var sb = new StringBuilder();
            // 文件不存在或为空时先写表头(注意:分隔行与首行数据之间不得留空行,否则 markdown 表格不渲染)
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                sb.AppendLine("# WPF 预览工作区");
                sb.AppendLine();
                sb.AppendLine("| View | 时间戳 | 评分 | 状态 |");
                sb.AppendLine("|---|---|---|---|");
            }
            string ts = Path.GetFileNameWithoutExtension(report.Screenshot);
            sb.AppendLine($"| {viewName} | {ts} | {report.Score} | {(report.Degraded ? "降级" : "完成")} |");
            File.AppendAllText(path, sb.ToString());
        }

        /// <summary>View 级索引 workspace/wpf-preview/&lt;View&gt;/index.md 追加一行</summary>
        public static void AppendView(string workspaceRoot, string viewName, ReviewReport report)
        {
            string dir = Path.Combine(workspaceRoot, viewName);
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "index.md");
            var sb = new StringBuilder();
            // 文件不存在或为空时先写表头(注意:分隔行与首行数据之间不得留空行,否则 markdown 表格不渲染)
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                sb.AppendLine($"# {viewName} 迭代历史");
                sb.AppendLine();
                sb.AppendLine("| 时间戳 | 评分 | 主要问题 | 截图 |");
                sb.AppendLine("|---|---|---|---|");
            }
            string ts = Path.GetFileNameWithoutExtension(report.Screenshot);
            string topIssue = report.Issues.Count > 0 ? report.Issues[0].Description : "-";
            string relShot = "runs/" + Path.GetFileName(report.Screenshot);
            sb.AppendLine($"| {ts} | {report.Score} | {topIssue} | {relShot} |");
            File.AppendAllText(path, sb.ToString());
        }
    }
}
