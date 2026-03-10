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

    public List<Node> FindPath(Vector3Int startPos, Vector3Int targetPos, bool allowDigging)
    {
        grid.ResetNodeCosts();
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
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in grid.GetNeighbors(currentNode))
            {
                if ((!neighbor.isWalkable && !neighbor.isDoor && !allowDigging) || closedSet.Contains(neighbor)) continue;

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;

                if (newCostToNeighbor < neighbor.gCost || !heap.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;
                    if (heap.Contains(neighbor)) heap.UpdateItem(neighbor);
                    else heap.Add(neighbor);
                }
            }
        }
        return null;
    }

    List<Node> RetracePath(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        return path;
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
