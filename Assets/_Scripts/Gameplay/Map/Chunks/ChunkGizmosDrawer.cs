using UnityEngine;

//A script that visualizes chunk bounds in editor
public class ChunkGizmosDrawer : MonoBehaviour
{
    const float OFFSET = 200;
    [SerializeField]
    int width;

    void OnDrawGizmos()
    {
        ChunkManager chunkManager = gameObject.GetComponent<ChunkManager>();
        for (int i = 0; i < width; i++)
        {
            Gizmos.DrawLine(new Vector3(-i * chunkManager.ChunkSize, -OFFSET, 0), new Vector3(-i * chunkManager.ChunkSize, OFFSET, 0));
        }
    }
}
