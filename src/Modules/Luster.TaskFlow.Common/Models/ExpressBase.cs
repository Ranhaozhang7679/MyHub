#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ExpressBase
* 机器名称:       L05123-02
* 命名空间:       Luster.TaskFlow.Common.Models
* 文 件 名:       ExpressBase.cs
* 创建时间:       2022/12/14 13:51:02
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d1f2eca0-6512-43c3-899c-f9bcae08e5af
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/14 13:51:02
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Common.Models
{
    public class ExpressBase
    {
        public static char[] Seperators = new char[] { '+', '-', '*', '÷', '>', '<', '=', '&', '|', '!', ' ', '(', ')' };
        /// <summary>
        /// 匹配表达式
        /// </summary>
        public const string Pattern = @"(?<=\=?)([\u4e00-\u9fa5|\w+|0-9]+[.][\u4e00-\u9fa5|\w|0-9]+)";

        /// <summary>
        /// 字符串表达式
        /// </summary>
        public string StringEx { get; set; }

        /// <summary>
        /// 变量
        /// </summary>
        public LArray<LVariable> Variables { get; set; }

        /// <summary>
        /// 缓存
        /// </summary>
        protected string cacheStringEx;

        /// <summary>
        /// 变量列表
        /// </summary>
        protected Dictionary<string, LVariable> vList = null;

        /// <summary>
        /// 表达式工具
        /// </summary>
        protected ExpressTool exprTool = null;


        /// <summary>
        /// 构造函数
        /// </summary>
        public ExpressBase()
        {
            Variables = new LArray<LVariable>();
        }

        /// <summary>
        /// 防止字符串将 \r替换成 \\r
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected string GetReplace(string param)
        {
            return param.Replace("\\r", "").Replace("\\n", "");
        }

        /// <summary>
        /// 表达式解析变量值
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="FriendlyException"></exception>
        protected List<string> MatchVariables(string text)
        {
            var splits = text.Replace("\r", "").Replace("\n", "").Split(Seperators);

            // 通过字符串表达式进行解析匹配
            Regex regex = new Regex(Pattern, RegexOptions.IgnoreCase);
            List<string> list = new List<string>();
            foreach (var item in splits)
            {
                var matches = regex.Matches(item);
                if (matches.Count == 0)
                {
                    continue;
                }

                foreach (var mItem in matches)
                {
                    // 排除掉数字
                    if (!double.TryParse(mItem.ToString(), out var value))
                    {
                        list.Add(mItem.ToString());
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 构建变量
        /// </summary>
        /// <param name="module"></param>
        protected virtual void GenVariables(IModule module)
        {
            if (Variables == null) return;

            if (string.IsNullOrEmpty(cacheStringEx) || vList == null)
            {
                cacheStringEx = StringEx;

                vList = new Dictionary<string, LVariable>();
                var matches = LStringEx.MatchVariables(StringEx);
                foreach (var strKey in matches)
                {
                    var val = Variables.FirstOrDefault(u => u.Alias == strKey);
                    if (val != null)
                    {
                        // 变量别名更换
                        string tmpKey = strKey.Replace(".", "_");

                        if (!module.TaskModules.Contains(val.ID))
                        {
                            throw new FriendlyException($"变量:{strKey}不存在,请重新配置变量!");
                        }

                        val.Tag = module.TaskModules[val.ID].Parameters[val.ParamKey];
                        vList[tmpKey] = val;

                        // 数据更换
                        cacheStringEx = cacheStringEx.Replace(strKey, tmpKey);
                    }
                }
            }
        }

        protected Dictionary<string, object> parameters = new Dictionary<string, object>();

        /// <summary>
        /// 构建对象
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        /// <exception cref="FriendlyException"></exception>
        protected Dictionary<string, object> BuildParamVals(IModule module)
        {
            foreach (var item in vList)
            {
                var p = item.Value.Tag as ParameterAttribute;

                if (p == null)
                {
                    throw new FriendlyException($"变量：{item.Value.Alias} 为找到对应的模块!");
                }

                var val = p.GetCacheValue(module);

                if (parameters.ContainsKey(item.Key))
                {
                    // 如果默认值为空，则使用默认值
                    if (val == null)
                    {
                        val = p.DefaultV;
                    }

                    parameters[item.Key] = val;
                }
                else
                {
                    parameters.Add(item.Key, val);
                }
            }

            return parameters;
        }

        /// <summary>
        /// 解析对象
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public virtual XElement ExportXml()
        {
            return this.ToXml();
        }
    }
}