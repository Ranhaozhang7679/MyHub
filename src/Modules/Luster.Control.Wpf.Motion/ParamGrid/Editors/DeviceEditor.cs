#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceEditor
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.ParamGrid.Editors
* 文 件 名:       DeviceEditor.cs
* 创建时间:       2022/6/8 11:10:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      391bcb0d-c6b1-456c-9825-fb30cca46905
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/8 11:10:29
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Control.Wpf.Motion.Controls;
using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class DeviceEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            var device = new DeviceCtrl();
            device.IsReadOnly = propertyItem.IsReadOnly;
            device.Parameter = propertyItem.Value as ParameterAttribute;
            device.ValueChanged += Device_ValueChanged;
            return device;
        }

        private void Device_ValueChanged(ParameterAttribute obj)
        {
            OnParamValueChanged(obj);
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return DeviceCtrl.ValueProperty;
        }
    }

    public class SlaveEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            var slave = new SlaveCtrl();
            slave.IsReadOnly = propertyItem.IsReadOnly;
            slave.Parameter = propertyItem.Value as ParameterAttribute;
            // slave.ValueChanged += Device_ValueChanged;
            return slave;
        }

        private void Device_ValueChanged(ParameterAttribute obj)
        {
            OnParamValueChanged(obj);
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return SlaveCtrl.ValueProperty;
        }
    }

    public class AlarmEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            var alarm = new AlarmCtrl();
            alarm.IsReadOnly = propertyItem.IsReadOnly;
            alarm.Parameter = propertyItem.Value as ParameterAttribute;
            // alarm.ValueChanged += Device_ValueChanged
            propertyItem.ParamType = "Normal";
            return alarm;
        }

        private void Device_ValueChanged(ParameterAttribute obj)
        {
            OnParamValueChanged(obj);
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return AlarmCtrl.ValueProperty;
        }
    }
}