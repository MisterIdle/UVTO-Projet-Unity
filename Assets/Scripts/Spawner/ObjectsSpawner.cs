using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    public List<SpawnableObject> spawnableObjects;
    private GlobalObjectSpawner globalObjectSpawner;
    [Range(0, 360)] public float spawnAngle = 0;
    [Range(0, 100)] public int spawnChance = 100;

    private void Awake()
    {
        globalObjectSpawner = GetComponentInParent<GlobalObjectSpawner>();
    }

    public void SpawnObject()
    {
        if (Random.Range(0, 100) > spawnChance)
        {
            Debug.Log("Spawn chance not met. No object spawned.");
            return;
        }

        if (spawnableObjects.Count == 0)
        {
            Debug.Log("No objects to spawn.");
            return;
        }

        var randomObject = spawnableObjects[Random.Range(0, spawnableObjects.Count)];

        if (!globalObjectSpawner.CanSpawnObject(randomObject))
        {
            Debug.Log($"Object {randomObject.Name} has already been spawned. Skipping...");
            return;
        }

        GameObject obj = Instantiate(randomObject.Prefab, transform.position, transform.rotation);
        obj.transform.SetParent(globalObjectSpawner.ObjectsParent.transform);

        obj.transform.rotation = Quaternion.Euler(0, Random.Range(-spawnAngle, spawnAngle), 0) * obj.transform.rotation;

        var grabbable = obj.AddComponent<Grabbable>();
        grabbable.ScoreValue = randomObject.Score;

        var rigidbody = obj.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = obj.AddComponent<Rigidbody>();
        }
        rigidbody.mass = randomObject.Mass;

        obj.name = randomObject.Name;

        globalObjectSpawner.MarkObjectAsSpawned(randomObject);

        Debug.Log($"Spawned: {randomObject.Name}");
    }
}
