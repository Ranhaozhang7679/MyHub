using Luster.Motion.EditorUI.Events;
using Luster.TaskFlow.Motion;
using System;
using System.Xml.Linq;

namespace Luster.Motion.EditorUI.UndoCommands
{
    public class ModuleRemoveCommand : IUndoCommand
    {
        private readonly XElement[] _moduleXmls;
        private readonly Guid _parentID;
        private readonly int[] _originalSorts;

        public string Description => "模块删除";

        public ModuleRemoveCommand(XElement[] moduleXmls, Guid parentID, int[] originalSorts)
        {
            _moduleXmls = moduleXmls;
            _parentID = parentID;
            _originalSorts = originalSorts;
        }

        public void Undo(FlowBus flowBus)
        {
            var parent = flowBus.GetModule(_parentID);
            if (parent == null) return;

            for (int i = 0; i < _moduleXmls.Length; i++)
            {
                flowBus.ParseModule(_moduleXmls[i], parent, _originalSorts[i]);
            }

            flowBus.SortModule(parent);
            flowBus.RebuildReferences();

            flowBus.Bus.GetEvent<TaskChangedEvent>().Publish();
        }

        public void Redo(FlowBus flowBus)
        {
            var parent = flowBus.GetModule(_parentID);
            if (parent == null) return;

            var removedModules = new System.Collections.Generic.List<IMotionModule>();
            for (int i = _moduleXmls.Length - 1; i >= 0; i--)
            {
                var moduleID = Guid.Parse(_moduleXmls[i].Attribute("ID").Value);
                var module = flowBus.GetModule(moduleID);
                if (module == null) continue;

                parent.Children.Remove(module);
                flowBus.RemoveModuleInternal(module);
                removedModules.Add(module);
            }

            flowBus.SortModule(parent);
            flowBus.Bus.GetEvent<ModuleRemoveEvent>().Publish(removedModules.ToArray());
        }
    }
}
