using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    private Transform cam;
    private ChunksArray chunkArray;
    public int visibleChunks = 0;

    public int chunkSize = 2000;
    private int pos = 0;
    private int prevPos = -1;

    class ChunksArray
    {
        public ChunksArray(int chunkSize)
        {
            this.chunkSize = chunkSize;
        }
        private int chunkSize;
        private List<Chunk> chunks = new List<Chunk>();
        public Chunk findByPos(int pos)
        {
            foreach (Chunk chunk in this.chunks)
            {
                if (chunk.pos == pos)
                {
                    return chunk;
                }
            }
            return null;
        }
        public void destroyByPos(int pos)
        {
            Chunk chunk = this.findByPos(pos);
            this.removeChunk(chunk);
        }
        public void createChunk(int pos)
        {
            GameObject chunkAsset = GameAssets.i.GetChunk(pos);
            if (chunkAsset != null)
            {
                Chunk newChunk = new Chunk(pos, GameObject.Instantiate(chunkAsset, new Vector3((-pos + .5f) * this.chunkSize, 0, 0), Quaternion.identity));
                this.chunks.Add(newChunk);
            }
        }
        void removeChunk(Chunk chunk)
        {
            Object.Destroy(chunk.obj);
            this.chunks.Remove(chunk);
        }
        public void clearAround(int pos, int range)
        {
            List<Chunk> chunksToRemove = new List<Chunk>();
            foreach (Chunk chunk in this.chunks)
            {
                if (Mathf.Abs(chunk.pos - pos) > range)
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
            this.pos = pos;
            this.obj = obj;
        }
        public int pos = 0;
        public GameObject obj = null;
    }

    void Start()
    {
        cam = Camera.main.transform;
        chunkArray = new ChunksArray(chunkSize);
    }

    void Update()
    {
        prevPos = pos;
        pos = -Mathf.FloorToInt(cam.position.x / chunkSize);
        if (pos != prevPos)
        {
            for (int i = -visibleChunks; i < visibleChunks; i++)
            {
                int cpos = pos + i;
                if (chunkArray.findByPos(cpos) == null)
                {
                    chunkArray.createChunk(cpos);
                }
            }
            chunkArray.clearAround(pos, visibleChunks);
        }
    }
}
