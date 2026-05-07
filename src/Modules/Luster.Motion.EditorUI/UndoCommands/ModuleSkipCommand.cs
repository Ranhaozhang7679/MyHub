using Luster.Motion.EditorUI.Events;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Linq;

namespace Luster.Motion.EditorUI.UndoCommands
{
    public class ModuleSkipCommand : IUndoCommand
    {
        private readonly (Guid ID, RunStatus OriginalStatus)[] _moduleStates;

        public string Description => "模块忽略";

        public ModuleSkipCommand((Guid ID, RunStatus OriginalStatus)[] moduleStates)
        {
            _moduleStates = moduleStates;
        }

        public void Undo(FlowBus flowBus)
        {
            foreach (var (id, origStatus) in _moduleStates)
            {
                var module = flowBus.GetModule(id);
                if (module != null)
                {
                    module.Status = origStatus;
                }
            }

            var modules = _moduleStates
                .Select(s => flowBus.GetModule(s.ID))
                .Where(m => m != null)
                .ToArray();

            flowBus.Bus.GetEvent<ModuleSkipEvent>().Publish(modules);
        }

        public void Redo(FlowBus flowBus)
        {
            foreach (var (id, _) in _moduleStates)
            {
                var module = flowBus.GetModule(id);
                if (module != null)
                {
                    if (module.Status == RunStatus.Skip)
                    {
                        module.Status = RunStatus.Default;
                    }
                    else
                    {
                        module.Status = RunStatus.Skip;
                    }
                }
            }

            var modules = _moduleStates
                .Select(s => flowBus.GetModule(s.ID))
                .Where(m => m != null)
                .ToArray();

            flowBus.Bus.GetEvent<ModuleSkipEvent>().Publish(modules);
        }
    }
}
