using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    /// <summary>
    /// 确认按钮，接收输入，最终给结果
    /// </summary>
    public interface IConfirmButton
    {
        /// <summary>
        /// 
        /// </summary>
        public event Action<string, bool> WaitConfirmEvent;

        /// <summary>
        /// 确认动作
        /// </summary>
        /// <param name="selectButton"></param>
        void Confirm(string selectButton);

        /// <summary>
        /// 选择的Button
        /// </summary>
        string SelectButton { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        string Module { get; }

        /// <summary>
        /// 获取按钮
        /// </summary>
        /// <returns></returns>
        string[] GetButtons();
    }
}
