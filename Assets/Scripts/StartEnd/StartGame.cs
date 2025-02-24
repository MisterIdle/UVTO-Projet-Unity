using UnityEngine;

public class StartGame : MonoBehaviour
{
    // Called on the frame when a script is enabled just before any of the Update methods are called the first time
    private void Start()
    {
        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called once per frame
    public void Update()
    {
        // Check if the space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Call the method to start the game
            StartGameButton();
        }
    }

    // Method to load the game scene
    public void StartGameButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
