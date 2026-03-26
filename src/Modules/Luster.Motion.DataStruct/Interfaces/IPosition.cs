using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Interfaces
{
    public interface IPosition
    {
        /// <summary>
        /// 是否在安全区域
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// 轴的名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 最小范围
        /// </summary>
        double Min { get; set; }

        /// <summary>
        /// 最大范围
        /// </summary>
        double Max { get; set; }

        /// <summary>
        /// 获取当前位置
        /// </summary>
        /// <returns></returns>
        double GetCurrentPos();

        /// <summary>
        /// 获取当前轴依赖的安全区域
        /// </summary>
        /// <returns></returns>
        List<SafeModel> GetSafeRegions();


        /// <summary>
        /// 获取当前点位依赖的安全区域
        /// </summary>
        /// <returns></returns>
        List<PosionSafeModel> GetPosionSafeRegions();

        /// <summary>
        /// 添加Postion
        /// </summary>
        /// <param name="position"></param>
        void AddPosition(SafeModel position);


        void AddPosionPosition(PosionSafeModel position);

        /// <summary>
        /// 通过ID获取Position
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IPosition GetPosition(Guid id);

        /// <summary>
        /// 移除安全区域
        /// </summary>
        /// <param name="sModel"></param>
        void RemovePosition(SafeModel sModel);

        /// <summary>
        /// 移除点位安全区域
        /// </summary>
        /// <param name="sModel"></param>
        void RemovePosionPosition(PosionSafeModel pModel);
    }
}
