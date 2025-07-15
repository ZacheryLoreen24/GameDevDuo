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

    private float[,,] densityMap;

    void Start()
    {
        GenerateDensity();
        GenerateMesh();
    }

    void GenerateDensity()
    {
        densityMap = new float[sizeX + 1, sizeY + 1, sizeZ + 1];

        for (int x = 0; x <= sizeX; x++)
        {
            for (int y = 0; y <= sizeY; y++)
            {
                for (int z = 0; z <= sizeZ; z++)
                {
                    //float noise = Mathf.PerlinNoise(x * noiseScale, z * noiseScale) * 2f - 1f;
                    //float heightFactor = y / (float)sizeY;
                    //densityMap[x, y, z] = noise - isovalue;
                    // Debug.Log(densityMap[x, y, z]);
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
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void MarchCube(Vector3Int position, List<Vector3> vertices, List<int> triangles)
    {
        float[] cube = new float[8];
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

            float t;// = (isovalue - valA) / (valB - valA);
            if (valB - valA < 0.0001f)
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

            triangles.Add(startIndex+2);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 0);
        }
    }
}
