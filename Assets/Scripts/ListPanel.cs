using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ListPanel : MonoBehaviour
{
    public GameObject listPanel;
    public TMP_Text ListText;

    public PlayerController PlayerController;

    private void Start()
    {
        PlayerController = GetComponentInParent<PlayerController>();

        if (listPanel != null)
        {
            listPanel.SetActive(false);
        }
    }

    public void ToggleList()
    {
        if (listPanel != null)
        {
            listPanel.SetActive(!listPanel.activeSelf);
        }
    }

    public void Hide()
    {
        listPanel.SetActive(false);
    }

    public void UpdateList(List<Borrowable> borrowedObjects)
    {
        ListText.text = "";
        foreach (var obj in borrowedObjects)
        {
            if (obj.IsBorrowed)
            {
                ListText.text += "<s>" + obj.name + "</s>\n";
            }
            else
            {
                ListText.text += obj.name + "\n";
            }
        }
    }

    public void OnList(InputAction.CallbackContext context)
    {
        if (context.performed && !PlayerController.IsDead)
        {
            ToggleList();
            Debug.Log("List toggled");
        }
    }
}
