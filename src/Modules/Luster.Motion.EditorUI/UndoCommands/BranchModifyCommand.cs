using Luster.Motion.EditorUI.Events;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Luster.Motion.EditorUI.UndoCommands
{
    /// <summary>
    /// 分支模块条件修改撤销命令（增量式）
    /// 只记录和恢复变更的部分，不销毁重建整个 Switch
    /// 覆盖 Switch（LCondition）和 Judge（LExpression）的修改操作
    /// </summary>
    public class BranchModifyCommand : IUndoCommand
    {
        private readonly Guid _moduleID;

        // === Switch 专用字段 ===

        // 条件参数旧值 / 新值
        private readonly XElement _oldConditionXml;
        private readonly XElement _newConditionXml;

        // 改名的分支
        private readonly (Guid ID, string OldAlias)[] _renamedOld;
        private readonly (Guid ID, string NewAlias)[] _renamedNew;

        // 新增的分支 ID（Undo 时删除）
        private readonly Guid[] _addedBranchIDs;
        // 新增分支的 XML 快照（Redo 时恢复）
        private readonly XElement[] _addedBranchXmls;

        // 被删除的分支 XML 快照 + 原始排序位置
        private readonly XElement[] _removedBranchXmls;
        private readonly int[] _removedBranchSorts;

        // === Judge 专用字段 ===
        private readonly bool _isJudge;
        private readonly XElement _oldModuleXml;
        private readonly XElement _newModuleXml;

        /// <summary>
        /// Switch 条件编辑的撤销命令（增量式）
        /// </summary>
        public BranchModifyCommand(
            Guid moduleID,
            XElement oldConditionXml, XElement newConditionXml,
            (Guid ID, string OldAlias)[] renamedOld, (Guid ID, string NewAlias)[] renamedNew,
            Guid[] addedBranchIDs, XElement[] addedBranchXmls,
            XElement[] removedBranchXmls, int[] removedBranchSorts)
        {
            _moduleID = moduleID;
            _oldConditionXml = oldConditionXml;
            _newConditionXml = newConditionXml;
            _renamedOld = renamedOld ?? Array.Empty<(Guid, string)>();
            _renamedNew = renamedNew ?? Array.Empty<(Guid, string)>();
            _addedBranchIDs = addedBranchIDs ?? Array.Empty<Guid>();
            _addedBranchXmls = addedBranchXmls ?? Array.Empty<XElement>();
            _removedBranchXmls = removedBranchXmls ?? Array.Empty<XElement>();
            _removedBranchSorts = removedBranchSorts ?? Array.Empty<int>();
            _isJudge = false;
            _oldModuleXml = null;
            _newModuleXml = null;
        }

        /// <summary>
        /// Judge 表达式编辑的撤销命令（整体快照，因为只改一个参数）
        /// </summary>
        public BranchModifyCommand(Guid moduleID, XElement oldModuleXml, XElement newModuleXml)
        {
            _moduleID = moduleID;
            _isJudge = true;
            _oldModuleXml = oldModuleXml;
            _newModuleXml = newModuleXml;
            _oldConditionXml = null;
            _newConditionXml = null;
            _renamedOld = Array.Empty<(Guid, string)>();
            _renamedNew = Array.Empty<(Guid, string)>();
            _addedBranchIDs = Array.Empty<Guid>();
            _addedBranchXmls = Array.Empty<XElement>();
            _removedBranchXmls = Array.Empty<XElement>();
            _removedBranchSorts = Array.Empty<int>();
        }

        public string Description => _isJudge ? "分支条件修改" : "分支条件修改";

        public void Undo(FlowBus flowBus)
        {
            if (_isJudge)
            {
                UndoJudge(flowBus);
            }
            else
            {
                UndoSwitch(flowBus);
            }
        }

        public void Redo(FlowBus flowBus)
        {
            if (_isJudge)
            {
                RedoJudge(flowBus);
            }
            else
            {
                RedoSwitch(flowBus);
            }
        }

        #region Switch 增量式撤销/重做

        private void UndoSwitch(FlowBus flowBus)
        {
            var module = flowBus.GetModule(_moduleID);
            if (module == null) return;

            // 1. 恢复条件值
            if (_oldConditionXml != null && module.Parameters.ContainsKey("Condition"))
            {
                var oldCondition = new LCondition();
                oldCondition.ParserXml(_oldConditionXml);
                module.Parameters["Condition"].Value = oldCondition;
                module.UpdateReferences();
            }

            // 2. 恢复改名分支的旧 Alias
            foreach (var (id, oldAlias) in _renamedOld)
            {
                var child = flowBus.GetModule(id);
                if (child != null) child.Alias = oldAlias;
            }

            // 3. 删除新增的分支
            foreach (var id in _addedBranchIDs)
            {
                var child = flowBus.GetModule(id);
                if (child != null)
                {
                    module.Children.Remove(child);
                    flowBus.RemoveModuleInternal(child);
                }
            }

            // 4. 恢复被删除的分支
            for (int i = 0; i < _removedBranchXmls.Length; i++)
            {
                flowBus.ParseModule(_removedBranchXmls[i], module, _removedBranchSorts[i]);
            }

            flowBus.SortModule(module);
            flowBus.RebuildReferences();

            flowBus.Bus.GetEvent<TaskChangedEvent>().Publish();
        }

        private void RedoSwitch(FlowBus flowBus)
        {
            var module = flowBus.GetModule(_moduleID);
            if (module == null) return;

            // 1. 设置新条件值
            if (_newConditionXml != null && module.Parameters.ContainsKey("Condition"))
            {
                var newCondition = new LCondition();
                newCondition.ParserXml(_newConditionXml);
                module.Parameters["Condition"].Value = newCondition;
                module.UpdateReferences();
            }

            // 2. 恢复改名分支的新 Alias
            foreach (var (id, newAlias) in _renamedNew)
            {
                var child = flowBus.GetModule(id);
                if (child != null) child.Alias = newAlias;
            }

            // 3. 恢复新增的分支
            for (int i = 0; i < _addedBranchXmls.Length; i++)
            {
                flowBus.ParseModule(_addedBranchXmls[i], module, -1);
            }

            // 4. 删除之前被删除的分支（按 ID 查找并移除）
            if (_removedBranchXmls.Length > 0)
            {
                var removedIds = _removedBranchXmls
                    .Select(x => Guid.Parse(x.Attribute("ID").Value))
                    .ToHashSet();

                var toRemove = module.Children.Where(c => removedIds.Contains(c.ID)).ToList();
                foreach (var child in toRemove)
                {
                    module.Children.Remove(child);
                    flowBus.RemoveModuleInternal(child);
                }
            }

            flowBus.SortModule(module);
            flowBus.RebuildReferences();

            flowBus.Bus.GetEvent<TaskChangedEvent>().Publish();
        }

        #endregion

        #region Judge 整体快照撤销/重做

        private void UndoJudge(FlowBus flowBus)
        {
            var module = flowBus.GetModule(_moduleID);
            if (module == null) return;

            var parent = module.Parent;
            if (parent == null) return;

            int originalIndex = module.Sort;

            parent.Children.Remove(module);
            flowBus.RemoveModuleInternal(module);

            flowBus.ParseModule(_oldModuleXml, parent, originalIndex);
            flowBus.SortModule(parent);
            flowBus.RebuildReferences();

            var restoredModule = flowBus.GetModule(_moduleID);
            if (restoredModule != null) restoredModule.IsCurrent = true;

            flowBus.Bus.GetEvent<TaskChangedEvent>().Publish();
        }

        private void RedoJudge(FlowBus flowBus)
        {
            var module = flowBus.GetModule(_moduleID);
            if (module == null) return;

            var parent = module.Parent;
            if (parent == null) return;

            int originalIndex = module.Sort;

            parent.Children.Remove(module);
            flowBus.RemoveModuleInternal(module);

            flowBus.ParseModule(_newModuleXml, parent, originalIndex);
            flowBus.SortModule(parent);
            flowBus.RebuildReferences();

            var restoredModule = flowBus.GetModule(_moduleID);
            if (restoredModule != null) restoredModule.IsCurrent = true;

            flowBus.Bus.GetEvent<TaskChangedEvent>().Publish();
        }

        #endregion
    }
}
