using Unity.VisualScripting;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    SpriteRenderer doorsprite;
    int unitstOnDoor = 0;

    public bool isOpen = false;
    private void Awake()
    {
        doorsprite = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        unitstOnDoor++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        unitstOnDoor--;
        if(unitstOnDoor <= 0)
        {
            unitstOnDoor = 0;
            doorsprite.enabled = true;
            isOpen = false;
        }
    }

    public void OpenDoor()
    {
        doorsprite.enabled = false;
        isOpen = true;
    }

}
