using System.Collections.Generic;
using UnityEngine;

public class GlobalObjectSpawner : MonoBehaviour
{
    public GameObject ObjectsParent;
    public ListPanel ListPanel;
    public HashSet<SpawnableObject> spawnedObjects = new HashSet<SpawnableObject>();
    public List<ObjectSpawner> spawners;

    private void Start()
    {
        SpawnObjectsForAllSpawners();
    }

    private void SpawnObjectsForAllSpawners()
    {
        for (int i = 0; i < spawners.Count; i++)
        {
            ObjectSpawner temp = spawners[i];
            int randomIndex = Random.Range(i, spawners.Count);
            spawners[i] = spawners[randomIndex];
            spawners[randomIndex] = temp;
        }

        foreach (var spawner in spawners)
        {
            spawner.SpawnObject();
        }

        ChooseBorrowableItem();
        Debug.Log("Objects spawned");
    }

    public bool CanSpawnObject(SpawnableObject spawnableObject)
    {
        return !spawnedObjects.Contains(spawnableObject);
    }

    public void MarkObjectAsSpawned(SpawnableObject spawnableObject)
    {
        spawnedObjects.Add(spawnableObject);
    }

    public void ChooseBorrowableItem()
    {
        var borrowableItems = new List<Borrowable>();
        var items = ObjectsParent.GetComponentsInChildren<Grabbable>();

        foreach (var i in items)
        {
            var borrowable = i.gameObject.AddComponent<Borrowable>();
            Destroy(i.gameObject.GetComponent<Grabbable>());
            borrowable.ScoreValue = i.ScoreValue;
            borrowableItems.Add(borrowable);
            Debug.Log($"Borrowed {i.name}");
        }

        GameManager.Instance.BorrowedObjectsList.AddRange(borrowableItems);
    }
}
