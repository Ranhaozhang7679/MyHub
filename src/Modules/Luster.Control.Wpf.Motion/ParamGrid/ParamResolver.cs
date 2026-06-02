using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Control.Wpf.Motion.Editors;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace Luster.Control.Wpf.Motion
{
    public class ParamResolver
    {
        private Dictionary<Type, UIEditorType> TypeCodeDic = new Dictionary<Type, UIEditorType>()
        {
            [typeof(string)] = UIEditorType.PlainText,
            [typeof(sbyte)] = UIEditorType.SByteNumber,
            [typeof(byte)] = UIEditorType.ByteNumber,
            [typeof(short)] = UIEditorType.Int16Number,
            [typeof(ushort)] = UIEditorType.UInt16Number,
            [typeof(int)] = UIEditorType.Int32Number,
            [typeof(uint)] = UIEditorType.UInt32Number,
            [typeof(long)] = UIEditorType.Int64Number,
            [typeof(ulong)] = UIEditorType.UInt64Number,
            [typeof(float)] = UIEditorType.SingleNumber,
            [typeof(double)] = UIEditorType.DoubleNumber,
            [typeof(bool)] = UIEditorType.Switch,
            [typeof(DateTime)] = UIEditorType.DateTime,
            [typeof(HorizontalAlignment)] = UIEditorType.HorizontalAlignment,
            [typeof(VerticalAlignment)] = UIEditorType.VerticalAlignment,
            [typeof(ImageSource)] = UIEditorType.ImageSource,
            [typeof(LPath)] = UIEditorType.FilePath,
            [typeof(LRange)] = UIEditorType.Range,
            [typeof(LExpress)] = UIEditorType.Express,
            [typeof(VDevice)] = UIEditorType.Device,
            [typeof(LNetwork)] = UIEditorType.Network,
            [typeof(IGlobal)] = UIEditorType.Global,
            [typeof(IGlobalBool)] = UIEditorType.GlobalBool,
            [typeof(IGlobalByType)] = UIEditorType.GlobalByType
        };

        /// <summary>
        /// 参数解析成对应的Editor
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public ParamEditorBase ResolveEditor(ParameterAttribute p)
        {
            var eType = p.EditorType ?? p.Type;

            ParamEditorBase editor = null;

            // 输出参数默认是返回只读窗体
            if (p.ParamType == TaskFlow.Common.Enums.ParamType.OUT)
            {
                editor = new ReadOnlyTextEditor();
                return editor;
            }

            if (p.RefOut != null)
            {
                editor = new CiteEditor();
                return editor;
            }

            if (TypeCodeDic.TryGetValue(eType, out var eVal))
            {
                switch (eVal)
                {
                    case UIEditorType.PlainText:
                        if (p.Datas == null)
                        {
                            editor = new PlainTextEditor();
                        }
                        else
                        {
                            editor = new ArrayEditor();
                        }
                        break;

                    case UIEditorType.Int16Number:
                    case UIEditorType.Int32Number:
                    case UIEditorType.Int64Number:
                    case UIEditorType.UInt16Number:
                    case UIEditorType.UInt32Number:
                    case UIEditorType.UInt64Number:
                    case UIEditorType.DoubleNumber:
                    case UIEditorType.SingleNumber:

                        // 获取数字的最大值和最小值
                        double min = -9999, max = 999999999;
                        var rangeAttr = p.Validations.OfType<RangeAttribute>().FirstOrDefault();
                        if (rangeAttr != null)
                        {
                            min = Convert.ToDouble(rangeAttr.Minimum);
                            max = Convert.ToDouble(rangeAttr.Maximum);
                        }

                        return new NumberEditor(min, max, p.DecimalPlaces);

                    case UIEditorType.Switch:
                        editor = new SwitchEditor();
                        break;

                    case UIEditorType.FilePath:
                        editor = new PathEditor();
                        break;

                    case UIEditorType.Range:
                        double rMin = -9999, rMax = 999999999;
                        var attr = p.Validations.OfType<RangeAttribute>().FirstOrDefault();
                        if (attr != null)
                        {
                            rMin = Convert.ToDouble(attr.Minimum);
                            rMax = Convert.ToDouble(attr.Maximum);
                        }
                        editor = new RangeEditor() { Minimum = rMin, Maximum = rMax };
                        break;

                    case UIEditorType.Rely:
                        editor = new RelyEditor();
                        break;

                    case UIEditorType.Network:
                        editor = new NetworkEditor();
                        break;
                    case UIEditorType.Global:
                        editor = new GlobalEditor();
                        break;
                    case UIEditorType.GlobalBool:
                        editor = new GlobalBoolEditor();
                        break;
                    case UIEditorType.GlobalByType:
                        editor = new GlobalTypeFilterEditor();
                        break;
                    default:
                        editor = new ReadOnlyTextEditor();
                        break;
                }
            }
            else if (p.Type == typeof(VDevice))
            {
                editor = new DeviceEditor();
            }
            else if (p.Type == typeof(SocketAction))
            {
                editor = new SlaveEditor();
            }
            else if (p.Type == typeof(VAlarm))
            {
                editor = new AlarmEditor();
            }
            else if (p.Type == typeof(VAxisMDevice) || p.Type == typeof(VAxisDevice))
            {
                editor = new AxisMEditor();
            }
            else if (p.Type == typeof(LCondition)
                || HasImplementedRawGeneric(p.Type, typeof(LArray<>))
                || p.Type == typeof(LStringEx)
                || p.Type == typeof(LExpression)
                || p.Type == typeof(LStringMatch)
                || p.Type == typeof(LStation)
                || p.Type == typeof(LModule)
                || p.Type == typeof(VAxisPos))
            {
                editor = new ConfigEditor();
            }
            else if (p.Type.IsSubclassOf(typeof(Enum)))
            {
                if (p.MultiValues)
                {
                    editor = new MEnumEditor();
                }
                else
                {
                    editor = new EnumEditor();
                }
            }
            else
            {
                editor = CustomType(eType);
            }

            // 设置隶属模块ID
            editor.ModuleID = p.Owner.ID;
            return editor;
        }

        public bool HasImplementedRawGeneric(Type type, Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (generic == null) throw new ArgumentNullException(nameof(generic));

            // 测试接口。
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);

            if (isTheRawGenericType) return true;

            // 测试类型。
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);

                if (isTheRawGenericType) return true;

                type = type.BaseType;
            }

            // 没有找到任何匹配的接口或类型。
            return false;

            // 测试某个类型是否是指定的原始接口。
            bool IsTheRawGenericType(Type test)
            => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        /// <summary>
        /// 可扩展设计，用于自定义类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>返回对应的Editor</returns>
        public virtual ParamEditorBase CustomType(Type type)
        {
            return new CiteEditor();
        }

        /// <summary>
        /// 获取数据类型是否支持引用、默认所有类型都支持范围Link
        /// </summary>
        /// <param name="p">参数对象</param>
        /// <returns>字符串</returns>
        public virtual string ResolveRef(ParameterAttribute p)
        {
            if (p.ParamType == TaskFlow.Common.Enums.ParamType.OUT)
            {
                if (typeof(ISave).IsAssignableFrom(p.Type))
                {
                    return "Save";
                }
                else
                {
                    return "Normal";
                }
            }

            if (p.CanRef == ParamRef.Ref)
            {
                return "Link";
            }

            if (p.CanRef == ParamRef.NoRef)
            {
                return "Normal";
            }

            if (p.Type.IsEnum ||
                p.Type.IsValueType ||
                typeof(string).IsAssignableFrom(p.Type) ||
                typeof(LRange).IsAssignableFrom(p.Type) ||
                typeof(LNetwork).IsAssignableFrom(p.Type) ||
                typeof(VAxisMDevice).IsAssignableFrom(p.Type) ||
                p.Type.HasImplementedRawGeneric(typeof(LArray<>)) ||
                typeof(VAxisPos).IsAssignableFrom(p.Type) ||
                typeof(LModule).IsAssignableFrom(p.Type) ||
                typeof(VDevice).IsAssignableFrom(p.Type) ||
                typeof(LCondition).IsAssignableFrom(p.Type) ||
                typeof(LStringEx).IsAssignableFrom(p.Type) ||
                typeof(LStringMatch).IsAssignableFrom(p.Type) ||
                typeof(LExpression).IsAssignableFrom(p.Type) ||
                typeof(LStation).IsAssignableFrom(p.Type) ||
                typeof(LRelyType).IsAssignableFrom(p.Type)) return "Normal";

            return "Link";
        }
    }

    internal enum UIEditorType
    {
        PlainText,
        SByteNumber,
        ByteNumber,
        Int16Number,
        UInt16Number,
        Int32Number,
        UInt32Number,
        Int64Number,
        UInt64Number,
        SingleNumber,
        DoubleNumber,
        Switch,
        DateTime,
        HorizontalAlignment,
        VerticalAlignment,
        ImageSource,
        FilePath,
        Range,
        Rely,
        Express,
        Device,
        Network,
        Global,
        GlobalBool,
        GlobalByType
    }
}