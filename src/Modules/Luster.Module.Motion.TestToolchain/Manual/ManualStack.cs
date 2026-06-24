using DC.Authorization;
using DC.Authorization.Models;
using System.Collections.Generic;

namespace Luster.Module.Motion.TestToolchain.Manual
{
    /// <summary>
    /// 手动操作回退栈服务（TES-34 P9-B，迁移自源端 MachineManager.mManualStack）。
    /// LIFO 记录手动操作，支持单步回退(Undo)/清空/快照，自动启动互锁（栈非空阻止自动运行）。
    /// 关键操作（手动操作入栈/回退）经 <see cref="IAuthorizationFacade"/> 权限校验 + 审计。
    /// </summary>
    /// <remarks>
    /// <b>对齐源端</b>：
    /// - 栈结构 LinkedList&lt;IManualOperation&gt;（源端用 LinkedList&lt;ManualBase&gt;，非 Stack&lt;T&gt;，为支持枚举/快照）
    /// - AddManual 同设备去重（源端 AddLast 前判 last.Component.Equals，折叠连续同设备操作，保留最早态）
    /// - RemoveLast(out complete) 契约：Backup 失败中止出栈，栈空 complete=true（源端 RemoveLast）
    /// - Start 互锁：栈非空阻止自动启动（源端 MachineManager.Start 警告“手动行为记录未恢复”）
    /// <b>源端 bug 修复</b>：源端 MotorGroupComponent.ManualOperate 漏 result&amp;&amp; 致运动失败也入栈；
    /// 本栈 <see cref="RecordIf"/> 强制 success&amp;&amp;才入栈，修正该语义。
    /// <b>非侵入</b>：独立服务门面，不改 lmv 既有 MachineManager/MotionController。
    /// </remarks>
    public class ManualStack : IManualStack
    {
        private readonly LinkedList<IManualOperation> _stack = new LinkedList<IManualOperation>();
        private readonly IAuthorizationFacade? _auth;

        /// <param name="auth">权限门面（可空：纯逻辑测试/未启用权限时放行）</param>
        public ManualStack(IAuthorizationFacade? auth = null)
        {
            _auth = auth;
        }

        /// <summary>栈中记录数</summary>
        public int Count => _stack.Count;

        /// <summary>是否可启动自动运行（栈非空时阻止，对齐源端 Start 互锁）</summary>
        public bool CanStartAuto => _stack.Count == 0;

        /// <summary>
        /// 记录手动操作（成功才入栈）。
        /// </summary>
        /// <param name="success">操作是否成功（false 不入栈，修复源端 MotorGroup 漏 result&amp;&amp; 的 bug）</param>
        /// <param name="operation">手动操作记录项</param>
        /// <returns>true=已入栈（含同设备折叠）；false=未入栈（操作失败或无权限）</returns>
        public bool RecordIf(bool success, IManualOperation operation)
        {
            if (!success || operation == null) return false;

            if (!CheckAuth(TestAuthItems.ManualOperate, "手动操作", operation.ComponentKey, operation.ToDetailString()))
            {
                return false;
            }

            // 同设备去重：栈顶同 ComponentKey 折叠（保留最早态，对齐源端 AddManual）
            if (_stack.Last != null && _stack.Last.Value.ComponentKey == operation.ComponentKey)
            {
                return true;
            }

            _stack.AddLast(operation);
            return true;
        }

        /// <summary>
        /// 回退栈顶一次（Undo）。
        /// </summary>
        /// <param name="complete">栈是否已空（回退后无剩余记录）</param>
        /// <returns>true=回退成功或栈已空；false=回退失败需中止（Backup 返回 false）</returns>
        public bool RemoveLast(out bool complete)
        {
            complete = _stack.Count == 0;
            if (complete) return true;

            if (!CheckAuth(TestAuthItems.ManualBackup, "手动回退", _stack.Last.Value.ComponentKey, null))
            {
                complete = false;
                return false;
            }

            var last = _stack.Last.Value;
            if (!last.Backup(out string msg))
            {
                // 回退失败：保留该条，中止出栈（对齐源端 RemoveLast）
                complete = false;
                return false;
            }

            _stack.RemoveLast();
            complete = _stack.Count == 0;
            return true;
        }

        /// <summary>
        /// 全部回退（Undo All）。逐条 Backup，遇失败中止并返回剩余数。
        /// </summary>
        /// <returns>剩余未回退记录数（0=全部回退成功）</returns>
        public int RemoveAll()
        {
            if (!CheckAuth(TestAuthItems.ManualBackup, "全部回退", null, null))
            {
                return _stack.Count;
            }

            while (_stack.Count > 0)
            {
                if (!RemoveLast(out _))
                {
                    return _stack.Count;
                }
            }
            return 0;
        }

        /// <summary>清空栈（不回退，对齐源端 ClearManualStack）。注意：IO 强制态可能残留</summary>
        public void Clear()
        {
            _stack.Clear();
        }

        /// <summary>栈快照拷贝（供 UI 列表显示，对齐源端 GetManualStack 返回拷贝）</summary>
        public IReadOnlyList<IManualOperation> GetSnapshot()
        {
            return new List<IManualOperation>(_stack);
        }

        /// <summary>权限校验 + 审计（对齐 RecipeManager.CheckAndAudit）</summary>
        private bool CheckAuth(AuthItem right, string operation, string before, string after)
        {
            // 用局部变量承接，null 检查后 flow analysis 识别为非 null（字段可空，局部可靠）
            var auth = _auth;
            if (auth == null) return true; // 未注入权限（纯逻辑测试）放行
            if (!auth.HasAuth(right))
            {
                auth.PopNoAuthNotification(right);
                return false;
            }
            auth.Audit(operation, $"{right.Operation}: {before ?? "(无)"} → {after ?? "(无)"}", before, after);
            return true;
        }
    }
}
