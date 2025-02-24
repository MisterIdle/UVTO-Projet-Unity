using UnityEngine;

public class EndGame : MonoBehaviour
{
    private UIManager _uiManager;
    private PlayerController _playerController;

    private void Start()
    {
        _uiManager = FindFirstObjectByType<UIManager>();
        _playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>() != null)
        {
            _playerController.IsDead = true;

            _uiManager.ShowEndGameText();
            _uiManager.SetGameOverText("You escaped!");
        }
    }
}
