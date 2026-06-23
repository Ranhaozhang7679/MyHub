using System;

namespace Luster.Module.Motion.Production.Vision
{
    /// <summary>
    /// PLC-Vision 拍照握手寄存器访问抽象（TES-33 P8-D，便于 mock 测）。
    /// 对齐源端 <c>HandoverSingle.Instance.ICW.CommWriteUshort/CommReadUshort</c>。
    /// </summary>
    public interface IPhotoHandshakeChannel
    {
        /// <summary>写 Request 寄存器（触发拍照，写 1）</summary>
        bool WriteRequest(ushort value);
        /// <summary>读 Response 寄存器（结果码）</summary>
        bool ReadResponse(out ushort code);
        /// <summary>清除 Response 寄存器（写 0，握手复位）</summary>
        bool ClearResponse();
        /// <summary>写产品信息（SN/ID，可选）</summary>
        bool WriteProductInfo(string sn);
    }

    /// <summary>
    /// 相机抽象（拍照握手用，便于 mock；真实实现适配 lmv <c>ICamera</c>）。
    /// </summary>
    public interface IPhotoCamera
    {
        /// <summary>触发采图（软件触发模式）</summary>
        bool Trigger();
        /// <summary>等待采图完成（硬件触发/软件触发后等帧）</summary>
        bool WaitFrame(int timeoutMs);
        /// <summary>图片字节（采图后）</summary>
        byte[] ImageData { get; }
    }

    /// <summary>
    /// PLC-Vision 拍照握手服务（TES-33 P8-D）。
    /// 封装源端 ICW 检测握手状态机（Clear→WriteInfo→Trigger→Poll→Clear）+ 相机采图，
    /// 端到端：触发拍照 → 等待结果 → 结果回写/记录追溯（接 P8-B Trace）。
    /// </summary>
    /// <remarks>
    /// 三种触发模式（对齐源端三条路径）：
    /// - <see cref="PhotoTriggerMode.IcwHandshake"/>：ICW 寄存器握手（源端 <c>ICW_CheckStart/End</c>，主路径）
    /// - <see cref="PhotoTriggerMode.HardwareTrigger"/>：运动拍照硬件脉冲（源端 <c>CrdContiOutput</c>，待 P3 轨迹节点接入）
    /// - <see cref="PhotoTriggerMode.SoftwareTrigger"/>：本地相机软件触发（源端 <c>CameraLBAS.Trigger</c>）
    /// 真实采图 + ICW 检测软件对接 ⚠️ 待现场。
    /// </remarks>
    public class PhotoHandshakeService
    {
        private readonly IPhotoHandshakeChannel _channel;
        private readonly IPhotoCamera _camera;

        public PhotoHandshakeService(IPhotoHandshakeChannel channel, IPhotoCamera camera = null)
        {
            _channel = channel;
            _camera = camera;
        }

        /// <summary>当前握手状态</summary>
        public PhotoHandshakeState CurrentState { get; private set; } = PhotoHandshakeState.Idle;

        /// <summary>
        /// 执行 ICW 寄存器握手拍照（主路径，对齐源端 ICW_CheckStart + ICW_CheckEnd）。
        /// </summary>
        /// <param name="sn">产品 SN（写产品信息用）</param>
        /// <param name="timeoutMs">结果轮询超时（源端 ReadTimeOut=3000）</param>
        /// <param name="pollIntervalMs">轮询间隔</param>
        public PhotoHandshakeResult TriggerIcwHandshake(string sn, int timeoutMs = 3000, int pollIntervalMs = 50)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (_channel == null)
            {
                CurrentState = PhotoHandshakeState.Failed;
                return PhotoHandshakeResult.Fail(PhotoResultCode.VisionSoftwareError, sw.Elapsed, "握手通道未注入");
            }

            // 1. Clear Response
            CurrentState = PhotoHandshakeState.ClearResponse;
            if (!_channel.ClearResponse())
            {
                CurrentState = PhotoHandshakeState.Failed;
                return PhotoHandshakeResult.Fail(PhotoResultCode.IcwFlowError, sw.Elapsed, "清除 Response 寄存器失败");
            }

            // 2. Write ProductInfo
            CurrentState = PhotoHandshakeState.WriteProductInfo;
            if (!string.IsNullOrEmpty(sn) && !_channel.WriteProductInfo(sn))
            {
                CurrentState = PhotoHandshakeState.Failed;
                return PhotoHandshakeResult.Fail(PhotoResultCode.PlcDataError, sw.Elapsed, "写产品信息失败");
            }

            // 3. Trigger Request=1
            CurrentState = PhotoHandshakeState.TriggerRequest;
            if (!_channel.WriteRequest(1))
            {
                CurrentState = PhotoHandshakeState.Failed;
                return PhotoHandshakeResult.Fail(PhotoResultCode.IcwFlowError, sw.Elapsed, "写 Request 触发失败");
            }

            // 4. Poll Response
            CurrentState = PhotoHandshakeState.PollResponse;
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (!_channel.ReadResponse(out ushort raw))
                {
                    CurrentState = PhotoHandshakeState.Failed;
                    return PhotoHandshakeResult.Fail(PhotoResultCode.IcwFlowError, sw.Elapsed, "读 Response 失败");
                }
                var code = (PhotoResultCode)raw;
                if (code == PhotoResultCode.Success)
                {
                    CurrentState = PhotoHandshakeState.Success;
                    return PhotoHandshakeResult.Ok(sw.Elapsed);
                }
                if (code != PhotoResultCode.None)
                {
                    // 非成功非 None = 错误/NG（源端 :1084 WarningPause）
                    CurrentState = PhotoHandshakeState.Failed;
                    return PhotoHandshakeResult.Fail(code, sw.Elapsed, $"检测失败: {code}");
                }
                System.Threading.Thread.Sleep(pollIntervalMs);
            }

            CurrentState = PhotoHandshakeState.Failed;
            return PhotoHandshakeResult.Timeout(sw.Elapsed);
        }

        /// <summary>
        /// 执行本地相机软件触发拍照（对齐源端 CameraLBAS.Trigger + SaveImage）。
        /// </summary>
        public PhotoHandshakeResult TriggerSoftware(int timeoutMs = 3000)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (_camera == null)
            {
                CurrentState = PhotoHandshakeState.Failed;
                return PhotoHandshakeResult.Fail(PhotoResultCode.VisionSoftwareError, sw.Elapsed, "相机未注入");
            }

            CurrentState = PhotoHandshakeState.TriggerRequest;
            if (!_camera.Trigger())
            {
                CurrentState = PhotoHandshakeState.Failed;
                return PhotoHandshakeResult.Fail(PhotoResultCode.VisionSoftwareError, sw.Elapsed, "相机触发失败");
            }

            CurrentState = PhotoHandshakeState.PollResponse;
            if (!_camera.WaitFrame(timeoutMs))
            {
                CurrentState = PhotoHandshakeState.Failed;
                return PhotoHandshakeResult.Timeout(sw.Elapsed);
            }

            CurrentState = PhotoHandshakeState.Success;
            return PhotoHandshakeResult.Ok(sw.Elapsed);
        }

        /// <summary>
        /// 状态机推进判定（纯逻辑，便于单测）。
        /// 给定当前状态 + 事件，返回下一状态。
        /// </summary>
        public static PhotoHandshakeState AdvanceState(PhotoHandshakeState current, PhotoHandshakeEvent evt)
        {
            switch (current)
            {
                case PhotoHandshakeState.Idle:
                    return evt == PhotoHandshakeEvent.Start ? PhotoHandshakeState.ClearResponse : current;
                case PhotoHandshakeState.ClearResponse:
                    return evt == PhotoHandshakeEvent.Done ? PhotoHandshakeState.WriteProductInfo : PhotoHandshakeState.Failed;
                case PhotoHandshakeState.WriteProductInfo:
                    return evt == PhotoHandshakeEvent.Done ? PhotoHandshakeState.WriteCheckMode : PhotoHandshakeState.Failed;
                case PhotoHandshakeState.WriteCheckMode:
                    return evt == PhotoHandshakeEvent.Done ? PhotoHandshakeState.TriggerRequest : PhotoHandshakeState.Failed;
                case PhotoHandshakeState.TriggerRequest:
                    return evt == PhotoHandshakeEvent.Done ? PhotoHandshakeState.PollResponse : PhotoHandshakeState.Failed;
                case PhotoHandshakeState.PollResponse:
                    if (evt == PhotoHandshakeEvent.Success) return PhotoHandshakeState.Success;
                    if (evt == PhotoHandshakeEvent.Fail) return PhotoHandshakeState.Failed;
                    return current; // 继续轮询
                case PhotoHandshakeState.Success:
                case PhotoHandshakeState.Failed:
                    return current; // 终态
                default:
                    return current;
            }
        }
    }

    /// <summary>握手事件（状态机推进用）</summary>
    public enum PhotoHandshakeEvent
    {
        Start,
        Done,
        Success,
        Fail
    }
}
