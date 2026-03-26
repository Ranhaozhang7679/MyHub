#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LExpress
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Data
* 文 件 名:       LExpress.cs
* 创建时间:       2022/2/11 10:42:14
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      21cddad5-b935-4817-8c4b-2d1b4ddf442c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/11 10:42:14
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using DynamicExpresso;
using Luster.Common.Extensions;
using Luster.Common.Models;
using Luster.TaskFlow.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Common.Data
{
    public class LExpress : IXMLParser, IEmptyObj
    {
        /// <summary>
        /// 表达式名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 表达式
        /// </summary>
        public string ExText { get; set; }

        /// <summary>
        /// 关键字
        /// </summary>
        public List<string> KeyWords { get; set; }

        /// <summary>
        /// 静态解析器
        /// </summary>
        private static Interpreter interpreter = new Interpreter();

        public LExpress()
        {
            ExText = string.Empty;
            Name = string.Empty;
            KeyWords = new List<string>();
        }

        /// <summary>
        /// 扩展
        /// </summary>
        /// <param name="express">表达式</param>
        /// <param name="key">表达式参数</param>
        /// <param name="val">值</param>
        /// <returns>最终判定结果</returns>
        public bool Judge(params KeyValue[] keyVals)
        {
            Parameter[] parameters = new Parameter[keyVals.Length];
            for (int i = 0; i < keyVals.Length; i++)
            {
                parameters[i] = new Parameter(keyVals[i].Key, keyVals[i].Value);
            }

            return interpreter.Eval<bool>(ExText, parameters);
        }

        /// <summary>
        /// 多条件判断
        /// </summary>
        /// <param name="express"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public bool Judge(Dictionary<string, object> vals)
        {
            List<Parameter> ps = new List<Parameter>();

            foreach (var item in vals)
            {
                ps.Add(new Parameter(item.Key, item.Value));
            }

            return interpreter.Eval<bool>(ExText, ps.ToArray());
        }
        /// <summary>
        /// 表达式是否时正常的表达式
        /// </summary>
        /// <param name="express">express</param>
        /// <param name="key">表达式关键字</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>表达式是否有效判断</returns>
        public bool IsValid(string key, out string errMsg)
        {
            bool isValid = true;
            errMsg = string.Empty;

            if (string.IsNullOrEmpty(ExText))
            {
                errMsg = $"express is null";
                return false;
            }

            // 条件是否未空
            if (string.IsNullOrEmpty(ExText.Replace(key, "")))
            {
                errMsg = $"{ExText} is null";
                return false;
            }

            try
            {
                var keys = key.Split('.');
                if (keys.Length > 1)
                {
                    string replaceKey = key.Replace('.', '_');

                    // 对 aa.Max参数名称支持
                    interpreter.Eval<bool>(ExText.Replace(key, replaceKey), new Parameter(replaceKey, 1));
                }
                else
                {
                    interpreter.Eval<bool>(ExText, new Parameter(key, 1));
                }
            }
            catch (Exception)
            {
                isValid = false;
                errMsg = $"表达式:{ExText} 不支持";
            }

            return isValid;
        }

        /// <summary>
        /// 表达式导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("Express");
            xRoot.SetAttributeValue("Name", Name);

            xRoot.Add(ExText);

            return xRoot;
        }

        /// <summary>
        /// 表达式解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Name", (name) => Name = name);
            ExText = xElement.Value;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(ExText);
        }
    }
}
