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
    }
}
