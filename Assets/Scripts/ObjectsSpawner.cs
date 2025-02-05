using UnityEngine;
using System.Collections.Generic;

public class ObjectsSpawner : MonoBehaviour
{
    public SpawnableObjectsList itemDatabase;

    private List<Transform> spawnPoints;
    private HashSet<SpawnableObjects> spawnedUniqueItems = new HashSet<SpawnableObjects>();

    void Start()
    {
        spawnPoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }

        SpawnItems();
    }

    void SpawnItems()
    {
        List<SpawnableObjects> mandatoryItems = itemDatabase.items.FindAll(item => item.IsMandatory);   

        foreach (var item in mandatoryItems)
        {
            if (spawnPoints.Count == 0) break;
            SpawnItemAtPoint(item);
        }   

        while (spawnPoints.Count > 0)
        {
            List<SpawnableObjects> availableItems = itemDatabase.items.FindAll(item => !item.IsUnique || !spawnedUniqueItems.Contains(item));   

            if (availableItems.Count == 0)
            {
                Debug.Log("All unique items are spawned and no other items are available to spawn.");
                break;
            }   

            SpawnableObjects selectedItem = availableItems[Random.Range(0, availableItems.Count)];  

            SpawnItemAtPoint(selectedItem);
        }
    }


    void SpawnItemAtPoint(SpawnableObjects item)
    {
        int spawnIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[spawnIndex];

        GameObject obj = Instantiate(item.Prefab, spawnPoint.position, Quaternion.identity);

        if (obj.TryGetComponent(out Grabbable grabbable))
        {
            grabbable.IsBorrowable = item.IsBorrowable && Random.value < (item.BorrowableChance / 100f);
            grabbable.IsListedInUI = grabbable.IsBorrowable && item.IsListedInUI;
        }

        if (item.IsUnique)
        {
            spawnedUniqueItems.Add(item);
        }

        spawnPoints.RemoveAt(spawnIndex);
    }
}