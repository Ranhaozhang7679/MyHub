#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionFunction
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion
* 文 件 名:       MotionFunction.cs
* 创建时间:       2022/5/19 18:13:28
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      62170af4-a7b9-4a22-9f3b-e7ac36bbd33d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/19 18:13:28
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Motion
{
    /// <summary>
    /// 运动函数
    /// </summary>
    public class MotionFunction : Function, IMotionFunction, INote
    {
        /// <summary>
        /// 覆盖父类的Owner对象
        /// </summary>
        private IMotionModule motionModule;
        public override IModule Owner
        {
            get => motionModule; set
            {
                motionModule = value as IMotionModule;

                // 初始化参数
                InitParameters();

                // 初始化
                Init();
            }
        }

        /// <summary>
        /// 对象初始化
        /// </summary>
        protected virtual void Init()
        {
        }

        /// <summary>
        /// 为了简写
        /// </summary>
        protected IMotionModule MyOwner => motionModule;

        #region 1.受保护方法
        /// <summary>
        /// 报警信息
        /// </summary>
        /// <param name="msg"></param>
        protected void OnAlarm(AlarmType alarmType, string msg, string code = "")
        {
            MyOwner.OnAlarm(alarmType, msg, code);
        }

        /// <summary>
        /// 计算暂停耗时
        /// </summary>
        private Stopwatch _pSw = new Stopwatch();
        /// <summary>
        /// 等待方法
        /// </summary>
        protected void WaitFunc(Func<bool> waitFunc, string statusMsg, int spleep = 20, int timeout = -1, Action timeFun = null, Action success = null)
        {
            int time = 0;

            // 1.如果执行一次直接成功，那么不进行while循环了
            if (waitFunc())
            {
                return;
            }

            MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"Module {MyOwner.Alias} {statusMsg} Start Waiting....");

            while (true)
            {
                // 如果暂停，则中断循环
                if (!MyOwner.BrokenOff.IsSet)
                {
                    _pSw.Restart();

                    // 不能中断线程，否则程序无法监控
                    //MyOwner.BrokenOff.Wait();
                }

                if (MyOwner.BrokenOff.IsSet && _pSw.IsRunning)
                {
                    // 计算暂停耗时
                    _pSw.Stop();
                    MyOwner.PauseTime = _pSw.ElapsedMilliseconds;
                }

                // 模块回零，需要终止流程
                if (MyOwner.IsBreak)
                {
                    MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Info, $"Module {MyOwner.Alias} Home,{statusMsg} Break!");
                    break;
                }

                // 检查方法
                if (waitFunc())
                {
                    success?.Invoke();
                    MyOwner.StatusMsg = "";
                    break;
                }

                // 记录当前状态
                MyOwner.StatusMsg = statusMsg;

                // 更节省CPU资源
                SpinWait.SpinUntil(() => false, spleep);
                //Thread.Sleep(spleep);
                time += spleep;

                if (timeout > -1 && time > timeout)
                {
                    if (timeFun != null)
                    {
                        timeFun();
                        return;
                    }
                    else
                    {
                        throw new DeviceTimeoutException("N03OOOO-01@Wait timeout", $"WaitFunc等待超时");
                    }
                }
            }

            // 如果是正常完成记录等待结果
            if (!MyOwner.IsBreak)
            {
                MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"Module {MyOwner.Alias} {statusMsg} Waiting End,Time:{time} ms");
            }
        }

        /// <summary>
        /// 获取设备对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="device"></param>
        /// <param name="outDevice"></param>
        /// <exception cref="FriendlyException"></exception>
        protected void GetVDevice<T>(VDevice device, out T outDevice) where T : IVirtualDevice
        {
            if (device == null)
            {
                outDevice = default;
                return;
            }

            // 如果对象已经获取过了，那么就不需要重复获取
            if (device.Virtual != null)
            {
                outDevice = (T)device.Virtual;
                return;
            }

            var vDevice = MyOwner.DeviceEngine.GetVirtualByID(device.DeviceID);
            if (vDevice == null)
            {
                throw new FriendlyException($"Device:{device.Name} is not existed!");
            }

            // 缓存已经查询对象
            device.Virtual = vDevice;

            // 结果对象
            outDevice = (T)vDevice;
        }

        #endregion

        #region 多轴动态参数
        private string GetExternKey(AxisItem aItem, ParamType pType = ParamType.IN)
        {
            if (pType == ParamType.IN)
            {
                // 字符串不能包含字符 -
                return $"Extern_{aItem.AxisID.ToString().Replace("-", "_")}";
            }
            else
            {
                // 字符串不能包含字符 -
                return $"Extern_Out_{aItem.AxisID.ToString().Replace("-", "_")}";
            }
        }

        private string GetExternKey(AxisPosItem aItem, ParamType pType = ParamType.IN)
        {
            if (pType == ParamType.IN)
            {
                // 字符串不能包含字符 -
                return $"Extern_{aItem.PosKey.ToString().Replace("-", "_")}";
            }
            else
            {
                // 字符串不能包含字符 -
                return $"Extern_Out_{aItem.PosKey.ToString().Replace("-", "_")}";
            }
        }

        /// <summary>
        /// 配置补偿值
        /// </summary>
        /// <param name="vAxisM"></param>
        protected void SetDynamicAxisM(VAxisMDevice vAxisM, ParamType pType = ParamType.IN)
        {
            // 获取动态补偿
            foreach (var item in vAxisM.Items)
            {
                string key = GetExternKey(item, pType);

                if (MyOwner.Parameters.ContainsKey(key))
                {
                    item.Compensate = (double)GetParameterVal(key);
                }
            }
        }

        protected void SetOutDynamicAxisM(VAxisMDevice vAxisM)
        {
            foreach (var item in vAxisM.Items)
            {
                string key = GetExternKey(item, ParamType.OUT);
                if (MyOwner.Parameters.ContainsKey(key))
                {
                    MyOwner.Parameters[key].Value = item.Axis.GetCurrentPos();
                }
            }
        }

        /// <summary>
        /// 构建动态轴=
        /// </summary>
        /// <param name="vDevice">设备</param>
        protected void BuildDynamicAxisM(VAxisMDevice vDevice, int startSort = 10, ParamType paramType = ParamType.IN, bool isClearHis = true)
        {
            // 1.记录当前已有对象
            List<string> noProp = new List<string>();

            // 2.多轴时，既要添加输入参数又要添加输出参数，所以需要
            if (isClearHis)
            {
                foreach (var item in MyOwner.Parameters)
                {
                    if (item.Value.Property == null)
                    {
                        noProp.Add(item.Key);
                    }
                }
            }


            // 2.动态构建参数
            foreach (var item in vDevice.Items)
            {
                string key = GetExternKey(item, paramType);
                if (noProp.Contains(key))
                {
                    noProp.Remove(key);
                }

                if (item.Axis != null && item.Axis.AxisType != AxisType.None)
                {
                    if (!MyOwner.Parameters.ContainsKey(key))
                    {
                        var param = ParameterAttribute.CreateByType(key, typeof(double), MyOwner, item.Axis.Name, startSort, paramType);
                        param.CanRef = paramType == ParamType.IN ? ParamRef.Ref : ParamRef.None;
                        param.Tips = item.Axis.Name;
                        MyOwner.Parameters.Add(key, param);
                    }

                    startSort++;
                }
            }

            foreach (var item in noProp)
            {
                MyOwner.Parameters.Remove(item);
            }

            // 参数数量发生了变换
            MyOwner.OnUpdate(ModuleUpdate.ParameterNum);
        }
        /// <summary>
        /// 构建动态轴=
        /// </summary>
        /// <param name="vDevice">设备</param>
        protected void BuildDynamicAxisPos(VAxisPos vAxisPos, int startSort = 10, ParamType paramType = ParamType.IN, bool isClearHis = true)
        {
            // 1.记录当前已有对象
            List<string> noProp = new List<string>();

            // 2.多轴时，既要添加输入参数又要添加输出参数，所以需要
            if (isClearHis)
            {
                foreach (var item in MyOwner.Parameters)
                {
                    if (item.Value.Property == null)
                    {
                        noProp.Add(item.Key);
                    }
                }
            }


            // 2.动态构建参数
            foreach (var item in vAxisPos)
            {
                string key = GetExternKey(item, paramType);
                if (noProp.Contains(key))
                {
                    noProp.Remove(key);
                }

                if (item.AxisPostion != null)
                {
                    if (!MyOwner.Parameters.ContainsKey(key))
                    {
                        var param = ParameterAttribute.CreateByType(key, typeof(double), MyOwner, item.AxisPostion.Name, startSort, paramType);
                        param.CanRef = paramType == ParamType.IN ? ParamRef.Ref : ParamRef.None;
                        param.Tips = item.AxisPostion.Name;
                        param.Value = 0;
                        MyOwner.Parameters.Add(key, param);
                    }

                    startSort++;
                }
            }

            foreach (var item in noProp)
            {
                MyOwner.Parameters.Remove(item);
            }

            // 参数数量发生了变换
            MyOwner.OnUpdate(ModuleUpdate.ParameterNum);
        }

        /// <summary>
        /// 配置补偿值
        /// </summary>
        /// <param name="vAxisM"></param>
        protected void SetDynamicAxisPos(VAxisPos vAxisPos, ParamType pType = ParamType.IN)
        {
            // 获取动态补偿
            foreach (var item in vAxisPos)
            {
                string key = GetExternKey(item, pType);

                if (MyOwner.Parameters.ContainsKey(key))
                {
                    item.Compensate = (double)GetParameterVal(key);
                }
            }
        }

        protected void SetOutDynamicAxisPos(VAxisPos vAxisPos)
        {
            foreach (var item in vAxisPos)
            {
                string key = GetExternKey(item, ParamType.OUT);
                if (MyOwner.Parameters.ContainsKey(key))
                {
                    MyOwner.Parameters[key].Value = item.Axis.GetCurrentPos();
                }
            }
        }
        /// <summary>
        /// 更新引用参数的值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        protected void SetParameterRefVal(string name, object val)
        {
            if (!MyOwner.Parameters.ContainsKey(name))
            {
                throw new FriendlyException($"变量:{name}不存在!");
            }

            var refAttr = MyOwner.Parameters[name].RefOut;
            if (refAttr == null)
            {
                throw new FriendlyException($"变量:{name} 未引用参数!");
            }

            refAttr.Value = val;

            // 更新属性值
            if (refAttr.Property != null)
            {
                refAttr.Property.SetValue(refAttr.Owner.TaskFunction, val);
            }
        }

        /// <summary>
        /// 空跑模式
        /// </summary>
        protected bool IsEmptyMode => MyOwner.DeviceEngine.DeviceMode == DeviceMode.Empty;
        #endregion

        #region 导入和导出
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="xFunc"></param>
        public override void ParserXml(XElement xFunc)
        {
            base.ParserXml(xFunc);
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public override XElement ExportXml()
        {
            XElement element = base.ExportXml();
            return element;
        }
        #endregion

        /// <summary>
        /// 被添加到模块中
        /// </summary>
        public virtual void Added()
        {
        }

        /// <summary>
        /// 数据传递
        /// </summary>
        /// <param name="refParameter"></param>
        protected void DataProcess(IMotionModule srcModule, IMotionModule dstModule, DataTransType tranType = DataTransType.None, int productCount = 1)
        {
            // 如果无需数据转移
            if (tranType == DataTransType.None) return;

            if (srcModule.Station == null)
            {
                srcModule.UpdateStation();
            }

            // 当前工站
            if (srcModule.Station.TaskFunction is not IStation curStation) return;

            // 目标工站
            IStation dstStation = null;
            if (dstModule != null)
            {
                dstModule.UpdateStation();
                dstModule = dstModule.Station;
                dstStation = dstModule.TaskFunction as IStation;
            }

            if (tranType == DataTransType.Get)
            {
                // 支持同时获取多个数据
                for (int i = 0; i < productCount; i++)
                {
                    if (dstStation.TryDequeue(out string dataID))
                    {
                        curStation.Enqueue(dataID);
                    }

                    MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"SN转移:原工站:{dstStation.Station}->目标站:{curStation.Station} 转移SN:{dataID}");

                    // 更新对应的数据ID
                    if (curStation.TryPeek(out dataID))
                    {
                        srcModule.Station.DataID = dataID;
                    }
                    else
                    {
                        srcModule.Station.DataID = "";
                    }
                }

                // 尝试从目标站中获取数据
                CopyToData(dstStation, curStation);
            }
            else
            {
                // 尝试从原始工站获取
                CopyToData(curStation, dstStation);
            }
        }

        /// <summary>
        /// 将一个工站数据转移到另外一个工站数据集合中
        /// </summary>
        /// <param name="srcStation">当前工站</param>
        /// <param name="dstStation">转移工站</param>
        private void CopyToData(IStation srcStation, IStation dstStation)
        {
            if (srcStation.Datas.Count > 0)
            {
                lock (lockList)
                {
                    // 应用 ToList() 快照
                    var snapshot = srcStation.Datas.ToList();
                    foreach (var item in snapshot)
                    {
                        if(!dstStation.Datas.Contains(item))
                            dstStation.Datas.Add(item);
                    }
                    //foreach (var item in srcStation.Datas)
                    //{
                    //    if (!dstStation.Datas.Any(u => u == item))
                    //    {
                    //        dstStation.Datas.Add(item);
                    //    }
                    //}

                    // 清理原始工站数据
                    srcStation.Datas.Clear();
                }
            }
        }

        /// <summary>
        /// 对List集合进行锁
        /// </summary>
        private static object lockList = new object();

        #region 注释相关
        /// <summary>
        /// 注释变更
        /// </summary>
        public event Action<string> NoteChangedEvent;

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);

            if (!parameter.IsSensitive)
            {
                return;
            }

            if (parameter.Owner is IGlobal) return;

            //Jack20230208
            if (parameter.IsBindUI && MyOwner != null)
            {
                parameter.Value = newV;
                try
                {
                    MyOwner.ModuleValid();
                }
                catch (Exception e)
                {
                    MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, e.Message);
                }
            }

            // 参数变更，要编辑引用模块
            if (parameter.IsBindUI && parameter.Owner.TaskFunction is IRefFunction refModule)
            {
                parameter.Owner.UpdateReferences();
                //foreach (var r in refModule.GetRefModule())
                //{
                //    var refM = MyOwner.TaskModules[r.Key] as IMotionModule;
                //    if (refM != null)
                //    {
                //        refM.RegisterRef(MyOwner, r.Value);
                //    }
                //}
            }

            // 自动注释
            if (this is INote m && m.NoteParams != null && m.NoteParams?.Length > 0 && m.NoteParams.Contains(parameter.Name))
            {
                string note = GetNote(m);
                NoteChangedEvent?.Invoke(note);
            }
        }

        /// <summary>
        /// 构建注释方法
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public virtual string GetNote(INote note)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in note.NoteParams)
            {
                if (MyOwner.Parameters.ContainsKey(item))
                {
                    var p = MyOwner.Parameters[item];
                    if (p.Type.IsEnum)
                    {
                        sb.Append($"{p.Value.GetDescription()}_");
                    }
                    else if (p.RefOut != null)
                    {
                        sb.Append($"{p.RefOut.Alias}_");
                    }
                    else
                    {
                        sb.Append($"{p.Value}_");
                    }
                }
                else
                {
                    sb.Append($"{item}_");
                }
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 自动注释
        /// </summary>
        public virtual string[] NoteParams
        {
            get;
            set;
        }

        #endregion

        #region 运控控制相关函数
        /// <summary>
        /// 回零
        /// </summary>
        public virtual void Home()
        {
            // 自动调用回零功能
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public virtual void Pause()
        {
            foreach (var item in MyOwner.Parameters)
            {
                var pAttr = item.Value;
                if (pAttr.ParamType == ParamType.OUT) continue;

                var parameter = item.Value;
                if (parameter.Value is VDevice v)
                {
                    var vDevice = MyOwner.DeviceEngine.GetVirtualByID(v.DeviceID);
                    if (vDevice is IPause s)
                    {
                        s.Pause();
                    }
                }
                else if (parameter.Value is IPause p)
                {
                    p.Pause();
                }
            }
        }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public virtual bool IsNeedPause { get; } = false;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime => MyOwner.StartTime;

        /// <summary>
        /// 停止
        /// </summary>
        public virtual void Stop()
        {
            foreach (var item in MyOwner.Parameters)
            {
                var pAttr = item.Value;
                if (pAttr.ParamType == ParamType.OUT) continue;

                if (item.Value.Value is VDevice v)
                {
                    var vDevice = MyOwner.DeviceEngine.GetVirtualByID(v.DeviceID);
                    if (vDevice is IStop s)
                    {
                        s.Stop();
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 获取表达式引用
        /// </summary>
        /// <param name="expressPropName"></param>
        /// <returns></returns>
        public virtual List<LReference> GetReferences(string expressPropName)
        {
            List<LReference> references = new List<LReference>();

            if (MyOwner.Parameters.ContainsKey(expressPropName))
            {
                var value = MyOwner.Parameters[expressPropName].Value;
                if (value == null || value is not LExpression lExp) return references;

                foreach (var vItem in lExp.Variables)
                {
                    references.Add(new LReference() { RefID = vItem.ID, RefName = vItem.ParamKey, ByRefName = expressPropName });
                }
            }

            return references;
        }
    }
}