using UnityEngine;

//My water physics script
public class Water : MonoBehaviour
{
    const float TOP_POSITION = 0;
    const float WIDTH = 600;
    const float HEIGHT = 130;
    const float SPRING = 0.001f;
    const float DAMPING = 0.006f;
    const float SPREAD = 0.003f;
    const float EFFECT = 0.00005f;
    const float MASS = 1;
    const float TOP_WIDTH = 0.1f;
    const float EDGE_WIDTH = 10f;
    const float WAVE_VELOCITY = 0.04f;
    const float HALF = 0.5f;
    const float DEFAULT_Z_POSITION = 0;
    const float DEFAULT_Z_SCALE = 0;
    const int UV_COUNT = 4;
    const float MESH_OFFSET = 0.1f;

    [SerializeField]
    Material waterTopMaterial;
    [SerializeField]
    GameObject waterMesh;
    [SerializeField]
    GameObject waterCollider;

    static int edgeCount = Mathf.RoundToInt(WIDTH / EDGE_WIDTH);
    static int nodeCount = edgeCount + 1;

    int position;
    int prevPosition;

    float leftPosition = -WIDTH / 2;
    const float bottomPosition = TOP_POSITION - HEIGHT;

    Transform followedTransform;
    LineRenderer lineBody;
    WaterNode[] nodes = new WaterNode[nodeCount];
    Mesh[] meshes = new Mesh[edgeCount];
    GameObject[] meshObjects = new GameObject[edgeCount];

    //A vertex of the water surface
    class WaterNode
    {
        public const float ZPosition = -1;
        public float velocity;
        public float acceleration;
        public float leftDelta, rightDelta;
        public Vector3 position;
        public GameObject buoyancyCollider;
        public WaterNode(float x, float y)
        {
            position = new Vector3(x, y, ZPosition);
        }
        public void destroy()
        {
            GameObject.Destroy(buoyancyCollider);
        }
    }

    //create a water Gameobject
    public void SpawnWater()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(WIDTH, HEIGHT);
        boxCollider.offset = new Vector2(0, -HEIGHT / 2);

        lineBody = gameObject.AddComponent<LineRenderer>();
        lineBody.material = waterTopMaterial;
        lineBody.material.renderQueue = 1000;
        lineBody.positionCount = nodeCount;
        lineBody.startWidth = TOP_WIDTH;
        lineBody.endWidth = TOP_WIDTH;

        for (int i = 0; i < nodeCount; i++)
        {
            createNode(i);
            lineBody.SetPosition(i, nodes[i].position);
        }
        for (int i = 0; i < edgeCount; i++)
        {
            createFace(i);
        }
    }

    void createNode(int i)
    {
        nodes[i] = new WaterNode(leftPosition + WIDTH * i / edgeCount, TOP_POSITION);
    }

    //Create water texture and collider
    void createFace(int i)
    {
        // Texture coordinates of a mesh;
        Vector2[] UVs = new Vector2[UV_COUNT];
        UVs[0] = new Vector2(0, 1);
        UVs[1] = new Vector2(1, 1);
        UVs[2] = new Vector2(0, 0);
        UVs[3] = new Vector2(1, 0);

        //Triangles in mesh
        int[] triangles = { 0, 1, 3, 3, 2, 0 };

        Vector3[] vertices = new Vector3[4];
        vertices[0] = nodes[i].position;
        vertices[1] = nodes[i + 1].position;
        vertices[2] = new Vector3(nodes[i].position.x, bottomPosition, WaterNode.ZPosition);
        vertices[3] = new Vector3(nodes[i + 1].position.x, bottomPosition, WaterNode.ZPosition);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = UVs;
        mesh.triangles = triangles;
        meshes[i] = mesh;

        GameObject meshObject = Instantiate(waterMesh, Vector3.zero, Quaternion.identity, transform);
        meshObject.GetComponent<MeshFilter>().mesh = meshes[i];
        meshObject.GetComponent<MeshRenderer>().sortingLayerName = "water";
        meshObjects[i] = meshObject;

        GameObject water = Instantiate(waterCollider, transform);
        water.transform.position = new Vector3(leftPosition + WIDTH * (i + HALF) / edgeCount, (bottomPosition + TOP_POSITION) / 2, DEFAULT_Z_POSITION);
        water.transform.localScale = new Vector3(WIDTH / edgeCount, TOP_POSITION - bottomPosition, DEFAULT_Z_SCALE);
        nodes[i].buoyancyCollider = water;
    }

    //Update water when camera position changes
    void UpdatePosition()
    {
        float globalPos = followedTransform.position.x;
        prevPosition = position;
        position = Mathf.FloorToInt(globalPos / EDGE_WIDTH);
        leftPosition = position * EDGE_WIDTH - WIDTH / 2;
        if (position != prevPosition)
        {
            GetComponent<BoxCollider2D>().offset = new Vector2(globalPos, -HEIGHT / 2);
            int delta = position - prevPosition;
            if (delta > nodeCount - 1)
            {
                delta = nodeCount - 1;
            }
            if (delta < -nodeCount + 1)
            {
                delta = -nodeCount + 1;
            }
            moveNodesInArray(delta);

            for (int i = 0; i < Mathf.Abs(delta); i++)
            {
                if (delta > 0)
                {
                    nodes[edgeCount - i].destroy();
                    createNode(edgeCount - i);
                }
                else
                {
                    nodes[i].destroy();
                    createNode(i);
                }
            }

            for (int i = 0; i < Mathf.Abs(delta); i++)
            {
                int index;
                if (delta > 0)
                {
                    index = edgeCount - 1 - i;
                }
                else
                {
                    index = i;
                }
                nodes[index].destroy();
                Object.Destroy(meshObjects[index]);
                Object.Destroy(meshes[index]);
                createFace(index);
            }
        }
    }

    void UpdateMeshes()
    {
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            Vector3[] vertices = new Vector3[UV_COUNT];
            vertices[0] = nodes[i].position + Vector3.forward * -MESH_OFFSET;
            vertices[1] = nodes[i + 1].position + Vector3.forward * -MESH_OFFSET;
            vertices[2] = new Vector3(nodes[i].position.x, bottomPosition, WaterNode.ZPosition - MESH_OFFSET);
            vertices[3] = new Vector3(nodes[i + 1].position.x, bottomPosition, WaterNode.ZPosition - MESH_OFFSET);
            meshes[i].vertices = vertices;
        }
    }

    void UpdatePhysics()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            float force = SPRING * (nodes[i].position.y - TOP_POSITION) + nodes[i].velocity * DAMPING;
            nodes[i].acceleration = -force / MASS;
            nodes[i].position.y += nodes[i].velocity;
            nodes[i].velocity += nodes[i].acceleration;
            lineBody.SetPosition(i, nodes[i].position + Vector3.down * TOP_WIDTH / 2);
        }

        for (int j = 0; j < 1; j++)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (i > 0)
                {
                    nodes[i].leftDelta = SPREAD * (nodes[i].position.y - nodes[i - 1].position.y);
                    nodes[i - 1].velocity += nodes[i].leftDelta;
                }
                if (i < nodes.Length - 1)
                {
                    nodes[i].rightDelta = SPREAD * (nodes[i].position.y - nodes[i + 1].position.y);
                    nodes[i + 1].velocity += nodes[i].rightDelta;
                }
            }
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            if (i > 0)
            {
                nodes[i - 1].position.y += nodes[i].leftDelta;
            }
            if (i < nodes.Length - 1)
            {
                nodes[i + 1].position.y += nodes[i].rightDelta;
            }
        }
        for (int i = 0; i < edgeCount; i++)
        {
            nodes[i].buoyancyCollider.transform.position = nodes[i].position + new Vector3(WIDTH / (nodes.Length - 1) / 2, bottomPosition / 2, 0);

        }
    }

    //Create a splash when an object hits water
    public void Splash(float xpos, float velocity)
    {
        if (xpos >= nodes[0].position.x && xpos <= nodes[nodes.Length - 1].position.x)
        {
            xpos -= nodes[0].position.x;

            int i = Mathf.RoundToInt((nodes.Length - 1) * (xpos / (nodes[nodes.Length - 1].position.x - nodes[0].position.x)));

            nodes[i].velocity += velocity * EFFECT;
        }
    }

    //Called when water initializes
    void Start()
    {
        followedTransform = Camera.main.transform;
        SpawnWater();
    }

    //Called once per frame
    void FixedUpdate()
    {
        UpdatePosition();
        UpdateMeshes();
        UpdatePhysics();
        generateWaves();
    }

    //Create an offset in node and face arrays
    void moveNodesInArray(int offset)
    {
        WaterNode[] newArr = new WaterNode[nodes.Length];

        for (int i = 0; i < nodes.Length; i++)
        {
            int index = (i - offset) % nodes.Length;
            if (index < 0)
            {
                index = nodes.Length + index;
            }
            newArr[index] = nodes[i];

        }
        nodes = newArr;

        Mesh[] meshArr = new Mesh[meshes.Length];
        GameObject[] meshObjectsArr = new GameObject[meshes.Length];

        for (int i = 0; i < meshes.Length; i++)
        {
            int index = (i - offset) % meshes.Length;
            if (index < 0)
            {
                index = meshes.Length + index;
            }
            meshArr[index] = meshes[i];
            meshObjectsArr[index] = meshObjects[i];

        }
        meshes = meshArr;
        meshObjects = meshObjectsArr;
    }
    //Random wave generation
    void generateWaves()
    {
        int i = Random.Range(0, edgeCount);
        float velocity = nodes[i].velocity;
        if (Mathf.Abs(velocity) < WAVE_VELOCITY)
        {
            velocity += (WAVE_VELOCITY - Mathf.Abs(velocity)) * Mathf.Sign(velocity);
            nodes[i].velocity = velocity;
        }
    }
}
