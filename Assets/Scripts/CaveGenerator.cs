using UnityEngine;
using static MarchingCubesTables;
using System.Collections.Generic;



public class CaveGenerator : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int sizeX = 16;
    public int sizeY = 16;
    public int sizeZ = 16;
    public float noiseScale = 0.1f;

    [Header("Threshold")]
    public float isovalue = 0f;

    [HideInInspector]
    public Vector3 noiseOffset = Vector3.zero;

    private float[,,] densityMap;
    private Mesh meshy;
    private int seed;


    void Start()
    {
        seed = transform.parent.GetComponent<CaveChunkGenerator>().seed;
        GenerateDensity();
        GenerateMesh();
    }

    // Generates a density map using Perlin noise
    void GenerateDensity()
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

        densityMap = new float[sizeX + 1, sizeY + 1, sizeZ + 1];

        // Go through each index in the density map and assign a pseudorandom value based on Perlin noise
        for (int x = 0; x <= sizeX; x++)
        {
            for (int y = 0; y <= sizeY; y++)
            {
                for (int z = 0; z <= sizeZ; z++)
                {
                    float xCoord = (x + transform.position.x) * noiseScale;
                    float yCoord = (y + transform.position.y) * noiseScale;
                    float zCoord = (z + transform.position.z) * noiseScale;

                    // Apply a seed-based offset to the noise coordinates, creating unique generation each time
                    float offsetX = (float)seed * 0.1f;
                    float offsetY = (float)seed * 0.2f;
                    float offsetZ = (float)seed * 0.3f;

                    float noise = (
                        Mathf.PerlinNoise(xCoord + offsetX, zCoord + offsetZ) +
                        Mathf.PerlinNoise(yCoord + offsetY, xCoord + offsetX) +
                        Mathf.PerlinNoise(zCoord + offsetZ, yCoord + offsetY)
                    ) / 3f * 2f - 1f;

                    float heightFactor = y / (float)sizeY;
                    densityMap[x, y, z] = noise - isovalue;
                }
            }
        }
    }

    void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    MarchCube(new Vector3Int(x, y, z), vertices, triangles);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        meshy = mesh;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = mesh;

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void MarchCube(Vector3Int position, List<Vector3> vertices, List<int> triangles)
    {
        // the density values at each corner of the cube
        float[] cube = new float[8];

        // the x y z positions of each corner of the cube
        Vector3[] vertexPosition = new Vector3[8];
        
        for (int i = 0; i < 8; i++)
        {
            Vector3 cornerPosition = position + cornerOffsets[i];
            vertexPosition[i] = cornerPosition;
            cube[i] = densityMap[(int)cornerPosition.x, (int)cornerPosition.y, (int)cornerPosition.z];
        }


        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > isovalue)
            {
                // Shift 1 by i bits
                cubeIndex |= (1 << i);
            }
        }

        if (triTable[cubeIndex].Length == 0)
            return;

        Vector3[] edgeVertices = new Vector3[12];

        for (int i = 0; i < 12; i++)
        {
            int a = edgeIndexTable[i, 0];
            int b = edgeIndexTable[i, 1];

            float valA = cube[a];
            float valB = cube[b];

            Vector3 p1 = vertexPosition[a];
            Vector3 p2 = vertexPosition[b];

            // Calculate the interpolation factor t, check to ensure we don't divide by zero (floats behave unpredictably)
            // Interpolation is essentially finding where the point lies between the two corners based on the isovalue
            float t;
            if (Mathf.Abs(valB - valA) < 0.000001f)
            {
                t = 0.5f; // Avoid division by zero, use midpoint
            }
            else
            {
                t = (isovalue - valA) / (valB - valA);
            }

            edgeVertices[i] = Vector3.Lerp(p1, p2, t);
        }

        for (int i = 0; i < triTable[cubeIndex].Length; i += 3)
        {
            int a = triTable[cubeIndex][i];
            int b = triTable[cubeIndex][i + 1];
            int c = triTable[cubeIndex][i + 2];

            int startIndex = vertices.Count;

            vertices.Add(edgeVertices[a]);
            vertices.Add(edgeVertices[b]);
            vertices.Add(edgeVertices[c]);

            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 0);
        }
    }
}
