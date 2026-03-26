using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Real
{
    public interface IFlyingPhoto
    {
        /// <summary>
        /// 从站数量
        /// </summary>
        int SlaveNum { get; set; }

        /// <summary>
        /// 连接总线
        /// </summary>
        void Connect(int slaveNo);

        void EncoderInit(int encoderNo = 0);

        void ClearEncoderData(int encoderNo);

        void GetEncoderCurData(int encoderNo, out long data);

        void SetTriggerPos(int trigNo, long position);

        void SetTriggerParm(int trigNo, int ltcNo, int type);

        void SetPulseWidth(int trigNo, int width);

        void SetTriggerCameraOffset(int trigNo, long offset);

        void GetTriggerCount(int trigNo, ref int count);

        void GetTriggerFifoCount(int trigNo, int Type, ref int count);

        void SetTriggerData(int preCompareNo, int[] posArray);

        void SetPreCompareEnable(int preCompareNo, bool onoff);

        void ResetTriggerOutCount(int trigNo);

        void SetPreCmpParam(int encoderNo, int preCmpNo, int trigNo);

    }
}
