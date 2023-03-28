using System.Collections.Generic;
using UnityEngine;

//Script that places random visible chunks of clouds
public class CloudManager : MonoBehaviour
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
    Transform chunkHolder;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        chunks = new List<int>();
        chunksObj = new List<GameObject>();
        chunkHolder = GameManager.Instance.chunkHolder;
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
                    RemoveCloudChunk(i);
                }
            }

            for (int i = -visibleChunks; i < visibleChunks; i++)
            {
                int chunkPos = i * chunkSize + position;
                int j = chunks.IndexOf(chunkPos);

                if (j == -1)
                {
                    CreateCloudChunk(chunkPos);
                }

            }
        }
    }

    void CreateCloudChunk(int pos)
    {
        chunks.Add(pos);
        chunksObj.Add(Instantiate(GameAssets.Instance.PickRandomCloud(), new Vector3(pos + Random.Range(0, chunkSize - objectSize), cloudHeight, 0), Quaternion.identity, chunkHolder));
    }

    void RemoveCloudChunk(int i)
    {
        Destroy(chunksObj[i]);
        chunksObj.RemoveAt(i);
        chunks.RemoveAt(i);
    }
}
