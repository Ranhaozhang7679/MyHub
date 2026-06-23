using Luster.Module.Motion.Production.Vision;
using System.Collections.Generic;
using Xunit;

namespace Luster.Module.Motion.ProductionTests
{
    /// <summary>
    /// TES-33 P8-D:PLC-Vision 拍照握手状态机 + 服务 单测。
    /// </summary>
    public class PhotoHandshakeTests
    {
        #region AdvanceState 状态机纯逻辑

        [Fact]
        public void AdvanceState_Idle_Start推进到ClearResponse()
        {
            Assert.Equal(PhotoHandshakeState.ClearResponse,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.Idle, PhotoHandshakeEvent.Start));
        }

        [Fact]
        public void AdvanceState_Idle非Start保持()
        {
            Assert.Equal(PhotoHandshakeState.Idle,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.Idle, PhotoHandshakeEvent.Done));
        }

        [Theory]
        [InlineData(PhotoHandshakeState.ClearResponse, PhotoHandshakeState.WriteProductInfo)]
        [InlineData(PhotoHandshakeState.WriteProductInfo, PhotoHandshakeState.WriteCheckMode)]
        [InlineData(PhotoHandshakeState.WriteCheckMode, PhotoHandshakeState.TriggerRequest)]
        [InlineData(PhotoHandshakeState.TriggerRequest, PhotoHandshakeState.PollResponse)]
        public void AdvanceState_Done事件顺序推进(PhotoHandshakeState from, PhotoHandshakeState to)
        {
            Assert.Equal(to, PhotoHandshakeService.AdvanceState(from, PhotoHandshakeEvent.Done));
        }

        [Fact]
        public void AdvanceState_PollResponse_Success到Success()
        {
            Assert.Equal(PhotoHandshakeState.Success,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.PollResponse, PhotoHandshakeEvent.Success));
        }

        [Fact]
        public void AdvanceState_PollResponse_Fail到Failed()
        {
            Assert.Equal(PhotoHandshakeState.Failed,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.PollResponse, PhotoHandshakeEvent.Fail));
        }

        [Fact]
        public void AdvanceState_PollResponse_其他保持轮询()
        {
            Assert.Equal(PhotoHandshakeState.PollResponse,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.PollResponse, PhotoHandshakeEvent.Done));
        }

        [Fact]
        public void AdvanceState_任一步骤Fail事件到Failed()
        {
            Assert.Equal(PhotoHandshakeState.Failed,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.ClearResponse, PhotoHandshakeEvent.Fail));
            Assert.Equal(PhotoHandshakeState.Failed,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.TriggerRequest, PhotoHandshakeEvent.Fail));
        }

        [Fact]
        public void AdvanceState_终态保持()
        {
            Assert.Equal(PhotoHandshakeState.Success,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.Success, PhotoHandshakeEvent.Start));
            Assert.Equal(PhotoHandshakeState.Failed,
                PhotoHandshakeService.AdvanceState(PhotoHandshakeState.Failed, PhotoHandshakeEvent.Done));
        }

        #endregion

        #region TriggerIcwHandshake（stub channel）

        [Fact]
        public void TriggerIcwHandshake_成功路径()
        {
            var channel = new StubChannel { Responses = new Queue<ushort>(new ushort[] { 0, 0, 1 }) };
            var svc = new PhotoHandshakeService(channel);

            var result = svc.TriggerIcwHandshake("SN001", timeoutMs: 1000);

            Assert.True(result.IsOK);
            Assert.Equal(PhotoResultCode.Success, result.ResultCode);
            Assert.Equal(PhotoHandshakeState.Success, svc.CurrentState);
        }

        [Fact]
        public void TriggerIcwHandshake_超时()
        {
            var channel = new StubChannel { Responses = new Queue<ushort>(new ushort[] { 0, 0, 0, 0, 0 }) };
            var svc = new PhotoHandshakeService(channel);

            var result = svc.TriggerIcwHandshake("SN001", timeoutMs: 200, pollIntervalMs: 50);

            Assert.True(result.IsTimeout);
            Assert.False(result.IsOK);
            Assert.Equal(PhotoHandshakeState.Failed, svc.CurrentState);
        }

        [Fact]
        public void TriggerIcwHandshake_检测软件异常返回Fail()
        {
            var channel = new StubChannel { Responses = new Queue<ushort>(new ushort[] { 12 }) }; // VisionSoftwareError
            var svc = new PhotoHandshakeService(channel);

            var result = svc.TriggerIcwHandshake("SN001", timeoutMs: 1000);

            Assert.False(result.IsOK);
            Assert.Equal(PhotoResultCode.VisionSoftwareError, result.ResultCode);
            Assert.Contains("检测失败", result.Message);
        }

        [Fact]
        public void TriggerIcwHandshake_通道未注入失败()
        {
            var svc = new PhotoHandshakeService(null);
            var result = svc.TriggerIcwHandshake("SN001");
            Assert.False(result.IsOK);
            Assert.Equal(PhotoResultCode.VisionSoftwareError, result.ResultCode);
        }

        [Fact]
        public void TriggerIcwHandshake_ClearResponse失败()
        {
            var channel = new StubChannel { ClearOk = false };
            var svc = new PhotoHandshakeService(channel);
            var result = svc.TriggerIcwHandshake("SN001");
            Assert.False(result.IsOK);
            Assert.Equal(PhotoResultCode.IcwFlowError, result.ResultCode);
        }

        [Fact]
        public void TriggerIcwHandshake_WriteRequest失败()
        {
            var channel = new StubChannel { WriteRequestOk = false, Responses = new Queue<ushort>() };
            var svc = new PhotoHandshakeService(channel);
            var result = svc.TriggerIcwHandshake("SN001");
            Assert.False(result.IsOK);
        }

        [Fact]
        public void TriggerIcwHandshake_NG码Reject()
        {
            var channel = new StubChannel { Responses = new Queue<ushort>(new ushort[] { 2 }) }; // Reject
            var svc = new PhotoHandshakeService(channel);
            var result = svc.TriggerIcwHandshake("SN001", timeoutMs: 1000);
            Assert.False(result.IsOK);
            Assert.Equal(PhotoResultCode.Reject, result.ResultCode);
        }

        #endregion

        #region TriggerSoftware（stub camera）

        [Fact]
        public void TriggerSoftware_成功()
        {
            var cam = new StubCamera { TriggerOk = true, WaitOk = true, Data = new byte[] { 1, 2, 3 } };
            var svc = new PhotoHandshakeService(null, cam);
            var result = svc.TriggerSoftware(1000);
            Assert.True(result.IsOK);
            Assert.Equal(PhotoHandshakeState.Success, svc.CurrentState);
        }

        [Fact]
        public void TriggerSoftware_触发失败()
        {
            var cam = new StubCamera { TriggerOk = false };
            var svc = new PhotoHandshakeService(null, cam);
            var result = svc.TriggerSoftware(1000);
            Assert.False(result.IsOK);
            Assert.Equal(PhotoResultCode.VisionSoftwareError, result.ResultCode);
        }

        [Fact]
        public void TriggerSoftware_等帧超时()
        {
            var cam = new StubCamera { TriggerOk = true, WaitOk = false };
            var svc = new PhotoHandshakeService(null, cam);
            var result = svc.TriggerSoftware(200);
            Assert.True(result.IsTimeout);
        }

        [Fact]
        public void TriggerSoftware_相机未注入失败()
        {
            var svc = new PhotoHandshakeService(null, null);
            var result = svc.TriggerSoftware(1000);
            Assert.False(result.IsOK);
        }

        #endregion

        #region PhotoResultCode 语义

        [Fact]
        public void PhotoHandshakeResult_IsOK仅Success()
        {
            Assert.True(new PhotoHandshakeResult(PhotoResultCode.Success, false, default, "").IsOK);
            Assert.False(new PhotoHandshakeResult(PhotoResultCode.Reject, false, default, "").IsOK);
            Assert.False(new PhotoHandshakeResult(PhotoResultCode.None, true, default, "").IsOK);
        }

        [Fact]
        public void PhotoResultCode_对齐源端ICWSyncResultCodeType()
        {
            Assert.Equal((ushort)0, (ushort)PhotoResultCode.None);
            Assert.Equal((ushort)1, (ushort)PhotoResultCode.Success);
            Assert.Equal((ushort)2, (ushort)PhotoResultCode.Reject);
            Assert.Equal((ushort)10, (ushort)PhotoResultCode.PlcDataError);
            Assert.Equal((ushort)12, (ushort)PhotoResultCode.VisionSoftwareError);
        }

        #endregion

        private class StubChannel : IPhotoHandshakeChannel
        {
            public bool ClearOk { get; set; } = true;
            public bool WriteRequestOk { get; set; } = true;
            public bool WriteInfoOk { get; set; } = true;
            public Queue<ushort> Responses { get; set; } = new Queue<ushort>();

            public bool ClearResponse() => ClearOk;
            public bool WriteProductInfo(string sn) => WriteInfoOk;
            public bool WriteRequest(ushort value) => WriteRequestOk;
            public bool ReadResponse(out ushort code)
            {
                if (Responses.Count > 0) { code = Responses.Dequeue(); return true; }
                code = 0; return true;
            }
        }

        private class StubCamera : IPhotoCamera
        {
            public bool TriggerOk { get; set; }
            public bool WaitOk { get; set; }
            public byte[] Data { get; set; }
            public byte[] ImageData => Data;
            public bool Trigger() => TriggerOk;
            public bool WaitFrame(int timeoutMs) => WaitOk;
        }
    }
}
