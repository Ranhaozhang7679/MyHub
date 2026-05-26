using Luster.Module.Motion.Device.Functions;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.IO
{
    /// <summary>
    /// 算法模块
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

            // 轴点位运动
            AddFunction<AxisPosMove>();
            AddFunction<AxisPosArray>();
            AddFunction<MultiAxis>();
            AddFunction<Turntable>();

            AddFunction<Vacuum>();
            AddFunction<Cylinder>();
            AddFunction<EleCylinder>();
            AddFunction<IOSimulation>();
            AddFunction<LightFlashing>();

            // 机械臂
            //AddFunction<AxisArm>();

            // 飞达
            //AddFunction<Feeder>();

            // 相机硬触发
            AddFunction<CameraIO>();

            // 力传感伺服轴
            AddFunction<ForceAxis>();

            // 激光测距传感器
            AddFunction<LaserSensor>();


            // 压力传感器
            AddFunction<PressureSensor>();

            // 设备模拟量采集与转换：将板卡采集的模拟量转换成设备真实的值
            AddFunction<AnalogConvert>();

            //判断当前轴是否到位
            AddFunction<AxisPos>();

            //机器人
            AddFunction<RobotMove>();

            //SDO读写
            AddFunction<SDOAction>();

            //PDO读写
            AddFunction<PDOAction>();

            //飞拍模块
            AddFunction<FlyingPhoto>();

            //太科电批模块
            AddFunction<TaiKeScrewDriver>();
            //鑫精诚压力模块
            AddFunction<XJCPressureSensor>();
            //ModbusRTU通用读取模块
            AddFunction<ModbusRTU>();

            //读取压力曲线显示
            AddFunction<PressDriver>();

            //增加激光读数
            AddFunction<SerialPortDrive>();

            //增加获取轴位置基元
            AddFunction<GenAxisPos>();

            //力值收集
            AddFunction<ForceCollect>();

            //SetAxisPos
            AddFunction<SetAxisPos>();

            //鑫精诚多通道压力传感器F600
            AddFunction<XJCPressureSensorF600>();

            //音圈电机
            AddFunction<JunRudderVCM>();
            AddFunction<DHRoboticsVCM>();

            // �豸ģ�����ɼ���ת��������ͼ
            AddFunction<AnalogConvertChart>();
        }
    }

    /// <summary>
    /// 模块创建
    /// </summary>
    public class DeviceCreator : MotionModuleCreator<Device>
    {
        public override int Sort => 2;

        public override string Icon => "\xe69b";
    }
}
