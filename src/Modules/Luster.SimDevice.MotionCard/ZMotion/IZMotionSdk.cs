using System;

namespace Luster.SimDevice.MotionCard.ZMotion
{
    internal interface IZMotionSdk
    {
        int OpenEth(string ip, out IntPtr handle);

        int Close(IntPtr handle);

        int SetTraceFile(int mode, string path);

        int SetDpos(IntPtr handle, int axis, float position);

        int GetDpos(IntPtr handle, int axis, ref float position);

        int SetAxisEnable(IntPtr handle, int axis, int enabled);

        int SingleDatum(IntPtr handle, int axis, int mode);

        int MoveAbs(IntPtr handle, int count, int[] axes, float[] positions);

        int Move(IntPtr handle, int count, int[] axes, float[] positions);

        int MoveAbsSp(IntPtr handle, int count, int[] axes, float[] positions);

        int MoveSp(IntPtr handle, int count, int[] axes, float[] positions);

        int MoveCircAbsSp(IntPtr handle, int count, int[] axes, float x, float y, float centerX, float centerY, int direction);

        int SingleVMove(IntPtr handle, int axis, int direction);

        int SingleCancel(IntPtr handle, int axis, int mode);

        int GetIfIdle(IntPtr handle, int axis, ref int idle);

        int SDORead(IntPtr handle, uint node, uint index, uint subindex, uint dataSize, ref int value);

        int SDOWrite(IntPtr handle, uint node, uint index, uint subindex, uint dataSize, int value);

        int DirectCommand(IntPtr handle, string command, out string response, int responseLength);

        // —— ADR-TES-110 五轴 Frame/FrameCal 卡端原语（对齐源端 ZAux_Direct_*）——

        /// <summary>写卡端 Table（ZAux_Direct_SetTable）。Frame 写结构参数、FrameCal 写采样点。</summary>
        int SetTable(IntPtr handle, int startAddr, int count, float[] data);

        /// <summary>读卡端 Table（ZAux_Direct_GetTable）。FrameCal 读 aZero(OutZeroTb) + 结构参数(OutRobotTb)。</summary>
        int GetTable(IntPtr handle, int startAddr, int count, float[] data);

        /// <summary>进入卡端 Connframe 正逆解模式（ZAux_Direct_Connframe）。frame=29 为五轴逆解（源端 :2795）。</summary>
        int ConnFrame(IntPtr handle, int realCount, int[] realAxes, int frame, int paraAddr, int virCount, int[] virAxes);

        /// <summary>读轴 Loaded 状态（ZAux_Direct_GetLoaded）。Frame 进逆解后轮询物理轴 Loaded（源端 :2804）。</summary>
        int GetLoaded(IntPtr handle, int axis, ref int loaded);

        /// <summary>多轴运动停止（ZAux_Direct_CancelAxisList）。ExitFrame 停实轴/虚轴组（源端 :3160-3162）。</summary>
        int CancelAxisList(IntPtr handle, int count, int[] axes, int mode);
    }
}
