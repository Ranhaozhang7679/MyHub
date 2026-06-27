using System.IO;

namespace Luster.VisualReviewer
{
    /// <summary>读取设计契约 md 全文,供视觉模型当评阅标准</summary>
    public static class ContractReader
    {
        public static string Read(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return "";
            return File.ReadAllText(path);
        }
    }
}
