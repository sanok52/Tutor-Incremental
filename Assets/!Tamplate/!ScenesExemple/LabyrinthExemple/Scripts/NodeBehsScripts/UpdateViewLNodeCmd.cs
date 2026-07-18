using UnityEngine;

public class UpdateViewLNodeCmd : LNodeBehCommand
{
    public override void Execute(SceneExecuterContext context)
    {
        RoomViewInformation info = LabyrinthModel.CreateRoomInfo(context, context.PlayerLTransfom);
        context.LabyrinthModel.UpdateRoomData(info);
    }
}