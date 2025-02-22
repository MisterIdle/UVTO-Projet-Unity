using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectSpawner : MonoBehaviour
{
    public List<SpawnableObject> spawnableObjects;

    private GlobalObjectSpawner globalObjectSpawner;

    private void Awake()
    {
        globalObjectSpawner = GetComponentInParent<GlobalObjectSpawner>();
    }

    public void SpawnObject()
    {
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
