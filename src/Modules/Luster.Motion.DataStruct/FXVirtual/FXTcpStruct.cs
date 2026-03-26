using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.FXVirtual
{
    /// <summary>
    /// 轴信号结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly struct AxisSignal
    {
        public readonly AxisState State; // 轴状态 (4字节)
        public readonly double Speed;    // 速度 (8字节)
        public readonly double Acc;      // 加速度 (8字节)
        public readonly double Dec;      // 减速度 (8字节)
        public readonly double Pos;      // 位置 (8字节)

        public AxisSignal(AxisState state, double speed, double acc, double dec, double pos)
        {
            State = state;
            Speed = speed;
            Acc = acc;
            Dec = dec;
            Pos = pos;
        }

        public byte[] ToBytes()
        {
            const int size = 36; // 4 + 8*4
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(BitConverter.GetBytes(State.RawValue), 0, buffer, 0, 4);

            WriteBigEndian(BitConverter.GetBytes(Speed), buffer, 4);
            WriteBigEndian(BitConverter.GetBytes(Acc), buffer, 12);
            WriteBigEndian(BitConverter.GetBytes(Dec), buffer, 20);
            WriteBigEndian(BitConverter.GetBytes(Pos), buffer, 28);
            //Buffer.BlockCopy(BitConverter.GetBytes(Speed), 0, buffer, 4, 8);
            //Buffer.BlockCopy(BitConverter.GetBytes(Acc), 0, buffer, 12, 8);
            //Buffer.BlockCopy(BitConverter.GetBytes(Dec), 0, buffer, 20, 8);
            //Buffer.BlockCopy(BitConverter.GetBytes(Pos), 0, buffer, 28, 8);
            return buffer;
        }

        private static void WriteBigEndian(byte[] source, byte[] destination, int offset)
        {
            // 如果是小端系统则反转字节顺序
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(source);
            }
            Buffer.BlockCopy(source, 0, destination, offset, source.Length);
        }
    }
    /// <summary>
    /// 轴状态结构体
    /// </summary> /// <remarks>
    /// State   int型          4字节
    /// 预留 | 预留 | 预留 | 预留 | 预留 | 预留 | Jog | Dir 
    /// 预留 | 预留 | 预留 | 预留 | 预留 | 预留 | Run | Sevon  
    /// 预留 | 预留 | 预留 | 预留 | Dstp |  Rdy | Ez  | Inp 
    /// Sln  | Slp  | 预留 | Org  | Emg  |  Eln | Elp |Alm      
    /// Speed   double型       8字节
    /// Acc     double型       8字节
    /// Dec     double型       8字节
    /// Pos     double型       8字节
    /// </remarks>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct AxisState
    {
        [FieldOffset(0)]
        private readonly uint value;  // 使用uint避免符号问题

        public AxisState(uint intValue) => value = intValue;

        // 支持字节序转换的构造函数
        public AxisState(byte[] data)
        {
            if (data == null || data.Length < 4)
                throw new ArgumentException("需要至少4字节数据");

            value = !BitConverter.IsLittleEndian ?
                (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]) :
                BitConverter.ToUInt32(data, 0);
        }

        // 状态标志访问器
        public bool Alm => (value & 0x01) != 0;
        public bool Elp => (value & 0x02) != 0;
        public bool Eln => (value & 0x04) != 0;
        public bool Emg => (value & 0x08) != 0;
        public bool Org => (value & 0x10) != 0;
        public bool Slp => (value & 0x40) != 0;
        public bool Sln => (value & 0x80) != 0;
        public bool Inp => (value & 0x100) != 0;
        public bool Ez => (value & 0x200) != 0;
        public bool Rdy => (value & 0x400) != 0;
        public bool Dstp => (value & 0x800) != 0;
        public bool Sevon => (value & 0x10000) != 0;
        public bool Run => (value & 0x20000) != 0;
        public bool Dir => (value & 0x1000000) != 0;
        public bool Jog => (value & 0x2000000) != 0;

        // 原始值访问
        public uint RawValue => value;

        // 添加ToString输出所有标志状态
        public override string ToString() => string.Join("|",
            Alm ? "ALM" : null,
            Elp ? "ELP" : null,
            Eln ? "ELN" : null,
            Emg ? "EMG" : null,
            Org ? "ORG" : null,
            Slp ? "SLP" : null,
            Sln ? "SLN" : null,
            Inp ? "INP" : null,
            Ez ? "EZ" : null,
            Rdy ? "RDY" : null,
            Dstp ? "DSTP" : null,
            Sevon ? "SEVON" : null,
            Run ? "RUN" : null,
            Dir ? "DIR" : null,
            Jog ? "JOG" : null
        ).Replace("||", "|").Trim('|');
    }
}
