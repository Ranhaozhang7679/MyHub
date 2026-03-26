using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    public interface IParallel
    {

    }

    public class Parallel : LogicFunction, IParallel
    {
        /// <summary>
        /// 并行参数
        /// </summary>
        private ParallelOptions _po = null;

        /// <summary>
        /// 用于执行并行模块的引擎
        /// </summary>
        private Dictionary<IMotionModule, MotionRunEngine> runEngine = null;
        /// <summary>
        /// 并行任务
        /// </summary>
        public Parallel()
        {
            this.Tips = "支持并行,内部多个模块并行运行";
            this.Icon = "\xe635";

            _po = new ParallelOptions();
            _po.MaxDegreeOfParallelism = Environment.ProcessorCount;
            _po.CancellationToken = new CancellationTokenSource().Token;
        }

        /// <summary>
        /// 并行模块
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            if (MyOwner.Children.Count == 0) return true;

            if (runEngine == null || runEngine.Count != MyOwner.Children.Count)
            {
                runEngine = new Dictionary<IMotionModule, MotionRunEngine>();
                foreach (var child in MyOwner.Children)
                {
                    runEngine.Add(child, new MotionRunEngine());
                }
            }

            string msg = "";

            bool isSuccess = true;

            // 对内部的模块进行并行处理
            System.Threading.Tasks.Parallel.ForEach(runEngine, _po, child =>
            {
                child.Value.Run(child.Key, ref isSuccess);

                // 出现异常中断线程
                if (!isSuccess)
                {
                    msg = child.Value.ErrorMessage;
                    _po.CancellationToken.ThrowIfCancellationRequested();
                }
            });

            // 记录错误结果
            errMsg = msg;

            // 返回运行结果
            return isSuccess;
        }
    }
}
