using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    public ChunkManager chunkPrefab;
    public int total_chunks_x = 3;
    public int total_chunks_y = 3;
    public int total_chunks_z = 3;
    public int chunk_size = 16;

    private float[,,] density_map;
    private ChunkManager[,,] chunk_map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeDensityMap();
        InitializeChunks();
    }

    // Initialize the density map with default values (1.0f for now)
    void InitializeDensityMap()
    {
        // Instantiate dimensions for reusability
        int x_dim = total_chunks_x * (chunk_size + 1);
        int y_dim = total_chunks_y * (chunk_size + 1);
        int z_dim = total_chunks_z * (chunk_size + 1);

        // Generate density map and set each value to 1.0f
        density_map = new float[x_dim,y_dim,z_dim];

        for (int x = 0; x < x_dim; x++)
        {
            for (int y = 0; y < y_dim; y++)
            {
                for (int z = 0; z < z_dim; z++)
                {
                    density_map[x,y,z] = -1.0f;
                }
            }
        }
    }

    // Initialize all chunks in the chunk map, store in chunk_map in order to easily reference later
    void InitializeChunks()
    {
        chunk_map = new ChunkManager[total_chunks_x,total_chunks_y,total_chunks_z];
        for (int x = 0; x < total_chunks_x; x++)
        {
            for (int y = 0; y < total_chunks_y; y++)
            {
                for (int z = 0; z < total_chunks_z; z++)
                {
                    ChunkManager newChunk = Instantiate(chunkPrefab, new Vector3(x * chunk_size, y * chunk_size, z * chunk_size), Quaternion.identity);
                    newChunk.transform.parent = this.transform;
                    chunk_map[x,y,z] = newChunk;
                    newChunk.Initialize(chunk_size, density_map);
                }
            }
        }
    }


    void GenerateTerrain()
    {

    }


}
