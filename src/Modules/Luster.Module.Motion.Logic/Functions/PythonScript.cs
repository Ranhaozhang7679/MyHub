using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using System;
using System.IO;
using System.Linq;
using Python.Runtime;

namespace Luster.Module.Motion.Logic.Functions
{
    public class PythonScript : MotionFunction
    {
        private LPath _scriptPath;
        private string _scriptContent;

        [Parameter("ScriptPath", 2, CN = "Python脚本文件", CanRef = ParamRef.NoRef)]
        public LPath ScriptPath
        {
            get => _scriptPath;
            set
            {
                if (value == null) return;
                _scriptPath = value;
                LoadScript();
            }
        }

        public PythonScript()
        {
            this.Tips = "Python脚本执行";
            this.Icon = "\xe613";
        }

        private void LoadScript()
        {
            if (_scriptPath == null || !File.Exists(_scriptPath.Path))
            {
                System.Diagnostics.Debug.WriteLine("Python脚本文件不存在");
                return;
            }

            try
            {
                _scriptContent = File.ReadAllText(_scriptPath.Path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取 Python 脚本时出错: {ex.Message}");
                throw new FriendlyException($"读取 Python 脚本时出错: {ex.Message}！");
            }
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            if (string.IsNullOrEmpty(_scriptContent))
            {
                errMsg = "脚本内容为空";
                return false;
            }

            try
            {
                PythonEngineManager.Initialize();

                using (Py.GIL())
                {
                    using (PyModule scope = Py.CreateScope())
                    {
                        scope.Exec(_scriptContent);
                        
                        dynamic runFunc = scope.Get("Run");
                        
                        // 注入当前上下文或其他对象
                        scope.Set("Context", this);

                        if (runFunc != null)
                        {
                            runFunc();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errMsg = $"执行 Python 脚本时出错: {ex.Message}";
                return false;
            }
        }
    }

    public static class PythonEngineManager
    {
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            string pythonEnvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonEnv");
            string pythonDllPath = Directory.GetFiles(pythonEnvPath, "python3*.dll")
                .FirstOrDefault(f => !Path.GetFileName(f).Equals("python3.dll", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(pythonDllPath) || !File.Exists(pythonDllPath))
            {
                // Fallback for debug/development purposes if python is installed in system paths
                // Just let pythonnet find the default, or warn the user.
                System.Diagnostics.Debug.WriteLine("找不到嵌入的 Python 环境！将尝试使用系统默认 Python 环境。");
            }
            else
            {
                Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDllPath);
                PythonEngine.PythonHome = pythonEnvPath;
                PythonEngine.PythonPath = $"{pythonEnvPath};{Path.Combine(pythonEnvPath, @"Lib\site-packages")}";
            }

            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads(); 

            _isInitialized = true;
        }
    }
}
