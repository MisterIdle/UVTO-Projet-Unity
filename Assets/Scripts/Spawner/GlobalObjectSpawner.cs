using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlobalObjectSpawner : MonoBehaviour
{
    public GameObject ObjectsParent;
    public ListPanel ListPanel;
    public List<SpawnableObject> UniqueItemsSpawned = new List<SpawnableObject>();
    public List<ObjectSpawner> ObjectSpawners = new List<ObjectSpawner>();

    private void Start()
    {
        ListPanel = FindFirstObjectByType<ListPanel>();

        ObjectSpawners = Shuffle(ObjectSpawners);
        foreach (var spawner in ObjectSpawners)
        {
            spawner.Init();
            Debug.Log($"Spawner {spawner.name} initialized");
        }

        ChooseBorrowableItem();
    }

    private void Update()
    {
        ListPanel.UpdateList(GameManager.Instance.BorrowedObjectsList);
    }

    public List<ObjectSpawner> Shuffle(List<ObjectSpawner> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            ObjectSpawner temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    public bool CanBorrowableSpawnItem()
    {
        return GameManager.Instance.BorrowedObjectsCount < GameManager.Instance.MaxBorrowedObjects;
    }

    public void IncrementBorrowableSpawnCount(Borrowable borrowable)
    {
        GameManager.Instance.BorrowedObjectsCount++;
        GameManager.Instance.BorrowedObjectsList.Add(borrowable);
    }

    public void DecrementBorrowableSpawnCount(Borrowable borrowable)
    {
        GameManager.Instance.BorrowedObjectsCount--;
        GameManager.Instance.BorrowedObjectsList.Remove(borrowable);

    }

    public void ChooseBorrowableItem()
    {
        Grabbable[] items = FindObjectsByType<Grabbable>(FindObjectsSortMode.None);

        foreach (var i in items)
        {
            if (CanBorrowableSpawnItem() && Random.Range(0, 100) < i.ChanceToBeBorrowed)
            {
                var borrowable = i.gameObject.AddComponent<Borrowable>();
                borrowable.ScoreValue = i.ScoreValue;
                
                Destroy(i);
                IncrementBorrowableSpawnCount(borrowable);
            }
        }
    }
}
