using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using System.Collections.Generic;

namespace Luster.Common.Assets.Tools
{
    internal class CommonHelper
    {
        static Dictionary<string, int> nums = new Dictionary<string, int>();

        /// <summary>
        /// 模块变量名称
        /// </summary>
        internal static List<string> VarList = new List<string>();
        internal static Dictionary<string, List<string>> ParamList = new Dictionary<string, List<string>>();

        internal static void BuildTreeNodes(List<LNode> nodes)
        {
            VarList.Clear();
            ParamList.Clear();
            nums = new Dictionary<string, int>();
            foreach (var item in nodes)
            {
                BuildVarTree(item);
            }
        }

        private static void BuildVarTree(LNode rNode)
        {
            foreach (var item in rNode.Children)
            {
                if (item.Tag is IModule m)
                {
                    BuildVarTree(item);
                    VarList.Add(m.Alias);
                }
                else if (item.Tag is ParameterAttribute p)
                {
                    if (!ParamList.ContainsKey(p.Owner.Alias))
                    {
                        ParamList.Add(p.Owner.Alias, new List<string>());
                    }

                    var list = ParamList[p.Owner.Alias];
                    list.Add(p.CN);

                    string varName = $"{p.Owner.Alias}.{p.CN}";

                    if (nums.ContainsKey(varName))
                    {
                        nums[varName]++;
                    }
                    else
                    {
                        nums.Add(varName, 1);
                    }

                    item.Tag1 = varName;
                }
            }
        }

        private static void FindNode(List<LNode> nodes, string varText, ref ParameterAttribute pNode)
        {
            foreach (var item in nodes)
            {
                if (item.Tag is not ParameterAttribute)
                {
                    FindNode(item.Children, varText, ref pNode);
                }
                else
                {
                    if (varText.Trim() == item.Tag1.ToString())
                    {
                        pNode = item.Tag as ParameterAttribute;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 解析关键字
        /// </summary>
        /// <param name="exText"></param>
        /// <returns></returns>
        internal static LArray<LVariable> ParseKeywords(List<LNode> nodes, string exText)
        {
            LArray<LVariable> keywords = new LArray<LVariable>();

            var matches = LStringEx.MatchVariables(exText);
            foreach (var item in matches)
            {
                ParameterAttribute mNode = null;
                FindNode(nodes, item, ref mNode);
                if (mNode != null)
                {
                    LVariable lV = new LVariable()
                    {
                        ID = mNode.Owner.ID,
                        Alias = $"{mNode.Owner.Alias}.{mNode.CN}",
                        ParamKey = mNode.Name,
                        Tag = mNode
                    };

                    keywords.Add(lV);
                }
                else
                {
                    throw new FriendlyException($"变量:{item} 不存在!");
                }
            }

            return keywords;
        }

        /// <summary>
        /// 检查
        /// </summary>
        internal static bool CheckVar(string alias)
        {
            if (!nums.ContainsKey(alias))
            {
                throw new FriendlyException($"变量:{alias}不存在!");
            }

            var num = nums[alias];
            if (num > 1)
            {
                throw new FriendlyException($"变量:{alias} 存在相同的模块!");
            }

            return true;
        }

        internal static List<string> GetNodeKeys(List<LNode> nodes)
        {
            List<string> keys = new List<string>();
            AddKeys(nodes, keys); ;
            return keys;
        }

        internal static void AddKeys(List<LNode> nodes, List<string> keys)
        {
            foreach (var item in nodes)
            {
                if (item.Tag is ParameterAttribute p)
                {
                    keys.Add(item.Tag1.ToString());
                }
                else
                {
                    AddKeys(item.Children, keys);
                }
            }
        }

        internal static void AddKeyToEditor(LNode lNode, TextEditor textEditor, TextDocument doc, string text)
        {
            if (lNode.Tag is ParameterAttribute p)
            {
                if (textEditor == null)
                {
                    doc.Text = $"{doc.Text}{p.Owner.Alias}.{p.CN}";
                }
                else
                {
                    doc.Insert(textEditor.SelectionStart, $"{p.Owner.Alias}.{p.CN}");
                }
            }
        }
    }
}
