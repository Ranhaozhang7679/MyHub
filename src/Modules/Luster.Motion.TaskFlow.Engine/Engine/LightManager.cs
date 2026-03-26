using Luster.Common.Tools.SharedMemory;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Engine
{
    /// <summary>
    /// 光源控制器
    /// </summary>
    public class LightManager : ILightManager
    {
        /// <summary>
        /// 配置
        /// </summary>
        private SystemConfig SysConfig = null;

        /// <summary>
        /// 设备管理器
        /// </summary>
        private IDeviceEngine _deviceEngine;

        public void SetDeviceEngine(IDeviceEngine deviceEngine)
        {
            _deviceEngine = deviceEngine;
            vPlc = _deviceEngine.GetVDevices<VPlc>().FirstOrDefault();
        }
        /// <summary>
        /// 控制蜂鸣器
        /// </summary>
        private CancellationTokenSource buzzerSource = null;

        /// <summary>
        /// 控制按钮灯频闪
        /// </summary>
        private CancellationTokenSource lightSource = null;



        /// <summary>
        /// 三色灯红灯频闪
        /// </summary>
        private CancellationTokenSource triRedLightSource = null;
        /// <summary>
        /// 三色灯黄灯频闪
        /// </summary>
        private CancellationTokenSource triYellowLightSource = null;

        /// <summary>
        /// PLC对象
        /// </summary>
        private VPlc vPlc = null;

        public LightManager(IDeviceEngine deviceEngine)
        {
            _deviceEngine = deviceEngine;
            vPlc = _deviceEngine.GetVDevices<VPlc>().FirstOrDefault();
        }

        /// <summary>
        /// 配置参数对象
        /// </summary>
        /// <param name="sysConfig"></param>
        public void SetSysConfig(object sysConfig)
        {
            SysConfig = sysConfig as SystemConfig;
        }


        private void OnLog(string msg)
        {
            _deviceEngine.OnLog(Common.DataStruct.Enums.LogType.Debug, msg);
        }

        /// <summary>
        /// IO 通用处理
        /// </summary>
        /// <param name="io"></param>
        private void ProcessIO(VDevice vDevice, Action<VIO> ioAction)
        {
            vPlc = _deviceEngine.GetVDevices<VPlc>().FirstOrDefault();
            // 启动按钮
            if (vDevice != null && !vDevice.IsEmpty())
            {
                var io = vDevice.GetVDevice<VIO>(_deviceEngine);
                ioAction(io);
            }
        }

        /// <summary>
        /// 按钮灯控制
        /// </summary>
        /// <param name="isOn"></param>
        private void SetButtonLight(bool isOn)
        {
            ProcessIO(SysConfig.LightStart, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.LightStop, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.LightPause, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.LightRecovery, io => io.SetDigital(isOn));

            // 关闭闪烁按钮灯
            WaitEndTask(ref lightSource);
        }

        /// <summary>
        /// 三色灯控制
        /// </summary>
        /// <param name="isOn"></param>
        private void ResetTricolorLight(bool isOn)
        {
            //保留单一功能
            isOn = false;
            //复位黄灯闪烁
            WaitEndTask(ref triYellowLightSource);
            //复位红灯闪烁
            WaitEndTask(ref triRedLightSource);
            ProcessIO(SysConfig.LightYellow, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.LightGreen, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.LightRed, io => io.SetDigital(isOn));
        }

        /// <summary>
        /// IO闪烁灯
        /// </summary>
        /// <param name="io"></param>
        private void Blink(VIO io, CancellationTokenSource tokenSource, int OnTime = 200, int OffTime = 200)
        {
            int interval = 50;
            Task.Run(() =>
            {
                bool isBlink = true;
                while (isBlink)
                {
                    // 先开启
                    io.SetDigital(true);

                    for (int i = 0; i < OnTime; i += interval)
                    {
                        Thread.Sleep(interval);

                        if (tokenSource.IsCancellationRequested)
                        {
                            isBlink = false;
                            break;
                        }
                    }


                    // 在关闭
                    io.SetDigital(false);
                    for (int i = 0; i < OffTime; i += interval)
                    {
                        Thread.Sleep(interval);
                        if (tokenSource.IsCancellationRequested)
                        {
                            isBlink = false;
                            break;
                        }
                    }
                }
            }, tokenSource.Token);
        }

        /// <summary>
        /// 确保闪烁灯一定被终止掉
        /// </summary>
        /// <param name="token"></param>
        private void WaitEndTask(ref CancellationTokenSource token)
        {
            lock (lockObj)
            {
                token?.Cancel();

                // 确保线程被结束掉
                Thread.Sleep(150);

                // 创建新的线程信号量
                token = new CancellationTokenSource();
            }
        }

        private static object lockObj = new object();

        /// <summary>
        /// 蜂鸣器控制
        /// </summary>
        public void SetBuzzer(bool isOn, int OnTime = 150, int OffTime = 150)
        {
            // 如果关闭,先判断当前机台是否在报警中，如果未报警就直接返回
            if (isOn == true)
            {
                if (buzzerSource != null &&!buzzerSource.IsCancellationRequested)
                {
                    return;
                }
            }
            else
            {
                
            }


            WaitEndTask(ref buzzerSource);

            ProcessIO(SysConfig.Buzzer, io =>
            {
                if (isOn)
                {
                    if (SysConfig.IsEnableBuzzer)
                    {
                        Blink(io, buzzerSource, OnTime, OffTime);
                    }
                }
                else
                {
                    io.SetDigital(false);
                    Thread.Sleep(200);
                    io.SetDigital(false);

                    // 将蜂鸣器对象关闭
                    buzzerSource?.Cancel();
                    buzzerSource = null;
                }
            });
        }

        /// <summary>
        /// 机台启动，亮黄灯，复位按钮闪烁
        /// </summary>
        public void StartLight()
        {
            SetBuzzer(false);
            var curStatus = EnumLight.Yellow;
            if (!LightStatus.SetLightStatus(curStatus))
            {
                return;
            }
            // 关闭红灯闪烁
            OnLog("三色灯-红灯关闭");
            ResetTricolorLight(false);
            SetButtonLight(false);
            // 黄灯亮
            ProcessIO(SysConfig.LightYellow, io => io.SetDigital(true));
            // 复位按钮灯闪烁
            ProcessIO(SysConfig.LightRecovery, io =>
            {
                Blink(io, lightSource);
            });
            //写入到PLC地址，目前三色灯状态,黄灯
            WritePlcValueInt(SysConfig.TricolorStatusAddr, 2);
            OnLog("StartLight()亮黄灯");
        }

        /// <summary>
        /// 机台回零完成，就绪
        /// </summary>
        public void ReadyLight()
        {
            SetBuzzer(false);
            var curStatus = EnumLight.Yellow;
            if (!LightStatus.SetLightStatus(curStatus))
            {
                //OnLog($"SetLight {curStatus} Fail");
                return;
            }
            // 1.关闭闪烁灯
            SetButtonLight(false);
            //关闭三色灯-0304
            ResetTricolorLight(false);

            // 2.回零完成后清除报警

            // 3.亮黄灯
            ProcessIO(SysConfig.LightYellow, io => io.SetDigital(true));

            // 4.启动灯闪烁
            ProcessIO(SysConfig.LightStart, io =>
            {
                Blink(io, lightSource);
            });

            // 5.关闭红灯闪烁
            OnLog("三色灯-红灯关闭");

            //6.写入到PLC地址，目前三色灯状态，黄灯
            WritePlcValueInt(SysConfig.TricolorStatusAddr, 2);
            OnLog("ReadyLight()亮黄灯");

        }

        /// <summary>
        /// 程序运行，绿灯亮，按钮灯灭,报警消除
        /// </summary>
        public void RunningLight(Action action = null)
        {
            SetBuzzer(false);
            var curStatus = EnumLight.Green;
            if (!LightStatus.SetLightStatus(curStatus))
            {
                //OnLog($"SetLight {curStatus} Fail");
                return;
            }
            // 关闭红灯闪烁
            OnLog("三色灯-红灯关闭");
            // 绿灯亮
            ResetTricolorLight(false);
            ProcessIO(SysConfig.LightGreen, io => io.SetDigital(true));

            // 启动按钮点亮
            SetButtonLight(false);
            ProcessIO(SysConfig.LightStart, io => io.SetDigital(true));


            //写入到PLC地址，目前三色灯状态，绿灯
            WritePlcValueInt(SysConfig.TricolorStatusAddr, 1);
            action?.Invoke();
            OnLog("RunningLight()亮绿灯");
        }

        /// <summary>
        /// 机台暂停
        /// </summary>
        /// <param name="isException"></param>
        public void PauseLight(bool isException = true)
        {

            //如果报警，就亮红灯
            if (isException)
            {
                var curStatus = EnumLight.RedShoutFlash;
                if (!LightStatus.SetLightStatus(curStatus))
                {
                    return;
                }
                ResetTricolorLight(false);
                //富士康客户要求，红灯闪烁
                //20250507，按照三色灯管控需求，红灯常亮，闪鸣
                OnLog("红灯开始闪烁");
                WritePlcValueInt(SysConfig.TricolorStatusAddr, 5);
                ProcessIO(SysConfig.LightRed, io => io.SetDigital(true));
                //ProcessIO(SysConfig.LightRed, io =>
                //{
                //    Blink(io, triRedLightSource);
                //});
                WritePlcValueInt(SysConfig.TricolorStatusAddr, 5);
                // ProcessIO(SysConfig.LightRed, io => io.SetDigital(true));
            }
            else
            {
                var curStatus = EnumLight.Yellow;
                if (!LightStatus.SetLightStatus(curStatus))
                {
                    return;
                }
                ResetTricolorLight(false);
                ProcessIO(SysConfig.LightYellow, io => io.SetDigital(true));
                WritePlcValueInt(SysConfig.TricolorStatusAddr, 2);
                OnLog("ReadyLight()亮黄灯");
            }

            // 暂停灯亮，启动灯闪烁
            SetButtonLight(false);

            if (isException)
            {
                ProcessIO(SysConfig.LightRecovery, io =>
                {
                    Blink(io, lightSource);
                });
            }
            else
            {
                ProcessIO(SysConfig.LightStart, io =>
                {
                    Blink(io, lightSource);
                });
            }

        }

        /// <summary>
        /// 机台停止
        /// </summary>
        public void StopLight(bool bIsBuzzer)
        {
            var curStatus = EnumLight.RedShoutFlash;
            if (!LightStatus.SetLightStatus(curStatus))
            {
                return;
            }
            ResetTricolorLight(false);
            SetButtonLight(false);
            WritePlcValueInt(SysConfig.TricolorStatusAddr, 5);
            // 红灯亮
            ProcessIO(SysConfig.LightRed, io => io.SetDigital(true));

            // 恢复等闪烁
            ProcessIO(SysConfig.LightRecovery, io =>
            {
                Blink(io, lightSource);
            });
            if (bIsBuzzer)
            {
                SetBuzzer(true, 400, 400);
                OnLog("StopLight()闪鸣");
            }
            //写入到PLC地址，目前三色灯状态，红灯

            OnLog("StopLight()亮红灯");
        }

        /// <summary>
        /// 闪红灯
        /// </summary>
        public void UnHome()
        {
            var curStatus = EnumLight.RedFlash;
            if (!LightStatus.SetLightStatus(curStatus))
            {
                return;
            }
            ResetTricolorLight(false);
            SetButtonLight(false);
            WritePlcValueInt(SysConfig.TricolorStatusAddr, 5);
            // 红灯闪烁
            ProcessIO(SysConfig.LightRed, io =>
            {
                Blink(io, triRedLightSource);
            });

            // 恢复等闪烁
            ProcessIO(SysConfig.LightRecovery, io =>
            {
                Blink(io, lightSource);
            });
            OnLog("UnHome()闪红灯");
        }

        /// <summary>
        /// 打开指定三色灯
        /// </summary>
        /// <param name="lightType"></param>
        public void OpenLight(LightType lightType, bool isBuzzer = false, int interval = 300)
        {
            if (_deviceEngine == null || SysConfig == null)
            {
                return;
            }

            // 如果灯有配置对应的类型，才进行切换
            if (lightType != LightType.None)
            {
                // 关闭三色灯
                ResetTricolorLight(false);
            }

            switch (lightType)
            {
                case LightType.Red:
                    WritePlcValueInt(SysConfig.TricolorStatusAddr, 5);
                    OnLog("LightType.Red亮红灯");
                    ProcessIO(SysConfig.LightRed, io => io.SetDigital(true));
                    //写入到PLC地址，目前三色灯状态，红灯
                    break;
                case LightType.Yellow:
                    ProcessIO(SysConfig.LightYellow, io => io.SetDigital(true));
                    //写入到PLC地址，目前三色灯状态，黄灯
                    WritePlcValueInt(SysConfig.TricolorStatusAddr, 2);
                    OnLog("LightType.Yellow亮黄灯");
                    break;
                case LightType.Green:
                    ProcessIO(SysConfig.LightGreen, io => io.SetDigital(true));
                    //写入到PLC地址，目前三色灯状态，绿灯
                    WritePlcValueInt(SysConfig.TricolorStatusAddr, 1);
                    OnLog("LightType.Green亮绿灯");
                    break;
            }

            // 开启或者关闭蜂鸣器
            if (isBuzzer)
            {
                SetBuzzer(true, interval / 2, interval / 2);
            }
            else
            {
                SetBuzzer(false);
            }
        }


        /// <summary>
        /// 写入PLC值，int
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool WritePlcValueInt(string addr, short val)
        {
            if (vPlc == null || string.IsNullOrEmpty(addr)) { return false; }

            try
            {
                vPlc.Write(addr, val, $"01 06 {addr} 1");
            }
            catch (Exception)
            {
            }
            return true;
        }

        public void IdleLight(int IdleDelayTime, Action action = null)
        {
            var curStatus = EnumLight.YellowFlash;
            SetBuzzer(false);
            if (!LightStatus.SetLightStatus(curStatus))
            {
                return;
            }
            //Thread.Sleep(IdleDelayTime);
            // 关闭三色灯
            OnLog("三色灯-关闭所有灯");
            ResetTricolorLight(false);
            SetButtonLight(false);

            OnLog("Idle 亮黄灯闪烁");
            //ProcessIO(SysConfig.LightYellow, io => io.SetDigital(true));//注释常亮黄灯
            //复位黄灯闪烁
            //WaitEndTask(ref triYellowLightSource);
            ProcessIO(SysConfig.LightYellow, io =>
            {
                Blink(io, triYellowLightSource);
            });
            //写入到PLC地址，目前三色灯状态，闪烁黄灯
            WritePlcValueInt(SysConfig.TricolorStatusAddr, 2);
            action?.Invoke();
        }

        public void TossLight()
        {
            SetBuzzer(false);
            var curStatus = EnumLight.YellowFlashShoutFlash;
            if (!LightStatus.SetLightStatus(curStatus))
            {
                return;
            }
            OnLog("三色灯-关闭所有灯");
            ResetTricolorLight(false);
            SetButtonLight(false);

            OnLog("Toss 亮黄灯闪烁闪鸣");
            ProcessIO(SysConfig.LightYellow, io =>
            {
                Blink(io, triYellowLightSource);
            });
            SetBuzzer(true, 400, 400);
            //写入到PLC地址，目前三色灯状态，闪烁黄灯
            WritePlcValueInt(SysConfig.TricolorStatusAddr, 2);
        }

    }

    public static class LightStatus
    {
        /// <summary>
        /// 上一次赋值的状态
        /// 与下一次赋值状态进行优先级比较
        /// </summary>
        public static EnumLight LastStatus { get; set; } = EnumLight.None;

        private static Dictionary<EnumLight, int> _lightLevelDic = new Dictionary<EnumLight, int>
        {
            {EnumLight.None, 0 },
            {EnumLight.Green, 1 },
            {EnumLight.RedShoutFlash, 2 },
            {EnumLight.RedFlash, 3 },
            {EnumLight.Yellow, 4 },
            {EnumLight.YellowFlash, 5 },
            {EnumLight.YellowFlashShoutFlash, 6 },
        };
        /// <summary>
        /// 设置灯光报警的优先级
        /// </summary>
        /// <param name="status"></param>
        /// <returns>true表示允许修改，false不允许修改</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static bool SetLightStatus(EnumLight status)
        {
            if (!_lightLevelDic.ContainsKey(status))
                throw new NotSupportedException("不支持给定的灯控枚举类型");
            if (_lightLevelDic[status] == _lightLevelDic[LastStatus])
            {
                return false;
            }
            else
            {
                LastStatus = status;
                return true;
            }
        }

    }

    public enum EnumLight
    {
        None = 0,//无状态
        Green = 1,//常绿
        RedShoutFlash = 2,//常红闪鸣
        RedFlash = 3,//闪红
        Yellow = 4,//常黄
        YellowFlash = 5,//闪黄
        YellowFlashShoutFlash = 6,//闪黄闪鸣
    }


    public class HardwareLightManager : IHardwareController
    {
        /// <summary>
        /// 配置
        /// </summary>
        private SystemConfig SysConfig = null;
        /// <summary>
        /// PLC对象
        /// </summary>
        private VPlc vPlc = null;

        /// <summary>
        /// 设备管理器
        /// </summary>
        private IDeviceEngine _deviceEngine;

        public void SetDeviceEngine(IDeviceEngine deviceEngine)
        {
            _deviceEngine = deviceEngine;
            vPlc = _deviceEngine.GetVDevices<VPlc>().FirstOrDefault();
        }

        /// <summary>
        /// 配置参数对象
        /// </summary>
        /// <param name="sysConfig"></param>
        public void SetSysConfig(object sysConfig)
        {
            SysConfig = sysConfig as SystemConfig;
        }

        private void OnLog(string msg)
        {
            _deviceEngine.OnLog(Common.DataStruct.Enums.LogType.Debug, msg);
        }

        /// <summary>
        /// IO 通用处理
        /// </summary>
        /// <param name="io"></param>
        private void ProcessIO(VDevice vDevice, Action<VIO> ioAction)
        {
            vPlc = _deviceEngine.GetVDevices<VPlc>().FirstOrDefault();
            // 启动按钮
            if (vDevice != null && !vDevice.IsEmpty())
            {
                var io = vDevice.GetVDevice<VIO>(_deviceEngine);
                ioAction(io);
            }
        }
        /// <summary>
        /// IO闪烁灯
        /// </summary>
        /// <param name="io"></param>
        private void Blink(VIO io, CancellationTokenSource tokenSource, int OnTime = 200, int OffTime = 200)
        {
            int interval = 50;
            Task.Run(() =>
            {
                bool isBlink = true;
                while (isBlink)
                {
                    // 先开启
                    io.SetDigital(true);

                    for (int i = 0; i < OnTime; i += interval)
                    {
                        Thread.Sleep(interval);

                        if (tokenSource.IsCancellationRequested)
                        {
                            isBlink = false;
                            break;
                        }
                    }


                    // 在关闭
                    io.SetDigital(false);
                    for (int i = 0; i < OffTime; i += interval)
                    {
                        Thread.Sleep(interval);
                        if (tokenSource.IsCancellationRequested)
                        {
                            isBlink = false;
                            break;
                        }
                    }
                }
            }, tokenSource.Token);
        }

        /// <summary>
        /// 确保闪烁灯一定被终止掉
        /// </summary>
        /// <param name="token"></param>
        private void WaitEndTask(ref CancellationTokenSource token)
        {
            lock (lockObj)
            {
                token?.Cancel();

                // 确保线程被结束掉
                Thread.Sleep(150);

                // 创建新的线程信号量
                token = new CancellationTokenSource();
            }
        }

        private static object lockObj = new object();

        #region ILight

        private CancellationTokenSource ctsLightGreen = null;
        private CancellationTokenSource ctsLightYellow = null;
        private CancellationTokenSource ctsLightRed = null;

        /// <summary>
        /// 清除三色灯的所有状态，不亮不闪
        /// </summary>
        private void SetLightFalse()
        {
            Green(false, false, 0, 0);
            Yellow(false, false, 0, 0);
            Red(false, false, 0, 0);
            WaitEndTask(ref ctsLightGreen);
            WaitEndTask(ref ctsLightYellow);
            WaitEndTask(ref ctsLightRed);
        }
        public void Green(bool IsOn, bool IsFlash, int OnTime, int OffTime)
        {
            SetLightFalse();
            if (!IsFlash)
            {
                ProcessIO(SysConfig.LightGreen, io => io.SetDigital(IsOn));
            }
            else if (IsFlash)
            {
                ProcessIO(SysConfig.LightGreen, io =>
                {
                    Blink(io, ctsLightGreen, OnTime, OffTime);
                });
            }
        }
        public void Yellow(bool IsOn, bool IsFlash, int OnTime, int OffTime)
        {
            SetLightFalse();
            if (!IsFlash)
            {
                ProcessIO(SysConfig.LightYellow, io => io.SetDigital(IsOn));
            }
            else if (IsFlash)
            {
                ProcessIO(SysConfig.LightYellow, io =>
                {
                    Blink(io, ctsLightYellow, OnTime, OffTime);
                });
            }
        }
        public void Red(bool IsOn, bool IsFlash, int OnTime, int OffTime)
        {
            SetLightFalse();
            if (!IsFlash)
            {
                ProcessIO(SysConfig.LightRed, io => io.SetDigital(IsOn));
            }
            else if (IsFlash)
            {
                ProcessIO(SysConfig.LightRed, io =>
                {
                    Blink(io, ctsLightRed, OnTime, OffTime);
                });
            }
        }
        #endregion

        #region IButtonLight

        private CancellationTokenSource ctsButtonGreen = null;
        private CancellationTokenSource ctsButtonYellow = null;
        private CancellationTokenSource ctsButtonRed = null;

        private void SetButtonFalse()
        {
            GreenStartBtn(false, false, 0, 0);
            YellowRecoveryBtn(false, false, 0, 0);
            RedPauseBtn(false, false, 0, 0);
        }
        public void GreenStartBtn(bool IsOn, bool IsFlash, int OnTime, int OffTime)
        {
            SetButtonFalse();

        }

        public void YellowRecoveryBtn(bool IsOn, bool IsFlash, int OnTime, int OffTime)
        {
            throw new NotImplementedException();
        }

        public void RedPauseBtn(bool IsOn, bool IsFlash, int OnTime, int OffTime)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IBuzzer
        public void Shout(bool IsOn, bool IsFlash, int OnTime, int OffTime)
        {
            throw new NotImplementedException();
        }
        #endregion


        public void Alarm()
        {
            throw new NotImplementedException();
        }



        public void HomingAndNotStart()
        {
            throw new NotImplementedException();
        }

        public void Idle()
        {
            throw new NotImplementedException();
        }


        public void Run()
        {
            throw new NotImplementedException();
        }



        public void Toss()
        {
            throw new NotImplementedException();
        }

        public void UnHome()
        {
            throw new NotImplementedException();
        }




    }

}
