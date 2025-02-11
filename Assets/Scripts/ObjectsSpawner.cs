using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectsSpawner : MonoBehaviour
{
    public SpawnableObjectsList ItemDatabase;
    public GameObject ObjectsParent;
    [SerializeField] private Borrowable[] _borrowableObjects;
    [SerializeField] private int numberOfObjectsBorrow;

    [Range(1, 100)] public int ChanceToBeEmpty = 50;

    private List<Transform> spawnPoints;
    private HashSet<SpawnableObject> spawnedUniqueItems = new HashSet<SpawnableObject>();

    void Start()
    {
        Debug.Log("Starting ObjectsSpawner...");
        InitializeSpawnPoints();
        SpawnItems();
    }

    private void InitializeSpawnPoints()
    {
        spawnPoints = new List<Transform>(transform.Cast<Transform>());
        Debug.Log($"Initialized {spawnPoints.Count} spawn points.");
    }

    private void SpawnItems()
    {
        List<SpawnableObject> mandatoryItems = ItemDatabase.items.Where(item => item.IsMandatory).ToList();
        Debug.Log($"Found {mandatoryItems.Count} mandatory items to spawn.");
        SpawnMandatoryItems(mandatoryItems);

        List<SpawnableObject> availableItems = ItemDatabase.items.Where(item => !item.IsUnique || !spawnedUniqueItems.Contains(item)).ToList();
        int availableItemCount = availableItems.Count;
        if (availableItemCount == 0) {
            Debug.Log("No more unique items to spawn.");
            return;
        }

        while (spawnPoints.Count > 0 && availableItemCount > 0)
        {
            int spawnIndex = Random.Range(0, availableItemCount);
            SpawnableObject selectedItem = availableItems[spawnIndex];
            Debug.Log($"Spawning item: {selectedItem.name}");
            SpawnItemAtPoint(selectedItem);
            availableItems = availableItems.Where(item => !spawnedUniqueItems.Contains(item)).ToList();
            availableItemCount = availableItems.Count;
        }
    }

    private void SpawnMandatoryItems(List<SpawnableObject> mandatoryItems)
    {
        foreach (var item in mandatoryItems)
        {
            if (spawnPoints.Count == 0) break;
            Debug.Log($"Spawning mandatory item: {item.name}");
            SpawnItemAtPoint(item);
        }
    }

    private void SpawnItemAtPoint(SpawnableObject item)
    {
        if (spawnPoints.Count == 0) return;

        int spawnIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[spawnIndex];

        if (!item.IsMandatory && Random.Range(0, 100) < ChanceToBeEmpty)
        {
            Debug.Log($"Skipping spawn for item: {item.name} due to chance.");
            spawnPoints.RemoveAt(spawnIndex);
            return;
        }

        if (item.Prefab == null)
        {
            Debug.LogWarning($"Item {item.name} has no prefab assigned.");
            return;
        }

        GameObject spawnedObject = Instantiate(item.Prefab, spawnPoint.position, spawnPoint.rotation, ObjectsParent.transform);
        Debug.Log($"Spawned item: {item.name} at position: {spawnPoint.position}");

        if (item.IsUnique)
        {
            spawnedUniqueItems.Add(item);
        }

        if (item.CanBeBorrowed && Random.Range(0, 100) < item.BorrowedChance)
        {
            var borrowable = spawnedObject.AddComponent<Borrowable>();
            var grabbable = spawnedObject.GetComponent<Grabbable>();
            
            borrowable.ScoreValue = item.Score;
            Debug.Log($"Item {item.name} can be borrowed with score value: {item.Score}");

            Destroy(grabbable);
        }

        spawnPoints.RemoveAt(spawnIndex);
    }
}
