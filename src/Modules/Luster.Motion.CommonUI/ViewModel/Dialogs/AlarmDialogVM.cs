#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.ViewModel.Dialogs
* 文 件 名:       AlarmDialogVM.cs
* 创建时间:       2022/7/14 13:39:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      05460fef-6080-4427-b164-203f27165bf2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/14 13:39:40
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using Luster.Common.DataStruct;
using static System.Windows.Forms.AxHost;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using Luster.Motion.TaskFlow.Engine;
using System.Security.Cryptography;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class AlarmDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 网格子项模型
        /// </summary>
        public class GridItemModel : BindableBase
        {
            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set => SetProperty(ref _isSelected, value);
            }

            public string DisplayText { get; set; }

            public int Index { get; set; }
        }
        private Dispatcher _dispatcher;
        /// <summary>
        /// 报警开始时间
        /// </summary>
        private DateTime startTime;

        // 流程引擎
        private IMotionEngine _motionEngine;

        /// <summary>
        /// 料盘行列数上限
        /// </summary>
        private const int MaxTrayDimension = 50;

        /// <summary>
        /// 报警类型
        /// </summary>
        private AlarmType _alarmType;
        public AlarmType AlarmType
        {
            get { return _alarmType; }
            set { SetProperty(ref _alarmType, value); }
        }

        /// <summary>
        /// 报警信息
        /// </summary>
        private string _alarmMsg;
        public string AlarmMsg
        {
            get { return _alarmMsg; }
            set { SetProperty(ref _alarmMsg, value); }
        }



        /// <summary>
        /// 穴位号
        /// </summary>
        private int _curve;
        public int Curve
        {
            get { return _curve; }
            set
            {
                if (SetProperty(ref _curve, value))
                {
                    SyncGridSelection();
                }
            }
        }



        /// <summary>
        /// 穴位号显示
        /// </summary>
        private bool _curveEnable;
        public bool CurveEnable
        {
            get { return _curveEnable; }
            set { SetProperty(ref _curveEnable, value); }
        }

        /// <summary>
        /// 料盘行数
        /// </summary>
        private int _trayRows;
        public int TrayRows
        {
            get => _trayRows;
            set => SetProperty(ref _trayRows, value);
        }

        /// <summary>
        /// 料盘列数
        /// </summary>
        private int _trayCols;
        public int TrayCols
        {
            get => _trayCols;
            set => SetProperty(ref _trayCols, value);
        }

        /// <summary>
        /// 网格是否显示
        /// </summary>
        private bool _gridEnable;
        public bool GridEnable
        {
            get => _gridEnable;
            set => SetProperty(ref _gridEnable, value);
        }

        /// <summary>
        /// 网格子项集合
        /// </summary>
        public ObservableCollection<GridItemModel> GridItems { get; } = new ObservableCollection<GridItemModel>();

        /// <summary>
        /// 选中网格子项命令
        /// </summary>
        public DelegateCommand<GridItemModel> SelectGridItemCommand { get; }

        /// <summary>
        /// 报警信息
        /// </summary>
        private string _alarmTitle;
        public string AlarmTitle
        {
            get { return _alarmTitle; }
            set { SetProperty(ref _alarmTitle, value); }
        }

        /// <summary>
        /// 轻度报警
        /// </summary>
        private bool _isLow;
        public bool IsLow
        {
            get { return _isLow; }
            set { SetProperty(ref _isLow, value); }
        }

        /// <summary>
        /// 中度报警
        /// </summary>
        private bool _isMiddle;
        public bool IsMiddle
        {
            get { return _isMiddle; }
            set { SetProperty(ref _isMiddle, value); }
        }

        /// <summary>
        /// PLC
        /// </summary>
        private bool _isPLC;
        public bool IsPLC
        {
            get { return _isPLC; }
            set { SetProperty(ref _isPLC, value); }
        }

        /// <summary>
        /// 中度报警
        /// </summary>
        private bool _isHigh;
        public bool IsHigh
        {
            get { return _isHigh; }
            set { SetProperty(ref _isHigh, value); }
        }

        /// <summary>
        /// 是否是NG处理
        /// </summary>
        private bool _isNg;
        public bool IsNG
        {
            get { return _isNg; }
            set { SetProperty(ref _isNg, value); }
        }

        /// <summary>
        /// 警告提示语
        /// </summary>
        private string _warningContent;
        public string WarningContent
        {
            get { return _warningContent; }
            set { SetProperty(ref _warningContent, value); }
        }


        private AlarmInfo alarmInfo;
        private string DirPath ;


        //总线
        ICommonBus commonBus;

        public AlarmDialogVM(Dispatcher dispatcher, IMotionEngine mEngine, ICommonBus _commonBus) : base()
        {
            _dispatcher = dispatcher;
            commonBus = _commonBus;
            _motionEngine = mEngine;
            DirPath = Path.Combine(commonBus.CurrentRecipe.ProjInfo.ProjPath, "ModuleConfig");
            points =ReadPoints();
            SelectGridItemCommand = new DelegateCommand<GridItemModel>(OnSelectGridItem);
        }


        private BitmapFrame moduleFrame;

        public BitmapFrame ModuleFrame
        {
            get { return moduleFrame; }
            set => SetProperty(ref moduleFrame, value);
        }


        private BitmapFrame deviceFrame;

        public BitmapFrame DeviceFrame
        {
            get { return deviceFrame; }
            set => SetProperty(ref deviceFrame, value);
        }

        private List<MyPoint> points;

        /// <summary>
        /// 选中网格子项
        /// </summary>
        private void OnSelectGridItem(GridItemModel item)
        {
            if (item == null) return;

            foreach (var gi in GridItems)
            {
                gi.IsSelected = false;
            }

            item.IsSelected = true;
            Curve = item.Index;
        }

        /// <summary>
        /// 同步网格选中状态（当 Curve 从输入框手动修改时调用）
        /// </summary>
        private void SyncGridSelection()
        {
            if (!GridEnable) return;
            foreach (var gi in GridItems)
            {
                gi.IsSelected = (gi.Index == Curve);
            }
        }

        /// <summary>
        /// 初始化网格数据
        /// </summary>
        private void InitGridItems()
        {
            GridItems.Clear();
            if (!GridEnable || TrayRows <= 0 || TrayCols <= 0) return;

            for (int row = 0; row < TrayRows; row++)
            {
                for (int col = 0; col < TrayCols; col++)
                {
                    int index = row * TrayCols + col;
                    GridItems.Add(new GridItemModel
                    {
                        DisplayText = $"{row + 1},{col + 1}",
                        Index = index,
                        IsSelected = (index == Curve)
                    });
                }
            }
        }

        /// <summary>
        /// 弹窗打开
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            // 1.报警消息
            if (parameters.TryGetValue<AlarmInfo>("AlarmInfo", out alarmInfo) && alarmInfo != null)
            {
                alarmInfo.AlarmProcEvent -= AlarmInfo_AlarmProcEvent;
                alarmInfo.AlarmProcEvent += AlarmInfo_AlarmProcEvent;

                AlarmMsg = alarmInfo.Message;
                AlarmType = alarmInfo.AlarmType;

                CurveEnable = false;
                GridEnable = false;
                var gModule = _motionEngine.Get(GlobalModule.GlobalID);
                if (gModule.Parameters.ContainsKey("Extend_料盘穴位索引")/* && AlarmMsg.Contains("料盘取料NG")*/)
                {
                    CurveEnable = true;//穴位输入框显示隐藏
                    var gItem = gModule.Parameters["Extend_料盘穴位索引"];
                    Curve = Convert.ToInt32(gItem.Value);

                    // 读取行列配置
                    if (gModule.Parameters.ContainsKey("Extend_料盘穴位行数")
                        && gModule.Parameters.ContainsKey("Extend_料盘穴位列数"))
                    {
                        int rows = Convert.ToInt32(gModule.Parameters["Extend_料盘穴位行数"].Value);
                        int cols = Convert.ToInt32(gModule.Parameters["Extend_料盘穴位列数"].Value);
                        if (rows > 0 && cols > 0 && rows <= MaxTrayDimension && cols <= MaxTrayDimension)
                        {
                            TrayRows = rows;
                            TrayCols = cols;
                            GridEnable = true;
                            InitGridItems();
                        }
                        else if (rows > MaxTrayDimension || cols > MaxTrayDimension)
                        {
                            WarningContent = $"料盘行数({rows})或列数({cols})超过上限{MaxTrayDimension}，网格选择器不可用";
                        }
                    }
                }
                switch (AlarmType)
                {
                    case AlarmType.InfoTip:
                    case AlarmType.PopInfoTip:
                        IsLow = true;
                        break;
                    case AlarmType.DeviceError:
                        IsHigh = true;
                        break;
                    case AlarmType.PlcAlarm:
                        IsPLC = true;
                        break;
                    case AlarmType.Timeout:
                    case AlarmType.FailError:
                        IsMiddle = true;
                        IsLow = true;
                        break;
                    case AlarmType.WarningTip:
                        IsHigh = true;
                        IsLow = true;
                        break;
                    case AlarmType.RetryAlarm:
                        IsLow = true;
                        break;
                    case AlarmType.ManuOperationAlarm:
                        IsLow = true;
                        break;
                }

                //var modulePoint=points.FirstOrDefault(x=>x.Name==alarmInfo.Module);
                //ModuleFrame = DrawPicture("Machine", modulePoint);

                //var devicePoint = points.FirstOrDefault(x => x.Name == alarmInfo.Name);
                //DeviceFrame=DrawPicture(alarmInfo.Module, devicePoint);

                startTime = DateTime.Now;

                // 获取报警标题
                AlarmTitle = $"{AlarmType.GetDescription()} {alarmInfo.AlarmCode}";

                if (alarmInfo.Sender is IMotionModule module && module != null)
                {
                    AlarmMsg = $"模块:{module.Alias} {AlarmMsg}";
                    if (module.NextModule != null)
                    {
                        IsNG = module.NextModule.TaskFunction is IActionNG;
                    }
                }
            }
        }

        private bool AlarmInfo_AlarmProcEvent(object arg1, AlarmProc arg2)
        {
            if (arg2 == AlarmProc.Contine)
            {
                CloseDialog(AlarmProc.Contine.ToString());

                return true;
            }
            else if (arg2 == AlarmProc.Retry && arg1 is IMotionModule m)
            {
                return m.Status != Luster.TaskFlow.Common.Enums.RunStatus.Alarmed;
            }

            return true;
        }

        /// <summary>
        /// 报警异常处理
        /// </summary>
        /// <param name="parameter"></param>
        protected override void CloseDialog(string parameter)
        {
            IDialogResult result = new DialogResult();

            if (parameter == "Continue") parameter ="Contine";
            if (Enum.TryParse<DataStruct.Enums.AlarmProc>(parameter, out var proc))
            {
                result.Parameters.Add("AlarmProc", proc);
                result.Parameters.Add("StartTime", startTime);
                result.Parameters.Add("Curve", Curve);
            }

            if (Curve < 0 || Curve >= 400)
            {
                WarningContent = $"输入的值为{Curve}:不在0~399范围内";
                return;
            }
            else
            {
                WarningContent = "";
            }



            // 关闭
            _dispatcher.Invoke(() => RaiseRequestClose(result));
        }

        private List<MyPoint> ReadPoints()
        {
            var points = new List<MyPoint>();
            if (!System.IO.Directory.Exists(DirPath))
            {
                System.IO.Directory.CreateDirectory(DirPath);
                return points;
            }
            if (!System.IO.File.Exists(DirPath +"\\"+ "Points.Xml"))
            {
                return points;
            }
            FileInfo file = new FileInfo(DirPath + "\\"+ "Points.Xml");
            if (file.Length <= 0)
            {
                return points;
            }
            XElement xElement = XElement.Load(DirPath +"\\"+"Points.Xml");
            var xParams = xElement.Elements();
            foreach (var x in xParams)
            {
                MyPoint point = new MyPoint();
                points.Add(point.ParseXml(x));
            }
            return points;
        }

        /// <summary>
        /// 画图
        /// </summary>
        /// <param name="name"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private BitmapFrame DrawPicture(string name,MyPoint point)
        { 
            if(!File.Exists($@"{DirPath}\{name}.png"))
            {
                return null; 
            }
            if(point==null)
            {
                return null;
            }
            Bitmap bitmap1 = new Bitmap($@"{DirPath}\{name}.png");
            var bitmap = bitmap1.Clone() as Bitmap;

            int startX = point.X;
            int startY = point.Y;
            int Width = point.Width;
            int Height = point.Height;
            int Thickness=point.Thickness;

            ///点位防呆
            if (startX + Thickness > bitmap.Width || startY + Thickness > bitmap.Height)
            {
                throw new Exception("起点(x,y)超过图片(width,height)");
            }
            if (startX + Width + Thickness >= bitmap.Width)
            {
                ///矩形框宽度超过图片尺寸，取图片最大
                Width = bitmap.Width - startX - Thickness;
            }
            if (startY + Height + Thickness >= bitmap.Height)
            {
                ///矩形框高度超过图片尺寸，取图片最大
                Height = bitmap.Height - Height - Thickness;
            }

            for (int i = (int)startX; i <= (int)startX + Width; i++)
            {
                for (int j = (int)startY; j <= (int)startY + Thickness; j++)
                {
                    bitmap.SetPixel(i, j, System.Drawing.Color.Red);
                }
            }

            for (int i = (int)startY; i <= (int)startY + Height; i++)
            {
                for (int j = (int)startX; j <= (int)startX + Thickness; j++)
                {
                    bitmap.SetPixel(j, i, System.Drawing.Color.Red);
                }
            }
            for (int i = (int)startX; i <= (int)startX + Width; i++)
            {
                for (int j = (int)(startY + Height); j <= (int)startY + Height + Thickness; j++)
                {
                    bitmap.SetPixel(i, j, System.Drawing.Color.Red);
                }
            }

            for (int i = (int)startY; i <= (int)startY + Height; i++)
            {
                for (int j = (int)(startX + Width); j <= (int)startX + Width + Thickness; j++)
                {
                    bitmap.SetPixel(j, i, System.Drawing.Color.Red);
                }
            }
            IntPtr hbitmap = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return BitmapFrame.Create(bitmapSource);
        }
    }
}