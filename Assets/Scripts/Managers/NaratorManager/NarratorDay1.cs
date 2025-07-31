using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DS;

public class NarratorDay1 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narratorText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] public DeathScreenEffect deathScreenEffect;
    [SerializeField] private CanvasGroup canvasGroup;
    private Color fadeColor = Color.black;
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
        backgroundImage.color = Color.black;
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(1f);
        narratorText.text = "Day 1 (Kelahiran)";
        yield return new WaitForSeconds(2f); 
        narratorText.text = "Dunia yang gelap ini.. aku selalu merasakan kehangatan";
        yield return new WaitForSeconds(3f);
        narratorText.gameObject.SetActive(false);
        backgroundImage.gameObject.SetActive(false);
        deathScreenEffect.TriggerFadeOut();

    }
}
