using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Control.Wpf.Motion.Editors;
using Luster.Motion.CommonUI.ViewModel;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class PythonEditorVM : MotionDialogVM
    {
        ICommonBus commonBus = null;

        private string _CodeEditorText = @"import sys
import clr

def Initial():
    print(""Script Initial"")

def Run():
    print(""Script Run"")
    
    # 获取Context, 等同于C#中传入的this
    # print(Context.ID)

def Destroy():
    print(""Script Destroy"")
";
        private string _outputDirectory;
        private string _latestGeneratedFile;
        private string _statusText;
        private string _outputDirText;
        private string _errorText;
        private string _outputText; 
        private string _pipLogOutput;

        IMotionModule _MotionModule = null;
        private string _ID;

        public string CodeEditorText 
        {
            get => _CodeEditorText;
            set => SetProperty(ref _CodeEditorText, value);
        }

        public string OutputDirectory
        {             
            get => _outputDirectory;
            set => SetProperty(ref _outputDirectory, value);
        }

        public string LatestGeneratedFile
        {
            get => _latestGeneratedFile;
            set => SetProperty(ref _latestGeneratedFile, value);
        }

        public string StatusText { get => _statusText; set=> SetProperty(ref _statusText, value); }

        public string ErrorText { get => _errorText; set=> SetProperty(ref _errorText, value); }
        public string OutputText { get => _outputText; set=> SetProperty(ref _outputText, value); }
        
        public string PipLogOutput { get => _pipLogOutput; set=> SetProperty(ref _pipLogOutput, value); }

        public DelegateCommand ImportPackageCommand { get; }
        public DelegateCommand SaveCommand { get; }

        public PythonEditorVM()
        {
            ImportPackageCommand = new DelegateCommand(OnImportPackageAsync);
            SaveCommand = new DelegateCommand(SaveScript);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.ContainsKey("Module"))
                _MotionModule = parameters.GetValue<IMotionModule>("Module");

            if (parameters.ContainsKey("ID"))
            {
                _ID = parameters.GetValue<string>("ID");
            }
            
            // 初始化输出目录
            OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonScripts");
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
            
            // 尝试读取已有的脚本或者加载默认脚本
            LoadScript();
        }

        private void LoadScript()
        {
            if (_MotionModule != null && _MotionModule.Parameters.ContainsKey("ScriptPath"))
            {
                var scriptPathObj = _MotionModule.Parameters["ScriptPath"].Value;
                if (scriptPathObj is LPath lPath && !string.IsNullOrEmpty(lPath.Path))
                {
                    if (File.Exists(lPath.Path))
                    {
                        CodeEditorText = File.ReadAllText(lPath.Path);
                        LatestGeneratedFile = Path.GetFileName(lPath.Path);
                    }
                }
            }
        }

        private void SaveScript()
        {
            try
            {
                string fileName = string.IsNullOrEmpty(LatestGeneratedFile) ? $"PyScript_{DateTime.Now:yyyyMMddHHmmss}.py" : LatestGeneratedFile;
                string filePath = Path.Combine(OutputDirectory, fileName);

                File.WriteAllText(filePath, CodeEditorText);
                
                if (_MotionModule != null && _MotionModule.Parameters.ContainsKey("ScriptPath"))
                {
                    _MotionModule.Parameters["ScriptPath"].Value = new LPath(filePath);
                }

                StatusText = $"保存成功于：{DateTime.Now.ToString("HH:mm:ss")}";
                LatestGeneratedFile = fileName;
            }
            catch (Exception ex)
            {
                StatusText = "保存失败";
                ErrorText = ex.Message;
            }
        }

        private async void OnImportPackageAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择 Python 离线依赖包";
            openFileDialog.Filter = "Python Wheel包 (*.whl)|*.whl|源码压缩包 (*.tar.gz)|*.tar.gz|所有文件 (*.*)|*.*";
            
            if (openFileDialog.ShowDialog() == true)
            {
                string packagePath = openFileDialog.FileName;
                PipLogOutput = $"开始安装离线包: {Path.GetFileName(packagePath)}...\n";

                bool success = await Task.Run(() => InstallPythonPackageOffline(packagePath));
                
                PipLogOutput += success ? "\n>>> 安装成功！" : "\n>>> 安装失败，请查看日志。";
            }
        }

        private bool InstallPythonPackageOffline(string packagePath)
        {
            try
            {
                string pythonEnvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonEnv");
                string pythonExePath = Path.Combine(pythonEnvPath, "python.exe");

                if (!File.Exists(pythonExePath))
                {
                    AppendLog("\n未找到 Python 环境，请检查 PythonEnv 文件夹。");
                    return false;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pythonExePath,
                    Arguments = $"-m pip install \"{packagePath}\" --no-warn-script-location",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = pythonEnvPath
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;

                    process.OutputDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            AppendLog(e.Data);
                    };

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            AppendLog("[Error] " + e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"\n安装出现异常: {ex.Message}");
                return false;
            }
        }

        private void AppendLog(string message)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                PipLogOutput += message + Environment.NewLine;
                return;
            }

            dispatcher.Invoke(() =>
            {
                PipLogOutput += message + Environment.NewLine;
            });
        }
    }
}
