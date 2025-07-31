using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DS;

public class NarratorDay1 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narratorText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;
    public IEnumerator Narrate()
    {
        // Set the background image for Day 1 black
        ResetUIState();

        switch (NarratorManager.Instance.currentTime)
        {
            case TimeOfDay.Night:
                yield return StartCoroutine(PlayNightSequence());
                break;
        }
    }

    private void ResetUIState()
    {
    }

    private IEnumerator PlayNightSequence()
    {
        CloseEyes();
        yield return new WaitForSeconds(1f);
        narratorText.text = "Day 1 (Kelahiran)";
        yield return new WaitForSeconds(2f); 
        narratorText.text = "Dunia yang gelap ini.. aku selalu merasakan kehangatan";
        yield return new WaitForSeconds(3f);
        narratorText.gameObject.SetActive(false);
    }

    private void CloseEyes()
    { 
        Color newColor = Color.black;
        newColor.a = 1f; // 1f = 255 penuh opacity (0 sampai 1 di Unity)
        backgroundImage.color = newColor;
        canvasGroup.alpha = 1f; 
    }

    private void FadeOpenEyes()
    {
        
    }

    private void FadeCloseEyes()
    {
        
    }
}
