public interface IRoadView
{
    void View(RoomViewInformation info);
    void ClearView();
}

public struct RoomViewInformation
{
    public MoveDirection[] transDirections;
    public LabyrinthNode Node;
    internal MoveDirection toExitDirecrion;
    internal MoveDirection[] localbleDirections;
    internal MoveDirection[] unlockableDirections;
}
