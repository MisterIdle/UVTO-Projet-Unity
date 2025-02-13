using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    public SpawnableObjectsList itemDatabase;
    public Transform[] spawnPoints;
    private GlobalObjectSpawner globalSpawner;
    private HashSet<int> usedSpawnPoints = new HashSet<int>();
    private List<SpawnableObject> uniquePerRoomItems = new List<SpawnableObject>();

    public void Init()
    {
        globalSpawner = GetComponentInParent<GlobalObjectSpawner>();
        uniquePerRoomItems = itemDatabase.items.FindAll(item => item.IsUniquePerRoom);
        ShuffleList(uniquePerRoomItems);
        ShuffleList(itemDatabase.items);
        SpawnItems();
    }

    private void SpawnItems()
    {
        SpawnMandatoryItems(uniquePerRoomItems, true); 
        SpawnMandatoryItems(uniquePerRoomItems, false);
        SpawnUniqueNonMandatoryItems();

        while (usedSpawnPoints.Count < spawnPoints.Length)
        {
            SpawnRemainingItems();
        }
    }

    private void SpawnMandatoryItems(List<SpawnableObject> items, bool isUniqueGlobal)
    {
        foreach (var item in items)
        {
            if (item.IsUniqueGlobal == isUniqueGlobal && CanSpawnUniqueItem(item))
            {
                if (usedSpawnPoints.Count >= spawnPoints.Length)
                {
                    Debug.LogWarning($"No available spawn points for item: {item.name}");
                    continue;
                }
                SpawnItem(item);
                if (item.IsUniqueGlobal) globalSpawner.UniqueItemsSpawned.Add(item);
            }
        }
    }

    private void SpawnUniqueNonMandatoryItems()
    {
        foreach (var item in itemDatabase.items)
        {
            if (item.IsUniqueGlobal && CanSpawnUniqueItem(item))
            {
                if (usedSpawnPoints.Count >= spawnPoints.Length)
                {
                    Debug.LogWarning($"No available spawn points for item: {item.name}");
                    continue;
                }
                SpawnItem(item);
                globalSpawner.UniqueItemsSpawned.Add(item);
            }
        }
    }

    private void SpawnRemainingItems()
    {
        List<SpawnableObject> shuffledItems = new List<SpawnableObject>(itemDatabase.items);
        ShuffleList(shuffledItems);
    
        foreach (var item in shuffledItems)
        {
            if (!item.IsUniqueGlobal && !item.IsUniquePerRoom)
            {
                if (usedSpawnPoints.Count >= spawnPoints.Length) break;
                SpawnItem(item);
            }
        }
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private bool CanSpawnUniqueItem(SpawnableObject item)
    {
        return !globalSpawner.UniqueItemsSpawned.Contains(item);
    }

    private void SpawnItem(SpawnableObject item)
    {
        if (usedSpawnPoints.Count >= spawnPoints.Length) return;
        int randomIndex = GetRandomUnusedIndex();
        GameObject obj = Instantiate(item.Prefab, spawnPoints[randomIndex].position, spawnPoints[randomIndex].rotation);
        obj.AddComponent<Grabbable>();
        usedSpawnPoints.Add(randomIndex);
        obj.transform.SetParent(globalSpawner.ObjectsParent.transform);
    }

    private int GetRandomUnusedIndex()
    {
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, spawnPoints.Length);
        } while (usedSpawnPoints.Contains(randomIndex));
        return randomIndex;
    }
}
