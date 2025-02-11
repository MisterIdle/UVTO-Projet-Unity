using UnityEngine;

public class Borrowable : MonoBehaviour
{
    private PlayerController _playerController;
    public float ScoreValue;

    private void Start()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
    }

    public void Borrow()
    {
        if (_playerController != null)
        {
            _playerController.AddScore(ScoreValue);
        }
        Destroy(gameObject);
    }
}
