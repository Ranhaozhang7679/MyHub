#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DialogExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.Extension
* 文 件 名:       DialogExtension.cs
* 创建时间:       2022/4/15 11:40:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      8bdb73bc-cea6-440c-b454-8d11de51d77b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/15 11:40:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Luster.SimDevice.SubSystem.Langs;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.Extension
{
    public static class DialogServiceExtension
    {
        /// <summary>
        /// 记录当前已经打开的Window,用于防止多次弹窗
        /// </summary>
        private static IDialogWindow _showWindow = null;

        /// <summary>
        /// 打开窗体时默认关闭已经存在的窗口，防止同时弹出多个窗口
        /// </summary>
        /// <param name="action"></param>
        public static void CloseExistShowWindow(this IDialogService service)
        {
            if (_showWindow != null)
            {
                _showWindow.Close();
            }
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowDeviceConfig(this IDialogService service, Type deviceType, DeviceModel deviceModel = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("DeviceType", deviceType);
            if (deviceModel != null)
            {
                dialogParameters.Add("DeviceModel", deviceModel);
            }

            string dialogName = "CameraDialog";
            if (typeof(ICamera).IsAssignableFrom(deviceType))
            {
                dialogParameters.Add("Title", LangProvider.GetLang("Camera"));

            }
            else if (typeof(IMotionCard).IsAssignableFrom(deviceType))
            {
                dialogParameters.Add("Title", LangProvider.GetLang("MotionCard"));
                dialogName = "MotionCardDialog";
            }
            else if (typeof(ILightController).IsAssignableFrom(deviceType))
            {
                dialogParameters.Add("Title", LangProvider.GetLang("LightController"));
                dialogName = "LightDialog";
            }
            else if (typeof(ILineLaser).IsAssignableFrom(deviceType))
            {
                dialogParameters.Add("Title", LangProvider.GetLang("LineLaser"));
                dialogName = "LineLaserDialog";
            }
            else if (typeof(IPrinter).IsAssignableFrom(deviceType))
            {
                dialogParameters.Add("Title", LangProvider.GetLang("Printer"));
                dialogName = "PrinterDialog";
            }
            else if (typeof(IRobot).IsAssignableFrom(deviceType))
            {
                dialogParameters.Add("Title", LangProvider.GetLang("Robot"));
                dialogName = "RobotDialog";
            }
            else if (typeof(IFlyingPhoto).IsAssignableFrom(deviceType))
            {
                dialogParameters.Add("Title", LangProvider.GetLang("FlyingPhoto"));
                dialogName = "FlyingPhotoDialog";
            }
            _showWindow = service.Show(dialogName, dialogParameters, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAxisDialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("Axis"));

            _showWindow = service.Show("AxisDialog", dialogParameters, callback);
        }


        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAxisDialog(this IDialogService service, AxisModel model=null,Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("Axis"));
            dialogParameters.Add("DeviceType", typeof(VAxis));
            dialogParameters.Add("AxisModel", model);
            _showWindow = service.Show("AxisDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowPlcDialog(this IDialogService service, VPlc plc = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VPlc"));
            dialogParameters.Add("PlcDevice", plc);
            _showWindow = service.Show("VPlcDialog", dialogParameters, callback);
        }

        /// <summary>
        ///  批量导入PLC地址信息
        /// </summary>
        /// <param name="service"></param>
        /// <param name="plc"></param>
        /// <param name="callback"></param>
        public static void ShowImportPlcAddress(this IDialogService service, VPlc plc = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("ImportPlcAddress"));
            dialogParameters.Add("PlcDevice", plc);
            _showWindow = service.Show("ImportAddressDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowPlcAddressDialog(this IDialogService service, VPlc plc = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("PlcAddress"));
            dialogParameters.Add("PlcDevice", plc);
            _showWindow = service.Show("PlcAddressDialog", dialogParameters, callback);
        }


        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowCommuncationDialog(this IDialogService service, ICommunication comm = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("Communcation"));
            dialogParameters.Add("Model", comm);
            _showWindow = service.Show("CommuncationAddressDialog", dialogParameters, callback);
        }


        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowAxisMDialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VAxisM"));

            _showWindow = service.Show("VAxisMDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowIODialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VIO"));

            _showWindow = service.Show("IODialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载IO更新弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="iOModel"></param>
        /// <param name="callback"></param>
        public static void ShowIOUpdateDialog(this IDialogService service, IOModel iOModel, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VIO"));
            dialogParameters.Add("IOModel", iOModel);

            _showWindow = service.Show("IOUpdateDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowIOSimDialog(this IDialogService service, VIOSimulationModel model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VIOSimulation"));
            dialogParameters.Add("Model", model);

            _showWindow = service.Show("IOSimDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSafeRegionDialog(this IDialogService service, AxisModel model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("SafeRegion"));
            dialogParameters.Add("AxisModel", model);

            service.ShowDialog("SafeRegionDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载点位安全区域对话框
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowPosionSafeRegionDialog(this IDialogService service, PositionItem model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("SafeRegion"));
            dialogParameters.Add("PositionItem", model);

            service.ShowDialog("PosionSafeRegionDialog", dialogParameters, callback);
        }


        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSocketDialog(this IDialogService service, VCommuncation model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VSocket"));
            dialogParameters.Add("Model", model);

            service.Show("SocketDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowPCylinder(this IDialogService service, VPCylinder model, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VSocket"));
            dialogParameters.Add("Model", model);

            service.Show("PCylinderDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowIOActionDialog(this IDialogService service, VIOSimulationModel model, SimActionModel simModel, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VIOSimulation"));
            dialogParameters.Add("Model", model);
            dialogParameters.Add("SimModel", simModel);

            _showWindow = service.Show("IOActionDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载真空显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowVacuumDialog(this IDialogService service, VacuumModel vacuumModel = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VVacuum"));
            if (vacuumModel != null)
            {
                dialogParameters.Add("VacuumModel", vacuumModel);
            }
            _showWindow = service.Show("VacuumDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 面阵相机显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowVCameraDialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VCamera"));

            _showWindow = service.Show("VCameraDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 机器人显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowVRobotDialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VRobot"));

            _showWindow = service.Show("VRobotDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 飞拍模块显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowVFlyingPhotoDialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VFlyingPhoto"));

            _showWindow = service.Show("VFlyingPhotoDialog", dialogParameters, callback);
        }


        /// <summary>
        /// 面阵相机显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowVLineLaserDialog(this IDialogService service, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VLineLaserDialog"));
            _showWindow = service.Show("VLineLaserDialog", dialogParameters, callback);
        }


        /// <summary>
        /// 加载气缸显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowCylinderDialog(this IDialogService service, CylinderModel cylinderModel = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VCylinder"));
            if (cylinderModel != null)
            {
                dialogParameters.Add("DeviceType", typeof(VCylinder));
                dialogParameters.Add("CylinderModel", cylinderModel);
            }
            _showWindow = service.Show("CylinderDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载打印机显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowPrinterDialog(this IDialogService service, PrinterModel printerModel = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("VPrinter"));
            if (printerModel != null)
            {
                dialogParameters.Add("DeviceType", typeof(VPrinter));
                dialogParameters.Add("PrinterModel", printerModel);
            }

            _showWindow = service.Show("VPrinterDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载打印机显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowBatchImport(this IDialogService service, bool isIO, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("BatchImport"));
            dialogParameters.Add("IsIO", isIO);

            _showWindow = service.Show("ImportExDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载打印机显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowSocketActionDialog(this IDialogService service, VCommuncation current, SocketActionModel sModel = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("CommuncationAction"));
            dialogParameters.Add("Current", current);
            dialogParameters.Add("ActionModel", sModel);

            _showWindow = service.Show("SocketActionDialog", dialogParameters, callback);
        }

        public static void ShowSlaveActionDialog(this IDialogService service, VCommuncation current, SocketActionModel sModel = null, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("CommuncationAction"));
            dialogParameters.Add("Current", current);
            dialogParameters.Add("ActionModel", sModel);

            _showWindow = service.Show("SlaveActionDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 加载打印机显示属性弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowTeachPositionDialog(this IDialogService service, double position, string axisName, bool isGroup = false, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("Teach"));
            dialogParameters.Add("Position", position);
            dialogParameters.Add("AxisName", axisName);
            dialogParameters.Add("IsGroup", isGroup);

            _showWindow = service.Show("TeachPositionDialog", dialogParameters, callback);
        }

        /// <summary>
        /// 机器人示教对话框
        /// </summary>
        /// <param name="service"></param>
        /// <param name="position"></param>
        /// <param name="axisName"></param>
        /// <param name="isGroup"></param>
        /// <param name="callback"></param>
        public static void ShowTeachRobotPositionDialog(this IDialogService service,List<KeyValue> point, bool IsSixRobot, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("Teach"));
            dialogParameters.Add("IsSixRobot", IsSixRobot);
            dialogParameters.Add("Point", point);
            //dialogParameters.Add("PositionY", point[1].Value);
            //dialogParameters.Add("PositionZ", point[2].Value);
            //dialogParameters.Add("PositionZ", point[3].Value);


            _showWindow = service.Show("TeachRobotPositionDialog", dialogParameters, callback);
        }

        public static void ShowTeachFlyingPhotoPointDialog(this IDialogService service, long encoderValue, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("Teach"));
            dialogParameters.Add("EncoderValue", encoderValue);

            _showWindow = service.Show("TeachFlyingPhotoPointDialog", dialogParameters, callback);
        }


        /// <summary>
        /// 运动条件设置弹窗
        /// </summary>
        /// <param name="service"></param>
        /// <param name="module"></param>
        public static void ShowMotionConditionsDialog(this IDialogService service, AxisModel model, List<AxisModel> list, string actionname, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", "运动条件");
            dialogParameters.Add("Axis", model);
            dialogParameters.Add("AxisList", list);
            dialogParameters.Add("Action", actionname);

            _showWindow = service.Show("MotionConditionsDialog", dialogParameters, callback);
        }

        public static void ShowAlarmConfigCustomDialog(this IDialogService service, string AlarmCode, string AlarmContent, string AlarmEnglish, Action<IDialogResult> callback = null)
        {
            service.CloseExistShowWindow();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", LangProvider.GetLang("AlarmConfigCustom"));
            dialogParameters.Add("AlarmCode", AlarmCode);
            dialogParameters.Add("AlarmContent", AlarmContent);
            dialogParameters.Add("AlarmEnglish", AlarmEnglish);

            _showWindow = service.Show("ErrorCustomDialog", dialogParameters, callback);
        }
    }

}