using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class SpawnableObjects
{
    public GameObject Prefab;
    public int Score;
    public bool IsUnique;
    public bool IsMandatory;

    // Inutile si IsManadatory est à true
    public bool CanBeBorrowed;
    // Inutile si IsUnique est à true
    [Range(0, 100)] public int BorrowedChance;
}


[CreateAssetMenu]
public class SpawnableObjectsList : ScriptableObject
{
    public List<SpawnableObjects> items;
}
