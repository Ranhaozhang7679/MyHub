#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NetworkEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.ParamGrid.Editors
* 文 件 名:       NetworkEditor.cs
* 创建时间:       2022/7/5 13:00:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ae48acf3-f46d-4758-a2a2-e81935bcc1cd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/5 13:00:00
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Control.Wpf.Motion.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class NetworkEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            return new NetworkCtrl();
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return NetworkCtrl.NetworkProperty;
        }
    }
}