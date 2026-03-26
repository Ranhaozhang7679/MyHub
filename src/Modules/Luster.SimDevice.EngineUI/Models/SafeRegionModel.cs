using Luster.Motion.DataStruct;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class SafeRegionModel : BindableBase
    {
        /// <summary>
        /// 最小范围 单位是mm
        /// </summary>
        private double _min;
        public double Min
        {
            get => _min; set
            {
                var srcV = _min;
                SetProperty(ref _min, value);
                if (srcV != value && Tag != null)
                {
                    Tag.Min = value;
                }
            }
        }

        /// <summary>
        /// 最大范围 单位是mm
        /// </summary>
        private double _max;
        public double Max
        {
            get => _max; set
            {
                var srcV = _max;
                SetProperty(ref _max, value);
                if (srcV != value && Tag != null)
                {
                    Tag.Max = value;
                }
            }
        }

        /// <summary>
        /// 轴名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }


        /// <summary>
        /// 隶属轴
        /// </summary>
        public SafeModel Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="safeModel"></param>
        public SafeRegionModel(SafeModel safeTag, double min, double max)
        {
            Min = min;
            Max = max;
            Tag = safeTag;

            // 依赖轴名称
            Name = safeTag.Pos.Name;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SafeRegionModel()
        {

        }
    }
}
