using UnityEngine;

public class CaveChunkGenerator : MonoBehaviour
{
    public CaveGenerator cavePrefab; // drag your CaveGenerator prefab here
    public int chunksX = 2;
    public int chunksY = 1;
    public int chunksZ = 2;

    public float chunkSize = 16f;
    public float noiseOffset = 1000f;

    [Header("Noise Settings")]
    public int seed;
    public bool randomizeSeed = true;


    void Start()
    {
        if (randomizeSeed)
        {
            seed = Random.Range(0, 100000);

        }

        GenerateCave();
        BuildCaveWalls();
    }
    void GenerateCave()
    {
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
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        if (randomizeSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        GenerateCave();
    }

}
