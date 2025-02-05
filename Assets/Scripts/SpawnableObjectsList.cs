using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpawnableObjects
{
    public GameObject Prefab;
    public bool IsUnique;
    public bool IsMandatory;
    public bool IsBorrowable;

    [Range(0, 100)]
    public int BorrowableChance = 50;
    public bool IsListedInUI = true;
}


[CreateAssetMenu]
public class SpawnableObjectsList : ScriptableObject
{
    public List<SpawnableObjects> items;
}
