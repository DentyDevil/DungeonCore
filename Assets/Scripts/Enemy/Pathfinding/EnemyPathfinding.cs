using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    Heap<Node> heap;
    public PathfindingGrid grid;
    private void Awake()
    {
        heap = new Heap<Node>(grid.gridHeight * grid.gridWidth);
    }

    public void FindPath(Vector3Int startPos, Vector3Int targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        heap.Clear();
    
        HashSet<Node> closedSet = new HashSet<Node>();

        heap.Add(startNode);

        while(heap.Count > 0)
        {
            Node currentNode = heap.RemoveFirst();

            closedSet.Add(currentNode);

            if(currentNode == targetNode)
            {

            }
        }
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        else
        {
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}
