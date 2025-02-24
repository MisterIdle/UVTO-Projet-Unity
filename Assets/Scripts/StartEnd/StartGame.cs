using UnityEngine;

public class StartGame : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartGameButton();
        }
    }

    public void StartGameButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
