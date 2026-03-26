#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LString
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Models
* 文 件 名:       LString.cs
* 创建时间:       2022/9/8 10:56:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5801cca8-78ef-4a98-b99c-d1c0da1b3915
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 10:56:00
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
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
    public class LStringEx : IXMLParser
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
        private string cacheStringEx;

        /// <summary>
        /// 变量列表
        /// </summary>
        private List<LVariable> vList;

        /// <summary>
        /// 字符检测
        /// </summary>
        private Dictionary<string, string> cacheVars = new Dictionary<string, string>();

        public LStringEx()
        {
            Variables = new LArray<LVariable>();
            cacheVars = new Dictionary<string, string>();
        }

        public LStringEx(string stringEx) : this()
        {
            StringEx = stringEx;
        }

        /// <summary>
        /// 重新ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return StringEx;
        }

        /// <summary>
        /// 防止字符串将 \r替换成 \\r
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private string GetReplace(string param)
        {
            return param.Replace("\\r", "\r").Replace("\\n", "\n");
        }

        /// <summary>
        /// 最终将表达式转换为真字符串
        /// AA.X>3&&AA.Y<3
        /// </summary>
        /// <returns></returns>
        public string GetString(IModule module)
        {
            if (Variables == null) return GetReplace(StringEx);

            if (string.IsNullOrEmpty(cacheStringEx) || vList == null)
            {
                cacheStringEx = StringEx;

                vList = new List<LVariable>();
                var matches = MatchVariables(StringEx);
                int i = 0;
                foreach (var strKey in matches)
                {
                    var val = Variables.FirstOrDefault(u => u.Alias == strKey);
                    if (val != null)
                    {
                        val.Tag = module.TaskModules[val.ID].Parameters[val.ParamKey];

                        cacheStringEx = cacheStringEx.Replace(strKey, String.Format("{{{0}}}", i));
                        i++;

                        vList.Add(val);
                    }
                }
            }

            List<object> vals = new List<object>();

            foreach (var item in vList)
            {
                var p = item.Tag as ParameterAttribute;

                if (p == null)
                {
                    throw new FriendlyException($"变量：{item.Alias} 为找到对应的模块!");
                }

                var val = p.GetCacheValue(module);
                vals.Add(val);
            }

            if (vals.Count == 0)
            {
                return GetReplace(StringEx);
            }
            else
            {
                string result = cacheStringEx;
                for (int i = 0; i < vals.Count; i++)
                {
                    result = result.Replace($"{{{i}}}", vals[i]?.ToString());
                }

                return result;
            }
        }


        /// <summary>
        /// 表达式解析变量值
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="FriendlyException"></exception>
        public static List<string> MatchVariables(string text)
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
                    string strItem = mItem.ToString();
                    if (double.TryParse(strItem, out var d))
                    {
                        continue;
                    }

                    list.Add(strItem);
                }
            }

            return list;
        }

        /// <summary>
        /// 解析对象
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            return this.ToXml();
        }
    }
}