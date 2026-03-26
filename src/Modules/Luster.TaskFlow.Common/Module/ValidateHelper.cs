#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ValidateHelper
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Module
* 文 件 名:       ValidateHelper
* 创建时间:       2021/10/31 15:50:38
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      aecb9b02-b5eb-4b24-8366-402d57c5a744
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 15:50:38
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using System.Reflection;

namespace Luster.TaskFlow.Common.Module
{
    public class ValidateHelper
    {
        /// <summary>
        /// 对输入参加验证并进行赋值
        /// </summary>
        /// <param name="pObj">pObj</param>
        /// <param name="val">val</param>
        /// <param name="inOut">inOut</param>
        /// <param name="errMsg">errMsg</param>
        /// <returns>bool</returns>
        public static bool ValidateIn(IFunction funcObj, ParameterAttribute pAttr, out string errMsg)
        {
            errMsg = string.Empty;
            object val = pAttr.GetValue(out errMsg);
            if (!string.IsNullOrEmpty(errMsg)) return false;

            // 如果有相对坐标系
            if (pAttr.RefOut == null && val is IRelativeCoord relativeCoord)
            {
                var refParam = funcObj.Owner?.GetRefCoord();
                if (refParam != null)
                {
                    relativeCoord.Coord = refParam.Value as ICoord3;
                }
            }

            // 3.循环验证条件,只有正常基元才进行验证
            foreach (var item in pAttr.Validations)
            {
                if (!item.IsValid(val))
                {
                    errMsg += item.FormatErrorMessage(string.Empty);
                }
            }

            // 如果是泛型类，那么对应类型参数类型
            Type baseTypePar = pAttr.Type.GetArrayGenericType();

            // 4.类型匹配检查
            if (val != null && !val.GetType().IsNumeric())
            {
                var vType = val.GetType();
                if (vType != pAttr.Type &&
                    !vType.IsAssignableFrom(pAttr.Type) &&
                    !pAttr.Type.IsAssignableFrom(vType) &&
                    baseTypePar != null && !baseTypePar.IsAssignableFrom(vType))
                {
                    errMsg += "Value'TypeIsNotSameToInType" + string.Format(",value's type is {0}", vType.ToString());
                }
            }

            // 5.检查是否由移除出现
            if (string.IsNullOrEmpty(errMsg))
            {
                // 5.1 如果都验证过了，值还是为空,那么代表不需要验证
                if (!pAttr.Type.IsValueType && val == null)
                {
                    if (pAttr.Property != null)
                    {
                        pAttr.Property.SetValue(funcObj, null);
                    }                    
                    return true;
                }

                object changeVal = null;
                if (baseTypePar != null && baseTypePar.IsAssignableFrom(val.GetType()))
                {
                    // 构建泛型数组
                    changeVal = Activator.CreateInstance(pAttr.Type);
                    pAttr.Type.InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null, changeVal, new object[] { val });
                }
                else
                {
                    changeVal = val.ConvertTo(pAttr.Type);
                }
                // 5.2 对象转换到指定类型
                //if (pAttr.Type.IsNumeric() || pAttr.Type == typeof(string))
                //{
                //    changeValue = val.ChangeType(inOut.InputType);
                //}
                //else
                //{
                //    // 1.如果类型是引用类型
                //    changeValue = val;
                //}
                if (pAttr.Property != null)
                {
                    pAttr.Property.SetValue(funcObj, changeVal);
                }

                // 如果是原始的属性
                //if (string.IsNullOrEmpty(inOut.PropertyRef) && inOut.PropInfo != null)
                //{
                //    pAttr.Property.SetValue(funcObj, changeValue);
                //}
                //else if (!string.IsNullOrEmpty(inOut.PropertyRef) && inOut.PropInfo != null)
                //{
                //    // 此时是依赖属性
                //    var propRef = inOut.TaskPrim.InOutList.FirstOrDefault(u => u.Name == inOut.PropertyRef);
                //    if (propRef != null && propRef.Value != null && changeValue != null)
                //    {
                //        if (inOut.PropInfo.GetSetMethod() != null)
                //        {
                //            // 更新Val值
                //            inOut.PropInfo.SetValue(propRef.Value, changeValue);

                //            // 更新对应属性值
                //            propRef.PropInfo.SetValue(pObj, propRef.Value);
                //        }
                //    }
                //}

                pAttr.Value = changeVal;

                return true;
            }
            else
            {
                // 记录那个参数出错
                return false;
            }
        }

        /// <summary>
        /// 验证引用类型的数据
        /// </summary>
        /// <param name="obj">obj</param>
        /// <param name="errMsg">errMsg</param>
        /// <returns>bool</returns>
        public static bool ValidateAllIn(IFunction func, out string errMsg)
        {
            bool result = true;
            errMsg = string.Empty;

            var parameters = func.Owner.Parameters;

            // 对输入参数进行校验
            foreach (var item in parameters)
            {
                var parameter = item.Value;

                // out参数不进行验证
                if (parameter.ParamType == ParamType.OUT) continue;

                if (!ValidateIn(func, parameter, out errMsg))
                {
                    errMsg = string.Format("{0}:{1}", parameter.CN, errMsg);
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}