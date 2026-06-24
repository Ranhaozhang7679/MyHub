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

        // —— ADR-TES-110 五轴 Frame/FrameCal 卡端原语 ——
        public int SetTable(IntPtr handle, int startAddr, int count, float[] data)
        {
            return zmcaux.ZAux_Direct_SetTable(handle, startAddr, count, data);
        }

        public int GetTable(IntPtr handle, int startAddr, int count, float[] data)
        {
            return zmcaux.ZAux_Direct_GetTable(handle, startAddr, count, data);
        }

        public int ConnFrame(IntPtr handle, int realCount, int[] realAxes, int frame, int paraAddr, int virCount, int[] virAxes)
        {
            return zmcaux.ZAux_Direct_Connframe(handle, realCount, realAxes, frame, paraAddr, virCount, virAxes);
        }

        public int GetLoaded(IntPtr handle, int axis, ref int loaded)
        {
            return zmcaux.ZAux_Direct_GetLoaded(handle, axis, ref loaded);
        }

        public int CancelAxisList(IntPtr handle, int count, int[] axes, int mode)
        {
            return zmcaux.ZAux_Direct_CancelAxisList(handle, count, axes, mode);
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

        // —— ADR-TES-110 五轴 Frame/FrameCal 卡端原语（对齐源端 Zmcaux.cs 签名）——

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_SetTable(IntPtr handle, int startAddr, int count, float[] data);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_GetTable(IntPtr handle, int startAddr, int count, float[] data);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_Connframe(IntPtr handle, int realCount, int[] realAxes, int frame, int paraAddr, int virCount, int[] virAxes);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_GetLoaded(IntPtr handle, int axis, ref int loaded);

        [DllImport("cszmcaux.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZAux_Direct_CancelAxisList(IntPtr handle, int count, int[] axes, int mode);
    }
}
