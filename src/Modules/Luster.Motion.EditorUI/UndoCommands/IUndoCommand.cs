namespace Luster.Motion.EditorUI.UndoCommands
{
    public interface IUndoCommand
    {
        void Undo(FlowBus flowBus);
        void Redo(FlowBus flowBus);
        string Description { get; }
    }
}
