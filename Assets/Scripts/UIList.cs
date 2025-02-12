using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class UIList : MonoBehaviour
{
    public TMP_Text ListText;

    public void UpdateList(List<Borrowable> borrowedObjects)
    {
        ListText.text = "Items:\n";
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
}
