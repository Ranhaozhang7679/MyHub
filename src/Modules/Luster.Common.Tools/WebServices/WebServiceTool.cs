#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WebServiceTool
* 机器名称:       L05123-02
* 命名空间:       Luster.Common.Tools.Tools
* 文 件 名:       WebServiceTool.cs
* 创建时间:       2023/2/7 10:33:41
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      342fcec9-bdb3-4b9c-b504-3aa2ea91259d
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/7 10:33:41
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace Luster.Common.Tools.WebServices
{
    public class WebServiceTool
    {
        /// <summary>
        /// 获取或设置WebService的地址
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// 获取或设置用于登录WebService所在站点的用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 获取或设置用于登录WebService所在站点的密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 获取或设置用于登录WebService所在站点的域
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值表示此站点是否使用默认身份认证
        /// </summary>
        public bool UseDefaultCredentials { get; set; }

        /// <summary>
        /// 获取或设置连接到该WebService所使用的连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                object returnObject;
                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                {
                    returnObject = new { URL, UseDefaultCredentials };
                }
                else if (string.IsNullOrEmpty(Domain))
                {
                    returnObject = new { URL, UserName, Password, UseDefaultCredentials };
                }
                else
                {
                    returnObject = new { URL, UserName, Password, Domain, UseDefaultCredentials };
                }
                return JsonConvert.SerializeObject(returnObject);
            }
            set
            {
                var jObject = JsonConvert.DeserializeObject<JObject>(value);
                URL = UserName = Password = Domain = null;
                UseDefaultCredentials = true;
                foreach (JProperty jProperty in jObject.Properties())
                {
                    switch (jProperty.Name)
                    {
                        case "URL":
                            URL = jProperty.Value.Value<string>();
                            break;
                        case "UserName":
                            UserName = jProperty.Value.Value<string>();
                            break;
                        case "Password":
                            Password = jProperty.Value.Value<string>();
                            break;
                        case "Domain":
                            Domain = jProperty.Value.Value<string>();
                            break;
                        case "UseDefaultCredentials":
                            UseDefaultCredentials = jProperty.Value.Value<bool>();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 获取WebService的服务说明
        /// </summary>
        public ServiceDescription ServiceDescription { private set; get; }

        /// <summary>
        /// 初始化一个WebService数据源提供程序的连接类的实例
        /// </summary>
        /// <param name="url">WebService的地址</param>
        /// <param name="useDefaultCredentials">此站点是否使用默认身份认证</param>
        public WebServiceTool(string url, bool useDefaultCredentials)
        {
            URL = url;
            UseDefaultCredentials = useDefaultCredentials;
        }

        /// <summary>
        /// 初始化一个WebService数据源提供程序的连接类的实例
        /// </summary>
        /// <param name="url">WebService的地址</param>
        /// <param name="userName">用于登录WebService所在站点的用户名</param>
        /// <param name="password">用于登录WebService所在站点的密码</param>
        public WebServiceTool(string url, string userName, string password) : this(url, false)
        {
            UserName = userName;
            Password = password;
        }

        /// <summary>
        /// 初始化一个WebService数据源提供程序的连接对象
        /// </summary>
        /// <param name="url">WebService的地址</param>
        /// <param name="userName">用于登录WebService所在站点的用户名</param>
        /// <param name="password">用于登录WebService所在站点的密码</param>
        /// <param name="domain">用于登录WebService所在站点的域</param>
        public WebServiceTool(string url, string userName, string password, string domain) : this(url, userName, password)
        {
            Domain = domain;
        }

        /// <summary>
        /// 初始化一个WebService数据源提供程序的连接对象
        /// </summary>
        /// <param name="connectionString">连接到该WebService所使用的连接字符串</param>
        public WebServiceTool(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// 从服务器加载WebService的服务说明
        /// </summary>
        public ServiceDescription LoadServiceDescription()
        {
            //下载WSDL信息
            var webClient = new WebClient
            {
                UseDefaultCredentials = UseDefaultCredentials
            };
            if (!UseDefaultCredentials)
            {
                if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password)
                    && string.IsNullOrEmpty(Domain))
                    webClient.Credentials = new NetworkCredential(UserName, Password, Domain);
                else if (string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password))
                {
                    var domainIndex = UserName.LastIndexOf('@');
                    if (domainIndex > 0 && domainIndex < UserName.Length - 1)
                    {
                        var userName = UserName.Remove(domainIndex);
                        var domain = UserName.Remove(0, domainIndex + 1);
                        webClient.Credentials = new NetworkCredential(userName, Password, domain);
                    }
                    else
                    {
                        webClient.Credentials = new NetworkCredential(UserName, Password);
                    }
                }
            }
            else webClient.Credentials = new NetworkCredential();
            var stream = webClient.OpenRead($"{URL}?WSDL");

            //创建和格式化WSDL文档
            ServiceDescription = ServiceDescription.Read(stream ?? throw new InvalidOperationException());
            return ServiceDescription;
        }

        /// <summary>
        /// 获取一个列表，该列表包含了此WebService的类
        /// </summary>
        /// <returns>类列表</returns>
        public IEnumerable<string> GetClassList()
        {
            //如果还没有获取服务说明则先获取服务说明
            if (ServiceDescription == null) LoadServiceDescription();
            var classList = new List<string>();
            foreach (Service service in ServiceDescription.Services)
            {
                classList.Add(service.Name);
            };
            return classList;
        }

        /// <summary>
        /// 获取一个列表，该列表包含了此WebService的类中的方法
        /// </summary>
        /// <param name="_class">类名</param>
        /// <returns>方法列表</returns>
        public IEnumerable<string> GetMethodList(string _class)
        {
            //如果还没有获取服务说明则先获取服务说明
            if (ServiceDescription == null) LoadServiceDescription();
            var methodList = new List<string>();
            foreach (Operation operation in ServiceDescription.PortTypes[$"{_class}Soap"].Operations)
            {
                methodList.Add(operation.Name);
            };
            return methodList;
        }

        /// <summary>
        /// 获取指定方法的描述
        /// </summary>
        /// <param name="_class">类名</param>
        /// <param name="method">方法名</param>
        /// <returns>指定方法的描述</returns>
        public string GetMethodDescription(string _class, string method)
        {
            //如果还没有获取服务说明则先获取服务说明
            if (ServiceDescription == null) LoadServiceDescription();
            var methodList = new List<string>();
            foreach (Operation operation in ServiceDescription.PortTypes[$"{_class}Soap"].Operations)
            {
                if (operation.Name == method)
                {
                    return operation.Documentation;
                }
            };
            throw new KeyNotFoundException($"method:{method} is not exist!"); //指定的方法不存在。
        }

        /// <summary>
        /// 获取指定方法的参数列表
        /// </summary>
        /// <param name="_class">类名</param>
        /// <param name="method">方法名</param>
        /// <returns>指定方法的描述</returns>
        public SortedDictionary<string, string> GetMethodParameterList(string _class, string method)
        {
            //如果还没有获取服务说明则先获取服务说明
            if (ServiceDescription == null) LoadServiceDescription();
            //var methodList = new List<string>();
            foreach (Operation operation in ServiceDescription.PortTypes[$"{_class}Soap"].Operations)
            {
                if (operation.Name != method) continue;
                if (operation.Messages.Input == null) continue;
                var messageName = operation.Messages.Input.Message.Name;
                var messageElement = ServiceDescription.Messages[messageName].Parts["parameters"].Element;
                var schemaElement = (System.Xml.Schema.XmlSchemaElement)ServiceDescription.Types.Schemas[messageElement.Namespace].Elements[messageElement];
                var particle = ((System.Xml.Schema.XmlSchemaComplexType)schemaElement.SchemaType).Particle;
                if (particle == null) return new SortedDictionary<string, string>();
                var parameterObjectConnection = ((System.Xml.Schema.XmlSchemaSequence)particle).Items;
                var parameters = new SortedDictionary<string, string>();
                foreach (var parameterObject in parameterObjectConnection)
                {
                    var parameterSchemaElement = (System.Xml.Schema.XmlSchemaElement)parameterObject;
                    //var parameter = new Dictionary<string, string>();
                    parameters.Add(parameterSchemaElement.Name, parameterSchemaElement.SchemaTypeName.Name);
                }
                return parameters;
            };
            throw new KeyNotFoundException($"method:{method} is not exist!"); //指定的方法不存在。
        }

        /// <summary>
        /// 获取指定方法的返回类型
        /// </summary>
        /// <param name="_class">类名</param>
        /// <param name="method">方法名</param>
        /// <returns>指定方法的描述</returns>
        public string GetMethodReturnType(string _class, string method)
        {
            //如果还没有获取服务说明则先获取服务说明
            if (ServiceDescription == null) LoadServiceDescription();
            var methodList = new List<string>();
            foreach (Operation operation in ServiceDescription.PortTypes[$"{_class}Soap"].Operations)
            {
                if (operation.Name != method) continue;
                if (operation.Messages.Output == null) continue;
                var messageName = operation.Messages.Output.Message.Name;
                var messageElement = ServiceDescription.Messages[messageName].Parts["parameters"].Element;
                var schemaElement = (System.Xml.Schema.XmlSchemaElement)ServiceDescription.Types.Schemas[messageElement.Namespace].Elements[messageElement];
                var particle = ((System.Xml.Schema.XmlSchemaComplexType)schemaElement.SchemaType).Particle;
                var parameterObjectConnection = ((System.Xml.Schema.XmlSchemaSequence)particle).Items;
                var parameterSchemaElement = (System.Xml.Schema.XmlSchemaElement)parameterObjectConnection[0];
                return parameterSchemaElement.SchemaTypeName.Name;
            };
            throw new KeyNotFoundException($"method:{method} is not exist!"); //指定的方法不存在。
        }

        /// <summary>
        /// 获取类似文档的方法说明
        /// </summary>
        /// <param name="_class">类名</param>
        /// <param name="method">方法名</param>
        /// <returns></returns>
        public string GetMethodDoc(string _class, string method)
        {
            var parameterListText = string.Empty;
            foreach (var parameterList in GetMethodParameterList(_class, method))
            {
                parameterListText += $"{parameterList.Value} {parameterList.Key}, ";
            }
            if (!string.IsNullOrEmpty(parameterListText)) parameterListText = parameterListText.Remove(parameterListText.Length - 2);
            var returnText = string.Format("{0}方法\n\n{1}\n\n类：{2}\n    {3}\n\n语法：\n  public {4} {0}({5})",
            method, GetMethodDescription(_class, method), _class, GetClassDescription(_class), GetMethodReturnType(_class, method), parameterListText);
            return returnText;
        }

        /// <summary>
        /// 获取指定类的描述
        /// </summary>
        /// <param name="_class">类名</param>
        /// <returns>指定类的描述</returns>
        public string GetClassDescription(string _class)
        {
            //如果还没有获取服务说明则先获取服务说明
            if (ServiceDescription == null) LoadServiceDescription();
            return ServiceDescription.Services[_class].Documentation;
        }

        /// <summary>
        /// 获取一个WebService查询对象
        /// </summary>
        /// <param name="method">要执行的方法</param>
        /// <param name="_class">所在的类</param>
        /// <returns>WebService查询对象</returns>
        public MethodCommand GetMethodCommand(string method, string _class)
        {
            return new MethodCommand(method, _class, this);
        }

    }
}