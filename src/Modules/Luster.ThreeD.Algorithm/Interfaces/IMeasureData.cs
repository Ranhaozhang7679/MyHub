using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Interfaces
{
    /// <summary>
    /// 测量对象
    /// </summary>
    public interface IMeasure
    {
        /// <summary>
        /// 公差类别
        /// </summary>
        List<LTolerance> Tolerances { get; set; }

        /// <summary>
        /// 通过公差类型获取对应的值
        /// </summary>
        /// <param name="toleranceType"></param>
        /// <returns></returns>
        double GetMeasure(ToleranceType toleranceType);

        /// <summary>
        /// 获取公差类型
        /// </summary>
        /// <returns></returns>
        ToleranceType[] GetToleranceTypes();
    }
}