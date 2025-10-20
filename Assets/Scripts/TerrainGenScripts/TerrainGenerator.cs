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
        // Create the starting position somewhere in the first chunk
        Vector3Int current_pos = new Vector3Int(
            Random.Range(chunk_size / 2, chunk_size),
            Random.Range(0, chunk_size / 8),
            Random.Range(chunk_size / 2, chunk_size)
        );

        // Define the end position in the last chunk
        Vector3Int end_pos = new Vector3Int(
            (total_chunks_x - 1) * chunk_size,
            (total_chunks_y - 1) * chunk_size,
            (total_chunks_z - 1) * chunk_size
        );
        Debug.Log("Carving path from " + current_pos + " to " + end_pos);

        // This List contains each point along the path. We will use this to carve spheres at each point to create a tunnel.
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int direction;
        int step_size;
        while (current_pos != end_pos)
        {
            path.Add(current_pos);
            direction = GetRandomDirection(current_pos, end_pos);
            if (direction == Vector3Int.zero)
            {
                break;
            }
            else if (direction == Vector3Int.right || direction == Vector3Int.left)
            {
                step_size = Random.Range(3, (int)((end_pos.x - current_pos.x) / 2));
            }
            else if (direction == Vector3Int.up || direction == Vector3Int.down)
            {
                step_size = Random.Range(3, 7);
            }
            else
            {
                step_size = Random.Range(3, (int)((end_pos.z - current_pos.z) / 2));
            }
            
            for (int i = 0; i < step_size; i++)
            {
                if (current_pos == end_pos)
                {
                    break;
                }
                
                current_pos.x = Mathf.Max(0, Mathf.Min(current_pos.x + direction.x + Random.Range(-1, 3), end_pos.x));
                current_pos.y = Mathf.Max(0, Mathf.Min(current_pos.y + Random.Range(0, 2), end_pos.y));
                current_pos.z = Mathf.Max(0, Mathf.Min(current_pos.z + direction.z + Random.Range(-1, 3), end_pos.z));

                path.Add(current_pos);
            }

            
        }
        path.Add(end_pos);

        for (int i = 0; i < path.Count; i++)
        {
            // Debug.Log("Carving at " + path[i]);
            CarveSphere(path[i], Random.Range(3, 7));
        }

    }

    Vector3Int GetRandomDirection(Vector3Int current_pos, Vector3Int end_pos)
    {
        // Potentially useful values for determining direction
        Vector3Int delta = end_pos - current_pos;
        float total = Mathf.Abs(delta.x) + Mathf.Abs(delta.z);
        if (total == 0) return Vector3Int.zero;

        float x_chance = Mathf.Abs(delta.x) / total;
        // float y_chance = Mathf.Abs(delta.y) / total;
        float z_chance = Mathf.Abs(delta.z) / total;
        // Debug.Log("Chances - X: " + x_chance + " Z: " + z_chance);

        int max_x = total_chunks_x * chunk_size;
        int max_y = total_chunks_y * chunk_size;
        int max_z = total_chunks_z * chunk_size;

        float roll = Random.value;

        if (roll < x_chance)
        {
            return delta.x > 0 ? Vector3Int.right : Vector3Int.left;
        }
        else
        {
            return delta.z > 0 ? Vector3Int.forward : Vector3Int.back;
        }


    }

    void CarveSphere(Vector3Int center, int radius)
    {
        int max_x = density_map.GetLength(0);
        int max_y = density_map.GetLength(1);
        int max_z = density_map.GetLength(2);

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

}
