using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProceduralTerrainGenerator : MonoBehaviour
{
    public int terrainSize = 100;
    public int terrainHeight = 20;
    public int treeDensity = 500;

    public Terrain terrain;
    public GameObject treePrototype;

    void Start()
    {
        GenerateTerrain();
        PlaceTrees();
    }

    void GenerateTerrain()
    {
        terrain.terrainData = GenerateTerrainData();
    }

    TerrainData GenerateTerrainData()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainSize + 1;
        terrainData.alphamapResolution = terrainSize;
        terrainData.baseMapResolution = terrainSize;
        terrainData.SetDetailResolution(terrainSize, 8);

        float[,] heights = GenerateHeights();
        terrainData.SetHeights(0, 0, heights);

        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[terrainSize, terrainSize];

        for (int x = 0; x < terrainSize; x++)
        {
            for (int y = 0; y < terrainSize; y++)
            {
                heights[x, y] = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * terrainHeight;
            }
        }

        return heights;
    }

    void PlaceTrees()
    {
        TreePrototype[] prototypes = new TreePrototype[1];

        prototypes[0] = new TreePrototype();
        prototypes[0].prefab = treePrototype;

        
        terrain.terrainData.treePrototypes = prototypes;

        for (int i = 0; i < treeDensity; i++)
        {
            float randomX = Random.Range(0, terrainSize);
            float randomY = Random.Range(0, terrainSize);
            float randomHeight = terrain.terrainData.GetHeight(Mathf.RoundToInt(randomX), Mathf.RoundToInt(randomY));

            if (randomHeight > 5f) // Adjust the threshold as needed
            {
                TreeInstance tree = new TreeInstance();
                tree.position = new Vector3(randomX / terrainSize, randomHeight / terrainHeight, randomY / terrainSize);
                tree.widthScale = 1;
                tree.heightScale = 1;

                terrain.AddTreeInstance(tree);
            }
        }
    }
}
