using Luster.Module.Motion.Business.Functions;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.Business
{

    /// <summary>
    /// ҵ��ģ��
    /// </summary>
    public class Business : MotionModule
    {
        public override void InitFunctions()
        {
            AddFunction<BeltCarry>();
            //AddFunction<WipPrint>();
            AddFunction<Pressurize>();

            // �ϲ�
            AddFunction<LoadingSilo>();
            AddFunction<UnLoadingSilo>();
            AddFunction<PositionOutput>();
            AddFunction<Heightfinder>();
            AddFunction<Tearing>();

            // �±�ѹ
            AddFunction<NewPressurize>();
            AddFunction<SFCFlow>();
            // ҵ��������������� 2025-5-5 wyy
            AddFunction<SFCFlowTiaoJi>();

            AddFunction<SingelAxisFlyShot>();

            AddFunction<VisionCalibration>();

            AddFunction<Robot>();
            AddFunction<EpsonRobot>();
            AddFunction<ReadRobotSpeed>();
            AddFunction<CalibByPosMove>();
            // 五轴标定参数数据模型(P2-D 补遗/P6-A 配套,前端 P6-B 标定 UI SelectedObject 契约)
            AddFunction<FiveAxisCaliParam>();

            AddFunction<PDCAFlow>();
            AddFunction<PDCAELimit>();
            AddFunction<PDCAWIP>();
            AddFunction<PDCAFailRetry>();

            //SFC���
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

            AddFunction<PressurizeChart>();

            // 排线SN管理
            AddFunction<CableSNManager>();
        }
    }

    /// <summary>
    /// ģ�鴴��
    /// </summary>
    public class BusinessCreator : MotionModuleCreator<Business>
    {
        public override int Sort => 3;

        public override string Icon => "\xe686";
    }
}
