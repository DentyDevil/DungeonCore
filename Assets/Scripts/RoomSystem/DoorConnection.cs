using UnityEngine;

public class DoorConnection
{
    public AutoDoor Door { get; }
    public RoomData TargetRoom { get; }
    public Vector3Int DoorTilePosition { get; }

    public DoorConnection(AutoDoor door, RoomData targetRoom, Vector3Int doorTilePosition)
    {
        Door = door;
        TargetRoom = targetRoom;
        DoorTilePosition = doorTilePosition;
    }
}
