using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class TriggerNoCountModel : BindableBase
    {
        private int _triggerNo;
        /// <summary>
        /// 触发输出通道号
        /// </summary>
        public int TriggerNo
        {
            get => _triggerNo;
            set => SetProperty(ref _triggerNo, value);
        }

        private int _triggerNoCount;
        /// <summary>
        /// 触发输出计数
        /// </summary>
        public int TriggerNoCount
        {
            get => _triggerNoCount;
            set => SetProperty(ref _triggerNoCount, value);
        }

        private int _triggerNoFifoCount;
        /// <summary>
        /// 触发输出Fifo计数
        /// </summary>
        public int TriggerNoFifoCount
        {
            get => _triggerNoFifoCount;
            set => SetProperty(ref _triggerNoFifoCount, value);
        }

    }
}
