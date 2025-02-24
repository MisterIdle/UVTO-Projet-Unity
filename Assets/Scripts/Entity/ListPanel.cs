using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ListPanel : MonoBehaviour
{
    // Reference to the UI panel that displays the list
    public GameObject listPanel;
    // Reference to the TextMeshPro text component that displays the list items
    public TMP_Text ListText;
    // Reference to the PlayerController script
    private PlayerController PlayerController;
    // Audio clip to play when toggling the list
    [SerializeField] AudioClip PaperSound;

    private void Start()
    {
        // Get the PlayerController component from the parent object
        PlayerController = GetComponentInParent<PlayerController>();

        // Ensure the list panel is initially hidden
        if (listPanel != null)
        {
            listPanel.SetActive(false);
        }
    }

    // Toggles the visibility of the list panel
    public void ToggleList()
    {
        if (listPanel != null)
        {
            listPanel.SetActive(!listPanel.activeSelf);
            // Play the paper sound effect when toggling the list
            SoundManager.Instance.PlaySound(PaperSound, transform, 0.5f);
        }
    }

    // Hides the list panel
    public void Hide()
    {
        listPanel.SetActive(false);
        // Play the paper sound effect when hiding the list
        SoundManager.Instance.PlaySound(PaperSound, transform, 0.5f);
    }

    // Updates the list with the provided borrowed objects
    public void UpdateList(List<Borrowable> borrowedObjects)
    {
        ListText.text = "";
        foreach (var obj in borrowedObjects)
        {
            // If the object is borrowed, display its name with a strikethrough
            if (obj.IsBorrowed)
            {
                ListText.text += "<s>" + obj.name + "</s>\n";
            }
            else
            {
                // Otherwise, display its name normally
                ListText.text += obj.name + "\n";
            }
        }
    }

    // Handles the input action to toggle the list
    public void OnList(InputAction.CallbackContext context)
    {
        if (context.performed && !PlayerController.IsDead)
        {
            ToggleList();
            Debug.Log("List toggled");
        }
    }
}
