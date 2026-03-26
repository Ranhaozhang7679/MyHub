#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VSimulation
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.VDevice
* 文 件 名:       VSimulation.cs
* 创建时间:       2022/7/15 17:31:28
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      11b2a94f-6761-460a-be14-16e8da701641
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/15 17:31:28
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct
{
    /// <summary>
    /// 基于IO构建的仿真设备
    /// </summary>
    public class VIOSimulation : VirtualDeviceBase, IHome, IDeviceShield, IDeviceError,IAutoCheck
    {
        /// <summary>
        /// 顺序
        /// </summary>
        public override int Sort => 10;

        public override DeviceError[] ErrorCodes => new DeviceError[]
        {
            DeviceError.SensorFail
        };
        /// <summary>
        /// 动作
        /// </summary>
        /// 
        private List<SimAction> _actions;

        /// <summary>
        /// 点检事件
        /// </summary>
        public event Func<string, bool, bool> OnCheckEvent;

        [Ignore]
        public List<SimAction> Actions
        {
            get => _actions; set
            {
                var src = _actions;
                _actions = value;
                if (src != _actions)
                {
                    OnPropertyChanged(nameof(Actions), src, value);
                }
            }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public VIOSimulation()
        {
            Actions = new List<SimAction>();
        }

        /// <summary>
        /// 获取所有引用对象
        /// </summary>
        /// <returns></returns>
        public override List<IVirtualDevice> GetRefObjs()
        {
            List<IVirtualDevice> refObjs = new List<IVirtualDevice>();
            if (Actions.Count == 0) return refObjs;

            foreach (var item in Actions)
            {
                item.IOActions.ForEach(u => refObjs.Add(u.IO));
            }

            return refObjs;
        }

        /// <summary>
        /// 动作执行
        /// </summary>
        /// <param name="action"></param>
        public void Action(string action, int interval = 10)
        {
            if (GetIsShield()) return;

            var simAction = Actions.FirstOrDefault(u => u.Action == action);
            if (simAction != null)
            {
                simAction.Excute(interval);
            }
        }

        /// <summary>
        /// 行为检测
        /// </summary>
        /// <param name="action"></param>
        public void Check(string checkAction, int timeout)
        {
            if (!GetIsShield())
            {
                var simAction = Actions.FirstOrDefault(u => u.Action == checkAction && u.IsCheck);
                if (simAction != null)
                {
                    var ios = simAction.IOActions.Select(u => u.IO).ToArray();
                    var result = simAction.IOActions.Select(u => u.Value).ToArray();

                    WaitDiagital(timeout, ios.ToArray(), result.ToArray(), () =>
                    {
                        throw new DeviceTimeoutException(Errors[Enums.DeviceError.SensorFail], 
                            this.ID, $"Action:{checkAction} timeout>{timeout}!",this.Module);
                    });
                }
            }
            else
            {
                System.Threading.Thread.Sleep(shieldSleep);
            }
        }

        /// <summary>
        /// 回零
        /// </summary>
        public override void Home()
        {
            if (NeedHome)
            {
                foreach (var item in Actions)
                {
                    if (item.IsHome)
                    {
                        Action(item.Action);

                        // 检查回零成功

                    }
                }
            }
        }

        #region 导入导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            var xRoot = base.ExportXml();
            foreach (var item in Actions)
            {
                xRoot.Add(item.ExportXml());
            }

            return xRoot;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);
            foreach (var xItem in xElement.Elements("Action"))
            {
                var newAction = new SimAction();
                newAction.ParserXml(xItem, Engine);
                Actions.Add(newAction);
            }
        }


        #endregion

        #region 属性对象
        public object[] GetPropDatas()
        {
            return Actions.Select(u => u.Action).ToArray();
        }

        /// <summary>
        /// 点检方法
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Check()
        {
            bool isOK=false;
            isOK = OnCheckEvent($"即将点检设备:{Name}",true);
            if(!isOK) return false;

            foreach(var item in Actions)
            {
                ///只点检输出动作
                if (!item.IsCheck)
                {
                    isOK = OnCheckEvent($"即将点检设备:{Name} 动作:{item.Action}", true);
                    if(!isOK) return false;
                    Action(item.Action);
                    isOK = OnCheckEvent($"确认设备:{Name} 动作:{item.Action}-动作正确", true);
                    if (!isOK) return false; 
                }
            }
            return isOK;
        }

        public string DisplayPath => "";

        public string ValuePath { get; set; } = "";
        #endregion
    }

    /// <summary>
    /// 仿真动作
    /// </summary>
    public class SimAction
    {
        /// <summary>
        /// 动作名称
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 是否回零
        /// </summary>
        public bool IsHome { get; set; }

        /// <summary>
        /// 对象检知
        /// </summary>
        public bool IsCheck { get; set; }

        /// <summary>
        /// 动作包含的IO
        /// </summary>
        public List<IOAction> IOActions { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SimAction()
        {
            IOActions = new List<IOAction>();
        }

        /// <summary>
        /// 动作
        /// </summary>
        public void Excute(int interval = 10)
        {
            foreach (var item in IOActions.OrderBy(u => u.Sort))
            {
                item.IO.SetDigital(item.Value);

                // 添加间隔
                Thread.Sleep(interval);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xAction = new XElement("Action");
            xAction.SetAttributeValue("Action", Action);
            xAction.SetAttributeValue("IsHome", IsHome);
            xAction.SetAttributeValue("IsCheck", IsCheck);
            foreach (var item in IOActions)
            {
                xAction.Add(item.ExportXml());
            }

            return xAction;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">xml对象</param>
        /// <param name="device"></param>
        public void ParserXml(XElement xElement, IDeviceEngine device)
        {
            // 动作
            xElement.GetAttribute("Action", (ac) =>
            {
                Action = ac;
            });

            // 是否回零
            xElement.GetAttribute("IsHome", (v) =>
            {
                if (bool.TryParse(v, out var value))
                {
                    IsHome = value;
                }
            });

            // 是否检知
            xElement.GetAttribute("IsCheck", (v) =>
            {
                if (bool.TryParse(v, out var value))
                {
                    IsCheck = value;
                }
            });

            foreach (var xItem in xElement.Elements())
            {
                var newAction = new IOAction();
                newAction.ParserXml(xItem, device);
                IOActions.Add(newAction);
            }
        }
    }

    /// <summary>
    /// IO 行为
    /// </summary>
    public class IOAction
    {
        /// <summary>
        /// IO对象
        /// </summary>
        public VIO IO { get; set; }

        /// <summary>
        /// 值类型
        /// </summary>
        public bool Value { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xAction = new XElement("IOAction");
            xAction.SetAttributeValue("ID", IO.ID);
            xAction.SetAttributeValue("Value", Value);
            xAction.SetAttributeValue("Sort", Sort);
            return xAction;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement">xml对象</param>
        /// <param name="device"></param>
        public void ParserXml(XElement xElement, IDeviceEngine device)
        {
            xElement.GetAttribute("ID", (id) =>
            {
                Guid ioId = Guid.Parse(id);
                IO = device.GetVirtualByID(ioId) as VIO;
            });

            xElement.GetAttribute("Value", (v) =>
            {
                if (bool.TryParse(v, out var value))
                {
                    Value = value;
                }
            });

            xElement.GetAttribute("Sort", (s) =>
            {
                if (int.TryParse(s, out var value))
                {
                    Sort = value;
                }
            });
        }
    }
}