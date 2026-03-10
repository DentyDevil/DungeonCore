using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DungeonCore : MonoBehaviour
{
    int boneCount = 20;

    public float dungeonCoreHealthPoints = 100;
    public int maxRoomSize = 100;

    public static DungeonCore Instance;

    public GameObject spriteSkeletonPrefab;
    public TextMeshProUGUI boneCountText;

    private void Awake()
    {
        Instance = this;
    }
    void UpdateUI()
    {
        boneCountText.text = "Bones: " + boneCount;
    }

    public void SpawnSkeleton()
    {
        if(boneCount > 0)
        {
            boneCount--;
            UpdateUI();
            Instantiate(spriteSkeletonPrefab, transform.position,Quaternion.identity);
        }
        else
        {
            Debug.Log("Not enough bones...");
        }
    }

    public void AddBone()
    {
        boneCount++;
        UpdateUI();
    }
}
