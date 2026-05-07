using Luster.Motion.EditorUI.Events;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luster.Motion.EditorUI.UndoCommands
{
    public class ModuleMoveCommand : IUndoCommand
    {
        private readonly Guid[] _moduleIDs;
        private readonly int[] _originalSorts;
        private readonly int _destIndex;

        public string Description => "模块移动";

        public ModuleMoveCommand(Guid[] moduleIDs, int[] originalSorts, int destIndex)
        {
            _moduleIDs = moduleIDs;
            _originalSorts = originalSorts;
            _destIndex = destIndex;
        }

        public void Undo(FlowBus flowBus)
        {
            var modules = _moduleIDs.Select(id => flowBus.GetModule(id)).Where(m => m != null).ToList();
            if (modules.Count == 0) return;

            var curModule = flowBus.GetCurrent();
            if (curModule == null) return;

            var descModules = modules.OrderByDescending(u => u.Sort).ToList();
            foreach (var item in descModules)
            {
                curModule.Children.RemoveAt(item.Sort);
            }

            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                var origSort = _originalSorts[i];
                if (origSort >= curModule.Children.Count)
                {
                    curModule.Children.Add(module);
                }
                else
                {
                    curModule.Children.Insert(origSort, module);
                }
            }

            flowBus.SortModule(curModule);
            flowBus.Bus.GetEvent<ModuleMovedEvent>().Publish(modules.ToArray());
        }

        public void Redo(FlowBus flowBus)
        {
            var modules = _moduleIDs.Select(id => flowBus.GetModule(id)).Where(m => m != null).ToList();
            if (modules.Count == 0) return;

            var curModule = flowBus.GetCurrent();
            if (curModule == null) return;

            var srcIndex = modules[0].Sort;

            if (srcIndex > _destIndex)
            {
                var descModules = modules.OrderByDescending(u => u.Sort).ToList();
                foreach (var item in descModules)
                {
                    curModule.Children.RemoveAt(item.Sort);
                }

                int idx = _destIndex;
                foreach (var item in modules)
                {
                    curModule.Children.Insert(idx++, item);
                }
            }
            else
            {
                int idx = _destIndex;
                foreach (var item in modules)
                {
                    idx++;
                    curModule.Children.Insert(idx, item);
                }

                foreach (var item in modules)
                {
                    curModule.Children.RemoveAt(item.Sort);
                }
            }

            flowBus.SortModule(curModule);
            flowBus.Bus.GetEvent<ModuleMovedEvent>().Publish(modules.ToArray());
        }
    }
}
