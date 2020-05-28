using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjects : MonoBehaviour
{
    private Transform cam;
    public int visibleChunks = 0;
    public int cloudHeight = 80;

    private int chunkSize = 200;
    private int objSize = 30;
    private int position = 1;
    private int prevPosition = -1;
    private List<int> chunks = new List<int>();
    private List<GameObject> chunksObj = new List<GameObject>();

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        prevPosition = position;
        position = Mathf.FloorToInt(cam.position.x / chunkSize) * chunkSize;

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
                int iPos = i * chunkSize + position;
                int id = chunks.IndexOf(iPos);

                if (id == -1)
                {
                    createCloudChunk(iPos);
                }

            }
        }
    }

    void createCloudChunk(int pos)
    {
        chunks.Add(pos);
        chunksObj.Add(GameObject.Instantiate(GameAssets.instance.pickRandomCloud(), new Vector3(pos + Random.Range(0, chunkSize - objSize), cloudHeight, 0), Quaternion.identity));
    }

    void removeCloudChunk(int i)
    {
        GameObject.Destroy(chunksObj[i]);
        chunksObj.RemoveAt(i);
        chunks.RemoveAt(i);
    }

}
