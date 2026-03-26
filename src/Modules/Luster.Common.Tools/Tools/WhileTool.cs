using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    public class WhileTool
    {
        /// <summary>
        /// 性能优化Reset
        /// </summary>
        private ManualResetEventSlim _whileReset;

        private int _emptyRunNum = 3;

        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource _cancelSource = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultRun">是否默认运行</param>
        /// <param name="emptyRunNum">空转次数</param>
        public WhileTool(bool defaultRun = false, int emptyRunNum = 3)
        {
            _whileReset = new ManualResetEventSlim(defaultRun);
            _emptyRunNum = emptyRunNum;
        }

        /// <summary>
        /// 启用有条件循环
        /// </summary>
        /// <param name="funAction">具有返回值的函数</param>
        /// <param name="sleep">休眠时间</param>
        public void While(Func<bool> funAction, int sleep = 10)
        {
            _cancelSource = new CancellationTokenSource();
            int num = 0;
            while (true)
            {
                // 支持通过外部进行中断循环
                if (_cancelSource != null && _cancelSource.IsCancellationRequested)
                {
                    break;
                }

                if (!funAction())
                {
                    // 记录当前次数
                    num++;
                }

                // 站厅
                if (num > _emptyRunNum)
                {
                    _whileReset.Reset();
                }

                // 如果等待完成，则将num重置
                if (!_whileReset.IsSet)
                {
                    _whileReset.Wait();
                    num = 0;
                }

                Thread.Sleep(sleep);
            }
        }

        /// <summary>
        /// 无限循环
        /// </summary>
        /// <param name="action"></param>
        /// <param name="sleep"></param>
        public void While(Action action, int sleep = 10)
        {
            _cancelSource = new CancellationTokenSource();
            while (true)
            {
                // 支持通过外部进行中断循环
                if (_cancelSource != null && _cancelSource.IsCancellationRequested)
                {
                    break;
                }

                action();

                Thread.Sleep(sleep);
            }
        }

        /// <summary>
        /// 重新开始
        /// </summary>
        public void Continue()
        {
            _whileReset.Set();
        }

        /// <summary>
        /// 支持循环终止
        /// </summary>
        public void Stop()
        {
            _cancelSource?.Cancel();
        }
    }
}
