#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ParamResolver
* 机器名称:       L05123-NB
* 命名空间:       Luster.Controls.Wpf.ParamGrid
* 文 件 名:       ParamResolver.cs
* 创建时间:       2022/4/18 16:51:48
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dd66796a-0b28-45f9-a7c9-ef70eb0a850d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 16:51:48
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Assets.ParamGrid
{
    public class ParamResolver
    {
        /// <summary>
        /// 解析
        /// </summary>
        /// <returns></returns>
        public ParamEditorBase ResolveEditor(PropItemAttribute p)
        {
            ParamEditorBase pType = null;
            Type editorType = p.EditorType;
            // 通过类型解析
            if (editorType == null)
            {
                editorType = p.Type;
            }

            // 如果是值类型
            if (editorType == typeof(bool))
            {
                pType = new SwitchEditor();
            }
            if (editorType.IsNumeric())
            {
                pType = new NumberEditor(p.MinV, p.MaxV, p.DecimalPlaces);
            }
            else if (editorType.IsEnum || editorType == typeof(LItems))
            {
                pType = new EnumEditor();
            }
            else if (editorType == typeof(string))
            {
                if (p.IsReadOnly)
                {
                    pType = new ReadOnlyTextEditor();
                }
                else
                {
                    pType = new PlainTextEditor();
                }
            }
            else if (editorType == typeof(LPath))
            {
                pType = new PathEditor();
            }
            else
            {
                pType = CustomType(editorType);

            }

            if (pType == null)
            {
                throw new NotImplementedException(editorType.Name);
            }

            return pType;
        }

        public virtual ParamEditorBase CustomType(Type type)
        {
            return null;
        }
    }
}