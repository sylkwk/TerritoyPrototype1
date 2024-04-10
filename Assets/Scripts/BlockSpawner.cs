using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public GameObject blockPrefab; // 블록 프리팹
    public Transform[] spawnPositions; // 스폰 위치

    void Start()
    {
        SpawnBlocks();
    }

    void SpawnBlocks()
    {
        foreach (var position in spawnPositions)
        {
            Instantiate(blockPrefab, position.position, Quaternion.identity, position);
        }
    }
}