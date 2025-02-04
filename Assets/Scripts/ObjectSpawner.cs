using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public DropTable dropTable;
    public bool isUnique;

    void Start()
    {
        SpawnItem();
    }

    void SpawnItem()
    {
        foreach (Transform child in transform)
        {
            Instantiate(dropTable.items[Random.Range(0, dropTable.items.Length)].itemPrefab, child.position, Quaternion.identity, transform);
        }
    }
}
