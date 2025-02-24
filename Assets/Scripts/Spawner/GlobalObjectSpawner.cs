using System.Collections.Generic;
using UnityEngine;

public class GlobalObjectSpawner : MonoBehaviour
{
    public GameObject ObjectsParent; // Parent object for spawned objects
    public ListPanel ListPanel; // UI panel to list objects
    public HashSet<SpawnableObject> spawnedObjects = new HashSet<SpawnableObject>(); // Set to track spawned objects
    public List<ObjectSpawner> spawners; // List of spawners

    private void Start()
    {
        SpawnObjectsForAllSpawners(); // Spawn objects when the game starts
    }

    private void SpawnObjectsForAllSpawners()
    {
        // Shuffle the spawners list
        for (int i = 0; i < spawners.Count; i++)
        {
            ObjectSpawner temp = spawners[i];
            int randomIndex = Random.Range(i, spawners.Count);
            spawners[i] = spawners[randomIndex];
            spawners[randomIndex] = temp;
        }

        // Spawn objects using each spawner
        foreach (var spawner in spawners)
        {
            spawner.SpawnObject();
        }

        ChooseBorrowableItem(); // Choose items that can be borrowed
        Debug.Log("Objects spawned");
    }

    public bool CanSpawnObject(SpawnableObject spawnableObject)
    {
        return !spawnedObjects.Contains(spawnableObject); // Check if the object can be spawned
    }

    public void MarkObjectAsSpawned(SpawnableObject spawnableObject)
    {
        spawnedObjects.Add(spawnableObject); // Mark the object as spawned
    }

    public void ChooseBorrowableItem()
    {
        var borrowableItems = new List<Borrowable>();
        var items = ObjectsParent.GetComponentsInChildren<Grabbable>(); // Get all grabbable items

        foreach (var i in items)
        {
            if (!GameManager.Instance.CanBorrowMore()) break; // Check if more items can be borrowed

            var borrowable = i.gameObject.AddComponent<Borrowable>(); // Add Borrowable component
            Destroy(i.gameObject.GetComponent<Grabbable>()); // Remove Grabbable component
            borrowable.ScoreValue = i.ScoreValue; // Transfer score value
            borrowableItems.Add(borrowable);

            GameManager.Instance.AddBorrowedObject(borrowable); // Register borrowed object
            Debug.Log($"Borrowed {i.name}");
        }
    }
}
