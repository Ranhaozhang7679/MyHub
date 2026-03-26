using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Interfaces
{
    /// <summary>
    /// 活动对象
    /// </summary>
    public interface IActor
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="interactor">渲染主题</param>
        void AddActor(IInteractor interactor);
    }
}