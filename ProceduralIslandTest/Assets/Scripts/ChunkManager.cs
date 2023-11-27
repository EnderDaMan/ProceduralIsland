//https://www.youtube.com/watch?v=bd4P5suj-L0
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager instance;
    public Vector2 worldSize;
    public int resolution = 16;
    public float chunkSize = 128f;

    static public Vector2 startPosition = new Vector2(-111,-111);
    static public Vector2 endPosition;
    static public float beachHeight = 30;
    

    [SerializeField]
    public Material material;
    [SerializeField]
    public float islandRadius = 900f;
    [SerializeField]
    List<GameObject> trees;

    public Vector2 worldCenter;

    

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        worldCenter = new Vector2((worldSize.x / 2f) * chunkSize, (worldSize.y / 2f) * chunkSize);
        StartCoroutine(GenerateChunks());
    }

    IEnumerator GenerateChunks()
    {
        TerrainGenerator tg = new();
        for (int x = 0; x < worldSize.x; x++)
            for(int y = 0; y < worldSize.y; y++)
            {
                GameObject current = new GameObject("Terrain" + (x * y), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                current.layer = LayerMask.NameToLayer("Default");
                current.transform.parent = transform;
                current.transform.localPosition = new Vector3(x * chunkSize, 0f ,y * chunkSize);

                //Initialize and generate terrain
                tg.Init(current);
                tg.Generate(material); 

                yield return new WaitForSeconds(0.1f);
            }

        tg.AddTrees(trees);
    }

    class TerrainGenerator
    {
        MeshFilter filter;
        MeshRenderer renderer;
        MeshCollider collider;
        Mesh mesh;

        float randomSeed;

        Vector3[] vertices;
        int[] triangles;
        Vector2[] UVs;
        public void Init(GameObject cur)
        {
            filter = cur.GetComponent<MeshFilter>();
            renderer = cur.GetComponent<MeshRenderer>();
            collider = cur.GetComponent<MeshCollider>();
            mesh = new();
        }

        public void Generate(Material mat)
        {
            Vector2 worldPosition =  new Vector2(filter.gameObject.transform.localPosition.x, filter.gameObject.transform.localPosition.z);
            if(ChunkManager.startPosition == new Vector2(-111, -111))
                ChunkManager.startPosition = worldPosition;

            ChunkManager.endPosition = worldPosition;
            int resolution = ChunkManager.instance.resolution;

            vertices = new Vector3[(resolution + 1) * (resolution + 1)];
            UVs = new Vector2[vertices.Length];

            Vector2 worldCenter = ChunkManager.instance.worldCenter;

            float distance;

            //randomSeed = Random.Range(0.005f, 0.01f);
            //System.Math.Round(randomSeed, 3);
            for(int i = 0, x = 0; x <= resolution; x++)
            {
                for(int z = 0; z <= resolution; z++)
                {
                    Vector2 vertexWorldPos = new Vector2(worldPosition.x + (x * ChunkManager.instance.chunkSize / resolution), worldPosition.y + (z * ChunkManager.instance.chunkSize / resolution));
                    distance = Vector2.Distance(worldCenter, vertexWorldPos);
                    float islandRadius = ChunkManager.instance.islandRadius;

                    float islandMultiplier = Mathf.Sin(Mathf.Clamp(((1 + distance) / islandRadius), 0f, 1f) + 90f) * Mathf.PerlinNoise(vertexWorldPos.x * 0.01f, vertexWorldPos.y * 0.01f);

                    vertices[i] = new Vector3(x * (ChunkManager.instance.chunkSize / resolution), islandMultiplier * 150, z * (ChunkManager.instance.chunkSize / resolution));
                    i++;
                }
            }



            for(int i = 0; i < UVs.Length; i++)
            {
                UVs[i] = new Vector2(vertices[i].x + worldPosition.x, vertices[i].z + worldPosition.y);
            }

            triangles = new int[resolution * resolution * 6];
            int tris = 0, vert = 0;

            for(int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    triangles[tris] = vert;
                    triangles[tris + 1] = vert + 1;
                    triangles[tris + 2] = (int)(vert + resolution + 1);
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = (int)(vert + resolution + 2);
                    triangles[tris + 5] = (int)(vert + resolution + 1);

                    vert++;
                    tris += 6;
                }
                vert++;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = UVs;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            collider.sharedMesh = mesh;
            filter.mesh = mesh;
            renderer.material = mat;
        }
        public void AddTrees(List<GameObject> trees)
        {
            Bounds bounds = renderer.bounds;
            float castHeight = bounds.max.y + 1000;
            Vector3 rayPos;

            for (float x = 0; x < ChunkManager.endPosition.x; x++)
            {
                for (float z = 0; z < ChunkManager.endPosition.y; z++)
                {
                    if (Random.Range(1, 75) == 1)
                    {
                        GameObject objToSpawn = trees[Random.Range(0, trees.Count)];

                        rayPos = new Vector3(x, castHeight, z);
                        RaycastHit hit;

                        if (Physics.Raycast(rayPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.NameToLayer("Ground")))
                        {
                            Debug.Log(hit.transform.gameObject.tag);
                            if(hit.point.y > beachHeight)
                                Instantiate(objToSpawn, hit.point, Quaternion.identity);
                        }
                    }
                }
            }
        }
    }
    

}
