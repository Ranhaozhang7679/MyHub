using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Models
{
    public struct LoadingModel
    {
        public int Total { get; set; }

        public int CurValue { get; set; }

        public string Message { get; set; }

        public LoadingModel(int total, int curValue, string message)
        {
            Total = total; CurValue = curValue; Message = message;
        }
    }
}
