using UnityEngine;

public class WasherMachine : MonoBehaviour
{
    public float RotationSpeed = 100f;
    public bool IsWashing = true;

    [SerializeField] private AudioClip _washerSound;
    [SerializeField] private AudioClip _washerEndSound;

    private void Start()
    {
        if (IsWashing)
        {
            SoundManager.Instance.PlayLoopSound(_washerSound, transform, 0.5f);
        }
    }

    private void Update()
    {
        if (IsWashing)
        {
            transform.Rotate(Vector3.forward, RotationSpeed * Time.deltaTime);
        }
        else
        {
            SoundManager.Instance.StopSound(_washerSound);
            SoundManager.Instance.PlaySound(_washerEndSound, transform, 0.5f);
            enabled = false;
        }
    }
}
