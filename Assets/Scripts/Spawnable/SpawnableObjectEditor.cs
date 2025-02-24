#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class SpawnableObjectEditor
{
    // Adds a menu item to create a Spawnable Object
    [MenuItem("Spawnable Object/Create Spawnable Object")]
    public static void CreateSpawnableObject()
    {
        // Get selected objects in the editor
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("Select prefabs to create Spawnable Objects.");
            return;
        }

        foreach (Object obj in selectedObjects)
        {
            GameObject selectedPrefab = obj as GameObject;
            // Check if the selected object is a prefab
            if (selectedPrefab == null || PrefabUtility.GetPrefabAssetType(selectedPrefab) == PrefabAssetType.NotAPrefab)
            {
                Debug.LogWarning($"Select a prefab to create a Spawnable Object. {obj.name} is not a prefab.");
                continue;
            }

            // Get the path and directory of the selected prefab
            string path = AssetDatabase.GetAssetPath(selectedPrefab);
            string directory = Path.GetDirectoryName(path);
            string spawnableDirectory = Path.Combine(directory, "Spawnable");

            // Create a "Spawnable" folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(spawnableDirectory))
            {
                AssetDatabase.CreateFolder(directory, "Spawnable");
            }

            // Define the asset path for the new SpawnableObject
            string assetPath = Path.Combine(spawnableDirectory, selectedPrefab.name + "_Spawnable.asset");

            // Create a new SpawnableObject and set its properties
            SpawnableObject spawnableObject = ScriptableObject.CreateInstance<SpawnableObject>();
            spawnableObject.Prefab = selectedPrefab;
            spawnableObject.Name = selectedPrefab.name;

            // Save the new SpawnableObject as an asset
            AssetDatabase.CreateAsset(spawnableObject, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"SpawnableObject created: {assetPath}");
        }

        // Set the last selected object as the active object
        Selection.activeObject = selectedObjects[selectedObjects.Length - 1];
    }

    // Validates if the selected object is a GameObject
    [MenuItem("Assets/Create/Spawnable Object", true)]
    public static bool ValidateCreateSpawnableObject()
    {
        return Selection.activeObject is GameObject;
    }
}
#endif
