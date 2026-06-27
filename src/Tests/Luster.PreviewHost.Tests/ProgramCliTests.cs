using System.Diagnostics;
using System.IO;
using Xunit;

namespace Luster.PreviewHost.Tests
{
    /// <summary>进程级 CLI 集成测试:跑实际 exe 验退出码与产物</summary>
    public class ProgramCliTests
    {
        // exe 与测试 DLL 同输出到 artifacts/bin/net472/(Directory.Build.props 扁平 OutputPath),
        // 故直接取 AppContext.BaseDirectory 下的 exe;brief 原 4×".." 路径在该布局下解析到仓库根之上,不存在。
        private static string ExePath =>
            Path.Combine(System.AppContext.BaseDirectory, "Luster.PreviewHost.exe");

        [Fact]
        public void Cli_RendersSampleView_ToPng()
        {
            var outPng = Path.GetTempFileName() + ".png";
            try
            {
                var psi = new ProcessStartInfo(ExePath)
                {
                    Arguments = $"--view Luster.PreviewHost.Fixtures.SampleView,Luster.PreviewHost " +
                                $"--designvm Luster.PreviewHost.Fixtures.SampleDesignVm,Luster.PreviewHost " +
                                $"--out {outPng} --width 400 --height 300",
                    UseShellExecute = false, RedirectStandardError = true, CreateNoWindow = true
                };
                var p = Process.Start(psi);
                string err = p.StandardError.ReadToEnd();
                p.WaitForExit(15000);
                Assert.Equal(0, p.ExitCode);
                Assert.True(File.Exists(outPng));
                Assert.True(new FileInfo(outPng).Length > 0);
            }
            finally { if (File.Exists(outPng)) File.Delete(outPng); }
        }
    }
}
