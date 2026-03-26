using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Models
{
    /// <summary>
    /// 工艺动作模块
    /// </summary>
    public class ProcActionModel
    {
        /// <summary>
        /// 进入模块
        /// </summary>
        public Guid InModule { get; set; }
        
        /// <summary>
        /// 出模块
        /// </summary>
        public Guid OutModule { get; set; }

    }
}
