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

        // 卡端 Table / Frame / 精标原语(ADR-TES-110,对齐源端 ZAux_Direct_*):
        // - SetTable/GetTable:卡端 Table 区批量读写(采样点写入、aZero/结构参数读回)
        // - ConnFrame/ConnReframe:进逆解/正解模式(源端 ZAux_Direct_Connframe/Connreframe)
        // - GetLoaded:等待进 Frame 模式 Loaded(源端 FrameTimeOut 超时)
        // - CancelAxisList:多轴运动停止(源端 Z5Axes_ExitFrame)
        int SetTable(IntPtr handle, int startAddr, int count, float[] values);

        int GetTable(IntPtr handle, int startAddr, int count, float[] values);

        int ConnFrame(IntPtr handle, int realCount, int[] realAxes, int step, int paraAddr, int virCount, int[] virAxes);

        int ConnReframe(IntPtr handle, int realCount, int[] realAxes, int step, int paraAddr, int virCount, int[] virAxes);

        int GetLoaded(IntPtr handle, int axis, ref int loaded);

        int CancelAxisList(IntPtr handle, int count, int[] axes, int mode);

        int SDORead(IntPtr handle, uint node, uint index, uint subindex, uint dataSize, ref int value);

        int SDOWrite(IntPtr handle, uint node, uint index, uint subindex, uint dataSize, int value);

        int DirectCommand(IntPtr handle, string command, out string response, int responseLength);
    }
}
