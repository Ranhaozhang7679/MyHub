using System;

namespace Luster.Module.Motion.Production.Vision
{
    /// <summary>
    /// PLC-Vision 拍照握手状态（TES-33 P8-D）。
    /// 对齐源端 ICW 检测握手状态机（<c>CheckStationTask.ICW_CheckStart</c>/<c>ICW_CheckEnd</c>）：
    /// Clear → WriteProductInfo → WriteRequest(触发) → PollResponse(等结果) → Clear。
    /// </summary>
    public enum PhotoHandshakeState
    {
        /// <summary>空闲</summary>
        Idle = 0,
        /// <summary>清除上次结果寄存器</summary>
        ClearResponse = 1,
        /// <summary>写产品信息（SN/ID）</summary>
        WriteProductInfo = 2,
        /// <summary>写检测模式/方向</summary>
        WriteCheckMode = 3,
        /// <summary>写 Request=1 触发拍照</summary>
        TriggerRequest = 4,
        /// <summary>轮询 Response 等待结果</summary>
        PollResponse = 5,
        /// <summary>拍照成功（Response=Success）</summary>
        Success = 6,
        /// <summary>拍照失败/超时/NG</summary>
        Failed = 7
    }

    /// <summary>
    /// 拍照握手结果码（对齐源端 <c>ICWSyncResultCodeType</c>）。
    /// </summary>
    public enum PhotoResultCode : ushort
    {
        /// <summary>无结果（未完成）</summary>
        None = 0,
        /// <summary>成功（OK）</summary>
        Success = 1,
        /// <summary>剔除产品（reject）</summary>
        Reject = 2,
        /// <summary>校验PLC数据异常</summary>
        PlcDataError = 10,
        /// <summary>ICW流程异常</summary>
        IcwFlowError = 11,
        /// <summary>检测软件异常</summary>
        VisionSoftwareError = 12,
        /// <summary>MES入站异常</summary>
        MesInError = 13,
        /// <summary>MES出站异常</summary>
        MesOutError = 14
    }

    /// <summary>拍照触发方式（对齐源端三条路径）</summary>
    public enum PhotoTriggerMode
    {
        /// <summary>ICW 寄存器握手（PLC 写 Request，检测软件采图，写 Response）</summary>
        IcwHandshake = 0,
        /// <summary>运动拍照硬件触发（motion card 脉冲触发线扫相机）</summary>
        HardwareTrigger = 1,
        /// <summary>本地相机软件触发（ICamera TriggerSoftware/StartGrab）</summary>
        SoftwareTrigger = 2
    }

    /// <summary>拍照握手结果</summary>
    public sealed class PhotoHandshakeResult
    {
        public PhotoResultCode ResultCode { get; }
        public bool IsOK => ResultCode == PhotoResultCode.Success;
        public bool IsTimeout { get; }
        public TimeSpan Elapsed { get; }
        public string Message { get; }

        public PhotoHandshakeResult(PhotoResultCode resultCode, bool isTimeout, TimeSpan elapsed, string message)
        {
            ResultCode = resultCode; IsTimeout = isTimeout; Elapsed = elapsed; Message = message ?? string.Empty;
        }

        public static PhotoHandshakeResult Ok(TimeSpan elapsed)
            => new PhotoHandshakeResult(PhotoResultCode.Success, false, elapsed, "拍照成功");
        public static PhotoHandshakeResult Timeout(TimeSpan elapsed)
            => new PhotoHandshakeResult(PhotoResultCode.None, true, elapsed, "拍照超时");
        public static PhotoHandshakeResult Fail(PhotoResultCode code, TimeSpan elapsed, string msg)
            => new PhotoHandshakeResult(code, false, elapsed, msg);
    }
}
