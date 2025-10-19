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
        CarveMainPath();
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

        // SET FIRST AND LAST CHUNK VALUES TO 1.0f TO CREATE START AND END "ROOMS"
        // First chunk boundaries range from (0,0,0) to (chunk_size,chunk_size,chunk_size)
        for (int x = 1; x <= chunk_size; x++)
        {
            for (int y = 1; y <= chunk_size; y++)
            {
                for (int z = 1; z <= chunk_size; z++)
                {
                    density_map[x,y,z] = 1.0f;
                }
            }
        }
        // Last chunk boundaries range from (total_chunks_x*chunk_size, total_chunks_y*chunk_size, total_chunks_z*chunk_size) to (x_dim, y_dim, z_dim)
        // (3 - 1) * 16 = 32 to (3 * 17) - 3 = 48 = total_chunks_x(y)(z) * chunk_size
        for (int x = (total_chunks_x - 1) * chunk_size; x < total_chunks_x * chunk_size; x++)
        {
            for (int y = (total_chunks_y - 1) * chunk_size; y < total_chunks_y * chunk_size; y++)
            {
                for (int z = (total_chunks_z - 1) * chunk_size; z < total_chunks_z * chunk_size; z++)
                {
                    density_map[x,y,z] = 1.0f;
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

    // Carve the main path through the terrain by modifying the density map
    void CarveMainPath()
    {
        Vector3Int current_pos = Vector3Int.zero;
        Vector3Int end_pos = new Vector3Int((total_chunks_x - 1) * chunk_size, (total_chunks_y - 1) * chunk_size, (total_chunks_z - 1) * chunk_size);
        Debug.Log("Carving path from " + current_pos + " to " + end_pos);

        List<Vector3Int> path = new List<Vector3Int>();
        while (current_pos != end_pos)
        {
            path.Add(current_pos);
            current_pos.x += 1;
            current_pos.y += 1;
            current_pos.z += 1;
        }
        path.Add(end_pos);

        for (int i = 0; i < path.Count; i++)
        {
            Debug.Log("Carving at " + path[i]);
            // density_map[path[i].x, path[i].y, path[i].z] = 1.0f;
            CarveSphere(path[i]);
        }

    }

    void CarveSphere(Vector3Int center)
    {
        int max_x = density_map.GetLength(0);
        int max_y = density_map.GetLength(1);
        int max_z = density_map.GetLength(2);

        int radius = 3;
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3Int pos = center + new Vector3Int(x, y, z);
                    if (pos.x >= 0 && pos.x < max_x && pos.y >= 0 && pos.y < max_y && pos.z >= 0 && pos.z < max_z)
                    {
                        if (Vector3Int.Distance(pos, center) <= radius)
                        {
                            density_map[pos.x, pos.y, pos.z] = 1.0f;
                        }

                    }
                }
            }
        }
    }

    void GenerateTerrain()
    {

    }


}
