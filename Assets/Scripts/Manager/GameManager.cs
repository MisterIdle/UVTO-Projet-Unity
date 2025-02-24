using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public float Score;
    public int MaxBorrowedObjects = 10;
    public int BorrowedObjectsCount;
    public float freeEnemyTimer = 10f;
    public float warningEnemyTimer = 5f;

    // List to keep track of borrowed objects
    public List<Borrowable> BorrowedObjectsList = new List<Borrowable>();

    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();

                if (_instance == null)
                {
                    // Create a new GameManager if one doesn't exist
                    GameObject singleton = new GameObject(nameof(GameManager));
                    _instance = singleton.AddComponent<GameManager>();
                    DontDestroyOnLoad(singleton);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Restart the game by resetting variables and reloading the scene
    public void RestartGame()
    {
        Score = 0;
        BorrowedObjectsCount = 0;
        BorrowedObjectsList.Clear();

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // Stop the game by quitting the application
    public void StopGame()
    {
        Application.Quit();
    }

    // Check if more objects can be borrowed
    public bool CanBorrowMore()
    {
        return BorrowedObjectsCount < MaxBorrowedObjects;
    }

    // Add a borrowed object to the list if the limit is not reached
    public void AddBorrowedObject(Borrowable obj)
    {
        if (CanBorrowMore())
        {
            BorrowedObjectsList.Add(obj);
            BorrowedObjectsCount++;
        }
    }
}
