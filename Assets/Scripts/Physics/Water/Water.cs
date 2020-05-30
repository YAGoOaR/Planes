﻿using UnityEngine;

public class Water : MonoBehaviour
{
    const float top = 0;
    const float width = 600;
    const float height = 130;
    const float spring = 0.001f;
    const float damping = 0.006f;
    const float spread = 0.003f;
    const float effect = 0.0001f;
    const float mass = 1;
    const float waterTopWidth = 0.1f;
    const float edgeWidth = 15f;
    const float waveVelocity = 0.05f;

    public Material waterTopMaterial;
    public GameObject waterMesh;
    public GameObject waterCollider;

    static int edgeCount = Mathf.RoundToInt(width / edgeWidth);
    static int nodeCount = edgeCount + 1;

    int position = 0;
    int prevPosition = 0;

    float left = -width / 2;
    float bottom = top - height;

    float waterLevel = 0;

    Transform followedTransform;
    LineRenderer body;
    WaterNode[] nodes = new WaterNode[nodeCount];
    Mesh[] meshes = new Mesh[edgeCount];
    GameObject[] meshObjects = new GameObject[edgeCount];

    class WaterNode
    {
        public static float startZ = -1;
        public float velocity = 0;
        public float acceleration = 0;
        public float leftDelta, rightDelta = 0;
        public Vector3 position;
        public GameObject buoyancyCollider;
        public WaterNode(float x, float y)
        {
            position.Set(x, y, startZ);
        }
        public void destroy()
        {
            GameObject.Destroy(buoyancyCollider);
        }
    }

    public void SpawnWater()
    {
        BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(width, height);
        boxCollider.offset = new Vector2(0, -height / 2);

        body = gameObject.AddComponent<LineRenderer>();
        body.material = waterTopMaterial;
        body.material.renderQueue = 1000;
        body.positionCount = nodeCount;
        body.startWidth = waterTopWidth;
        body.endWidth = waterTopWidth;

        for (int i = 0; i < nodeCount; i++)
        {
            createNode(i);
            body.SetPosition(i, nodes[i].position);
        }
        for (int i = 0; i < edgeCount; i++)
        {
            createMesh(i);
        }
    }

    void createNode(int i)
    {
        nodes[i] = new WaterNode(left + width * i / edgeCount, top);
    }

    void createMesh(int i)
    {
        Vector2[] UVs = new Vector2[4];
        UVs[0] = new Vector2(0, 1);
        UVs[1] = new Vector2(1, 1);
        UVs[2] = new Vector2(0, 0);
        UVs[3] = new Vector2(1, 0);

        int[] triangles = new int[6] { 0, 1, 3, 3, 2, 0 };

        Vector3[] vertices = new Vector3[4];
        vertices[0] = nodes[i].position;
        vertices[1] = nodes[i + 1].position;
        vertices[2] = new Vector3(nodes[i].position.x, bottom, WaterNode.startZ);
        vertices[3] = new Vector3(nodes[i + 1].position.x, bottom, WaterNode.startZ);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = UVs;
        mesh.triangles = triangles;
        meshes[i] = mesh;

        GameObject obj = meshObjects[i];
        obj = Object.Instantiate(waterMesh, Vector3.zero, Quaternion.identity);
        obj.GetComponent<MeshFilter>().mesh = meshes[i];
        obj.GetComponent<MeshRenderer>().sortingLayerName = "water";
        obj.transform.parent = transform;

        GameObject water = GameObject.Instantiate(waterCollider, transform);
        water.transform.position = new Vector3(left + width * (i + 0.5f) / edgeCount, (bottom + top) / 2, 0);
        water.transform.localScale = new Vector3(width / edgeCount, top - bottom, 1);
        nodes[i].buoyancyCollider = water;
    }

    void UpdatePosition()
    {
        float globalPos = followedTransform.position.x;
        prevPosition = position;
        position = Mathf.FloorToInt(globalPos / edgeWidth);
        left = position * edgeWidth - width / 2;
        if (position != prevPosition)
        {
            gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(globalPos, -height / 2);
            int delta = position - prevPosition;
            if (delta > nodeCount - 1) delta = nodeCount - 1;
            if (delta < -nodeCount + 1) delta = -nodeCount + 1;
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
                    index = edgeCount - i - 1;
                }
                else
                {
                    index = i;
                }
                nodes[index].destroy();
                Object.Destroy(meshObjects[index]);
                Object.Destroy(meshes[index]);
                createMesh(index);
            }
        }
    }

    void UpdateMeshes()
    {
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            Vector3[] vertices = new Vector3[4];
            vertices[0] = nodes[i].position + Vector3.forward * -0.1f;
            vertices[1] = nodes[i + 1].position + Vector3.forward * -0.1f;
            vertices[2] = new Vector3(nodes[i].position.x, bottom, WaterNode.startZ - 0.1f);
            vertices[3] = new Vector3(nodes[i + 1].position.x, bottom, WaterNode.startZ - 0.1f);
            meshes[i].vertices = vertices;
        }
    }

    void UpdatePhysics()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            float force = spring * (nodes[i].position.y - top) + nodes[i].velocity * damping;
            nodes[i].acceleration = -force / mass;
            nodes[i].position.y += nodes[i].velocity;
            nodes[i].velocity += nodes[i].acceleration;
            body.SetPosition(i, nodes[i].position + Vector3.down * waterTopWidth / 2);
        }

        for (int j = 0; j < 1; j++)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (i > 0)
                {
                    nodes[i].leftDelta = spread * (nodes[i].position.y - nodes[i - 1].position.y);
                    nodes[i - 1].velocity += nodes[i].leftDelta;
                }
                if (i < nodes.Length - 1)
                {
                    nodes[i].rightDelta = spread * (nodes[i].position.y - nodes[i + 1].position.y);
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
            nodes[i].buoyancyCollider.transform.position = nodes[i].position + new Vector3(width / (nodes.Length - 1) / 2, bottom / 2, 0);

        }
    }

    public void Splash(float xpos, float velocity)
    {
        if (xpos >= nodes[0].position.x && xpos <= nodes[nodes.Length - 1].position.x)
        {
            xpos -= nodes[0].position.x;

            int index = Mathf.RoundToInt((nodes.Length - 1) * (xpos / (nodes[nodes.Length - 1].position.x - nodes[0].position.x)));

            nodes[index].velocity += velocity * effect;
        }
    }

    void Start()
    {
        followedTransform = Camera.main.transform;
        SpawnWater();
        waterLevel = transform.position.y + 1;
    }


    void FixedUpdate()
    {
        UpdatePosition();
        UpdateMeshes();
        UpdatePhysics();
        generateWaves();
    }
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

    void generateWaves()
    {
        int i = Random.Range(0, edgeCount);
        float v = nodes[i].velocity;
        if (Mathf.Abs(v) < waveVelocity)
        {
            v += (waveVelocity - Mathf.Abs(v)) * Mathf.Sign(v);
            nodes[i].velocity = v;
        }
    }
}