using UnityEngine;

public class EndGame : MonoBehaviour
{
    private UIManager _uiManager; // Reference to the UIManager
    private PlayerController _playerController; // Reference to the PlayerController

    private void Start()
    {
        // Initialize references to UIManager and PlayerController
        _uiManager = FindFirstObjectByType<UIManager>();
        _playerController = FindFirstObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has a PlayerController component
        if (other.gameObject.GetComponent<PlayerController>() != null)
        {
            // Set the player's state to dead
            _playerController.IsDead = true;

            // Show end game UI and set the game over text
            _uiManager.ShowEndGameText();
            _uiManager.SetGameOverText("You escaped!");
        }
    }
}
