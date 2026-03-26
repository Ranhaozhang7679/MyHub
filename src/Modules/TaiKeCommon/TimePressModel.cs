using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.IO.Ports;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore;
using System.Windows.Data;


namespace TaiKeCommon  
{
    public class TimePressModel
    {

        public string Time { get; set; } 



        public double Press { get; set; } 

    }
}
