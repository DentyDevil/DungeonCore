using UnityEngine;

public class ExplorationTarget : IHeapItem<ExplorationTarget>
{
    public Vector3Int position;
    public float priority;

    public int HeapIndex { get; set; }

    public int CompareTo(ExplorationTarget other)
    {
        if(priority < other.priority) return -1;
        else if(priority > other.priority) return 1;
        else return 0;
    }

}

