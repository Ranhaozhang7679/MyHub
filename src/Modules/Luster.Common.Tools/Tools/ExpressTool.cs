#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ExpressTool
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Tools.Tools
* 文 件 名:       ExpressTool.cs
* 创建时间:       2022/4/2 17:00:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      d5c9eae5-8bd0-4f3b-8a93-b6ccf5df9e99
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/2 17:00:59
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using DynamicExpresso;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    public class ExpressTool
    {
        /// <summary>
        /// 静态解析器
        /// </summary>
        private Interpreter interpreter = null;

        private static List<string> MathKeys;

        static ExpressTool()
        {
            MathKeys = GetMathKeys();
        }

        /// <summary>
        /// 构建Math关键字，用于替换表达式
        /// </summary>
        /// <returns></returns>
        private static List<string> GetMathKeys()
        {
            List<string> keys = new List<string>();
            var methods = typeof(Math).GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            foreach (var item in methods)
            {
                var name = item.Name;
                if (!keys.Contains(name))
                {
                    keys.Add(name);
                }
            }

            return keys;
        }

        private string cacheExpr = string.Empty;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="exp"></param>
        public ExpressTool(string expr)
        {
            cacheExpr = ReplaceKeys(expr);
            interpreter = new Interpreter();

            // 识别常量字符 False/True
            var identifiers = interpreter.DetectIdentifiers(cacheExpr);
            if (identifiers != null && identifiers.UnknownIdentifiers.Count() > 0)
            {
                foreach (var item in identifiers.UnknownIdentifiers)
                {
                    if (bool.TryParse(item, out var res))
                    {
                        Expression express = Expression.Constant(res);
                        interpreter.SetIdentifier(new Identifier(item, express));
                    }
                }
            }
        }

        /// <summary>
        /// 替换表达式
        /// </summary>
        /// <param name="expressStr">表达式字符串</param>
        /// <returns>最新的表达式</returns>
        private static string ReplaceKeys(string expressStr)
        {
            foreach (var item in MathKeys)
            {
                expressStr = expressStr.Replace(item, $"Math.{item}");
            }

            return expressStr;
        }

        /// <summary>
        /// 多条件判断
        /// </summary>
        /// <param name="express"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public bool Judge(Dictionary<string, object> vals)
        {
            foreach (var item in vals)
            {
                // 变量名称不能带.
                if (item.Value != null && item.Value is LStatus s)
                {
                    interpreter.SetVariable(item.Key, s.IsRunEnd);
                }
                else
                {
                    interpreter.SetVariable(item.Key, item.Value);
                }
                if (item.Value == null)
                {
                    return false;
                }
            }

            var isTrue = interpreter.Eval<bool>(cacheExpr);
            

            return isTrue;
        }

        /// <summary>
        /// 计算器
        /// </summary>
        /// <param name="expressText"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public double Calulate(Dictionary<string, object> vals)
        {
            double retVal = 0.0;
            foreach (var item in vals)
            {
                interpreter.SetVariable(item.Key, item.Value);
            }

            retVal = interpreter.Eval<double>(cacheExpr);

            return retVal;
        }

        /// <summary>
        /// 表达式是否时正常的表达式
        /// </summary>
        /// <param name="express">express</param>
        /// <param name="key">表达式关键字</param>
        /// <param name="errMsg">错误消息</param>
        /// <returns>表达式是否有效判断</returns>
        public bool IsValid(string expresText, string key, out string errMsg)
        {
            bool isValid = true;
            errMsg = string.Empty;

            if (string.IsNullOrEmpty(expresText))
            {
                errMsg = $"express is null";
                return false;
            }

            // 条件是否未空
            if (string.IsNullOrEmpty(expresText.Replace(key, "")))
            {
                errMsg = $"{expresText} is null";
                return false;
            }

            try
            {
                var keys = key.Split('.');
                if (keys.Length > 1)
                {
                    string replaceKey = key.Replace('.', '_');

                    // 对 aa.Max参数名称支持
                    interpreter.Eval<bool>(expresText.Replace(key, replaceKey), new Parameter(replaceKey, 1));
                }
                else
                {
                    interpreter.Eval<bool>(expresText, new Parameter(key, 1));
                }
            }
            catch (Exception)
            {
                isValid = false;
                errMsg = $"表达式:{expresText} 不支持";
            }

            return isValid;
        }
    }
}