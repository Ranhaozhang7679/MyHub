using Luster.Motion.EditorUI.Events;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Luster.Motion.EditorUI.UndoCommands
{
    public class ModulePasteCommand : IUndoCommand
    {
        private readonly XElement[] _moduleXmls;
        private readonly Guid _parentID;
        private readonly Guid[] _pastedModuleIDs;

        public string Description => "模块粘贴";

        public ModulePasteCommand(XElement[] moduleXmls, Guid parentID, Guid[] pastedModuleIDs)
        {
            _moduleXmls = moduleXmls;
            _parentID = parentID;
            _pastedModuleIDs = pastedModuleIDs;
        }

        public void Undo(FlowBus flowBus)
        {
            var parent = flowBus.GetModule(_parentID);
            if (parent == null) return;

            var modules = _pastedModuleIDs
                .Select(id => flowBus.GetModule(id))
                .Where(m => m != null)
                .OrderByDescending(m => m.Sort)
                .ToList();

            foreach (var module in modules)
            {
                parent.Children.Remove(module);
                flowBus.RemoveModuleInternal(module);
            }

            flowBus.SortModule(parent);
            flowBus.Bus.GetEvent<ModuleRemoveEvent>().Publish(modules.ToArray());
        }

        public void Redo(FlowBus flowBus)
        {
            var parent = flowBus.GetModule(_parentID);
            if (parent == null) return;

            List<IMotionModule> newModules = new List<IMotionModule>();
            foreach (var xModule in _moduleXmls)
            {
                var restoredModule = flowBus.ParseModule(xModule, parent, -1);
                newModules.Add(restoredModule);
            }

            flowBus.SortModule(parent);
            flowBus.RebuildReferences();

            flowBus.Bus.GetEvent<ModulePastedEvent>().Publish(newModules.ToArray());
        }
    }
}
