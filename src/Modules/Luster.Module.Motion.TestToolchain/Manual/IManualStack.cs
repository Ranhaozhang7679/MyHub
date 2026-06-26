using System.Collections.Generic;

namespace Luster.Module.Motion.TestToolchain.Manual
{
    /// <summary>
    /// 手动操作回退栈服务接口（TES-34 P9-B）。
    /// 对齐 <see cref="Luster.Module.Motion.Production.IRecipeManager"/> 门面模式，供调用方注入。
    /// </summary>
    public interface IManualStack
    {
        /// <summary>栈中记录数</summary>
        int Count { get; }

        /// <summary>是否可启动自动运行（栈非空时阻止，对齐源端 Start 互锁）</summary>
        bool CanStartAuto { get; }

        /// <summary>记录手动操作（成功才入栈，修复源端 MotorGroup 漏 result&amp;&amp; 的 bug）</summary>
        bool RecordIf(bool success, IManualOperation operation);

        /// <summary>回退栈顶一次（Undo）。complete=true 表示栈已空</summary>
        bool RemoveLast(out bool complete);

        /// <summary>全部回退（Undo All）。返回剩余未回退记录数</summary>
        int RemoveAll();

        /// <summary>清空栈（不回退，对齐源端 ClearManualStack）</summary>
        void Clear();

        /// <summary>栈快照拷贝（供 UI 列表显示）</summary>
        IReadOnlyList<IManualOperation> GetSnapshot();
    }
}
