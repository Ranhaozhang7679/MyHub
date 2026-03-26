using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;
using System.IO;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    /// <summary>
    /// 报警显示模块对话框
    /// </summary>
    public class ShowModuleDialogVM: MotionDialogVM
    {
        private DateTime beginTime;

        private readonly string DirPath = "D:\\Luster\\ModuleConfig\\";

        private BitmapFrame bitmapFrame;

        public BitmapFrame BitmapFrame
        { 
            get { return bitmapFrame; } 
            set=>SetProperty(ref bitmapFrame, value);
        }

        private string _module;

        private Dispatcher _dispatcher;
        public ShowModuleDialogVM(Dispatcher dispatcher) : base()
        {
            _dispatcher = dispatcher;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            beginTime = DateTime.Now;
            // 1.报警消息
            if (parameters.TryGetValue<string>("Module", out _module) && !string.IsNullOrEmpty(_module))
            {
                BitmapFrame = LoadModuleFrame(_module);
            }
        }

        private BitmapFrame LoadModuleFrame(string module)
        {
         
            if (!File.Exists(DirPath + module + ".png"))
            {
                return null;
            }
            else
            {
                Bitmap bitmap = new Bitmap(DirPath + module + ".png");
                IntPtr hbitmap = bitmap.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                return BitmapFrame.Create(bitmapSource);
            }
        }

        protected override void CloseDialog(string parameter)
        {
            IDialogResult result = new DialogResult();

            if (Enum.TryParse<DataStruct.Enums.AlarmProc>(parameter, out var proc))
            {
                result.Parameters.Add("AlarmProc", proc);
                result.Parameters.Add("StartTime", beginTime);
            }

            // 关闭
            _dispatcher.Invoke(() => RaiseRequestClose(result));
        }

    }
}
