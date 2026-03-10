using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExploreRoomState : EnemyBaseState
{
    List<Node> room;
    Node startNode;
    int maxSizeRoom;

    Queue<Vector3Int> targets = new Queue<Vector3Int>();
    public EnemyExploreRoomState(WarriorEnemy enemy, EnemyStateMachine stateMachine, Node _startNode, EnemyBaseState _nextState) : base(enemy, stateMachine)
    {
        startNode = _startNode;
        maxSizeRoom = DungeonCore.Instance.maxRoomSize; ;
    }

    public override void Enter()
    {
        if (room != null) return;

        room = GetRoomTiles(startNode, maxSizeRoom);

        foreach (Node node in room)
        {
            if (!enemy.visitedCells.Contains(Vector3Int.FloorToInt(node.worldPosition))) enemy.visitedCells.Add(Vector3Int.FloorToInt(node.worldPosition));
            else continue;
        }

        if (room.Count == 0) return;

        for (int i = 0; i < 3; i++) 
        {
            int randomIndex = Random.Range(0, room.Count);
            Node randNode = room[randomIndex];
            targets.Enqueue(Vector3Int.FloorToInt(randNode.worldPosition));
        }
    }
    public override void Execute()
    {
       

        if (targets.Count > 0)
        {
            List<Node> nextPoint = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(enemy.transform.position), targets.Dequeue(), false);
            stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, nextPoint, enemy.enemy, this));
        }
        else
        {
            stateMachine.ChangeState(new EnemyScanState(enemy, stateMachine));
        }
    }
    public override void Exit()
    {

    }

    List<Node> GetRoomTiles(Node startNode, int maxSize)
    {
        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> roomTiles = new HashSet<Node>();
        HashSet<Node> foundDoors = new HashSet<Node>();
        queue.Enqueue(startNode);
        roomTiles.Add(startNode);

        while (queue.Count > 0 && roomTiles.Count <= maxSize)
        {
            Node currentNode = queue.Dequeue();
            foreach(Node roomTileNeighbors in PathfindingManager.Instance.Grid.GetOrthogonalNeighbors(currentNode))
            {
                if (!roomTiles.Contains(roomTileNeighbors) && roomTileNeighbors.isWalkable && !roomTileNeighbors.isDoor)
                {
                    roomTiles.Add(roomTileNeighbors);
                    queue.Enqueue(roomTileNeighbors);
                }
                else if(roomTileNeighbors.isDoor)
                {
                    if (enemy.visitedCells.Contains(Vector3Int.FloorToInt(roomTileNeighbors.worldPosition)) && foundDoors.Contains(roomTileNeighbors)) continue;
                    foundDoors.Add(roomTileNeighbors);
                    Vector3Int pos = Vector3Int.FloorToInt(roomTileNeighbors.worldPosition);
                    float distance = Vector3.Distance(enemy.transform.position, roomTileNeighbors.worldPosition);
                    float priority = distance + Random.Range(-0.5f, 0.5f);

                    enemy.explorationMemory.Add(new ExplorationTarget { position = pos, priority = priority });
                }
            }
        }
        return new List<Node>(roomTiles);
    }
}
