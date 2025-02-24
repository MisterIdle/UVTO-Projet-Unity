using System.Collections;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    private Animation _anim;
    public GameObject enemy;
    public Transform spawnPoint;

    private void Start()
    {
        _anim = GetComponent<Animation>();

        Spawn();
    }

    public void Spawn()
    {
        _anim.Play();
        StartCoroutine(WaitForAnimation());
    }

    private IEnumerator WaitForAnimation()
    {
        while (_anim.isPlaying)
        {
            yield return null;
        }
        Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);
    }

}
