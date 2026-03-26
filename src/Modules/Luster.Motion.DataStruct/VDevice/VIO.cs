#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VIO
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Virtual
* 文 件 名:       VIO.cs
* 创建时间:       2022/4/6 14:45:17
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      35511b2f-9a33-4d41-bbce-d7d752cea3c2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/6 14:45:17
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.FXVirtual;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// IO 信号
    /// </summary>
    public class VIO : VirtualDeviceBase, IDeviceError
    {
        public override int Sort => 1;

        /// <summary>
        /// 隶属卡编号
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// 对应的索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 对应的子定义
        /// </summary>
        public string SubDefinite { get; set; }

        /// <summary>
        /// IO 类别 属于数字信号还是模拟信号
        /// </summary>
        public IOType IOType { get; set; }

        /// <summary>
        /// 行为 属于输入还是输出
        /// </summary>
        public IOBehavior Behavior { get; set; }

        /// <summary>
        /// IO监控
        /// </summary>
        public IOMonitor Montor { get; set; }

        /// <summary>
        /// 当前值
        /// </summary>
        [Ignore]
        public double Value { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public double DefaultValue { get; set; }

        /// <summary>
        /// 表达式计算器
        /// </summary>
        private string anglogExp = string.Empty;
        [Ignore]
        public string AnglogExp
        {
            get
            {
                return anglogExp;
            }
            set
            {
                if (expressTool != null)
                {
                    expressTool = null;
                }

                anglogExp = value;
            }
        }

        /// <summary>
        /// 当前设备
        /// </summary>
        private IMotionCard motionCard;

        public override DeviceError[] ErrorCodes
 => new DeviceError[]
 {
DeviceError.SensorFail
 };

        /// <summary>
        /// 模拟输入值
        /// </summary>
        /// <param name="value"></param>
        public void SetIOValue(double value)
        {
            double srcV = Value;

            // 更新值
            Value = value;
        }

        /// <summary>
        /// 设备初始化
        /// </summary>
        /// <param name="hardDevice"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool SetDevice(IDevice hardDevice, out string errMsg)
        {
            base.SetDevice(hardDevice, out errMsg);
            motionCard = hardDevice as IMotionCard;
            motionCard = new FXVirtualMotionCardAspect(motionCard, Engine);
            return true;
        }

        /// <summary>
        /// 获取当前开关
        /// </summary>
        /// <returns></returns>
        public bool GetDigitalIn()
        {
            bool isOn = false;
            ProcessAction(() =>
            {
                isOn = motionCard.GetDigitalIn(Index);
            }, () =>
            {
                isOn = Value > 0;
            });

            return isOn;
        }

        public bool GetDigitalOut()
        {
            bool isOn = false;
            ProcessAction(() =>
            {
                isOn = motionCard.GetDigitalOut(Index);
            }, () =>
            {
                isOn = Value > 0;
            });

            return isOn;
        }

        /// <summary>
        /// 获取IO数字信号的值
        /// </summary>
        /// <returns></returns>
        public bool GetDigital()
        {
            if (Behavior == Luster.Motion.DataStruct.Enums.IOBehavior.Input)
            {
                return GetDigitalIn();
            }
            else
            {
                return GetDigitalOut();
            }
        }

        public void SetDigital(bool isOn)
        {
            ProcessAction(() =>
            {
                if (Behavior == IOBehavior.Output)
                {
                    motionCard.SetDigitalOut(Index, isOn);
                }
            },
            () =>
            {
                SetIOValue(isOn ? 1 : 0);
            });
        }

        /// <summary>
        /// 表达式计算器
        /// </summary>
        private ExpressTool expressTool = null;

        public double GetAnglogIn()
        {
            double val = Value;
            ProcessAction(() =>
            {
                val = motionCard.GetAnalogIn(Index);
            });

            return val;
        }

        /// <summary>
        /// 表达式计算器
        /// </summary>
        /// <param name="val"></param>
        private void Calculate(ref double val)
        {
            if (!string.IsNullOrEmpty(AnglogExp))
            {
                if (expressTool == null)
                {
                    expressTool = new ExpressTool(AnglogExp);
                }

                val = expressTool.Calulate(new Dictionary<string, object>()
                                {
                                    {"Value",val }
                                });
            }
        }

        /// <summary>
        /// 清理表达式
        /// </summary>
        public void ClearExp()
        {
            expressTool = null;
            AnglogExp = string.Empty;
        }

        public double GetAnglogOut()
        {
            double val = Value;
            ProcessAction(() =>
            {
                val = motionCard.GetAnalogOut(Index);
            });

            return val;
        }

        public double GetAnalogExpVal()
        {
            if (Behavior == IOBehavior.Input)
            {
                var val = GetAnglogIn();
                Calculate(ref val);

                return val;
            }
            else
            {
                var val = GetAnglogOut();
                Calculate(ref val);
                return val;
            }
        }

        public void SetAnglog(double val)
        {
            ProcessAction(() =>
            {
                if (Behavior == IOBehavior.Output)
                {
                    motionCard.SetAnalogOut(Index, val);
                }
            },
            () =>
            {
                SetIOValue(val);
            });
        }

        /// <summary>
        /// 等待IO
        /// </summary>
        /// <param name="isOn"></param>
        /// <param name="timeout">如果值为-1，代表持续的等待信号</param>
        /// <returns></returns>
        public void WaitIO<T>(T val, int timeout, Action timeAction = null)
        {
            ProcessAction(() =>
            {
                Type type = typeof(T);

                if (type == typeof(bool))
                {
                    CalcTime(() =>
                     {
                         bool getVal = false;
                         if (Behavior == IOBehavior.Input)
                         {
                             getVal = motionCard.GetDigitalIn(Index);
                         }
                         else
                         {
                             getVal = motionCard.GetDigitalOut(Index);
                         }

                         // 如果值返回和判断值相同,那么就中断
                         return getVal.Equals(val);
                     }, timeout, 5, timeAction);
                }
                else
                {
                    CalcTime(() =>
                    {
                        double getVal = 0;
                        if (Behavior == IOBehavior.Input)
                        {
                            getVal = motionCard.GetAnalogIn(Index);
                        }
                        else
                        {
                            getVal = motionCard.GetAnalogOut(Index);
                        }

                        // 如果值返回和判断值相同,那么就中断
                        return getVal.Equals(val);
                    }, timeout, 5, timeAction);
                }
            },
            () =>
            {
                Type type = typeof(T);
                if (type == typeof(bool))
                {
                    CalcTime(() =>
                    {
                        bool getVal = Value >= 1;
                        // 如果值返回和判断值相同,那么就中断
                        return getVal.Equals(val);
                    }, timeout, 5, timeAction);
                }
                else
                {
                    CalcTime(() =>
                    {
                        double getVal = Value;
                        // 如果值返回和判断值相同,那么就中断
                        return getVal.Equals(val);
                    }, timeout, 5, timeAction);
                }
            });
        }

        public override XElement ExportXml()
        {
            var xml = base.ExportXml();

            if (!string.IsNullOrEmpty(AnglogExp))
            {
                xml.Add(new XElement(nameof(AnglogExp), AnglogExp));
            }

            if (!string.IsNullOrEmpty(SubDefinite))
            {
                xml.Add(new XElement(nameof(SubDefinite), SubDefinite));
            }

            return xml;
        }

        public override void ParserXml(XElement xElement)
        {
            base.ParserXml(xElement);

            // 获取模拟量
            foreach (var item in xElement.Elements())
            {
                if (item.Name.LocalName == nameof(AnglogExp))
                {
                    AnglogExp = item.Value;
                }

                if (item.Name.LocalName == nameof(SubDefinite))
                {
                    SubDefinite = item.Value;
                }
            }
        }
    }
}