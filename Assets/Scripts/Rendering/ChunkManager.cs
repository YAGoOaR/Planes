using System.Collections.Generic;
using UnityEngine;

//A script that places game chunks
public class ChunkManager : MonoBehaviour
{
    const float HALF = 0.5f;

    Transform cameraTransform;
    ChunksArray chunkArray;
    [SerializeField]
    int visibleChunks;
    [SerializeField]
    int chunkSize = 2000;

    public int ChunkSize
    {
        get { return chunkSize; }
        set { chunkSize = value; }
    }

    int position;
    int prevPosition;

    class ChunksArray
    {
        int chunkSize;
        private List<Chunk> chunks = new List<Chunk>();

        public ChunksArray(int chunkSize)
        {
            this.chunkSize = chunkSize;
        }

        public Chunk findByPosition(int pos)
        {
            foreach (Chunk chunk in this.chunks)
            {
                if (chunk.position == pos)
                {
                    return chunk;
                }
            }
            return null;
        }

        public void createChunk(int pos)
        {
            GameObject chunkAsset = GameAssets.Instance.GetChunk(pos);
            if (chunkAsset != null)
            {
                Chunk newChunk = new Chunk(pos, GameObject.Instantiate(chunkAsset, new Vector3((-pos + HALF) * this.chunkSize, 0, 0), Quaternion.identity));
                this.chunks.Add(newChunk);
            }
        }

        void removeChunk(Chunk chunk)
        {
            Object.Destroy(chunk.obj);
            this.chunks.Remove(chunk);
        }

        public void clearChunksAround(int position, int range)
        {
            List<Chunk> chunksToRemove = new List<Chunk>();
            foreach (Chunk chunk in this.chunks)
            {
                if (Mathf.Abs(chunk.position - position) > range)
                {
                    chunksToRemove.Add(chunk);
                }
            }
            foreach (Chunk chunk in chunksToRemove)
            {
                this.removeChunk(chunk);
            }
        }
    }

    class Chunk
    {
        public Chunk(int pos, GameObject obj)
        {
            this.position = pos;
            this.obj = obj;
        }
        public int position;
        public GameObject obj;
    }

    void Start()
    {
        cameraTransform = Camera.main.transform;
        chunkArray = new ChunksArray(chunkSize);
    }

    void Update()
    {
        prevPosition = position;
        position = -Mathf.FloorToInt(cameraTransform.position.x / chunkSize);
        if (position != prevPosition)
        {
            for (int i = -visibleChunks; i < visibleChunks; i++)
            {
                int chunkPos = position + i;
                if (chunkArray.findByPosition(chunkPos) == null)
                {
                    chunkArray.createChunk(chunkPos);
                }
            }
            chunkArray.clearChunksAround(position, visibleChunks);
        }
    }
}
