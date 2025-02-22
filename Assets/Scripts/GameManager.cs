using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public float Score;
    public int MaxBorrowedObjects = 10;
    public int BorrowedObjectsCount;

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

    public void RestartGame()
    {
        Score = 0;
        BorrowedObjectsCount = 0;
        BorrowedObjectsList.Clear();

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
