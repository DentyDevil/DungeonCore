using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DungeonCore : MonoBehaviour
{
    int boneCount = 2;

    public GameObject spriteSkeletonPrefab;
    public TextMeshProUGUI boneCountText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        
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
