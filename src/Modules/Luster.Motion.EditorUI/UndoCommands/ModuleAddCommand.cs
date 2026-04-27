using Luster.Motion.EditorUI.Events;
using Luster.TaskFlow.Motion;
using System;
using System.Xml.Linq;

namespace Luster.Motion.EditorUI.UndoCommands
{
    public class ModuleAddCommand : IUndoCommand
    {
        private readonly XElement _moduleXml;
        private readonly Guid _parentID;
        private readonly int _insertIndex;

        public string Description => "模块新增";

        public ModuleAddCommand(XElement moduleXml, Guid parentID, int insertIndex)
        {
            _moduleXml = moduleXml;
            _parentID = parentID;
            _insertIndex = insertIndex;
        }

        public void Undo(FlowBus flowBus)
        {
            var moduleID = Guid.Parse(_moduleXml.Attribute("ID").Value);
            var module = flowBus.GetModule(moduleID);
            if (module == null) return;

            var parent = module.Parent;
            if (parent != null)
            {
                parent.Children.Remove(module);
            }

            flowBus.RemoveModuleInternal(module);
            if (parent != null) flowBus.SortModule(parent);
            flowBus.Bus.GetEvent<ModuleRemoveEvent>().Publish(new[] { module });
        }

        public void Redo(FlowBus flowBus)
        {
            var parent = flowBus.GetModule(_parentID);
            if (parent == null) return;

            var newModule = flowBus.ParseModule(_moduleXml, parent, _insertIndex);
            flowBus.SortModule(parent);
            newModule.UpdateStation();
            flowBus.Bus.GetEvent<ModuleAddEvent>().Publish(newModule);
        }
    }
}
