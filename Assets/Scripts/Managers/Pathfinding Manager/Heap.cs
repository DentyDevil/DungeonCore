public class Heap<T> where T : IHeapItem<T>
{
    T[] heap;
    int heapCount;
    public int Count { get { return heapCount; } }
    int maxHeapSize;
    int parentIndex;

    public Heap(int maxHeapSize)
    {
        this.maxHeapSize = maxHeapSize;
        heap = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        if (heapCount < maxHeapSize)
        {
            heap[heapCount] = item;
            item.HeapIndex = heapCount;
            SortUp(item);
            heapCount++;
        }
    }

    public void Clear()
    {
        heapCount = 0;
    }

    void SortUp(T item)
    {
        while (item.HeapIndex > 0)
        {
            parentIndex = (item.HeapIndex - 1) / 2;
            T parenItem = heap[parentIndex];

            if (item.CompareTo(parenItem) == 1)
            {
                Swap(item, parenItem);
            }
            else break;
        }
    }
    void Swap(T itemA, T itemB)
    {
        T box = itemA;
        heap[itemA.HeapIndex] = itemB;
        heap[itemB.HeapIndex] = box;

        int indexBox = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = indexBox;
    }

    public T RemoveFirst()
    {
        T item = heap[0];
        heapCount--;
        if (heapCount > 0)
        {
            heap[0] = heap[heapCount];
            heap[0].HeapIndex = 0;
            SortDown(heap[0]);
        }
        return item;
    }

    public T GetFirst()
    {
        return heap[0];
    }

    void SortDown(T item)
    {
        while(true)
        {
            int childIndexLeft = (2 * item.HeapIndex) + 1;
            int childIndexRight = (2 * item.HeapIndex) + 2;

            if (childIndexLeft < heapCount)
            {
                int swapIndex = childIndexLeft;
                if (childIndexRight < heapCount)
                {
                    if (heap[childIndexRight].CompareTo(heap[swapIndex]) == 1)
                    {
                        swapIndex = childIndexRight;
                    }
                }
                if (heap[swapIndex].CompareTo(item) == 1)
                {
                    Swap(item, heap[swapIndex]);
                    
                }
                else break;
            }
            else break;
        }
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
        SortDown(item);
    }

    public bool Contains(T item)
    {
        if (item.HeapIndex >= 0 && item.HeapIndex < heapCount)
        {
            return heap[item.HeapIndex].Equals(item);
        }
        else return false;
    }

    public T[] GetItems()
    {
        T[] items = new T[heapCount];

        for (int i = 0; i < heapCount; i++)
        {
            items[i] = heap[i];
        }

        return items;
    }
}
