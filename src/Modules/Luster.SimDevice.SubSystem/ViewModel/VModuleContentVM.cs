using Luster.Motion.DataStruct.Interfaces;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using Luster.SimDevice.SubSystem.Views;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using System.Reflection;
using System.ComponentModel;
using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.VDevice;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class VModuleContentVM : PageVM
    {
        private  string DirPath;

        private ObservableCollection<string> _moduleNames;

        private List<MyPoint> myPoints=new List<MyPoint>();

        public ISimDeviceEngineUI simDeviceEngineUI;

        /// <summary>
        /// 模块名称
        /// </summary>
        public ObservableCollection<string> ModuleNames
        {
            get { return _moduleNames; }
            set { SetProperty(ref _moduleNames, value); }
        }

        private ObservableCollection<string> _deviceNames;

        /// <summary>
        /// 设备名称
        /// </summary>
        public ObservableCollection<string> DeviceNames
        {
            get => _deviceNames;
            set {  SetProperty(ref _deviceNames, value);}
        }

        #region 模组点位
        private int _thickness;

        /// <summary>
        /// 矩形框的宽度
        /// </summary>
        public int Thickness
        {
            get => _thickness;
            set { SetProperty(ref _thickness, value);}
        }

        private int startXPos;

        /// <summary>
        /// 起点X
        /// </summary>
        public int StartXPos
        {
            get => startXPos;
            set => SetProperty(ref startXPos, value);
        }

        private int startYPos;

        public int StartYPos
        {
            get => startYPos;
            set { SetProperty(ref startYPos, value); }
        }

        public int endXPos;

        public int endYPos;

        /// <summary>
        /// 红色矩形框的宽度
        /// </summary>
        private int _width;

        public int Width
        {
            get => _width;
            set { SetProperty(ref _width, value); }
        }

        /// <summary>
        /// 红色矩形框的高度
        /// </summary>
        private int _height;

        public int Height
        {
            get => _height;
            set { SetProperty(ref _height, value); }
        }

        #endregion

        #region 元件点位
        private int _Dthickness;

        /// <summary>
        /// 矩形框的宽度
        /// </summary>
        public int DThickness
        {
            get => _Dthickness;
            set { SetProperty(ref _Dthickness, value); }
        }


        private int DstartXPos;

        /// <summary>
        /// 起点X
        /// </summary>
        public int DStartXPos
        {
            get => DstartXPos;
            set => SetProperty(ref DstartXPos, value);
        }

        private int DstartYPos;

        public int DStartYPos
        {
            get => DstartYPos;
            set { SetProperty(ref DstartYPos, value); }
        }

        public int DendXPos;

        public int DendYPos;

        /// <summary>
        /// 红色矩形框的宽度
        /// </summary>
        private int _Dwidth;

        public int DWidth
        {
            get => _Dwidth;
            set { SetProperty(ref _Dwidth, value); }
        }

        /// <summary>
        /// 红色矩形框的高度
        /// </summary>
        private int _Dheight;

        public int DHeight
        {
            get => _Dheight;
            set { SetProperty(ref _Dheight, value); }
        }
        #endregion

        private string _selectedmoduleName;
        /// <summary>
        /// 被选中的模块
        /// </summary>
        public string SelectedmoduleName
        {
            get { return _selectedmoduleName; }
            set { SetProperty(ref _selectedmoduleName, value); }
        }

        private string _selectedDevice;

        /// <summary>
        /// 被选中的模块
        /// </summary>
        public string SelectedDevice
        {
            get =>  _selectedDevice;
            set =>   SetProperty(ref _selectedDevice, value);
        }
        /// <summary>
        /// 所有的模组名
        /// </summary>
        private List<string> _modules = new List<string>();

        /// <summary>
        /// 模组下面所有的设备名
        /// </summary>
        private List<string> _devices = new List<string>();

        #region 设备布局图
        private BitmapFrame _machineLayOutImage = null;

        /// <summary>
        /// 设备整体布局图
        /// </summary>
        public BitmapFrame MachineLayOutImage
        {
            get { return _machineLayOutImage; }
            set { SetProperty(ref _machineLayOutImage, value); }
        }

        private BitmapFrame _machineDivideImage = null;

        /// <summary>
        /// 设备划分图
        /// </summary>
        public BitmapFrame MachineDivideImage
        {
            get => _machineDivideImage;
            set =>SetProperty(ref _machineDivideImage, value);
        } 

        private BitmapFrame _moduleLayOutImage = null;

        /// <summary>
        /// 模块布局图
        /// </summary>
        public BitmapFrame ModuleLayOutImage
        {
            get { return _moduleLayOutImage; }
            set { SetProperty(ref _moduleLayOutImage, value); }
        }

        private BitmapFrame _moduleDivideImage = null;

        /// <summary>
        /// 模块划分
        /// </summary>
        public BitmapFrame ModuleDivideImage
        {
            get => _moduleDivideImage;
            set => SetProperty(ref _moduleDivideImage, value);
        }

        #endregion
        protected VModuleContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            simDeviceEngineUI = _engine;
            ///拿到所有的模组名称
            _modules = deviceEngine.GetModules();

            //获取路径
            DirPath = deviceEngine.ModuleConfigPath;
            ///模组集合赋值
            ModuleNames = new ObservableCollection<string>();
            foreach (string module in _modules)
            {
                ModuleNames.Add(module);
            }
            SelectedmoduleName = ModuleNames[0];
            SelectedCommand = new DelegateCommand<string>(SelectModule);

            LoadMachinePic();

            ///拿到该模组下的设备
            _devices= GetDeviceByModule(SelectedmoduleName);
            DeviceNames = new ObservableCollection<string>();
            foreach (string device in _devices)
            {
                DeviceNames.Add(device);
            }
            if (DeviceNames.Count> 0)
            {
                SelectedDevice = DeviceNames[0];
            }
            ModuleLayOutImage=LoadModulePic(SelectedmoduleName);
            SelectedCommandDevice = new DelegateCommand<string>(SelectDevice);

            myPoints=ReadPoints();
        }

        #region 指令方法实现
        public DelegateCommand<string> SelectedCommand { get; set; }
        public DelegateCommand<string> SelectedCommandDevice {  get; set; }

        /// <summary>
        /// 加载设备布局指令
        /// </summary>

        private DelegateCommand _loadMachineLayOutCommand;
        public DelegateCommand LoadMachineLayOutCommand => _loadMachineLayOutCommand ?? (_loadMachineLayOutCommand = new DelegateCommand(
        () =>
        {
            LoadMachineLayOut();
        }));

        private DelegateCommand _loadModuleLayOutCommand;
        public DelegateCommand LoadModuleLayOutCommand => _loadModuleLayOutCommand ?? (_loadModuleLayOutCommand = new DelegateCommand(
        () =>
        {
            LoadModuleLayOut(SelectedmoduleName);
        }));

        /// <summary>
        /// 加载设备图片指令
        /// </summary>
        private void LoadMachineLayOut()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图片文件|*.Bmp;*.Jpg;*.Png";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    MachineLayOutImage = BitmapFrame.Create(openFileDialog.OpenFile());
                    SavePic("Machine", MachineLayOutImage);
                    MachineDivideImage = null;
                }
                catch
                {

                }
            }
            else
            {
                return;
            }

        }

        private void LoadModuleLayOut(string moduleName)
        {
            if(moduleName != null) 
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "图片文件|*.Bmp;*.Jpg;*.Png";
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        ModuleLayOutImage = BitmapFrame.Create(openFileDialog.OpenFile());
                        SavePic(moduleName, ModuleLayOutImage);
                        ModuleDivideImage = null;
                    }
                    catch
                    {

                    }
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// 开始绘图指令
        /// </summary>
        private DelegateCommand _startDrawCommand;

        public DelegateCommand StartDrawCommand => _startDrawCommand ?? (_startDrawCommand = new DelegateCommand(
        () =>
        {
            StartDraw();
        }));

        /// <summary>
        /// 开始绘图
        /// </summary>
        private void StartDraw(bool isDraw=true)
        {
            if (isDraw)
            {
                if (Thickness <= 0)
                {
                    Thickness = 1;
                }
                MachineDivideImage = DrawPic(StartXPos, StartYPos, Width, Height, Thickness);
                if (myPoints.All(x => x.Name != SelectedmoduleName) && !string.IsNullOrEmpty(SelectedmoduleName))
                {
                    myPoints.Add(new MyPoint()
                    {
                        Name = SelectedmoduleName,
                        X = this.StartXPos,
                        Y = this.StartYPos,
                        Width = this.Width,
                        Height = this.Height,
                        Thickness = this.Thickness
                      
                    });
                }
                if (myPoints.Any(x => x.Name == SelectedmoduleName) && !string.IsNullOrEmpty(SelectedmoduleName))
                {
                    var mypoint = myPoints.Find(x => x.Name == SelectedmoduleName);
                    mypoint.X = this.StartXPos;
                    mypoint.Y = this.StartYPos;
                    mypoint.Width = this.Width;
                    mypoint.Height = this.Height;
                    mypoint.Thickness = this.Thickness;
                }
            }
        }

        private DelegateCommand _startDrawDeviceCommand;
        public DelegateCommand StartDrawDeviceCommand => _startDrawDeviceCommand ?? (_startDrawDeviceCommand = new DelegateCommand(
        () =>
        {
            StartDrawDevice();
        }));

        /// <summary>
        /// 开始绘图
        /// </summary>
        private void StartDrawDevice(bool isDraw=true)
        {
            if (isDraw)
            {
                BitmapFrame frame;
                if (DThickness <= 0)
                {
                    ///线宽最小也是1
                    DThickness = 1;
                }
                frame = DrawPic(SelectedmoduleName, DStartXPos, DStartYPos, DWidth, DHeight,DThickness);
                ModuleDivideImage = frame;

                ///新增点位
                if (myPoints.All(x => x.Name != SelectedDevice) && !string.IsNullOrEmpty(SelectedDevice))
                {
                    myPoints.Add(new MyPoint()
                    {
                        Name = SelectedDevice,
                        X = this.DStartXPos,
                        Y = this.DStartYPos,
                        Width = this.DWidth,
                        Height = this.DHeight,
                        Thickness = this.DThickness
                    });
                }
                ///更新点位
                if (myPoints.Any(x => x.Name == SelectedDevice) && !string.IsNullOrEmpty(SelectedDevice))
                {
                     var mypoint=myPoints.Find(x => x.Name == SelectedDevice);
                     mypoint.X = this.DStartXPos;
                     mypoint.Y = this.DStartYPos;
                     mypoint.Width = this.DWidth;
                     mypoint.Height = this.DHeight;
                     mypoint.Thickness = this.DThickness;
                }
            }
        }

        /// <summary>
        /// 绘图方法
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        private BitmapFrame DrawPic( double startX, double startY, double Width, double Height,int Thickness)
        {
            Bitmap bitmap1 = new Bitmap($@"{DirPath}\Machine.png");
            var bitmap=bitmap1.Clone() as Bitmap;
            if(startX < 0||startY<0) 
            {
                throw new Exception("起点(x,y)不能为负值");
            }

            if (Width <= 0 || Height <=0)
            { 
                throw new Exception("(width,height)应为正数");
            }

            ///点位防呆
            if (startX+Thickness>bitmap.Width||startY+Thickness>bitmap.Height) 
            {
                throw new Exception("起点(x,y)超过图片(width,height)");
            }

            if(startX+Width+Thickness>=bitmap.Width)
            {
                ///矩形框宽度超过图片尺寸，取图片最大
                Width = bitmap.Width - startX - Thickness;
            }

            if (startY + Height+Thickness >= bitmap.Height)
            {
                ///矩形框高度超过图片尺寸，取图片最大
                Height = bitmap.Height - Height-Thickness;
            }

            for (int i = (int)startX; i <=(int)startX + Width; i++)
            {
                for (int j = (int)startY; j <=(int)startY + Thickness; j++)
                {
                    bitmap.SetPixel(i, j, System.Drawing.Color.Red);
                }
            }

            for (int i = (int)startY; i <=(int)startY + Height; i++)
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

        /// <summary>
        /// 绘图方法
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        private BitmapFrame DrawPic(string frameFile, double startX, double startY, double Width, double Height, int Thickness = 1)
        {
            Bitmap bitmap1 = new Bitmap($@"{DirPath}\{frameFile}.png");
            var  bitmap = bitmap1.Clone() as Bitmap;

            if (startX < 0 || startY < 0)
            {
                throw new Exception("起点(x,y)不能为负值");
            }

            if (Width <= 0 || Height <= 0)
            {
                throw new Exception("(width,height)应为正数");
            }

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

        /// <summary>
        /// 模块选择
        /// </summary>
        /// <param name="Module"></param>
        private void SelectModule(string Module)
        {
            if (!string.IsNullOrEmpty(Module))
            {
                SelectedmoduleName = Module;
                ModuleLayOutImage=LoadModulePic(SelectedmoduleName);

                _devices=GetDeviceByModule(SelectedmoduleName);
                DeviceNames.Clear();
                foreach(var device in _devices)
                {
                    DeviceNames.Add(device);
                }
            }
            MachineDivideImage = null;
            var myPoint = myPoints.FirstOrDefault(x => x.Name == Module);
            if (myPoint != null)
            {
                StartXPos = myPoint.X;
                StartYPos = myPoint.Y;
                Width = myPoint.Width;
                Height = myPoint.Height;
                Thickness = myPoint.Thickness;
                StartDraw();
            }
            else
            {
                StartDraw(false);
            }
        }

        /// <summary>
        /// 设备选择
        /// </summary>
        /// <param name="Device"></param>
        private void SelectDevice(string Device)
        {
            ModuleDivideImage = null;
            if (!string.IsNullOrEmpty(Device))
            {
                SelectedDevice = Device;
                ///模组图不为空且包含点位信息
                if (ModuleLayOutImage != null&& myPoints.Any(x => x.Name == Device))
                {
                    MyPoint point=myPoints.FirstOrDefault(x => x.Name == Device);
                    ModuleDivideImage = DrawPic(SelectedmoduleName, point.X, point.Y, point.Width, point.Height, point.Thickness);
                }
            }
            var myPoint=myPoints.FirstOrDefault(x=>x.Name==Device);
            if (myPoint != null)
            {
                DStartXPos = myPoint.X;
                DStartYPos = myPoint.Y;
                DWidth = myPoint.Width;
                DHeight = myPoint.Height;
                DThickness = myPoint.Thickness;
                StartDrawDevice();
            }
            else { StartDrawDevice(false); }
        }

        /// <summary>
        /// 保存模组
        /// </summary>
        private DelegateCommand _saveModulePointCommand;

        public DelegateCommand SaveModulePointCommand => _saveModulePointCommand ?? (_saveModulePointCommand =
            new DelegateCommand(() =>
            {
                WritePoints(myPoints);
            }));

        private DelegateCommand _saveDevicePointCommand;

        public DelegateCommand SaveDevicePointCommand => _saveDevicePointCommand ?? (_saveDevicePointCommand =
            new DelegateCommand(() =>
            {
                WritePoints(myPoints);
            }));

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="frame"></param>
        private void SavePic(string FileName, BitmapFrame frame)
        {
            if (!Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }
            if (frame != null)
            {
                string FullPath = Path.Combine(DirPath ,FileName) + ".png";
                using FileStream stream = new FileStream(FullPath, FileMode.Create, FileAccess.Write);
                BitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                pngBitmapEncoder.Frames.Add(BitmapFrame.Create(frame));
                pngBitmapEncoder.Save(stream);
                stream.Dispose();
                stream.Close();
            }
        }

        /// <summary>
        /// 加载模组图
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        private BitmapFrame LoadModulePic(string FileName)
        {
            if (!File.Exists(DirPath +"\\" +FileName + ".png"))
            {
                return null;
            }
            else
            {
                Bitmap bitmap1 = new Bitmap(DirPath + "\\" + FileName + ".png");
                Bitmap bitmap = bitmap1.Clone() as Bitmap;
                bitmap1.Dispose();
                IntPtr hbitmap = bitmap.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                return BitmapFrame.Create(bitmapSource);
            }
        }

       /// <summary>
       /// 加载设备图片
       /// </summary>
        private void LoadMachinePic()
        {
            if (!File.Exists($@"{DirPath}\Machine.png")) 
            {
                return;
            }
            Bitmap bitmap1 = new Bitmap($@"{DirPath}\Machine.png");
            Bitmap bitmap = bitmap1.Clone() as Bitmap;
            bitmap1.Dispose();
            IntPtr hbitmap = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            MachineLayOutImage= BitmapFrame.Create(bitmapSource);
        }

        /// <summary>
        /// 通过模块查找设备
        /// </summary>
        /// <param name="ModuleName"></param>
        /// <returns></returns>
        private List<string> GetDeviceByModule(string ModuleName)
        {
            var deviceList=  deviceEngine.GetDevices().Where(x=>x.Module == ModuleName).
                Where(x=>x is VCylinder || x is VVacuum||x is VIOSimulation||x is VAxis||x is VPlc).ToList();

            if (deviceList.Count == 0)
            {
                return new List<string>();
            }
            else
            {
                List<string> list = new List<string>();
                deviceList.ForEach(x => list.Add(x.Name));
                return list;
            }
        }

        #endregion

        #region 读取&保存点位
        /// <summary>
        /// 读取点位
        /// </summary>
        /// <returns></returns>
        private List<MyPoint> ReadPoints()
        {
            var points = new List<MyPoint>();
            if (!System.IO.Directory.Exists(DirPath))
            {
                System.IO.Directory.CreateDirectory(DirPath);
                return points;
            }
            if(!System.IO.File.Exists(DirPath+"\\"+"Points.Xml")) 
            {
                return points;
            }
            FileInfo file = new FileInfo(DirPath + "\\" + "Points.Xml");
            if (file.Length<=0)
            {
               return points;
            }
            XElement xElement = XElement.Load(DirPath + "\\" + "Points.Xml");
            var xParams = xElement.Elements();
            foreach (var x in xParams)
            {
                MyPoint point = new MyPoint();
                points.Add(point.ParseXml(x));
            }
            return points;
        }

        /// <summary>
        /// 写入点位
        /// </summary>
        /// <param name="MyPoints"></param>
        private void WritePoints(List<MyPoint> MyPoints)
        {
            if (!System.IO.Directory.Exists(DirPath))
            {
                System.IO.Directory.CreateDirectory(DirPath);
            }
            if (!System.IO.File.Exists(DirPath + "\\" + "Points.Xml"))
            {
                File.Create(DirPath + "\\" + "Points.Xml");
            }
            XElement xPoints = new XElement("Points");
            foreach(var myPoints in MyPoints)
            {
               xPoints.Add(myPoints.ExportXml());
            }
            xPoints.Save(DirPath + "\\" + "Points.Xml");
        }

        #endregion
    }
}
