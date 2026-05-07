using Luster.Module.Motion.Protocol.Functions;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.IO
{
    /// <summary>
    /// 协议模块
    /// </summary>
    public class Protocol : MotionModule
    {
        public override void InitFunctions()
        {
            //AddFunction<GetIO>();
            //AddFunction<WaitIO>();
            //AddFunction<WriteOutput>();
            AddFunction<ScanBarcode>();
            AddFunction<GetModbus>();
            AddFunction<SetModbus>();
            AddFunction<WaitModbus>();
            AddFunction<Communication>();
            AddFunction<WebHttp>();
            AddFunction<ReadPlc>();
            AddFunction<WritePlc>();
            AddFunction<WaitPlc>();
            AddFunction<FTPUpload>();
            AddFunction<ReadMC>();
            AddFunction<WriteMC>();
            AddFunction<WaitMC>();
            AddFunction<ReadFins>();
            AddFunction<WriteFins>();
            AddFunction<WaitFins>();

            AddFunction<SFTPUpload>();
            AddFunction<SetModbusEx>();
            AddFunction<ReadModbus>();

        }
    }

    /// <summary>
    /// 模块创建
    /// </summary>
    public class ProtocolCreator : MotionModuleCreator<Protocol>
    {
        public override int Sort => 5;

        public override string Icon => "\xea1a";
    }
}
