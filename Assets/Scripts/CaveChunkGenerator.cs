using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class CaveChunkGenerator : MonoBehaviour
{
    public CaveGenerator cavePrefab; // drag your CaveGenerator prefab here
    public int chunksX = 2;
    public int chunksY = 1;
    public int chunksZ = 2;

    public float chunkSize = 16f;
    public float noiseOffset = 1000f;
    public float noiseScale = 0.1f;
    public float isovalue = 0f;
    [Header("Noise Settings")]
    public int seed;
    public bool randomizeSeed = true;


    [Header("Cellular Automata Settings")]
    public int birthLimit = 4;
    public int deathLimit = 3;
    public int iterations = 5;

    private Dictionary<Vector3Int, CaveGenerator> chunkMap = new();
    public Vector3Int chunkDimensions;
    private float[,,] superDensityMap;
    private bool[,,] superSolidMap;
    private List<Vector3Int> pathPoints;



    void Start()
    {
        if (randomizeSeed)
        {
            seed = Random.Range(0, 100000);

        }

        chunkDimensions = new Vector3Int(
            (int)(chunkSize * chunksX),
            (int)(chunkSize * chunksY),
            (int)(chunkSize * chunksZ)
        );
        GenerateSuperDensity();
        GenerateCave();
        ApplyCellularAutomata();
        // CarvePath();
        UpdateAllDensityMaps();
        GenerateMeshes();

        BuildCaveWalls();
    }


    void GenerateCave()
    {
        if (chunkDimensions == Vector3Int.zero)
        {
            chunkDimensions = new Vector3Int((int)chunkSize, (int)chunkSize, (int)chunkSize);
        }

        for (int x = 0; x < chunksX; x++)
        {
            for (int y = 0; y < chunksY; y++)
            {
                for (int z = 0; z < chunksZ; z++)
                {
                    Vector3 pos = new Vector3(x, y, z) * chunkSize;
                    CaveGenerator chunk = Instantiate(cavePrefab, pos, Quaternion.identity, transform);

                    // Give each chunk a unique offset
                    chunk.noiseOffset = new Vector3(x * noiseOffset, y * noiseOffset, z * noiseOffset);

                    // Register chunk
                    Vector3Int index = new Vector3Int(x * (int)chunkSize, y * (int)chunkSize, z * (int)chunkSize);
                    chunkMap[index] = chunk;
                }
            }
        }
    }


    void BuildCaveWalls()
    {
        float totalSizeX = chunkSize * chunksX;
        float totalSizeY = chunkSize * chunksY;
        float totalSizeZ = chunkSize * chunksZ;

        Vector3[] normals = {
            Vector3.left, Vector3.right,
            Vector3.down, Vector3.up,
            Vector3.back, Vector3.forward
        };

        Vector3[] positions = {
            new Vector3(0, totalSizeY / 2f, totalSizeZ / 2f),
            new Vector3(totalSizeX, totalSizeY / 2f, totalSizeZ / 2f),
            new Vector3(totalSizeX / 2f, 0, totalSizeZ / 2f),
            new Vector3(totalSizeX / 2f, totalSizeY, totalSizeZ / 2f),
            new Vector3(totalSizeX / 2f, totalSizeY / 2f, 0),
            new Vector3(totalSizeX / 2f, totalSizeY / 2f, totalSizeZ)
        };

        Vector3[] scales = {
            new Vector3(0.1f, totalSizeY, totalSizeZ),
            new Vector3(0.1f, totalSizeY, totalSizeZ),
            new Vector3(totalSizeX, 0.1f, totalSizeZ),
            new Vector3(totalSizeX, 0.1f, totalSizeZ),
            new Vector3(totalSizeX, totalSizeY, 0.1f),
            new Vector3(totalSizeX, totalSizeY, 0.1f)
        };

        GameObject wallParent = new GameObject("CaveWalls");
        wallParent.transform.parent = transform;
        wallParent.SetActive(false); // Start inactive

        for (int i = 0; i < 6; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{i}";
            wall.transform.parent = wallParent.transform;
            wall.transform.localPosition = positions[i];
            wall.transform.localScale = scales[i];
            wall.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 0.05f);

            var renderer = wall.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1, 1, 1, 0.05f);
            }
        }
    }

    [ContextMenu("Regenerate Cave")]
    public void RegenerateCave()
    {
        int count = 0;

        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            DestroyImmediate(child.gameObject);
            count++;
        }

        Debug.Log($"Destroyed {count} cave chunks.");

        if (randomizeSeed)
            seed = Random.Range(0, 100000);

        GenerateCave();
        GenerateSuperDensity();
        ApplyCellularAutomata();
        CarvePath();
        UpdateAllDensityMaps();
        GenerateMeshes();
        BuildCaveWalls();
    }

    public CaveGenerator GetChunkAtWorldPosition(Vector3Int worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / chunkSize);
        int y = Mathf.FloorToInt(worldPos.y / chunkSize);
        int z = Mathf.FloorToInt(worldPos.z / chunkSize);


        Vector3Int index = new Vector3Int(x, y, z);
        return chunkMap.TryGetValue(index, out var chunk) ? chunk : null;
    }

    public void GenerateMeshes()
    {
        foreach (var chunk in chunkMap.Values)
        {
            chunk.GenerateMesh();
        }
    }


    // Generates a density map using Perlin noise
    void GenerateSuperDensity()
    {
        /* 
         * size + 1 because there is always one more vertex than the number of cubes in each dimension
         *
         *  . . . .
         *  . . . .
         *  . . . .
         *
         * notice how this 2x3 square has 3x4 vertices (each . is a vertex)
         */

        superDensityMap = new float[((int)chunkSize + 1) * chunksX, ((int)chunkSize + 1) * chunksY, ((int)chunkSize + 1) * chunksZ];
        superSolidMap = new bool[(int)chunkSize * chunksX + 1, (int)chunkSize * chunksY + 1, (int)chunkSize * chunksZ + 1];
        // Go through each index in the density map and assign a pseudorandom value based on Perlin noise
        for (int x = 0; x <= chunkSize * chunksX; x++)
        {
            for (int y = 0; y <= chunkSize * chunksY; y++)
            {
                for (int z = 0; z <= chunkSize * chunksZ; z++)
                {
                    superDensityMap[x, y, z] = calculateNoise(x, y, z) - isovalue;
                    superSolidMap[x, y, z] = superDensityMap[x, y, z] > isovalue;
                }
            }
        }
    }

    float calculateNoise(int x, int y, int z)
    {
        float xCoord = (x + transform.position.x) * noiseScale + noiseOffset;
        float yCoord = (y + transform.position.y) * noiseScale + noiseOffset;
        float zCoord = (z + transform.position.z) * noiseScale + noiseOffset;
        // Apply a seed-based offset to the noise coordinates, creating unique generation each time
        float offsetX = (float)seed * 0.1f;
        float offsetY = (float)seed * 0.2f;
        float offsetZ = (float)seed * 0.3f;
        float noise = (
            Mathf.PerlinNoise(xCoord + offsetX, zCoord + offsetZ) +
            Mathf.PerlinNoise(yCoord + offsetY, xCoord + offsetX) +
            Mathf.PerlinNoise(zCoord + offsetZ, yCoord + offsetY)
        ) / 3f * 2f - 1f;
        return noise;
    }

    float[,,] CopyDensityChunk(int startX, int startY, int startZ)
    {
        float[,,] chunk = new float[(int)chunkSize + 1, (int)chunkSize + 1, (int)chunkSize + 1];

        for (int x = 0; x <= chunkSize; x++)
        {
            for (int y = 0; y <= chunkSize; y++)
            {
                for (int z = 0; z <= chunkSize; z++)
                {
                    chunk[x, y, z] = superDensityMap[startX + x, startY + y, startZ + z];
                }
            }
        }

        return chunk;
    }

    bool[,,] CopySolidChunk(int startX, int startY, int startZ)
    {
        bool[,,] solidChunk = new bool[(int)chunkSize + 1, (int)chunkSize + 1, (int)chunkSize + 1];

        for (int x = 0; x <= chunkSize; x++)
        {
            for (int y = 0; y <= chunkSize; y++)
            {
                for (int z = 0; z <= chunkSize; z++)
                {
                    solidChunk[x, y, z] = superSolidMap[startX + x, startY + y, startZ + z];
                }
            }
        }

        return solidChunk;
    }

    void UpdateAllDensityMaps()
    {
        foreach(var chunkIndex in chunkMap.Keys)
        {
            CaveGenerator chunk = chunkMap[chunkIndex];
            float[,,] newDensityMap = CopyDensityChunk(chunkIndex.x, chunkIndex.y, chunkIndex.z);
            bool[,,] newSolidMap = CopySolidChunk(chunkIndex.x, chunkIndex.y, chunkIndex.z);
            chunk.SetMaps(newDensityMap, newSolidMap);
        }
        
    }

    void CarvePath()
    {
        if (pathPoints != null)
        {
            pathPoints.Clear();
        }
        
        int sizeX = ((int)chunkSize + 1) * chunksX;
        int sizeY = ((int)chunkSize + 1) * chunksY;
        int sizeZ = ((int)chunkSize + 1) * chunksZ;

        Vector3Int currentPos = Vector3Int.zero;
        Vector3Int endPos = new Vector3Int((chunksX - 1) * (int)chunkSize, (chunksY - 1) * (int)chunkSize, (chunksZ - 1) * (int)chunkSize);

        pathPoints = new List<Vector3Int>();
        pathPoints.Add(currentPos);

        System.Random rng = new System.Random();

        while (Vector3Int.Distance(currentPos, endPos) > 1)
        {
            Vector3Int direction = Vector3Int.zero;

            // Move toward the end, but with randomness
            if (currentPos.x < endPos.x) direction.x = rng.NextDouble() < 0.7 ? 1 : 0;
            if (currentPos.y < endPos.y) direction.y = rng.NextDouble() < 0.7 ? 1 : 0;
            if (currentPos.z < endPos.z) direction.z = rng.NextDouble() < 0.7 ? 1 : 0;

            // Add randomness (chance to wiggle)
            if (rng.NextDouble() < 0.2) direction += RandomDirection();

            // Clamp to valid space
            currentPos += direction;
            currentPos.x = Mathf.Clamp(currentPos.x, 0, sizeX - 1);
            currentPos.y = Mathf.Clamp(currentPos.y, 0, sizeY - 1);
            currentPos.z = Mathf.Clamp(currentPos.z, 0, sizeZ - 1);

            pathPoints.Add(currentPos);
        }

        // Carve the path
        foreach (var point in pathPoints)
        {
            CarveSphere(point, 3); // radius = 3
        }
    }

    Vector3Int RandomDirection()
    {
        int x = UnityEngine.Random.Range(-1, 2);
        int y = UnityEngine.Random.Range(-1, 2);
        int z = UnityEngine.Random.Range(-1, 2);
        return new Vector3Int(x, y, z);
    }

    void CarveSphere(Vector3Int center, int radius)
    {
        int sizeX = superDensityMap.GetLength(0);
        int sizeY = superDensityMap.GetLength(1);
        int sizeZ = superDensityMap.GetLength(2);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3Int pos = center + new Vector3Int(x, y, z);
                    if (pos.x < 0 || pos.y < 0 || pos.z < 0 ||
                        pos.x >= sizeX || pos.y >= sizeY || pos.z >= sizeZ)
                        continue;

                    if (Vector3Int.Distance(pos, center) <= radius)
                    {
                        superDensityMap[pos.x, pos.y, pos.z] = -1f; // empty/passable
                    }
                }
            }
        }
    }

    public void ApplyCellularAutomata()
    {
        int sizeX = superSolidMap.GetLength(0);
        int sizeY = superSolidMap.GetLength(1);
        int sizeZ = superSolidMap.GetLength(2);
        Debug.Log($"Applying Cellular Automata: {sizeX}x{sizeY}x{sizeZ}, Iterations: {iterations}, Birth Limit: {birthLimit}, Death Limit: {deathLimit}");
        for (int i = 0; i < iterations; i++)
        {
            bool[,,] newMap = new bool[sizeX, sizeY, sizeZ];

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        int solidNeighbors = CountSolidNeighbors(x, y, z, sizeX, sizeY, sizeZ);

                        if (superSolidMap[x, y, z])
                            newMap[x, y, z] = solidNeighbors >= deathLimit;
                        else
                            newMap[x, y, z] = solidNeighbors > birthLimit;
                    }
                }
            }

            superSolidMap = newMap;
        }

        // After smoothing, optionally regenerate density values
        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
                for (int z = 0; z < sizeZ; z++)
                    superDensityMap[x, y, z] = superSolidMap[x, y, z] ? 1f : -1f;
    }

    int CountSolidNeighbors(int x, int y, int z, int sizeX, int sizeY, int sizeZ)
    {
        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;
                    int nz = z + dz;

                    if (nx >= 0 && ny >= 0 && nz >= 0 && nx < sizeX && ny < sizeY && nz < sizeZ)
                    {
                        if (superSolidMap[nx, ny, nz]) count++;
                    }
                    else
                    {
                        // Treat out-of-bounds as solid to keep edges filled
                        count++;
                    }
                }

        return count;
    }



}
