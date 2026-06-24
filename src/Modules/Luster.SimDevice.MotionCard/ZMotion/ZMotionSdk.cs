using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Luster.SimDevice.MotionCard.ZMotion
{
    internal class ZMotionSdk : IZMotionSdk
    {
        public int OpenEth(string ip, out IntPtr handle)
        {
            return zmcaux.ZAux_OpenEth(ip, out handle);
        }

        public int Close(IntPtr handle)
        {
            return zmcaux.ZAux_Close(handle);
        }

        public int SetTraceFile(int mode, string path)
        {
            return zmcaux.ZAux_SetTraceFile(mode, path);
        }

        public int SetDpos(IntPtr handle, int axis, float position)
        {
            return zmcaux.ZAux_Direct_SetDpos(handle, axis, position);
        }

        public int GetDpos(IntPtr handle, int axis, ref float position)
        {
            return zmcaux.ZAux_Direct_GetDpos(handle, axis, ref position);
        }

        public int SetAxisEnable(IntPtr handle, int axis, int enabled)
        {
            return zmcaux.ZAux_Direct_SetAxisEnable(handle, axis, enabled);
        }

        public int SingleDatum(IntPtr handle, int axis, int mode)
        {
            return zmcaux.ZAux_Direct_Single_Datum(handle, axis, mode);
        }

        public int MoveAbs(IntPtr handle, int count, int[] axes, float[] positions)
        {
            return zmcaux.ZAux_Direct_MoveAbs(handle, count, axes, positions);
        }

        public int Move(IntPtr handle, int count, int[] axes, float[] positions)
        {
            return zmcaux.ZAux_Direct_Move(handle, count, axes, positions);
        }

        public int MoveAbsSp(IntPtr handle, int count, int[] axes, float[] positions)
        {
            return zmcaux.ZAux_Direct_MoveAbsSp(handle, count, axes, positions);
        }

        public int MoveSp(IntPtr handle, int count, int[] axes, float[] positions)
        {
            return zmcaux.ZAux_Direct_MoveSp(handle, count, axes, positions);
        }

        public int MoveCircAbsSp(IntPtr handle, int count, int[] axes, float x, float y, float centerX, float centerY, int direction)
        {
            return zmcaux.ZAux_Direct_MoveCircAbsSp(handle, count, axes, x, y, centerX, centerY, direction);
        }

        public int SingleVMove(IntPtr handle, int axis, int direction)
        {
            return zmcaux.ZAux_Direct_Single_Vmove(handle, axis, direction);
        }

        public int SingleCancel(IntPtr handle, int axis, int mode)
        {
            return zmcaux.ZAux_Direct_Single_Cancel(handle, axis, mode);
        }

        public int GetIfIdle(IntPtr handle, int axis, ref int idle)
        {
            return zmcaux.ZAux_Direct_GetIfIdle(handle, axis, ref idle);
        }

        public int SDORead(IntPtr handle, uint node, uint index, uint subindex, uint dataSize, ref int value)
        {
            return zmcaux.ZAux_BusCmd_SDORead(handle, 0, node, index, subindex, dataSize, ref value);
        }

        public int SDOWrite(IntPtr handle, uint node, uint index, uint subindex, uint dataSize, int value)
        {
            return zmcaux.ZAux_BusCmd_SDOWrite(handle, 0, node, index, subindex, dataSize, value);
        }

        public int DirectCommand(IntPtr handle, string command, out string response, int responseLength)
        {
            var builder = new StringBuilder();
            var result = zmcaux.ZAux_DirectCommand(handle, command, builder, responseLength);
            response = builder.ToString();
            return result;
        }

        // ===== 连续插补 + 高速锁存(P5-3) =====

        public int SetMerge(IntPtr handle, int axis, int value)
        {
            return zmcaux.ZAux_Direct_SetMerge(handle, axis, value);
        }

        public int SetCornerMode(IntPtr handle, int axis, int value)
        {
            return zmcaux.ZAux_Direct_SetCornerMode(handle, axis, value);
        }

        public int SetZsmooth(IntPtr handle, int axis, float value)
        {
            return zmcaux.ZAux_Direct_SetZsmooth(handle, axis, value);
        }

        public int SetDecelAngle(IntPtr handle, int axis, float value)
        {
            return zmcaux.ZAux_Direct_SetDecelAngle(handle, axis, value);
        }

        public int SetStopAngle(IntPtr handle, int axis, float value)
        {
            return zmcaux.ZAux_Direct_SetStopAngle(handle, axis, value);
        }

        public int SetMovemark(IntPtr handle, int axis, int value)
        {
            return zmcaux.ZAux_Direct_SetMovemark(handle, axis, value);
        }

        public int GetMoveCurmark(IntPtr handle, int axis, ref int value)
        {
            return zmcaux.ZAux_Direct_GetMoveCurmark(handle, axis, ref value);
        }

        public int MoveOp(IntPtr handle, int axis, int ioIndex, int value)
        {
            return zmcaux.ZAux_Direct_MoveOp(handle, axis, ioIndex, value);
        }

        public int MoveTable(IntPtr handle, uint baseAxis, uint tableNum, float value)
        {
            return zmcaux.ZAux_Direct_MoveTable(handle, baseAxis, tableNum, value);
        }

        public int MoveDelay(IntPtr handle, int axis, int ms)
        {
            return zmcaux.ZAux_Direct_MoveDelay(handle, axis, ms);
        }

        public int GetTable(IntPtr handle, int tableStart, int numes, float[] values)
        {
            return zmcaux.ZAux_Direct_GetTable(handle, tableStart, numes, values);
        }

        public int GetRemainBuffer(IntPtr handle, int axis, ref int value)
        {
            return zmcaux.ZAux_Direct_GetRemain_Buffer(handle, axis, ref value);
        }
    }

    internal static class zmcaux
    {
        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int ZAux_OpenEth(string ip, out IntPtr handle);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Close(IntPtr handle);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int ZAux_SetTraceFile(int mode, string path);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetDpos(IntPtr handle, int axis, float position);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_GetDpos(IntPtr handle, int axis, ref float position);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetAxisEnable(IntPtr handle, int axis, int enabled);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_Single_Datum(IntPtr handle, int axis, int mode);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_MoveAbs(IntPtr handle, int count, int[] axes, float[] positions);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_Move(IntPtr handle, int count, int[] axes, float[] positions);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_MoveAbsSp(IntPtr handle, int count, int[] axes, float[] positions);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_MoveSp(IntPtr handle, int count, int[] axes, float[] positions);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_MoveCircAbsSp(IntPtr handle, int count, int[] axes, float x, float y, float centerX, float centerY, int direction);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_Single_Vmove(IntPtr handle, int axis, int direction);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_Single_Cancel(IntPtr handle, int axis, int mode);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_GetIfIdle(IntPtr handle, int axis, ref int idle);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_BusCmd_SDORead(IntPtr handle, uint bus, uint node, uint index, uint subindex, uint dataSize, ref int value);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_BusCmd_SDOWrite(IntPtr handle, uint bus, uint node, uint index, uint subindex, uint dataSize, int value);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int ZAux_DirectCommand(IntPtr handle, string command, StringBuilder response, int responseLength);

        // ===== 连续插补 + 高速锁存(P5-3,签名对齐源端 Zmcaux.cs) =====

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetMerge(IntPtr handle, int iaxis, int iValue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetCornerMode(IntPtr handle, int iaxis, int piValue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetZsmooth(IntPtr handle, int iaxis, float fValue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetDecelAngle(IntPtr handle, int iaxis, float fValue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetStopAngle(IntPtr handle, int iaxis, float fValue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetMovemark(IntPtr handle, int iaxis, int iValue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_GetMoveCurmark(IntPtr handle, int iaxis, ref int piValue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_MoveOp(IntPtr handle, int iaxis, int ioutnum, int ivalue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_MoveTable(IntPtr handle, uint base_axis, uint table_num, float fvalue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_MoveDelay(IntPtr handle, int iaxis, int itimems);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_GetTable(IntPtr handle, int tabstart, int numes, float[] pfValue);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_GetRemain_Buffer(IntPtr handle, int iaxis, ref int piValue);
    }
}
