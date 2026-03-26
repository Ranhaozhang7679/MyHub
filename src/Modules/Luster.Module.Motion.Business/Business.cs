using Luster.Module.Motion.Business.Functions;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.Business
{

    /// <summary>
    /// 业务模块
    /// </summary>
    public class Business : MotionModule
    {
        public override void InitFunctions()
        {
            AddFunction<BeltCarry>();
            //AddFunction<WipPrint>();
            AddFunction<Pressurize>();

            // 料仓
            AddFunction<LoadingSilo>();
            AddFunction<UnLoadingSilo>();
            AddFunction<PositionOutput>();
            AddFunction<Heightfinder>();
            AddFunction<Tearing>();

            // 新保压
            AddFunction<NewPressurize>();
            AddFunction<SFCFlow>();
            // 业务类别里新增工具 2025-5-5 wyy
            AddFunction<SFCFlowTiaoJi>();

            AddFunction<SingelAxisFlyShot>();

            AddFunction<VisionCalibration>();

            AddFunction<Robot>();
            AddFunction<EpsonRobot>();
            AddFunction<CalibByPosMove>();

            AddFunction<PDCAFlow>();
            AddFunction<PDCAELimit>();
            AddFunction<PDCAWIP>();
            AddFunction<PDCAFailRetry>();

            //SFC拆解
            AddFunction<FX_OrderQuery>();
            AddFunction<FX_BindCarrier>();
            AddFunction<FX_RouteQuery>();
            AddFunction<FX_UnBindCarrier>();
            AddFunction<FX_UploadResult>();
            AddFunction<KeyMaterialQuery>();
            AddFunction<TimeLogEvent>();
            AddFunction<ManualGetBarcode>();
            AddFunction<SetMachineMode>();

            AddFunction<RoolMaterialCal>();

            AddFunction<GetMachineStatus>();
            AddFunction<LSMesUnLoad>();
            AddFunction<HiveCT>();

        }
    }

    /// <summary>
    /// 模块创建
    /// </summary>
    public class BusinessCreator : MotionModuleCreator<Business>
    {
        public override int Sort => 3;

        public override string Icon => "\xe686";
    }
}
