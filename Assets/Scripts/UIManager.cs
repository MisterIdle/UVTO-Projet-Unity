using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Image BaseCrosshair;
    public Sprite CrosshairsNormal;
    public Sprite CrosshairsInteract;
    public TMP_Text InteractionText;
    public TMP_Text ScoreText;
    public TMP_Text EndGameText;

    public void SetCrosshair(bool isInteracting)
    {
        BaseCrosshair.sprite = isInteracting ? CrosshairsInteract : CrosshairsNormal;
    }

    public void SetInteractionText(string text)
    {
        InteractionText.text = text;
    }

    public void UpdateScore(float score)
    {
        ScoreText.text = "Score: " + score;
    }

    public void ShowEndGameText()
    {
        EndGameText.gameObject.SetActive(true);
    }

    public void SetGameOverText(string text)
    {
        EndGameText.text = text;
    }
}