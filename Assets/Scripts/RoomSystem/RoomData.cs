using System.Collections.Generic;
using UnityEngine;

public class RoomData
{
    public int RoomId { get; }
    public List<Vector3Int> RoomTiles { get; }
    public List<Vector3Int> DoorsInRoom { get; } = new();

    public int TileCount => RoomTiles.Count;

    public RoomData(List<Vector3Int> roomTiles, int roomId)
    {
        RoomTiles = new List<Vector3Int>(roomTiles);
        RoomId = roomId;
    }
}
