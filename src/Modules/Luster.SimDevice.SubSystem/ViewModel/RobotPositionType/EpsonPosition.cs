using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.RobotPositionType
{
    public class EpsonPosition
    {
        /// <summary>
        /// 标签
        /// </summary>
        public string Num { get; set; }
        public string Mark { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        public string U { get; set; }
        public string V { get; set; }
        public string W { get; set; }
        public string R { get; set; }
        public string S { get; set; }
        public string T { get; set; }
        public string Hand { get; set; }
        public string Elbow { get; set; }
        public string Wrist { get; set; }
        public string J1Flag { get; set; }
        public string J2Flag { get; set; }
        public string J4Flag { get; set; }
        public string J6Flag { get; set; }
        public string J1Angle { get; set; }
        public string J4Angle { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}
