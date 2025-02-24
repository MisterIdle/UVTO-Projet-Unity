using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // UI Elements
    public Image BaseCrosshair;
    public RawImage StartGamePanel;
    public Sprite CrosshairsNormal;
    public Sprite CrosshairsInteract;
    public TMP_Text InteractionText;
    public TMP_Text QuitText;
    public TMP_Text ScoreText;
    public TMP_Text EndGameText;

    private void Start()
    {
        // Start fading out the start game panel
        StartCoroutine(FadeOut(StartGamePanel, 3f));
    }

    // Set the crosshair sprite based on interaction state
    public void SetCrosshair(bool isInteracting)
    {
        BaseCrosshair.sprite = isInteracting ? CrosshairsInteract : CrosshairsNormal;
    }

    // Update the interaction text
    public void SetInteractionText(string text)
    {
        InteractionText.text = text;
    }

    // Update the quit text
    public void SetQuitText(string text)
    {
        QuitText.text = text;
    }

    // Update the score text
    public void UpdateScore(float score)
    {
        ScoreText.text = "Score: " + score;
    }

    // Show end game text and hide other UI elements
    public void ShowEndGameText()
    {
        BaseCrosshair.gameObject.SetActive(false);
        InteractionText.gameObject.SetActive(false);
        EndGameText.gameObject.SetActive(true);
    }

    // Hide end game text
    public void HideEndGameText()
    {
        EndGameText.gameObject.SetActive(false);
    }

    // Set the game over text
    public void SetGameOverText(string text)
    {
        EndGameText.text = text;
    }

    // Coroutine to fade out an image over a duration
    private IEnumerator FadeOut(RawImage image, float duration)
    {
        float time = 0;
        Color initialColor = image.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, time / duration);
            image.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        image.gameObject.SetActive(false);
    }
}