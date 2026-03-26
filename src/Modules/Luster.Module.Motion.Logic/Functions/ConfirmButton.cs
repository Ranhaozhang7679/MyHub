using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.Enums;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Models;
using System.Threading;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.interfaces;

namespace Luster.Module.Motion.Logic.Functions
{
    public class ConfirmButton : MotionFunction, IConfirmButton, IHomeFunction, IStopFunction
    {
        /// <summary>
        /// NG内容
        /// </summary>
        [Parameter("NG内容", 0, CN = "按钮种类")]
        public LArray<string> Buttons { get; set; }

        /// <summary>
        /// OK对应的结果
        /// </summary>
        [Parameter("用于确认OK结果", 1, CN = "OK按钮")]
        public string OKButton { get; set; }

        /// <summary>
        /// 确认条件
        /// </summary>
        [NotEmpty]
        [Parameter("用于确认用户的条件", 1, CN = "确认条件")]
        public LExpression WaitExpress { get; set; }

        /// <summary>
        /// 更新变量
        /// </summary>
        [Parameter("自动更新,全局变量", 2, CN = "更新变量", CanRef = ParamRef.Ref)]
        public bool GlobalVar { get; set; }

        /// <summary>
        /// 复位
        /// </summary>
        [Parameter("最终结果", 4, CN = "最终结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        /// <summary>
        /// 信号等待
        /// </summary>
        private ManualResetEventSlim mReset = new ManualResetEventSlim(false);

        /// <summary>
        /// 确认按钮
        /// </summary>
        public ConfirmButton()
        {
            this.Tips = "确认按钮";
            this.Icon = "\xe60e";
        }

        public override bool DoExcute(out string errMsg)
        {
            // 信号中断
            var condition = WaitExpress.GetResult(MyOwner);
            if (condition)
            {
                WaitConfirmEvent?.Invoke(Module, true);

                mReset.Reset();

                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"模块:{MyOwner.Alias} 等待用户确认结果!");
                mReset.Wait();
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"模块:{MyOwner.Alias} 用户确认结果完成!");

                WaitConfirmEvent?.Invoke(Module, false);
            }

            // 最终结果
            Result = SelectButton == OKButton;

            // 更新对应的全局参数
            var gValue = MyOwner.Parameters[nameof(GlobalVar)];
            if (gValue != null && gValue.RefOut != null)
            {
                gValue.RefOut.Value = Result;
            }

            return base.DoExcute(out errMsg);
        }

        public event Action<string, bool> WaitConfirmEvent;

        /// <summary>
        /// 选择的Button
        /// </summary>
        public string SelectButton { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string Module => MyOwner.Alias;

        /// <summary>
        /// 响应前端UI配置
        /// </summary>
        /// <param name="selectButton"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Confirm(string selectButton)
        {
            SelectButton = selectButton;
            mReset.Set();
        }

        /// <summary>
        /// 获取按钮
        /// </summary>
        /// <returns></returns>
        public string[] GetButtons()
        {
            List<string> buttons = new List<string>();
            var btnParameter = MyOwner.Parameters[nameof(Buttons)];
            if (btnParameter.Value != null)
            {
                buttons = btnParameter.Value as LArray<string>;
            }

            return buttons.ToArray();
        }

        public virtual List<LReference> GetReferences()
        {
            return GetReferences(nameof(WaitExpress));
        }

        public override void Home()
        {
            base.Home();

            // 终结线程
            mReset.Set();
        }

        public override void Stop()
        {
            base.Stop();

            // 终结线程
            mReset.Set();
        }
    }
}
