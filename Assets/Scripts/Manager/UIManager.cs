using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
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
        StartCoroutine(FadeOut(StartGamePanel, 3f));
    }

    public void SetCrosshair(bool isInteracting)
    {
        BaseCrosshair.sprite = isInteracting ? CrosshairsInteract : CrosshairsNormal;
    }

    public void SetInteractionText(string text)
    {
        InteractionText.text = text;
    }

    public void SetQuitText(string text)
    {
        QuitText.text = text;
    }

    public void UpdateScore(float score)
    {
        ScoreText.text = "Score: " + score;
    }

    public void ShowEndGameText()
    {
        BaseCrosshair.gameObject.SetActive(false);
        InteractionText.gameObject.SetActive(false);
        EndGameText.gameObject.SetActive(true);
    }

    public void HideEndGameText()
    {
        EndGameText.gameObject.SetActive(false);
    }

    public void SetGameOverText(string text)
    {
        EndGameText.text = text;
    }

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