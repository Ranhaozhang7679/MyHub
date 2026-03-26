#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MethodCommand
* 机器名称:       L05123-02
* 命名空间:       Luster.Common.Tools.WebServices
* 文 件 名:       MethodCommand.cs
* 创建时间:       2023/2/7 10:41:10
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4301fe33-afa4-44ae-9cbf-6cde3b7984e0
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/7 10:41:10
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using System;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;

namespace Luster.Common.Tools.WebServices
{
    /// <summary>
    /// 使用方法的WebService数据源提供程序的查询类
    /// </summary>
    public class MethodCommand
    {
        /// <summary>
        /// WebService客户端CodeDOM程序图容器
        /// </summary>
        protected readonly CodeCompileUnit CodeCompileUnit;

        //方法信息
        private readonly MethodInfo _methodInfo;

        //WebServiceObject对象
        private readonly object _webServiceObject;

        //参数值列表
        private readonly object[] _parameterValues;

        /// <summary>
        /// 获取或设置此查询所使用的参数
        /// </summary>
        public object this[string parameterName]
        {
            get => this[_methodInfo.GetParameters().ToList().FindIndex(p => p.Name == parameterName)];
            set => this[_methodInfo.GetParameters().ToList().FindIndex(p => p.Name == parameterName)] = value;
        }

        /// <summary>
        /// 获取或设置此查询所使用的参数
        /// </summary>
        public object this[int index]
        {
            get => _parameterValues[index];
            set
            {

                if (value.GetType() != _methodInfo.GetParameters()[index].ParameterType)
                    throw new ArgumentException(
                        string.Format("形参 {0} 的类型为 {1} ，不能接受类型为 {2} 的实参。",
                            _methodInfo.GetParameters()[index].Name,
                            _methodInfo.GetParameters()[index].ParameterType,
                            value.GetType())
                        );
                _parameterValues[index] = value;
            }
        }


        /// <summary>
        /// 获取此查询基类所使用的WebService数据源提供程序的连接
        /// </summary>
        public WebServiceTool Connection { get; }

        /// <summary>
        /// 获取此查询实例所使用的方法
        /// </summary>
        public string Method => _methodInfo.Name;

        /// <summary>
        /// 获取此查询实例所使用的方法
        /// </summary>
        public string Class => _webServiceObject.GetType().Name;

        /// <summary>
        /// 使用CodeDom编译客户端代理类
        /// </summary>
        /// <param name="connection">WebService数据源提供程序的连接类的实例</param>
        protected MethodCommand(WebServiceTool connection)
        {
            Connection = connection;

            //如果还没有获取服务说明则先获取服务说明
            if (connection.ServiceDescription == null) connection.LoadServiceDescription();

            //创建客户端代理类
            var serviceDescriptionImporter = new ServiceDescriptionImporter
            {
                ProtocolName = "Soap", // 指定访问协议。
                Style = ServiceDescriptionImportStyle.Client, // 生成客户端代理
                CodeGenerationOptions = CodeGenerationOptions.GenerateProperties  //| CodeGenerationOptions.GenerateNewAsync
            };

            serviceDescriptionImporter.AddServiceDescription(connection.ServiceDescription, null, null); // 添加 WSDL 文档

            //使用CodeDom编译客户端代理类
            var codeNamespace = new CodeNamespace(); // 为代理类添加命名空间，缺省为全局空间
            CodeCompileUnit = new CodeCompileUnit();
            CodeCompileUnit.Namespaces.Add(codeNamespace);
            serviceDescriptionImporter.Import(codeNamespace, CodeCompileUnit);
        }

        /// <summary>
        /// 初始化一个使用方法的WebService数据源提供程序的查询类的实例
        /// </summary>
        /// <param name="method"></param>
        /// <param name="_class"></param>
        /// <param name="connection"></param>
        public MethodCommand(string method, string _class, WebServiceTool connection) : this(connection)
        {
            var codeDomProvider = CodeDomProvider.CreateProvider("CSharp");

            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                ReferencedAssemblies =
                {
                    "System.dll",
                    "System.Xml.dll",
                    "System.Web.Services.dll",
                    "System.Data.dll"
                }
            };

            var compilerResults = codeDomProvider.CompileAssemblyFromDom(compilerParameters, CodeCompileUnit);

            //使用Reflection调用WebService
            var assembly = compilerResults.CompiledAssembly;
            var type = assembly.GetType(_class); // 如果在前面为代理类添加了命名空间，此处需要将命名空间添加到类型前面。

            _webServiceObject = Activator.CreateInstance(type);
            _methodInfo = type.GetMethod(method);

            //获取参数列表
            _parameterValues = _methodInfo != null ? new object[_methodInfo.GetParameters().Length] : new object[0];
        }

        /// <summary>
        /// 从远处服务器查询数据
        /// </summary>
        /// <returns>查询结果对象</returns>
        public object Query(params object[] vals)
        {
            SetParameters(vals);
            return _methodInfo.Invoke(_webServiceObject, _parameterValues);
        }

        /// <summary>
        /// 基于jsonData进行查询
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public object Query(string jsonData)
        {
            var jData = JsonTool.ToDict(jsonData);
            int i = 0;
            foreach (var item in jData)
            {
                this[i] = item.Value;
                i++;
            }

            return _methodInfo.Invoke(_webServiceObject, _parameterValues);
        }

        /// <summary>
        /// 更新参数
        /// </summary>
        /// <param name="pVals"></param>
        public void SetParameters(params object[] pVals)
        {
            for (int i = 0; i < pVals.Length; i++)
            {
                this[i] = pVals[i];
            }
        }
    }
}