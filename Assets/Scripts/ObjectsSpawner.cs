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
        InitializeSpawnPoints();
        SpawnItems();
    }

    private void InitializeSpawnPoints()
    {
        spawnPoints = new List<Transform>(transform.Cast<Transform>());
    }

    private void SpawnItems()
    {
        List<SpawnableObject> mandatoryItems = ItemDatabase.items.Where(item => item.IsMandatory).ToList();
        SpawnMandatoryItems(mandatoryItems);

        List<SpawnableObject> availableItems = ItemDatabase.items.Where(item => !item.IsUnique || !spawnedUniqueItems.Contains(item)).ToList();
        int availableItemCount = availableItems.Count;

        if (availableItemCount == 0)
            return;

        while (spawnPoints.Count > 0 && availableItemCount > 0)
        {
            int spawnIndex = Random.Range(0, availableItemCount);
            SpawnableObject selectedItem = availableItems[spawnIndex];

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
            spawnPoints.RemoveAt(spawnIndex);
            return;
        }

        if (item.Prefab == null)
            return;

        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        if (item.HasRandomPositionSpawn)
        {
            spawnPosition += new Vector3(Random.Range(-item.PositionChance, item.PositionChance), 0, Random.Range(-item.PositionChance, item.PositionChance));
        }

        if (item.HasRandomRotationSpawn)
        {
            spawnRotation *= Quaternion.Euler(0, Random.Range(0, item.RotationChance), 0);
        }

        GameObject spawnedObject = Instantiate(item.Prefab, spawnPosition, spawnRotation, ObjectsParent.transform);

        if (item.IsUnique || (item.CanBeBorrowed && Random.Range(0, 100) < item.BorrowedChance))
        {
            var borrowable = spawnedObject.AddComponent<Borrowable>();
            var grabbable = spawnedObject.GetComponent<Grabbable>();

            borrowable.ScoreValue = item.Score;

            if (item.IsUnique)
            {
                spawnedUniqueItems.Add(item);
            }

            Destroy(grabbable);
        }

        spawnPoints.RemoveAt(spawnIndex);
    }
}
