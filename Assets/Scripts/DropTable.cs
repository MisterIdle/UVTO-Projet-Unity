using UnityEngine;

[System.Serializable]
public class DropItem
{
    public GameObject itemPrefab;
    public float spawnChance;
    public bool isUnique;
}

[CreateAssetMenu(fileName = "NewDropTable", menuName = "Items/DropTable")]
public class DropTable : ScriptableObject
{
    public DropItem[] items;
}
