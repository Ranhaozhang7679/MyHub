using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class LightModel : BindableBase
    {
        /// <summary>
        /// 原光源通道号
        /// </summary>
        public int OriginalId { get; set; }
       
        /// <summary>
        /// 光源通道号
        /// </summary>
        private int _channelId;
        public int ChannelId
        {
            get => _channelId;
            set
            {
                OriginalId = _channelId;
                SetProperty(ref _channelId, value);
            }
        }

        /// <summary>
        /// 光源强度等级
        /// </summary>
        private int _intensity;
        public int Intensity
        {
            get => _intensity;
            set => SetProperty(ref _intensity, value);
        }
    }
}
