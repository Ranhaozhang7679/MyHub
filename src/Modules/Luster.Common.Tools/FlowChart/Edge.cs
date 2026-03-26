using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools.FlowChart
{
    /// <summary>
    /// 边信息
    /// </summary>
    [Serializable]
    public class Edge
    {
        public string SoureKey { get; set; }
        public string SoureText { get; set; }
        public string TargetKey { get; set; }
        public string TargetText { get; set; }
        public string LabelText { get; set; }

        public Color EdgeColor { get; set; }= Color.DimGray;
    }
}
