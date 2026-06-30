using Luster.Module.Motion.Device.Functions;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.IO
{
    /// <summary>
    /// �㷨ģ��
    /// </summary>
    public class Device : MotionModule
    {
        public override void InitFunctions()
        {
            AddFunction<GetIO>();
            AddFunction<WaitIO>();
            AddFunction<SetIO>();
            AddFunction<CheckIO>();
            AddFunction<SingleAxis>();

            // ���λ�˶�
            AddFunction<AxisPosMove>();
            AddFunction<AxisPosArray>();
            AddFunction<MultiAxis>();
            AddFunction<Turntable>();

            AddFunction<Vacuum>();
            AddFunction<Cylinder>();
            AddFunction<EleCylinder>();
            AddFunction<IOSimulation>();
            AddFunction<LightFlashing>();

            // ��е��
            //AddFunction<AxisArm>();

            // �ɴ�
            //AddFunction<Feeder>();

            // ���Ӳ����
            AddFunction<CameraIO>();

            // �������ŷ���
            AddFunction<ForceAxis>();

            // �����ഫ����
            AddFunction<LaserSensor>();


            // ѹ��������
            AddFunction<PressureSensor>();

            // �豸ģ�����ɼ���ת�������忨�ɼ���ģ����ת�����豸��ʵ��ֵ
            AddFunction<AnalogConvert>();
            AddFunction<AnalogConvertChart>();
            //�жϵ�ǰ���Ƿ�λ
            AddFunction<AxisPos>();

            //������
            AddFunction<RobotMove>();

            //SDO��д
            AddFunction<SDOAction>();

            //PDO��д
            AddFunction<PDOAction>();

            //����ģ��
            AddFunction<FlyingPhoto>();
            
            //̫�Ƶ���ģ��
            AddFunction<TaiKeScrewDriver>();
            //�ξ���ѹ��ģ��
            AddFunction<XJCPressureSensor>();
            //ModbusRTUͨ�ö�ȡģ��
            AddFunction<ModbusRTU>();

            //��ȡѹ��������ʾ
            AddFunction<PressDriver>();

            //���Ӽ������
            AddFunction<SerialPortDrive>();

            //���ӻ�ȡ��λ�û�Ԫ
            AddFunction<GenAxisPos>();

            //��ֵ�ռ�
            AddFunction<ForceCollect>();

            //SetAxisPos
            AddFunction<SetAxisPos>();

            //�ξ��϶�ͨ��ѹ��������F600
            AddFunction<XJCPressureSensorF600>();

            //��Ȧ���
            AddFunction<JunRudderVCM>();
            AddFunction<DHRoboticsVCM>();
            AddFunction<DHCalcu>();

            // P2-E: trajectory list transform node (Device module, avoid FiveAxisModule P5-3 conflict)
            AddFunction<TrajectoryListTransformNode>();
        }
    }

    /// <summary>
    /// ģ�鴴��
    /// </summary>
    public class DeviceCreator : MotionModuleCreator<Device>
    {
        public override int Sort => 2;

        public override string Icon => "\xe69b";
    }
}
