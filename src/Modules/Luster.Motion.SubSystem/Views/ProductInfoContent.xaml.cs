using Luster.Motion.Integration.Web;
using Luster.Motion.SubSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// ProductInfoContent.xaml 的交互逻辑
    /// </summary>
    public partial class ProductInfoContent : UserControl
    {
        public string MainVersion = "NN.NN";
        public ProductInfoContent()
        {
            InitializeComponent();
            MainVersion = "03.07";
            //MainVersion = Common.Tools.SoftVersionTool.GetVersion("LusterMotion.exe");
            //MainVersion = $"{MainVersion.Substring(0, 1).PadLeft(2, '0')}.{MainVersion.Substring(MainVersion.Length - 2, 2)}";
            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is ProductInfoContentVM vModel)
            {
                TextBox txt = e.Source as TextBox;
                if (txt != null)
                {
                    if (txt.IsKeyboardFocused)
                    {
                        vModel.SoftVersion = $"Luster-{MainVersion}-{vModel.VisionVersion}-{vModel.RobotVersion}-{vModel.LaserVersion}-{GetSoftVersion()}";
                        vModel.IsSave = true;
                        //if (ConfigurationManager.AppSettings.AllKeys.Contains("UITemplate"))
                        //{
                        //    var keyvalue = ConfigurationManager.AppSettings["UITemplate"];
                        //    if (keyvalue == "")
                        //    {
                        //        vModel.SoftVersion = $"Luster-{MainVersion}-{vModel.VisionVersion}-{vModel.RobotVersion}-{vModel.LaserVersion}_{GetSoftVersion()}";
                        //        vModel.IsSave = true;
                        //    }
                        //    else
                        //    { 
                        //         vModel.SoftVersion = $"{MainVersion}_{vModel.VisionVersion}_{vModel.RobotVersion}_{vModel.LaserVersion}_{GetSoftVersion()}_VAL";
                        //        vModel.IsSave = true;
                        //    }
                        //}
                    }
                }
            }
        }

        private string GetSoftVersion()
        {
            return "250427";
            string baseDir = "";
            string result = "0.0.0.0";
            if (string.IsNullOrEmpty(baseDir))
            {
                baseDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            string fileName = System.IO.Path.Combine(baseDir, "LusterMotion.exe");
            FileInfo file = new FileInfo(fileName);
            return file.LastWriteTime.ToString("yyMMdd");
            ////日期固定值
            //return "240506";

        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProductInfoContentVM vModel)
            {
                vModel.Password = passwordBox.Password;
            }
        }
    }
}
