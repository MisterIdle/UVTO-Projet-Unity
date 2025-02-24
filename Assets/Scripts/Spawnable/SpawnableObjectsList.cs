using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu] // Attribute to create an asset from this class in the Unity editor
public class SpawnableObjectsList : ScriptableObject
{
    // List to hold spawnable objects
    public List<SpawnableObject> items;
}
