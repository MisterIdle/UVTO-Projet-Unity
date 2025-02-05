using System.Collections.Generic;
using UnityEngine;

public class ObjectsBorrowed : MonoBehaviour
{
    public int BorrowedObjects = 0;
    public int MaxBorrowedObjects = 3;

    private List<Grabbable> BorrowedObjectsList = new List<Grabbable>();

    void Start()
    {
        InitializeBorrowedObjects();
        Debug.Log("Borrowed objects are initialized.");
    }

    private void InitializeBorrowedObjects()
    {
        foreach (Transform child in transform)
        {
            Grabbable grabbable = child.GetComponent<Grabbable>();

            if (grabbable != null)
            {
                if (grabbable.CanBeBorrowed && BorrowedObjects < MaxBorrowedObjects)
                {
                    BorrowedObjectsList.Add(grabbable);
                    BorrowedObjects++;
                }
                else if (grabbable.IsMandatory)
                {
                    grabbable.CanBeBorrowed = true;
                }
                else
                {
                    grabbable.CanBeBorrowed = false;
                }
            }
        }
    }
}
