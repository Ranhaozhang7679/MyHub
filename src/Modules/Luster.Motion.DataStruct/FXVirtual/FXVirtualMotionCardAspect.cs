using Aspose.Cells.Charts;
using Luster.Common.DataStruct;//to use FriendlyException
using Luster.Motion.DataStruct.Enums;//to use Enum HomeMode
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Luster.Motion.DataStruct.FXVirtual
{
    /* 数字孪生实现
     * 需求：
     *      FX需要在Luster原有轴卡接口上，增加额外的业务逻辑;
     *      所有轴卡的指令和返回值都需要经过额外处理，通过tcp通讯发送给FX，并获取返回值；
     *      通讯协议需要与FX保持一致；
     * 实现：
     *      应用适配器模式，对原有的接口增加中间封装类，改动如下；
     *      1、原始 LSMotionCard继承 MotionCardBase，IMotionCard接口
     *      2、现在 FXVirtualMotionCardAspect 继承 MotionCardBase，IMotionCard接口
     *      将原有的轴卡实现，都改为继承 FXVirtualMotionCardAspect
     *      3、并在FXVirtualMotionCardAspect中实现IMotionCard接口，根据FX通讯协议，增加额外的业务逻辑；
     *      4、通过Factory模式，创建FXVirtualMotionCardAspect的实例，增加外部逻辑判断切入点；
     *      5、兼容原有的板卡监控功能；
     *      6、原有IO\轴的设置可以转换为XSD标准文件；
     */
    public class FXVirtualMotionCardAspect : IMotionCard
    {
        /// <summary>
        /// 由于反射获取方法名和行号会有性能损耗，
        /// 且在调试模式下会抛出异常，
        /// 所以这里设置一个开关来控制是否抛出异常。
        /// </summary>
        private const bool IsDebugThrow = true;
        private readonly IMotionCard _innerCard;
        //public static bool IsAspectMode { get; set; } = false;

        public static int AspectMode { get; set; } = 0;

        private const int NoTcp = 0;
        private const int AllTcp = 1;
        private const int OnlyTcpSend = 2;

        public static void SetAspectMode(int modeEnum)
        {
            AspectMode = modeEnum;
        }
        //public static void SetAspectMode(bool isAspectMode)
        //{
        //    IsAspectMode = isAspectMode;
        //}

        public FXVirtualMotionCardAspect(IMotionCard card, IDeviceEngine deviceEngine)
        {
            if (card == null)
            {
                return;
            }
            _innerCard = card;
            if (!FXTCP.Insatnce.ReConnect(deviceEngine: deviceEngine))
            {
                //IsAspectMode = false;
                AspectMode = 0;
            }
        }

        /// <summary>
        /// 通用方法调用
        /// </summary>
        /// <param name="coreLogic"></param>
        /// <param name="methodName"></param>
        /// <exception cref="FriendlyException"></exception>
        private void ExecuteWithAspectHandling(Action coreLogic, bool bIsReceive = false, [CallerMemberName] string methodName = null)
        {
            //如果是仿真模式，就发送
            //如果是在线调试模式，就不接收
            bool bIsContinue =
                   AspectMode == AllTcp ||
                   (AspectMode == OnlyTcpSend && !bIsReceive);
            if (bIsContinue)
            {
                try
                {
                    //开始日志
                    coreLogic();//外部调用TCP通讯
                    //结束日志
                }
                catch (Exception ex)
                {
                    if (IsDebugThrow)
                        throw new FriendlyException($"方法名: {methodName}，异常信息: {ex.Message}", ex);
                }
            }
        }

        #region IMotionCard实现，通过IsAspectMode判断是否增加额外的业务逻辑
        public bool GetDigitalIn(int index)
        {
            bool res = false;
            ExecuteWithAspectHandling(() =>
            {
                res = FXTCP.Insatnce.GetDigitalIn(index);
            }, true);
            if (AspectMode != AllTcp)
            {
                return _innerCard.GetDigitalIn(index);
            }
            return res;
        }

        public bool GetDigitalOut(int index)
        {
            bool res = false;
            ExecuteWithAspectHandling(() =>
            {
                res = FXTCP.Insatnce.GetDigitalOut(index);
            }, true);
            if (AspectMode != AllTcp)
            {
                return _innerCard.GetDigitalOut(index);
            }
            return res;
        }

        public void SetDigitalOut(int index, bool digitalOut)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.SetDigitalOut(index, digitalOut);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.SetDigitalOut(index, digitalOut);
            }
        }

        public double GetAnalogIn(int index)
        {
            double res = 0;
            ExecuteWithAspectHandling(() =>
            {
                res = FXTCP.Insatnce.GetAnalogIn(index);
            }, true);
            if (AspectMode != AllTcp)
            {
                return _innerCard.GetAnalogIn(index);
            }
            return res;
        }

        public void ScanAxis(out uint axisNum)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.ScanAxis(out _);
            }, true);
            axisNum = 0;
            if (AspectMode != AllTcp)
            {
                _innerCard.ScanAxis(out axisNum);
            }
        }

        public void ScanDigitalIO(out uint digitalIn, out ushort digitalOut)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.ScanDigitalIO(out _, out _);
            }, true);
            digitalIn = digitalOut = 0;
            if (AspectMode != AllTcp)
            {
                _innerCard.ScanDigitalIO(out digitalIn, out digitalOut);
            }
        }

        public void ScanAnglog(out ushort anglogIn, out ushort anglogOut)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.ScanAnglog(out _, out _);
            }, true);
            anglogIn = anglogOut = 0;
            if (AspectMode != AllTcp)
            {
                _innerCard.ScanAnglog(out anglogIn, out anglogOut);
            }
        }

        public bool CheckMotionDone(int precision, int axisNo = 0, double targetPulse = 0)
        {
            bool res = false;
            ExecuteWithAspectHandling(() =>
            {
                res = FXTCP.Insatnce.CheckMotionDone(precision, axisNo, targetPulse);
            }, true);
            if (AspectMode != AllTcp)
            {
                return _innerCard.CheckMotionDone(precision, axisNo, targetPulse);
            }
            return res;
        }

        public double GetCurrentPos(int axisNo, double perPulse)
        {
            double pos = 0;
            ExecuteWithAspectHandling(() =>
            {
                pos = FXTCP.Insatnce.GetCurrentPos(axisNo, perPulse);
            }, true);
            if (AspectMode != AllTcp)
            {
                return _innerCard.GetCurrentPos(axisNo, perPulse);
            }
            return pos;
        }

        public void Home(int axisNo, HomeMode homeMode, double high, double low, double perPlus, double homeAcc, double Offset, AxisPML axisPML)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.Home(axisNo, homeMode, high, low, perPlus, homeAcc, Offset, 0);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.Home(axisNo, homeMode, high, low, perPlus, homeAcc, Offset, axisPML);
            }
        }

        public void HomeCancel(int axisNo)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.HomeCancel(axisNo);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.HomeCancel(axisNo);
            }
        }

        public void Jog(double vel, double acc, double dec, double perPlus, double slineTime, int axisNo, AxisPML axisPML)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.Jog(vel, acc, dec, perPlus, slineTime, axisNo, 0);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.Jog(vel, acc, dec, perPlus, slineTime, axisNo, axisPML);
            }
        }

        public void Move(double pos, double vel, double acc, double dec, double perPlus, double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.Move(pos, vel, acc, dec, perPlus, slineTime, isAbsMove, axisNo, 0);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.Move(pos, vel, acc, dec, perPlus, slineTime, isAbsMove, axisNo, axisPML);
            }
        }

        public void Stop(int axisNo, bool isAll = false)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.Stop(axisNo, isAll);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.Stop(axisNo, isAll);
            }
        }

        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.MoveLine(axisId, pos, perPlusArr, vel, acc);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.MoveLine(axisId, pos, perPlusArr, vel, acc);
            }
        }

        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.MoveCircle(axisId, pos, perPlusArr, vel, acc, radius, dir);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.MoveCircle(axisId, pos, perPlusArr, vel, acc, radius, dir);
            }
        }

        public Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo, bool IsThrowException = true)
        {
            Dictionary<AxisStatus, bool> res = new Dictionary<AxisStatus, bool>();
            ExecuteWithAspectHandling(() =>
            {
                res = FXTCP.Insatnce.GetAxisStatus(axisNo);
            }, true);
            if (AspectMode != AllTcp)
            {
                return _innerCard.GetAxisStatus(axisNo, IsThrowException);
            }
            return res;
        }

        public void ServOn(int axisNo, bool isOn)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.ServOn(axisNo, isOn);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.ServOn(axisNo, isOn);
            }
        }

        public void ResetState(int axisNo)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.ResetState(axisNo);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.ResetState(axisNo);
            }
        }

        public void SetCurrentPos(int axisNo, double perPulse, double position)
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.SetCurrentPos(axisNo, perPulse, position);
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.SetCurrentPos(axisNo, perPulse, position);
            }
        }

        public bool CheckHomeDone(int axisNo = 0)
        {
            bool res = false;
            ExecuteWithAspectHandling(() =>
            {
                res = FXTCP.Insatnce.CheckHomeDone(axisNo);
            }, true);
            if (AspectMode != AllTcp)
            {
                return _innerCard.CheckHomeDone(axisNo);
            }
            return res;
        }

        public double GetAnalogOut(int index)
        {
            throw new NotImplementedException();
        }

        public void SetAnalogOut(int index, double analogVal)
        {
            throw new NotImplementedException();
        }

        public void SDORead(short slave, short index, short subindex, short data_size, out int value, short count)
        {
            if (AspectMode != AllTcp)
            {
                _innerCard.SDORead(slave, index, subindex, data_size, out var v1, count);
                value = v1;
            }
            else
            {
                value = 0;
            }

        }

        public void SDOWrite(short slave, short index, short subindex, int data, short data_size)
        {
            if (AspectMode != AllTcp)
            {
                _innerCard.SDOWrite(slave, index, subindex, data, data_size);
            }
        }

        public void PDORead(short axis, short index, short subindex, short data_size, ref int value, short count)
        {
            if (AspectMode != AllTcp)
            {
                _innerCard.PDORead(axis, index, subindex, data_size, ref value, count);
            }
        }

        public void PDOWrite(short axis, short index, short subindex, int data, short data_size)
        {
            if (AspectMode != AllTcp)
            {
                _innerCard.PDOWrite(axis, index, subindex, data, data_size);
            }
        }

        public void AxisContinuousMove(int axisNo, double acc, double dec, double perPulse, List<double> pos, List<double> vel)
        {
            throw new NotImplementedException();
        }

        public void ClearEmg()
        {
            ExecuteWithAspectHandling(() =>
            {
                FXTCP.Insatnce.ClearEmg();
            });
            if (AspectMode != AllTcp)
            {
                _innerCard.ClearEmg();
            }
        }

        #endregion

    }

}
