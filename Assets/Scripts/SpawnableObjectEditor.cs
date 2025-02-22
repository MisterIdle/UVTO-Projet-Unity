#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class SpawnableObjectEditor
{
    [MenuItem("Spawnable Object/Create Spawnable Object")]
    public static void CreateSpawnableObject()
    {
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("Select prefabs to create Spawnable Objects.");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            GameObject selectedPrefab = obj as GameObject;
            if (selectedPrefab == null || PrefabUtility.GetPrefabAssetType(selectedPrefab) == PrefabAssetType.NotAPrefab)
            {
                Debug.LogWarning($"Select a prefab to create a Spawnable Object. {obj.name} is not a prefab.");
                continue;
            }

            string path = AssetDatabase.GetAssetPath(selectedPrefab);
            string directory = Path.GetDirectoryName(path);
            string spawnableDirectory = Path.Combine(directory, "Spawnable");

            if (!AssetDatabase.IsValidFolder(spawnableDirectory))
            {
                AssetDatabase.CreateFolder(directory, "Spawnable");
            }

            string assetPath = Path.Combine(spawnableDirectory, selectedPrefab.name + "_Spawnable.asset");

            SpawnableObject spawnableObject = ScriptableObject.CreateInstance<SpawnableObject>();
            spawnableObject.Prefab = selectedPrefab;
            spawnableObject.Name = selectedPrefab.name;

            AssetDatabase.CreateAsset(spawnableObject, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"SpawnableObject created: {assetPath}");
        }

        Selection.activeObject = selectedObjects[selectedObjects.Length - 1];
    }

    [MenuItem("Assets/Create/Spawnable Object", true)]
    public static bool ValidateCreateSpawnableObject()
    {
        return Selection.activeObject is GameObject;
    }
}
#endif
