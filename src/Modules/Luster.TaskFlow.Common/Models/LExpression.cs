#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LExpression
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Models
* 文 件 名:       LExpression.cs
* 创建时间:       2022/9/29 21:31:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      679f00f1-ed5e-4bf5-9936-2208e7f9a5bb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/29 21:31:40
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Common.Tools;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Common.Models
{
    public class LExpression : ExpressBase, IXMLParser
    {
        /// <summary>
        /// 表达式名称
        /// </summary>
        public string Name { get; set; } = "";

        public LExpression() : base() { }

        public LExpression(string stringEx) : this()
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
        /// 最终将表达式转换为真字符串
        /// AA.X>3&&AA.Y<3
        /// </summary>
        /// <returns></returns>
        public bool GetResult(IModule module, Action<IModule> pAction = null)
        {
            if (Variables == null) return false;

            GenVariables(module);

            if (exprTool == null)
            {
                // 去除换行符号
                cacheStringEx = GetReplace(cacheStringEx);
                exprTool = new ExpressTool(cacheStringEx);
            }

            if (vList.Count == 0 && bool.TryParse(cacheStringEx, out var r))
            {
                return r;
            }
            else
            {
                var vals = BuildParamVals(module);

                // 最终结果
                bool isResult = exprTool.Judge(vals);

                // 如果成功，并且Value是LStatus类型，则需要特殊处理
                if (isResult)
                {
                    // 表达式添加Log打印
                    module.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"Express:{cacheStringEx},Value:{vals.ToSafeString()}");

                    // 如果满足条件，那么判断是否存在等待模块
                    foreach (var item in vals)
                    {
                        // 变量名称不能带.
                        if (item.Value != null && item.Value is LStatus s && s.IsRunEnd)
                        {
                            s.SetDefault();
                            pAction?.Invoke(s.Owner as IModule);
                        }
                    }
                }

                return isResult;
            }
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void ClearCache()
        {
            exprTool = null;
            vList?.Clear();
            cacheStringEx = string.Empty;
        }

        /// <summary>
        /// 表达式验证
        /// </summary>
        public void ValidExpress(IModule module)
        {
            GenVariables(module);
        }

        /// <summary>
        /// 数字
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public double GetValue(IModule module)
        {
            GenVariables(module);

            if (exprTool == null)
            {
                // 去除换行符号
                cacheStringEx = GetReplace(cacheStringEx);
                exprTool = new ExpressTool(cacheStringEx);
            }

            if (vList.Count == 0 && double.TryParse(cacheStringEx, out var r))
            {
                return r;
            }
            else
            {
                var vals = BuildParamVals(module);

                return exprTool.Calulate(vals);
            }
        }

    }
}