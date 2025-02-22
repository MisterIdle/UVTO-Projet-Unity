using UnityEngine;

public class EndGame : MonoBehaviour
{
    public PlayerController _playerController;
    public Enemy _enemy;
    public UIManager _uiManager;

    public void Start()
    {
        _playerController = FindFirstObjectByType<PlayerController>();
        _uiManager = FindFirstObjectByType<UIManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>() != null)
        {
            _uiManager.ShowEndGameText();
            _playerController.enabled = false;
            _playerController.IsDead = true;
        }
    }
}
