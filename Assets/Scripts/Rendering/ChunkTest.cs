using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkTest : MonoBehaviour
{

    private ChunkManager chunkManager;
    public int width;

    void OnDrawGizmos()
    {
        chunkManager = gameObject.GetComponent<ChunkManager>();
        for (int i = 0; i < width; i++) Gizmos.DrawLine(new Vector3(-i * chunkManager.chunkSize, -100, 0), new Vector3(-i * chunkManager.chunkSize, 300, 0));
    }
}
