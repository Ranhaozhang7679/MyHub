#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ParameterAttribute
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Attributes
* 文 件 名:       ParameterAttribute
* 创建时间:       2021/10/30 9:48:57
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      8ee6ed4f-7ed6-4d91-95fd-cb1d2f703b77
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/30 9:48:57
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Module;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using System.ComponentModel;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.IO;

namespace Luster.TaskFlow.Common.Attributes
{

    public enum ParamRef
    {
        [Description("空")]
        None,

        [Description("引用")]
        Ref,

        [Description("非引用")]
        NoRef
    }

    /// <summary>
    /// 参数特性
    /// </summary>
    public class ParameterAttribute : Attribute, IXMLParser, INameInfo, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 小数点精度
        /// </summary>
        private static readonly int DEC = 3;
        #region NameInfo

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Tips { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        #endregion

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// 中文参数名称
        /// </summary>
        public string CN { get; set; }

        /// <summary>
        /// 分组
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 对应的属性
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// 参数类型，默认是in参数
        /// </summary>
        public ParamType ParamType { get; set; } = ParamType.IN;

        /// <summary>
        /// 是否作为数据输出
        /// </summary>
        public bool IsOutData { get; set; }

        /// <summary>
        /// 隶属对象
        /// </summary>
        public IModule Owner { get; set; }

        /// <summary>
        /// 参数记忆
        /// </summary>
        public bool IsMemoric { get; set; }

        /// <summary>
        /// 回零恢复默认值,默认为True
        /// </summary>
        public bool IsHomeDefault { get; set; } = true;

        /// <summary>
        /// 引用来自于哪个输出参数
        /// </summary>
        private ParameterAttribute _refOut;

        public ParameterAttribute RefOut
        {
            get => _refOut; set
            {
                var srcRef = _refOut;

                // 删除引用
                if (srcRef != null)
                {
                    Owner.UnRegisterByRef(srcRef.Owner, Name);
                }

                _refOut = value;

                // 清理缓存
                if (value == null)
                {
                    _val = null;
                }

                RefChangedEvent?.Invoke(this);
                if (srcRef != value && IsBindUI)
                {
                    // 更新引用关系
                    Owner.UpdateReferences();

                    Owner.OnLog(LogType.Debug, $"[{Owner.Alias}]参数{CN} 原始引用:{srcRef?.Owner.Alias},新引用:{_refOut?.Owner.Alias}");
                    Owner.OnUpdate(ModuleUpdate.ParameterVal);
                }
            }
        }

        /// <summary>
        /// 参数验证
        /// </summary>
        public List<ValidationAttribute> Validations { get; set; }

        /// <summary>
        /// 依赖对象
        /// </summary>
        public List<KeyValue> DependOns { get; set; }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// 对象是否可以进行展开
        /// </summary>
        public bool CanExpand { get; set; }

        /// <summary>
        /// 是否绑定UI
        /// </summary>
        public bool IsBindUI { get; set; } = false;

        /// <summary>
        /// 是否敏感参数
        /// </summary>
        public bool IsSensitive { get; set; } = false;

        /// <summary>
        /// 参数对应的值
        /// </summary>
        private object _val;
        public object Value
        {
            get
            {
                return _val;
            }
            set
            {
                var srcV = _val;
                _val = value;

                bool isChanged = (value != null && !value.Equals(srcV)) ||
                                  (srcV != null && !srcV.Equals(value)) ||
                                  (srcV == null && value != null) ||
                                  (value == null && srcV != null);

                //bool isChanged = value != srcV;

                // 触发参数变更事件
                if (Owner != null && ParamType == ParamType.IN && isChanged && EditorValue == null)
                {
                    Owner.TaskFunction.OnNotifyPropertyUIChanged(this, value);

                    if (IsSensitive)
                    {
                        Owner.OnPropertyChanged(Name, srcV, value);
                    }

                    if (RefOut == null && IsBindUI)
                    {
                        Owner.OnLog(LogType.Info, $"[{Owner.Alias}]参数{CN} 原始值:{srcV},新值:{value}");
                        Owner.OnUpdate(ModuleUpdate.ParameterVal);
                    }
                }
                
                if (isChanged)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        /// <summary>
        /// 值获取
        /// </summary>
        /// <returns></returns>
        public object GetValue(out string errMsg)
        {
            errMsg = string.Empty;
            if (RefOut == null) return _val;

            if (RefOut.Owner.Status == RunStatus.Skip)
            {
                errMsg = $"引用模块:{RefOut.Owner.Alias} 被忽略!";
                return null;
            }

            // 如果没有运行成功，那么就直接返回默认的运行状态
            if (Name == "InStatus" && RefOut.Owner.Status != RunStatus.Success)
            {
                return RefOut.Owner.TaskFunction.Status;
            }

            return RefOut.GetCacheValue(Owner);
        }

        /// <summary>
        /// 优先从缓存中获取引用值
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public object GetCacheValue(IModule module)
        {
            // 如果对象作为数据列输出，并且存在缓存对象，那么尝试从缓存对象中获取
            if (IsOutData && !Owner.IsSameRoot(module) && module.CacheManager != null && !string.IsNullOrEmpty(module.DataID))
            {
                return module.CacheManager.GetCache(module.DataID, Owner.ID, Name);
            }
            else if (Owner.Status == RunStatus.Success || Owner is IGlobal)
            {
                // 正常模块需要检查石佛运行成功 全局模块不需要运行成功
                return _val;
            }
            else
            {
                // 如果是值类型就返回默认值
                if (Type.IsValueType || Type == typeof(LStatus))
                {
                    return _val;
                }

                return null;
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 小数位位数
        /// </summary>
        public int DecimalPlaces { get; set; }

        /// <summary>
        /// 是否多选
        /// </summary>
        public bool MultiValues { get; set; }

        /// <summary>
        /// 文件过滤器
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultV { get; set; }

        /// <summary>
        /// 参数对应的UI类型
        /// </summary>
        public Type EditorType { get; set; }

        /// <summary>
        /// 是否支持引用类型
        /// </summary>
        public ParamRef CanRef { get; set; }

        /// <summary>
        /// 默认指
        /// </summary>
        private object _editorVal;

        /// <summary>
        /// 参数对应的值
        /// </summary>
        public object EditorValue
        {
            get
            {
                return _editorVal;
            }
            set
            {
                var srcV = _editorVal;
                _editorVal = value;

                //bool isChanged = value != srcV;
                bool isChanged = (value != null && !value.Equals(srcV)) || srcV != null && !srcV.Equals(value);

                // 触发参数变更事件
                if (Owner != null && isChanged && ParamType == ParamType.IN)
                {
                    Owner.TaskFunction.OnNotifyPropertyUIChanged(this, value);

                    if (IsBindUI && srcV != null)
                    {
                        Owner.OnLog(LogType.Debug, $"[{Owner.Alias}]参数{CN} 原始值:{srcV},新值:{value}");
                        Owner.OnUpdate(ModuleUpdate.ParameterVal);
                    }
                }
            }
        }

        /// <summary>
        /// 引用发生变更
        /// </summary>
        public event Action<ParameterAttribute> RefChangedEvent;

        /// <summary>
        /// 数据源变更
        /// </summary>
        public event Action<ParameterAttribute, IEnumerable<object>> DataChanged;

        /// <summary>
        /// 数据源
        /// </summary>
        private object[] datas;
        public object[] Datas
        {
            get => datas; set
            {
                datas = value;
                DataChanged?.Invoke(this, value);
            }
        }

        /// <summary>
        /// 参数配置
        /// </summary>
        /// <param name="sort"></param>
        public ParameterAttribute(string tips, int sort)
        {
            Tips = tips;
            Alias = Name;
            Sort = sort;
            DecimalPlaces = 0;
            Validations = new List<ValidationAttribute>();
            DefaultV = default(Type);

            if (string.IsNullOrEmpty(CN))
            {
                CN = Name;
            }

            if (Type == typeof(double))
            {
                DecimalPlaces = DEC;
            }
        }

        /// <summary>
        /// 获取真实类型名称
        /// </summary>
        /// <param name="realType"></param>
        /// <returns></returns>
        private string GetTypeName(Type realType)
        {
            foreach (var item in _typeDict)
            {
                if (item.Value == realType)
                {
                    // 如果是接口或者抽象类，那么类型设置为真实类型
                    return item.Key;
                }
            }

            return realType.Name;
        }

        #region 导入导出接口

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xParam = new XElement(Name);

            // 扩展参数
            if (Property == null)
            {
                if (!string.IsNullOrEmpty(CN))
                {
                    xParam.SetAttributeValue("CN", CN);
                    if (string.IsNullOrEmpty(Alias))
                    {
                        Alias = CN;
                    }
                }

                if (!string.IsNullOrEmpty(Tips))
                {
                    xParam.SetAttributeValue("Tips", Tips);
                }

                if (CanRef == ParamRef.Ref)
                {
                    xParam.SetAttributeValue("CanRef", true);
                }

                if (DefaultV != null)
                {
                    xParam.SetAttributeValue("DefaultV", DefaultV);
                }

                if (ParamType == ParamType.OUT)
                {
                    xParam.SetAttributeValue("ParamType", ParamType);
                }
            }

            xParam.SetAttributeValue("Alias", Alias);

            // 设置序号
            xParam.SetAttributeValue("Sort", Sort);

            // 设置真实类型
            if ((this.Type.IsInterface || this.Type.IsAbstract) && this.Value != null)
            {
                var realType = this.Value.GetType();
                xParam.SetAttributeValue("ImplType", GetTypeName(realType));
            }

            xParam.SetAttributeValue("Type", GetTypeName(this.Type));

            // 范围大小设定
            if (Validations.Count > 0)
            {
                foreach (var item in Validations)
                {
                    if (item is RangeAttribute range)
                    {
                        xParam.SetAttributeValue("Min", range.Minimum);
                        xParam.SetAttributeValue("Max", range.Maximum);

                        break;
                    }
                }
            }

            // 添加Editor
            if (EditorType != null && EditorValue != null)
            {
                if (EditorValue is IXMLParser xmlParser)
                {
                    // 是否是Editor对象
                    xParam.SetAttributeValue("IsEditor", true);
                    xParam.Add(xmlParser.ExportXml());
                }
            }

            // 如果对象是引用对象，则直接返回
            if (RefOut != null)
            {
                xParam.SetAttributeValue("RefID", RefOut.Owner.ID);
                xParam.SetAttributeValue("RefName", RefOut.Name);

                return xParam;
            }

            if (IsOutData)
            {
                xParam.SetAttributeValue("IsOutData", true);
            }

            // 如果存在值的情况
            if (Value == null)
            {
                return xParam;
            }

            // 只有输入参数，才需要记录值类型
            if (ParamType == ParamType.IN)
            {
                if (Type.IsNumeric() || typeof(string) == Type || Type.IsEnum)
                {
                    // 1.如果是值类型或者字符串类型
                    xParam.SetAttributeValue("Value", Value?.ToString());
                }
                else if (Value is IXMLParser xParser)
                {
                    xParam.Add(xParser.ExportXml());
                }
            }
            xParam.SetAttributeValue("CN", CN);

            xParam.SetAttributeValue ("IsMemoric",IsMemoric);
            xParam.SetAttributeValue("IsHomeDefault", IsHomeDefault);

            return xParam;
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="xElement">解析</param>
        public void ParserXml(XElement xParameter)
        {
            this.Name = xParameter.Name.LocalName;

            // 别名
            xParameter.GetAttribute("Alias", (alias) => this.Alias = alias);

            // 扩展参数
            if (Property == null)
            {
                xParameter.GetAttribute("CN", (cn) => this.CN = cn);

                xParameter.GetAttribute("CanRef", (canRef) =>
                {
                    if (bool.TryParse(canRef, out var isRef))
                    {
                        CanRef = ParamRef.Ref;
                    }
                });

                // 参数类型
                xParameter.GetAttribute("ParamType", (pType) =>
                {
                    ParamType = pType.ToEnum<ParamType>();
                });

                this.Group = ParamType == ParamType.IN ? "InputParameter" : "OutputParameter";

                // 数据类型
                xParameter.GetAttribute("Type", (type) =>
                {
                    if (_typeDict.ContainsKey(type))
                        Type = _typeDict[type];
                });
            }

            // 备注信息
            xParameter.GetAttribute("Tips", (tips) => this.Tips = tips);

            // 序号
            xParameter.GetAttribute("Sort", (sort) => this.Sort = Convert.ToInt32(sort));

            // 默认值
            xParameter.GetAttribute("DefaultV", (defaultV) =>
            {
                this.DefaultV = defaultV.ConvertToType(Type);
            });

            // 作为输出参数
            xParameter.GetAttribute("IsOutData", (oData) =>
            {
                IsOutData = true;
            });

            xParameter.GetAttribute("IsMemoric", (_memoric) =>
            {
                if (bool.TryParse(_memoric, out var isMemoric))
                {
                    IsMemoric = isMemoric;
                }
                else
                {
                    IsMemoric = false;
                }
            });

            xParameter.GetAttribute("IsHomeDefault", (_homeDefault) =>
            {
                if (bool.TryParse(_homeDefault, out var isHomeDefault))
                {
                    IsHomeDefault = isHomeDefault;
                }
                else
                {
                   IsHomeDefault = true;
                }
            });

            // 引用来源与XML
            LoadFromRefXml(xParameter);

            // 设置Value
            var xValue = xParameter.Elements().FirstOrDefault();
            if (xValue != null)
            {
                Type realType = Type;

                xParameter.GetAttribute("ImplType", (impType) =>
                {
                    if (_typeDict.ContainsKey(impType))
                        realType = _typeDict[impType];
                });

                // 是否来源editor
                bool isEditor = false;
                xParameter.GetAttribute("IsEditor", editor => isEditor = bool.Parse(editor));

                // 如果类型继承自IXMLParser  
                if (typeof(IXMLParser).IsAssignableFrom(realType) && !isEditor)
                {
                    if (Value != null && Value is IXMLParser parser)
                    {
                        parser.ParserXml(xValue);
                    }
                    else
                    {
                        var newV = Activator.CreateInstance(realType) as IXMLParser;
                        newV.ParserXml(xValue);
                        LoadFromXml(() => Value = Convert.ChangeType(newV, realType));
                    }
                }
                else if (EditorType != null)
                {
                    if (Activator.CreateInstance(EditorType) is IXMLParser parser)
                    {
                        parser.ParserXml(xValue);
                        // 设置Value
                        LoadFromXml(() => EditorValue = Convert.ChangeType(parser, EditorType));
                    }
                }

            }
            else if (Type.IsEnum)
            {
                // 枚举类型需要特殊转换
                xParameter.GetAttribute("Value", (value) =>
                {
                    try
                    {
                        var objVal = Enum.Parse(Type, value);
                        LoadFromXml(() => Value = objVal);
                    }
                    catch (ArgumentException ex)
                    {
                        Owner.OnLog(LogType.Error, $"{value} is not exist!");
                    }
                });
            }
            else
            {
                // 基础类型直接转换
                xParameter.GetAttribute("Value", (value) =>
                {
                    LoadFromXml(() =>
                    {
                        // 对历史的string类型进行兼容
                        if (Type == typeof(LStringEx) && value is string s)
                        {
                            Value = new LStringEx(s);
                        }
                        else
                        {
                            if (value == "NA" && (Type == typeof(int) || Type == typeof(double) || Type == typeof(float)))
                            {
                                Value = 0;
                            }
                            else
                            {
                                Value = Convert.ChangeType(value, Type);
                            }
                        }
                    }, $"参数名称:{Name} 转换失败!");
                });
            }
        }

        /// <summary>
        /// 通过参数添加引用对象
        /// </summary>
        /// <param name="xParameter"></param>
        public void LoadFromRefXml(XElement xParameter = null)
        {
            // 已经解析了，就部进行二次解析了
            if (RefOut != null) return;

            if (xParameter == null && !string.IsNullOrEmpty(cacheXML))
            {
                xParameter = XElement.Parse(cacheXML);
            }

            if (xParameter == null) return;

            var xRefID = xParameter.Attribute("RefID");
            if (xRefID != null)
            {
                var refID = Guid.Parse(xRefID.Value);
                var refName = xParameter.Attribute("RefName").Value;
                if (Owner.TaskModules.Contains(refID))
                {
                    var refModule = Owner.TaskModules[refID];
                    var refV = refModule.Parameters.FirstOrDefault(u => u.Key == refName);
                    if (refV.Value != null)
                    {
                        LoadFromXml(() => RefOut = refV.Value);
                    }

                    // 清除数据
                    cacheXML = "";
                }
                else
                {
                    cacheXML = xParameter.ToString();
                }
            }
        }

        // 缓存由于没有找到对应模块造成
        private string cacheXML = "";

        /// <summary>
        /// 用于定义首次加载参数
        /// </summary>
        /// <param name="action"></param>
        private void LoadFromXml(Action action, string logTitle = "")
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Owner.OnLog(LogType.Error, $"{logTitle} {ex.Message}");
            }
        }


        #endregion

        /// <summary>
        /// 通过属性来创建参数
        /// </summary>
        /// <param name="property">属性类型</param>
        /// <returns></returns>
        public static ParameterAttribute CreateByProperty(PropertyInfo u, IModule module)
        {
            ParameterAttribute paramAttr = u.GetCustomAttribute<ParameterAttribute>();
            if (paramAttr == null) return null;

            // 通过属性就行赋值
            paramAttr.Name = u.Name;
            paramAttr.Type = u.PropertyType;
            paramAttr.Property = u;
            paramAttr.Owner = module;
            if (string.IsNullOrEmpty(paramAttr.CN))
            {
                paramAttr.CN = u.Name;
            }

            // 设置默认分组
            if (string.IsNullOrEmpty(paramAttr.Group))
                paramAttr.Group = (paramAttr.ParamType == ParamType.IN) ? "InputParameter" : "OutputParameter";

            // 是否支持展开值
            paramAttr.CanExpand = typeof(IExpand).IsAssignableFrom(u.PropertyType);

            // 如果是double类型,并且没有设置默认的小数位数，默认保留两位小数
            if (u.PropertyType == typeof(double) && paramAttr.DecimalPlaces == 0)
            {
                paramAttr.DecimalPlaces = DEC;
            }
            else if (u.PropertyType == typeof(int))
            {
                // 默认是0
                paramAttr.DecimalPlaces = 0;
            }

            // 设置ComponentType
            //if (paramAttr.CType == ComponentType.Null)
            //{
            //    paramAttr.CType = RelationTool.Instance.GetCType(u.PropertyType);
            //}

            // 通过类型获取DType
            //paramAttr.DType = RelationTool.Instance.GetDType(u.PropertyType);

            // 获取所有待验证的特性
            var valids = u.GetCustomAttributes<ValidationAttribute>();
            foreach (var attr in valids)
            {
                paramAttr.Validations.Add(attr);
            }

            if (paramAttr.DefaultV != null)
            {
                paramAttr._val = paramAttr.DefaultV;
            }
            else
            {
                object v = u.GetValue(module.TaskFunction);
                if (v != null)
                    paramAttr._val = v;
            }

            // 获取依赖的参数
            var depends = u.GetCustomAttributes<DependOnAttribute>();
            if (depends != null)
            {
                paramAttr.DependOns = new List<KeyValue>();
                foreach (var item in depends)
                {
                    foreach (var subVal in item.DependValue)
                    {
                        paramAttr.DependOns.Add(new KeyValue()
                        {
                            Key = item.DependProp,
                            Value = subVal,
                        });
                    }
                }
            }


            // 引用类型
            //var type = paramAttr.InputType;

            //int sort = paramAttr.Sort;
            //// 获取值类型的属性值
            //foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            //{
            //    var outMember = prop.GetCustomAttribute<OutMemberAttribute>(true);
            //    if (outMember != null)
            //    {
            //        // 构建隶属属性
            //        var curProperty = paramAttr.CreateByInOutType(paramAttr.PType, prop.Name);

            //        curProperty.Sort = sort + 1;
            //        paramAttr.PropInOuts.Add(curProperty);

            //        sort++;
            //    }
            //}

            return paramAttr;
        }

        /// <summary>
        /// 通过属性来创建参数
        /// </summary>
        /// <param name="property">属性类型</param>
        /// <returns></returns>
        public static ParameterAttribute CreateByType(string pName, Type pType, IModule module, string cn = "", int sort = 5, ParamType paramType = ParamType.IN)
        {
            ParameterAttribute paramAttr = new ParameterAttribute("extend paramters", 0);
            if (paramAttr == null) return null;

            // 通过属性就行赋值
            paramAttr.Name = pName;
            paramAttr.Type = pType;
            paramAttr.Property = null;
            paramAttr.Owner = module;
            paramAttr.ParamType = paramType;
            paramAttr.Sort = sort;
            if (string.IsNullOrEmpty(paramAttr.CN))
            {
                paramAttr.CN = cn;
            }

            // 设置默认分组
            if (string.IsNullOrEmpty(paramAttr.Group))
                paramAttr.Group = (paramAttr.ParamType == ParamType.IN) ? "InputParameter" : "OutputParameter";

            // 是否支持展开值
            paramAttr.CanExpand = false;

            // 如果是double类型,并且没有设置默认的小数位数，默认保留两位小数
            if (pType == typeof(double) && paramAttr.DecimalPlaces == 0)
            {
                paramAttr.DecimalPlaces = DEC;
            }
            else if (pType == typeof(int))
            {
                // 默认是0
                paramAttr.DecimalPlaces = 0;
            }

            // 设置ComponentType
            //if (paramAttr.CType == ComponentType.Null)
            //{
            //    paramAttr.CType = RelationTool.Instance.GetCType(u.PropertyType);
            //}

            // 通过类型获取DType
            paramAttr.DataType = ParameterAttribute.GetDataTypeByType(pType);

            // 默认添加必须的参数
            //paramAttr.Validations.Add(new RequiredAttribute());

            if (paramAttr.DefaultV != null)
                paramAttr._val = paramAttr.DefaultV;

            return paramAttr;
        }

        /// <summary>
        /// 字典集合
        /// </summary>
        private static Dictionary<string, Type> _typeDict = new Dictionary<string, Type>();

        /// <summary>
        /// 类型注册
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="type"></param>
        public static void ResgisterTypeMaps(string typeName, Type type)
        {
            if (!_typeDict.ContainsKey(typeName))
            {
                _typeDict.Add(typeName, type);
            }
        }

        /// <summary>
        /// 获取所有类型
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Type> GetAllTypes()
        {
            return _typeDict;
        }

        public static string GetDataTypeByType(Type type)
        {
            foreach (var item in _typeDict)
            {
                if (item.Value == type)
                {
                    return item.Key;
                }
            }

            return "String";
        }

        public static Type GetTypeByDataType(string dType)
        {
            if (_typeDict.ContainsKey(dType))
            {
                return _typeDict[dType];
            }

            return null;
        }


        #region 外部事件触发
        /// <summary>
        /// 配置事件
        /// </summary>
        public event Action<ParameterAttribute> ConfigEvent;

        /// <summary>
        /// 外部方法触发
        /// </summary>
        /// <param name="pAttr">特性</param>
        public void OnConfig(ParameterAttribute pAttr)
        {
            ConfigEvent?.Invoke(pAttr);
        }
        #endregion
    }
}