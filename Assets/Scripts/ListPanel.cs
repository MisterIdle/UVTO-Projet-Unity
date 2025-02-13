using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ListPanel : MonoBehaviour
{
    public GameObject listPanel;
    public TMP_Text ListText;

    private void Start()
    {
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

    public void UpdateList(List<Borrowable> borrowedObjects)
    {
        ListText.text = "";
        foreach (var obj in borrowedObjects)
        {
            if (obj.IsBorrowed)
            {
                ListText.text += "<color=#D3D3D3><s>" + obj.name + "</s></color>\n";
            }
            else
            {
                ListText.text += obj.name + "\n";
            }
        }
    }

    public void OnList(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleList();
            Debug.Log("List toggled");
        }
    }
}
