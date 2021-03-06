﻿using System.Collections.Generic;
using UnityEngine;

//Script that places random visible chunks of clouds
public class RandomObjects : MonoBehaviour
{
    private Transform cameraTransform;
    [SerializeField]
    int visibleChunks;
    [SerializeField]
    int cloudHeight = 80;
    const int chunkSize = 200;
    const int objectSize = 30;
    int position = 1;
    int prevPosition = -1;
    List<int> chunks;
    List<GameObject> chunksObj;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        chunks = new List<int>();
        chunksObj = new List<GameObject>();
    }

    void Update()
    {
        prevPosition = position;
        position = Mathf.FloorToInt(cameraTransform.position.x / chunkSize) * chunkSize;

        if (position != prevPosition)
        {

            for (int i = 0; i < chunks.Count; i++)
            {
                if (Mathf.Abs(chunks[i] - position) > chunkSize * visibleChunks)
                {
                    removeCloudChunk(i);
                }
            }

            for (int i = -visibleChunks; i < visibleChunks; i++)
            {
                int chunkPos = i * chunkSize + position;
                int j = chunks.IndexOf(chunkPos);

                if (j == -1)
                {
                    createCloudChunk(chunkPos);
                }

            }
        }
    }

    void createCloudChunk(int pos)
    {
        chunks.Add(pos);
        chunksObj.Add(GameObject.Instantiate(GameAssets.Instance.pickRandomCloud(), new Vector3(pos + Random.Range(0, chunkSize - objectSize), cloudHeight, 0), Quaternion.identity));
    }

    void removeCloudChunk(int i)
    {
        GameObject.Destroy(chunksObj[i]);
        chunksObj.RemoveAt(i);
        chunks.RemoveAt(i);
    }
}
