using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DungeonCore : MonoBehaviour
{
    int boneCount = 20;

    public GameObject spriteSkeletonPrefab;
    public TextMeshProUGUI boneCountText;

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
