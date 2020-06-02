using UnityEngine;

//A script that visualizes chunk bounds in editor
public class ChunkTest : MonoBehaviour
{
    const float OFFSET = 200;
    private ChunkManager chunkManager;
    public int width;

    void OnDrawGizmos()
    {
        chunkManager = gameObject.GetComponent<ChunkManager>();
        for (int i = 0; i < width; i++) Gizmos.DrawLine(new Vector3(-i * chunkManager.chunkSize, -OFFSET, 0), new Vector3(-i * chunkManager.chunkSize, OFFSET, 0));
    }
}
