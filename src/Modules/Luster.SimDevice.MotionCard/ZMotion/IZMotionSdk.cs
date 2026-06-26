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

        // ===== 连续插补 + 高速锁存(P5-3,对齐源端 ZMCMotion 调用) =====

        /// <summary>设置插补合并模式(0=关闭连续插补,1=开启)。对应源端 OpenCrdConti/CloseCrdConti。</summary>
        int SetMerge(IntPtr handle, int axis, int value);

        /// <summary>设置拐角模式。对应源端 CrdSetSmoothProfile。</summary>
        int SetCornerMode(IntPtr handle, int axis, int value);

        /// <summary>设置拐角平滑半径。对应源端 CrdSetSmoothProfile。</summary>
        int SetZsmooth(IntPtr handle, int axis, float value);

        /// <summary>设置减速角度阈值(弧度)。对应源端 CrdSetSmoothProfile。</summary>
        int SetDecelAngle(IntPtr handle, int axis, float value);

        /// <summary>设置停止角度阈值(弧度)。对应源端 CrdSetSmoothProfile。</summary>
        int SetStopAngle(IntPtr handle, int axis, float value);

        /// <summary>设置运动标记号(供 ReadContiOutFlag/GetMoveCurmark 对齐)。对应源端 AddContiLine/AddContiDelay。</summary>
        int SetMovemark(IntPtr handle, int axis, int value);

        /// <summary>读取当前运动标记号。对应源端 GetContiCurrentIndex。</summary>
        int GetMoveCurmark(IntPtr handle, int axis, ref int value);

        /// <summary>插补器中追加同步输出(在标记点翻转 IO)。对应源端 AddContiOutput(底层 MoveOp)。</summary>
        int MoveOp(IntPtr handle, int axis, int ioIndex, int value);

        /// <summary>插补器中追加同步输出表项(对应源端 AddContiOutFlag,底层 MoveTable)。</summary>
        int MoveTable(IntPtr handle, uint baseAxis, uint tableNum, float value);

        /// <summary>插补器中追加延时(ms)。对应源端 AddContiDelay(底层 MoveDelay)。</summary>
        int MoveDelay(IntPtr handle, int axis, int ms);

        /// <summary>读取插补器数据表(锁存计数/锁存值/输出标志回读共用)。对应源端 ReadContiOutFlag/GetHighLatchedCount/GetHighLatchedValue(底层 GetTable)。</summary>
        int GetTable(IntPtr handle, int tableStart, int numes, float[] values);

        /// <summary>查询插补器剩余缓冲空间。对应源端 GetContiRemainSpace(底层 GetRemain_Buffer)。</summary>
        int GetRemainBuffer(IntPtr handle, int axis, ref int value);
    }
}
