using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool isWalkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public int movementPenalty;

    public int fCost {get { return gCost + hCost; } }

    public Node parent;

    public Node(bool _isWalcable, Vector3 _worldPos, int _gridX, int _gridY, int _movementPenalty)
    {
        isWalkable = _isWalcable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _movementPenalty;
    }

    public int HeapIndex { get; set; }

    public int CompareTo(Node nodeToCompare)
    {
        if (fCost < nodeToCompare.fCost) return 1;
        else if(fCost > nodeToCompare.fCost) return -1;
        else if(fCost == nodeToCompare.fCost)
        {
            if (hCost < nodeToCompare.hCost) return 1;
            else if(hCost > nodeToCompare.hCost) return -1;
        }
        return 0;
    }

}
