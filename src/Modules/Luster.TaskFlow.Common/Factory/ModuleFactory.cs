#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LoadFactory
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.TaskFlow.Engine.Factory
* 文 件 名:       LoadFactory
* 创建时间:       2021/10/31 17:28:44
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      cb04ed71-7ad5-451a-97d0-913a99703ab8
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 17:28:44
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Module;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace Luster.TaskFlow.Common
{
    public class ModuleFactory : IModuleFactory
    {
        /// <summary>
        /// 模块构建几何节点
        /// </summary>
        private List<IModuleCreator> _creators;

        /// <summary>
        /// 加载根节点,因为涉及到多套系统
        /// </summary>
        private List<LNode> rootNodes;

        /// <summary>
        /// 所有的函数模块
        /// </summary>
        private List<LNode> _functions;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ModuleFactory()
        {
            _creators = new List<IModuleCreator>();
            _functions = new List<LNode>();
            rootNodes = new List<LNode>();
        }

        /// <summary>
        /// 获取功能节点树
        /// </summary>
        /// <returns>功能节点</returns>
        public LNode GetModuleNode(string system)
        {
            var rootNode = rootNodes.FirstOrDefault(u => u.Tag?.ToString() == system);
            if (rootNode != null && rootNode.Children.Count > 0) return rootNode;

            rootNode = new LNode()
            {
                Key = string.Empty,
                Text = string.Empty,
                Level = 0,
                Tag = system
            };

            var list = _creators.Where(u => u.System == system).OrderBy(u => u.Sort);

            // 构建树结构
            foreach (var item in list)
            {
                // 构建Module节点
                LNode subNode = new LNode()
                {
                    Key = item.Name,
                    Text = item.Alias,
                    Parent = rootNode,
                    Level = 1,
                    Tag = item,
                    Tips = item.Tips,
                    Icon = item.Icon
                };

                // 构建 Function
                IModule module = item.Create() as IModule;
                foreach (var funcItem in module.FuncTypes)
                {
                    IFunction func = Activator.CreateInstance(funcItem.Value) as IFunction;

                    if (!func.IsVisible) continue;

                    if (!string.IsNullOrEmpty(func.Group))
                    {
                        var existGroup = subNode.Children.FirstOrDefault(u => u.Key == func.Group);
                        if (existGroup == null)
                        {
                            existGroup = new LNode()
                            {
                                Key = func.Group,
                                Text = func.Group,
                                Parent = subNode,
                                Level = 2,
                                Tag = string.Empty,
                                Icon = func.GroupIcon
                            };

                            subNode.Children.Add(existGroup);
                        }

                        // 函数子节点
                        LNode funcNode = new LNode()
                        {
                            Key = func.Name,
                            Text = func.Alias,
                            Parent = existGroup,
                            Level = 3,
                            Tag = func,
                            Tag1 = module,
                            Tips = func.Tips,
                            Icon = string.IsNullOrEmpty(func.Icon) ? existGroup.Icon : func.Icon,
                        };

                        // 添加到节点中
                        _functions.Add(funcNode);

                        existGroup.Children.Add(funcNode);
                    }
                    else
                    {
                        // 函数子节点
                        LNode funcNode = new LNode()
                        {
                            Key = func.Name,
                            Text = string.IsNullOrEmpty(func.Alias) ? func.Name : func.Alias,
                            Parent = subNode,
                            Level = 2,
                            Tag = func,
                            Tag1 = module,
                            Tips = func.Tips,
                            Icon = string.IsNullOrEmpty(func.Icon) ? subNode.Icon : func.Icon,
                        };

                        _functions.Add(funcNode);

                        subNode.Children.Add(funcNode);
                    }
                }

                rootNode.Children.Add(subNode);
            }

            if (rootNode.Children.Count > 0)
            {
                rootNodes.Add(rootNode);
            }

            return rootNode;
        }

        /// <summary>
        /// 加密狗检查
        /// </summary>
        /// <exception cref="FriendlyException"></exception>
        private void CheckDongle()
        {
            try
            {
                // 加密狗检查
                Luster.Common.Dongle.DongleTool.Init();
                Luster.Common.Dongle.DongleTool.DongleError += str =>
                {
                    if (!Luster.Common.Dongle.SentinelTool.Instance.IsExisted())
                    {
                        System.Windows.Forms.MessageBox.Show("Dongle is not Exist!");
                        Luster.Common.Tools.CommonTool.ApplicatinExit();
                    }
                };
                Luster.Common.Dongle.DongleTool.Login();
            }
            catch (Exception)
            {
                if (!Luster.Common.Dongle.SentinelTool.Instance.IsExisted())
                {
                    System.Windows.Forms.MessageBox.Show("Dongle is not Exist!");
                    Luster.Common.Tools.CommonTool.ApplicatinExit();
                }
            }
        }

        /// <summary>
        /// 动态加载模块列表
        /// </summary>
        public void LoadModules(string dllPath, string dllStart = "Luster.Module")
        {
            //CheckDongle();

            if (Directory.Exists(dllPath))
            {
                var dirInfo = new DirectoryInfo(dllPath);

                // 2.加载模块信息
                foreach (var item in dirInfo.GetFileSystemInfos("*.dll"))
                {
                    var dllName = item.Name;
                    if (dllName.IndexOf(dllStart) >= 0)
                    {
                        var creators = GetByAssembly<IModuleCreator>(item.FullName);
                        if (creators.Count > 0)
                        {
                            _creators.AddRange(creators);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public List<T> GetByAssembly<T>(string assemblyName) where T : class
        {
            Assembly assembly = Assembly.LoadFrom(assemblyName);

            return GetByAssembly<T>(assembly);
        }

        /// <summary>
        /// 动态加载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static List<T> GetByAssembly<T>(Assembly assembly) where T : class
        {
            List<T> ts = new List<T>();
            foreach (var item in assembly.GetTypes())
            {
                // 1.忽略抽象类
                if (item.IsAbstract || item.IsInterface) continue;

                // 2.类继承
                if (typeof(T).IsAssignableFrom(item))
                {
                    ts.Add(Activator.CreateInstance(item) as T);
                }
            }

            return ts;
        }


        /// <summary>
        /// 获取当前程序集所在路径
        /// </summary>
        /// <returns>程序集所在路径</returns>
        public string GetAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// 获取Creator生成器
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns>模块</returns>
        public IModuleCreator GetCreator(string moduleName, string system)
        {
            return _creators.FirstOrDefault(u => u.System == system && u.Name == moduleName);
        }

        /// <summary>
        /// 通过模块名称获取
        /// </summary>
        /// <param name="moduleName">模块的名称</param>
        /// <returns>对应模块和子函数</returns>
        public List<LNode> GetFunctions(string moduleName)
        {
            List<LNode> functions = new List<LNode>();

            // 通过Creator
            var item = _creators.First(u => u.Name == moduleName);
            IModule module = item.Create() as IModule;
            foreach (var funcItem in module.FuncTypes)
            {
                IFunction func = Activator.CreateInstance(funcItem.Value) as IFunction;
                if (!string.IsNullOrEmpty(func.Group))
                {
                    var existGroup = functions.FirstOrDefault(u => u.Key == func.Group);
                    if (existGroup == null)
                    {
                        existGroup = new LNode()
                        {
                            Key = func.Group,
                            Text = func.Group,
                            Level = 2,
                            Tag = string.Empty,
                            Icon = func.Icon
                        };

                        functions.Add(existGroup);
                    }

                    // 函数子节点
                    LNode funcNode = new LNode()
                    {
                        Key = func.Name,
                        Text = func.Alias,
                        Parent = existGroup,
                        Level = 3,
                        Tag = func,
                        Tips = func.Tips,
                        Icon = func.Icon
                    };

                    existGroup.Children.Add(funcNode);
                }
                else
                {
                    // 函数子节点
                    LNode funcNode = new LNode()
                    {
                        Key = func.Name,
                        Text = string.IsNullOrEmpty(func.Alias) ? func.Name : func.Alias,
                        Level = 2,
                        Tag = func,
                        Tips = func.Tips,
                        Icon = func.Icon
                    };

                    functions.Add(funcNode);
                }
            }

            return functions;
        }

        /// <summary>
        /// 获取函数所有节点
        /// </summary>
        /// <returns></returns>
        public List<LNode> GetAllFunctionNodes()
        {
            return _functions;
        }
    }
}