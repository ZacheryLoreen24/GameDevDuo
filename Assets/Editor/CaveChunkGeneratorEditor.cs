using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CaveChunkGenerator))]
public class CaveChunkGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CaveChunkGenerator generator = (CaveChunkGenerator)target;

        if (GUILayout.Button("Regenerate Cave"))
        {
            generator.RegenerateCave();
        }
    }
}
