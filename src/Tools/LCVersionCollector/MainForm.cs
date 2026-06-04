using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace LCVersionCollector
{
    public class MainForm : Form
    {
        #region P/Invoke

        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern short M_Open(short card, short param);

        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern short M_Close(short card);

        [DllImport("ecat_motion.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern short M_GetVersion(out byte pVersion, int size, short card);

        #endregion

        // 控件
        private NumericUpDown numCardNo;
        private Button btnRead;
        private TextBox txtVersionInfo;
        private TextBox txtVersionId;
        private Button btnSave;
        private Label lblStatus;
        private Button btnOpenOutputDir;

        // 读取到的版本数据
        private string _dllVersion, _fpgaVersion, _dspVersion, _authorization;
        private bool _versionRead;

        public MainForm()
        {
            Text = "LC 板卡版本收集工具";
            Size = new Size(520, 460);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Microsoft YaHei UI", 9f);

            InitControls();
        }

        private void InitControls()
        {
            int y = 16;
            int margin = 12;

            // 卡号
            var lblCard = new Label { Text = "卡号", Location = new Point(margin, y + 4), AutoSize = true };
            numCardNo = new NumericUpDown
            {
                Location = new Point(60, y),
                Size = new Size(60, 28),
                Minimum = 0,
                Maximum = 7,
                Value = 0
            };
            btnRead = new Button
            {
                Text = "连接并读取",
                Location = new Point(140, y - 1),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0x1E, 0x88, 0xE5),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRead.FlatAppearance.BorderSize = 0;
            btnRead.Click += BtnRead_Click;

            y += 42;

            // 版本信息
            var lblVer = new Label { Text = "版本信息", Location = new Point(margin, y + 4), AutoSize = true };
            y += 24;
            txtVersionInfo = new TextBox
            {
                Location = new Point(margin, y),
                Size = new Size(ClientSize.Width - margin * 2, 140),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9.5f),
                BackColor = Color.FromArgb(0xF5, 0xF5, 0xF5)
            };

            y += 150;

            // 版本ID
            var lblId = new Label { Text = "版本 ID", Location = new Point(margin, y + 4), AutoSize = true };
            txtVersionId = new TextBox
            {
                Location = new Point(80, y),
                Size = new Size(200, 28),
                Enabled = false
            };
            btnSave = new Button
            {
                Text = "保存到配置",
                Location = new Point(300, y - 1),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0x43, 0xA0, 0x47),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            y += 42;

            // 打开输出目录
            btnOpenOutputDir = new Button
            {
                Text = "打开输出目录",
                Location = new Point(margin, y),
                Size = new Size(120, 28),
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnOpenOutputDir.Click += (s, e) =>
            {
                try { System.Diagnostics.Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory); }
                catch { }
            };

            y += 40;

            // 状态栏
            lblStatus = new Label
            {
                Text = "就绪",
                Location = new Point(margin, y),
                Size = new Size(ClientSize.Width - margin * 2, 24),
                ForeColor = Color.Gray
            };

            Controls.AddRange(new Control[] {
                lblCard, numCardNo, btnRead,
                lblVer, txtVersionInfo,
                lblId, txtVersionId, btnSave,
                btnOpenOutputDir,
                lblStatus
            });
        }

        private void BtnRead_Click(object sender, EventArgs e)
        {
            btnRead.Enabled = false;
            lblStatus.Text = "正在连接板卡...";
            lblStatus.ForeColor = Color.Gray;
            Application.DoEvents();

            try
            {
                short cardNo = (short)numCardNo.Value;
                short ret = M_Open(cardNo, 0);
                if (ret != 0)
                {
                    MessageBox.Show($"打开板卡失败，错误码={ret}\n\n请确认：\n1) 板卡已安装\n2) 未被其他程序占用", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "连接失败";
                    return;
                }

                try
                {
                    byte[] ver = new byte[100];
                    ret = M_GetVersion(out ver[0], 100, cardNo);
                    if (ret != 0)
                    {
                        MessageBox.Show($"M_GetVersion 失败，错误码={ret}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        lblStatus.Text = "读取失败";
                        return;
                    }

                    string str = Encoding.Default.GetString(ver).TrimEnd('\0');
                    string[] results = str.Split(';');
                    if (results.Length < 7)
                    {
                        MessageBox.Show($"版本数据解析失败\n原始数据：{str}", "错误",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _dllVersion = (results[1] + " " + results[0]).Trim();
                    _fpgaVersion = (results[3] + " " + results[2]).Trim();
                    _dspVersion = (results[5] + " " + results[4]).Trim();
                    _authorization = results[6].Length >= 5 ? results[6].Substring(0, 5).Trim() : results[6].Trim();
                    _versionRead = true;

                    txtVersionInfo.Text =
                        $"DLL 版本:   {_dllVersion}\r\n" +
                        $"FPGA 版本:  {_fpgaVersion}\r\n" +
                        $"DSP 版本:   {_dspVersion}\r\n" +
                        $"授权信息:   {_authorization}\r\n" +
                        $"机台名称:   {Environment.MachineName}\r\n" +
                        $"采集时间:   {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                    txtVersionId.Text = GenerateVersionId(_fpgaVersion);
                    txtVersionId.Enabled = true;
                    btnSave.Enabled = true;
                    btnOpenOutputDir.Enabled = true;
                    lblStatus.Text = "读取成功";
                    lblStatus.ForeColor = Color.FromArgb(0x43, 0xA0, 0x47);
                }
                finally
                {
                    M_Close(cardNo);
                }
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show("找不到 ecat_motion.dll，请确保该文件与本程序在同一目录。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "DLL 缺失";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "异常";
            }
            finally
            {
                btnRead.Enabled = true;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!_versionRead) return;

            string versionId = txtVersionId.Text.Trim();
            if (string.IsNullOrEmpty(versionId))
            {
                MessageBox.Show("请输入版本 ID", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string dllsDir = Path.Combine(exeDir, "LC", "DLLs");
                string versionDir = Path.Combine(dllsDir, versionId);
                string configPath = Path.Combine(dllsDir, "lc_version_config.json");

                // 拷贝 DLL
                Directory.CreateDirectory(versionDir);
                string sourceDll = Path.Combine(exeDir, "ecat_motion.dll");
                string targetDll = Path.Combine(versionDir, "ecat_motion.dll");

                if (File.Exists(sourceDll))
                {
                    File.Copy(sourceDll, targetDll, true);
                }
                else
                {
                    var dr = MessageBox.Show("未找到 ecat_motion.dll，是否仍要保存配置？", "提示",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr != DialogResult.Yes) return;
                }

                // 更新配置
                UpdateConfig(configPath, versionId, _dllVersion, _fpgaVersion, _dspVersion, _authorization);

                // 保存摘要
                string summaryPath = Path.Combine(exeDir, $"lc_collect_{Environment.MachineName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(summaryPath,
                    $"机台: {Environment.MachineName}\r\n" +
                    $"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n" +
                    $"DLL版本: {_dllVersion}\r\n" +
                    $"FPGA版本: {_fpgaVersion}\r\n" +
                    $"DSP版本: {_dspVersion}\r\n" +
                    $"授权: {_authorization}\r\n" +
                    $"版本ID: {versionId}\r\n",
                    Encoding.UTF8);

                lblStatus.Text = $"已保存 → {configPath}";
                lblStatus.ForeColor = Color.FromArgb(0x43, 0xA0, 0x47);
                MessageBox.Show(
                    $"版本收集成功！\n\nDLL 已拷贝: {targetDll}\n配置已更新: {configPath}\n\n将 LC/DLLs/ 目录复制到主程序即可。",
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "保存失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "保存失败";
                lblStatus.ForeColor = Color.Red;
            }
        }

        #region 工具方法

        static string GenerateVersionId(string fpgaVersion)
        {
            var parts = fpgaVersion.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                string v = parts[0].Replace(".", "").Replace("V", "v").Replace("v", "v");
                if (!string.IsNullOrEmpty(v))
                    return v;
            }
            return "v" + DateTime.Now.ToString("yyyyMMdd");
        }

        static void UpdateConfig(string configPath, string versionId, string dllVersion, string fpgaVersion, string dspVersion, string auth)
        {
            JObject config;
            if (File.Exists(configPath))
            {
                config = JObject.Parse(File.ReadAllText(configPath, Encoding.UTF8));
            }
            else
            {
                config = new JObject { ["ActiveVersion"] = versionId, ["Versions"] = new JArray() };
            }

            var versions = (JArray)config["Versions"];
            if (versions == null) { versions = new JArray(); config["Versions"] = versions; }

            // 去重
            for (int i = versions.Count - 1; i >= 0; i--)
            {
                if (versions[i]["Id"]?.ToString() == versionId)
                    versions.RemoveAt(i);
            }

            versions.Add(new JObject
            {
                ["Id"] = versionId,
                ["DllSubPath"] = $"LC/DLLs/{versionId}/ecat_motion.dll",
                ["Description"] = $"机台 {Environment.MachineName} 采集于 {DateTime.Now:yyyy-MM-dd}",
                ["ExpectedDllVersion"] = dllVersion,
                ["ExpectedFpgaVersion"] = fpgaVersion,
                ["ExpectedDspVersion"] = dspVersion
            });

            config["ActiveVersion"] = versionId;

            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, config.ToString(Formatting.Indented), Encoding.UTF8);
        }

        #endregion
    }
}
