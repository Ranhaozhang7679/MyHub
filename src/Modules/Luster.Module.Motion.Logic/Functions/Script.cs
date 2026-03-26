using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Luster.Module.Motion.Logic.Functions
{
    public class Script : MotionFunction
    {
        private LPath _dllPath;

        [Parameter("DllPath", 2, CN = "库文件", CanRef = ParamRef.NoRef)]
        public LPath DllPath
        {
            get => _dllPath;
            set
            {
                if (value == null) return;
                //if (_dllPath?.Path != value?.Path)
                //{
                _dllPath = value;

                LoadDll();
                // }

            }
        }


        public Script()
        {
            this.Tips = "C#脚本编写";
            this.Icon = "\xe612";
        }

        Assembly _assembly;


        object _instance;

        void LoadDll()
        {
            if (_dllPath == null)
            {
                System.Diagnostics.Debug.WriteLine("DLL路径为空");
                return;
            }

            try
            {
                if (_assembly != null && _method_detroy != null)
                {
                    _method_detroy.Invoke(_instance, null);
                }

                //_assembly = Assembly.LoadFrom(_dllPath.Path);
                byte[] assemblyBytes = File.ReadAllBytes(_dllPath.Path);
                _assembly = Assembly.Load(assemblyBytes);
                Type type = _assembly.GetType("LusterMotionScript.UserScript");

                if (type == null)
                {
                    System.Diagnostics.Debug.WriteLine("未找到类型 LusterMotionScript.UserScript");
                    return;
                }

                _instance = Activator.CreateInstance(type);
                _method_run = type.GetMethod("Run");
                _method_detroy = type.GetMethod("Destroy");
                MethodInfo method_intial = type.GetMethod("Initial");
                if (_method_run == null)
                {
                    System.Diagnostics.Debug.WriteLine("未找到方法 Run");
                    throw new FriendlyException($"未找到方法 Run！");
                }

                if (method_intial != null)
                {
                    method_intial.Invoke(_instance, null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载 DLL 时出错: {ex.Message}");
                throw new FriendlyException($"加载 DLL 时出错: {ex.Message}！");
            }
        }


        MethodInfo _method_run;
        MethodInfo _method_detroy;
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            if (_method_run == null || _instance == null)
            {
                errMsg = "方法或实例未正确初始化";
                return false;
            }

            try
            {
                _method_run.Invoke(_instance, null);
                return true;
            }
            catch (Exception ex)
            {
                errMsg = $"执行方法时出错: {ex.Message}";
                return false;
            }
        }

    }
}
