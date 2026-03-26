using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    public Dictionary<Vector3Int, RoomData> tileRoomMap = new();
    List<RoomData> allRooms = new();
    int nextRoomId = 1;
    PathfindingGrid Grid => PathfindingManager.Instance.Grid;
    JobManager JobManager => JobManager.Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CreateRoom(AutoDoor autoDoor)
    {
        Node nodeDoorPos = Grid.NodeFromWorldPoint(autoDoor.transform.position);

        RemoveRoomIfExists(Vector3Int.FloorToInt(autoDoor.transform.position));
        foreach (Node neighbors in Grid.GetOrthogonalNeighbors(nodeDoorPos))
        {
            if(neighbors != null && neighbors.isWalkable && !neighbors.isDoor)
            {
                if (tileRoomMap.ContainsKey(Vector3Int.FloorToInt(neighbors.worldPosition))) RemoveRoomIfExists(Vector3Int.FloorToInt(neighbors.worldPosition));
                
                var (roomTiles, doors) = GetRoomTiles(100, neighbors);

                if (roomTiles.Count < 100)
                {
                    RoomData newRoom = new RoomData(roomTiles, nextRoomId, false);
                    newRoom.DoorsInRoom.AddRange(doors.Distinct());

                    foreach (var roomTile in roomTiles)
                    {
                        tileRoomMap[roomTile] = newRoom;
                    }
                    Debug.LogWarning($"Комната успешно создана и записана. RoomId - {nextRoomId}, Размер комнаты - {roomTiles.Count} тайлов. Дверей в комнате - {newRoom.DoorsInRoom.Count}");
                    allRooms.Add(newRoom);
                    nextRoomId++;
                }
            }
        }
    }

    (List<Vector3Int> roomTiles, List<Vector3Int> doors) GetRoomTiles(int maxRoomSize, Node doorNode)
    {
        List<Vector3Int> roomTiles = new();
        List<Vector3Int> foundDoors = new();

        Queue<Node> queue = new Queue<Node>();

        roomTiles.Add(Vector3Int.FloorToInt(doorNode.worldPosition));
        queue.Enqueue(doorNode);

        while (queue.Count > 0 && roomTiles.Count < maxRoomSize)
        {
            Node currentNode = queue.Dequeue();
            foreach (var neighbors in Grid.GetOrthogonalNeighbors(currentNode))
            {
                Vector3Int neighborsIntPos = Vector3Int.FloorToInt(neighbors.worldPosition);
                if (!roomTiles.Contains(neighborsIntPos) && neighbors.isWalkable && !neighbors.isDoor)
                {
                    roomTiles.Add(neighborsIntPos);
                    queue.Enqueue(neighbors);
                }
                else if (neighbors.isDoor && !foundDoors.Contains(Vector3Int.FloorToInt(neighbors.worldPosition)))
                {
                    foundDoors.Add(Vector3Int.FloorToInt(neighbors.worldPosition));
                }
            }
        }
        return (roomTiles, foundDoors);
    }

    void RemoveRoomIfExists(Vector3Int neighbors)
    {
        if (tileRoomMap.ContainsKey(neighbors))
        {
            allRooms.Remove(tileRoomMap[neighbors]);

            RoomData roomToDelete = tileRoomMap[neighbors];

            foreach (var tile in roomToDelete.RoomTiles)
            {
                tileRoomMap.Remove(tile);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (tileRoomMap == null || tileRoomMap.Count == 0) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        foreach (Vector3Int pos in tileRoomMap.Keys)
        {
            Gizmos.DrawCube(pos + new Vector3(0.5f, 0.5f, 0f), Vector3.one);
        }
    }
}
