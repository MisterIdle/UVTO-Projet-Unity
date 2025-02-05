using UnityEngine;
using System.Collections.Generic;

public class ObjectsSpawner : MonoBehaviour
{
    public SpawnableObjectsList ItemDatabase;
    public GameObject ItemParent;

    [Range(1, 100)] public int ChanceToBeEmpty = 50;

    private List<Transform> spawnPoints;
    private HashSet<SpawnableObjects> spawnedUniqueItems = new HashSet<SpawnableObjects>();

    void Awake()
    {        
        InitializeSpawnPoints();
        SpawnItems();
        Debug.Log("All items are spawned.");
    }

    private void InitializeSpawnPoints()
    {
        spawnPoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
    }

    private void SpawnItems()
    {
        List<SpawnableObjects> byPassItem = GetMandatoryItems();
        SpawnMandatoryItems(byPassItem);

        while (spawnPoints.Count > 0)
        {
            List<SpawnableObjects> availableItems = GetAvailableItems();

            if (availableItems.Count == 0)
            {
                Debug.Log("All unique items are spawned and no other items are available to spawn.");
                break;
            }

            SpawnableObjects selectedItem = SelectRandomItem(availableItems);
            SpawnItemAtPoint(selectedItem);
        }
    }

    private List<SpawnableObjects> GetMandatoryItems()
    {
        return ItemDatabase.items.FindAll(item => item.IsMandatory);
    }

    private void SpawnMandatoryItems(List<SpawnableObjects> byPassItems)
    {
        foreach (var item in byPassItems)
        {
            if (spawnPoints.Count == 0) break;
            SpawnItemAtPoint(item);
        }
    }

    private List<SpawnableObjects> GetAvailableItems()
    {
        return ItemDatabase.items.FindAll(item => !item.IsUnique || !spawnedUniqueItems.Contains(item));
    }

    private SpawnableObjects SelectRandomItem(List<SpawnableObjects> availableItems)
    {
        return availableItems[Random.Range(0, availableItems.Count)];
    }

    private void SpawnItemAtPoint(SpawnableObjects item)
    {
        int spawnIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[spawnIndex];
    
        if (!item.IsMandatory && ChanceToBeEmpty >= Random.Range(0, 100))
        {
            spawnPoints.RemoveAt(spawnIndex);
            return;
        }
    
        GameObject obj = Instantiate(item.Prefab, spawnPoint.position, spawnPoint.rotation);
        obj.transform.SetParent(ItemParent.transform);
    
        if (item.IsUnique)
        {
            spawnedUniqueItems.Add(item);
        }
    
        HandleGrabbableItems(obj, item);
    
        spawnPoints.RemoveAt(spawnIndex);
    }


    private void HandleGrabbableItems(GameObject obj, SpawnableObjects item)
    {
        if (obj.TryGetComponent(out Grabbable grabbable))
        {
            if (item.IsMandatory)
            {
                grabbable.IsMandatory = true;
            }

            if (item.CanBeBorrowed)
            {
                grabbable.CanBeBorrowed = item.BorrowedChance >= Random.Range(0, 100);
                grabbable.Score = item.Score;
            }
        }
    }
}
