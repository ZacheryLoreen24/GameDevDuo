using UnityEngine;
using static MarchingCubesTables;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    // Chunk information
    private int chunk_size = 16;
    public float isovalue = 0f;

    private float [,,] local_density_map;
    private float [,,] global_density_map;

    private Mesh chunk_mesh;

    public void Initialize(int chunk_size, float [,,] global_density_map)
    {
        this.chunk_size = chunk_size;
        this.global_density_map = global_density_map;
        InitializeLocalDensityMap();
        GenerateMesh();

    }

    // Initialize the local density map for this chunk based on its position in the global density map
    void InitializeLocalDensityMap()
    {
        local_density_map = new float[chunk_size+1, chunk_size+1, chunk_size+1];
        // Calculate the starting indices in the global density map based on the chunk's position
        int start_pos_x = (int)transform.position.x;
        int start_pos_y = (int)transform.position.y;
        int start_pos_z = (int)transform.position.z;
        for (int x = 0; x <= chunk_size; x++)
        {
            for (int y = 0; y <= chunk_size; y++)
            {
                for (int z = 0; z <= chunk_size; z++)
                {
                    local_density_map[x, y, z] = global_density_map[start_pos_x + x, start_pos_y + y, start_pos_z + z];
                }
            }
        }
    }

    // Generate the mesh for this chunk using the Marching Cubes algorithm
    public void GenerateMesh()
    {
        Debug.Log("Generating mesh for chunk at position: " + transform.position);
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < chunk_size; x++)
        {
            for (int y = 0; y < chunk_size; y++)
            {
                for (int z = 0; z < chunk_size; z++)
                {
                    MarchCube(new Vector3Int(x, y, z), vertices, triangles);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        chunk_mesh = mesh;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MeshCollider mesh_collider = GetComponent<MeshCollider>();
        if (mesh_collider == null)
            mesh_collider = gameObject.AddComponent<MeshCollider>();

        mesh_collider.sharedMesh = mesh;

        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Perform the Marching Cubes algorithm on a single cube at the given position
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
            cube[i] = local_density_map[(int)cornerPosition.x, (int)cornerPosition.y, (int)cornerPosition.z];
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
