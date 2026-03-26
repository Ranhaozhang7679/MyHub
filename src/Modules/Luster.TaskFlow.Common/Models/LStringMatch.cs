#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LStringMatch
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Models
* 文 件 名:       LStringMatch.cs
* 创建时间:       2022/10/18 18:06:07
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      cd3639a8-a804-443d-862c-e629b04c0962
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/18 18:06:07
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Module;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Common.Models
{
    public class LStringMatch : IXMLParser
    {
        /// <summary>
        /// 匹配表达式
        /// </summary>
        public string MatchString { get; set; }

        /// <summary>
        /// 字符变量
        /// </summary>
        public List<StringVar> Variables { get; set; } = new List<StringVar>();

        /// <summary>
        /// 匹配变量
        /// </summary>
        /// <param name="strVal"></param>
        public void Match(string strVal)
        {
            // 当前匹配项
            //int startIndex = 0;

            // 匹配的字符索引
            int matchIndex = 0;


            //先对申明的变量进行查找，查不到的直接不查询
            List<int> sy = new List<int>();  //在匹配模板中有的索引
            List<int> sypx = new List<int>();  //排序后的索引
            List<int> dz = new List<int>();  //索引对应的地址
            List<int> dzpx = new List<int>();   //排序后的地址
            List<int> zfcd = new List<int>();  //查找到的需要匹配的元素长度
            List<int> zfcdpx = new List<int>();
            sy.Clear();
            dz.Clear();
            sypx.Clear();
            dzpx.Clear();
            zfcd.Clear();
            zfcdpx.Clear();
            for (int i = 0; i < Variables.Count; i++)
            {
                int matchloc = MatchString.IndexOf(Variables[i].Name);
                if (matchloc >= 0)
                {
                    sy.Add(i);
                    dz.Add(matchloc);
                    dzpx.Add(matchloc);
                    zfcd.Add(Variables[i].Name.Length);
                }
                else
                {
                    Variables[i].Value = "NA"; //找不着的也要记得给值
                }
            }

            //给找到的拍个顺序，开始查找分隔
            dzpx.Sort();
            foreach (var varItem in dzpx)
            {
                int d = dz.IndexOf(varItem);
                sypx.Add(sy[d]);
                zfcdpx.Add(zfcd[d]);
            }
            //查找分隔符
            for (int i = 0; i < dzpx.Count; i++)
            {
                string result = "NA";
                //前面的找2个之间的分隔符
                if (i < dzpx.Count - 1)
                {
                    string zf = MatchString.Substring(dzpx[i] + zfcdpx[i], dzpx[i + 1] - dzpx[i] - zfcdpx[i]);//提取出分隔符号
                    int cz = strVal.IndexOf(zf, matchIndex);

                    if (cz >= 0)
                    {
                        result = strVal.Substring(matchIndex, cz - matchIndex);
                        matchIndex = cz + zf.Length;
                    }
                }
                //最后一个找最后一个分隔符号，然后将所有内容放进去
                else
                {
                    string zfx = MatchString.Substring(dzpx[dzpx.Count - 2] + zfcdpx[dzpx.Count - 2], dzpx[dzpx.Count - 1] - dzpx[dzpx.Count - 2] - zfcdpx[dzpx.Count - 2]);
                    int czx = strVal.LastIndexOf(zfx, matchIndex);
                    if (czx > 0 && czx <= matchIndex)
                    {
                        result = strVal.Substring(matchIndex, strVal.Length - czx - 1);
                    }
                }
                Variables[sypx[i]].Value = TypeConvert(Variables[sypx[i]], result);
            }




            foreach (var varItem in Variables)
            {
                #region
                //varItem.Value = "NA";
                //int matchLoc = MatchString.IndexOf(varItem.Name, matchIndex);

                //// 模板中未包含改变量
                //if (matchLoc == -1) continue;

                //string prevChar = "";
                //string lastChar = "";

                //// 匹配模板变量前面和后面两个分隔符
                //if (matchLoc > 0)
                //{
                //    prevChar = MatchString.Substring(matchLoc - 1, 1);
                //}

                //var lastMatchIndex = matchLoc + varItem.Name.Length;

                //matchIndex = lastMatchIndex;

                //if (lastMatchIndex + 1 < MatchString.Length)
                //{
                //    lastChar = MatchString.Substring(matchLoc + varItem.Name.Length, 1);
                //}

                //// 从后往前匹配真实字符串分隔符
                //int sIndex = 0;
                //int lastIndex = strVal.Length - 1;
                //if (!string.IsNullOrEmpty(prevChar))
                //{
                //    sIndex = strVal.IndexOf(prevChar, startIndex) + 1;

                //    // sIndex == 0 起始字符没有匹配到
                //    if (sIndex == 0)
                //    {
                //        continue;
                //    }
                //}

                //if (varItem.Length < 0)
                //{
                //    // 从前往后匹配真实字符串分隔符号
                //    if (!string.IsNullOrEmpty(lastChar))
                //    {
                //        lastIndex = strVal.IndexOf(lastChar, sIndex + 1);

                //        if (lastIndex > -1)
                //        {
                //            // 再次从后往前匹配前一个字符防止相同字符问题 A@B @C;
                //            if (!string.IsNullOrEmpty(prevChar))
                //            {
                //                int tmpIndex = strVal.LastIndexOf(prevChar, lastIndex - 1) + 1;
                //                if (tmpIndex != sIndex)
                //                {
                //                    sIndex = tmpIndex;
                //                }
                //            }
                //        }
                //        else
                //        {

                //            // 默认到结尾
                //            lastIndex = strVal.Length;
                //        }
                //    }
                //    else
                //    {
                //        // 处于字符尾部
                //        lastIndex = strVal.Length;
                //    }
                //}
                //else
                //{
                //    lastIndex = sIndex + varItem.Length;
                //}

                //int charLen = lastIndex - sIndex;
                //varItem.Value = TypeConvert(varItem, strVal.Substring(sIndex, charLen));

                //// 更新位置
                //startIndex = lastIndex;
                #endregion
            }
        }

        /// <summary>
        /// 对固定类型进行转换
        /// </summary>
        /// <param name="strVal"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        private object TypeConvert(StringVar strVal, string str)
        {
            object val = str;
            switch (strVal.DataType)
            {
                case DataType.Bool:
                    if (bool.TryParse(str, out var boolVal))
                    {
                        val = boolVal;
                    }
                    else
                    {
                        val = false;
                    }
                    break;
                case DataType.Short:
                    if (short.TryParse(str, out var shortVal))
                    {
                        val = shortVal;
                    }
                    else
                    {
                        val = -1;
                    }
                    break;
                case DataType.Int:
                    if (int.TryParse(str, out var intVal))
                    {
                        val = intVal;
                    }
                    else
                    {
                        val = -1;
                    }
                    break;
                case DataType.Float:
                    if (float.TryParse(str, out var floatV))
                    {
                        val = floatV;
                    }
                    else
                    {
                        val = -1;
                    }
                    break;
                case DataType.Double:
                    if (double.TryParse(str, out var dVal))
                    {
                        val = dVal;
                    }
                    else
                    {
                        val = -1;
                    }
                    break;
                default:
                    val = str;
                    break;
            }

            return val;
        }

        #region 导入和导出
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("StringMatch");
            xRoot.SetAttributeValue(nameof(MatchString), MatchString);

            // 变量对象
            foreach (var item in Variables)
            {
                xRoot.Add(item.ToXml());
            }

            return xRoot;
        }

        /// <summary>
        /// 数据导出
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute(nameof(MatchString), m =>
            {
                MatchString = m;
            });

            Variables?.Clear();

            foreach (var xItem in xElement.Elements())
            {
                var strVal = new StringVar();
                strVal.FromXml(xItem);
                Variables.Add(strVal);
            }
        }
        #endregion

        public override string ToString()
        {
            return MatchString;
        }
    }

    /// <summary>
    /// 字符串变量
    /// </summary>
    public class StringVar
    {
        /// <summary>
        /// 变量名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 变量类型
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// 变量值
        /// </summary>
        public object Value { get; set; } = "None";

        /// <summary>
        /// 支持固定长度字符
        /// </summary>
        public int Length { get; set; } = -1;

        /// <summary>
        /// 获取对应的类型
        /// </summary>
        public Type Type
        {
            get
            {
                Type type = typeof(string);

                switch (DataType)
                {
                    case DataType.Bool:
                        type = typeof(bool);
                        break;
                    case DataType.Int:
                        type = typeof(int);
                        break;
                    case DataType.Short:
                        type = typeof(short);
                        break;
                    case DataType.Long:
                        type = typeof(long);
                        break;
                    case DataType.Float:
                        type = typeof(float);
                        break;
                    case DataType.Double:
                        type = typeof(double);
                        break;
                    default:
                        break;
                }

                return type;
            }
        }

        public ParameterAttribute CreateParameter(IModule module, int sort)
        {
            return ParameterAttribute.CreateByType(Name, Type, module, Name, sort, ParamType.OUT);
        }
    }
}