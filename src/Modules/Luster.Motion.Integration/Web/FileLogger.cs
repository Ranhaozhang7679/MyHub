using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.Integration.Web
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public static class FileLogger
    {
        private static readonly object _lock = new object();

        public static void Log(string message, string filePath = @"Logs\app")
        {
            Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        // 自动处理路径格式
                        var fullPath = Path.ChangeExtension(filePath, ".txt");
                        var directory = Path.GetDirectoryName(fullPath);

                        // 创建目录（如果不存在）
                        Directory.CreateDirectory(directory!);

                        // 添加时间戳
                        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}";

                        // 追加日志内容
                        File.AppendAllText(fullPath, logEntry);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"日志写入失败: {ex.Message}");
                }
            });
        }
    }

    // 使用示例：
    // SimpleLogger.Log("系统启动", @"C:\MyApp\Logs\20230820"); // 自定义路径
    // SimpleLogger.Log("用户操作");  // 使用默认路径（当前目录下的Logs/app.txt）
}
