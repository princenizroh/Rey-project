using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerAnswerManager : MonoBehaviour
{
    public Button[] answerButtons; // Assign 3+ buttons in Inspector

    private System.Action<int> onChoiceSelected;
    // private MapMovementAnimation moveMap;
    // Store LeanTween ids for each button
    private Dictionary<Button, int> buttonTweenIds = new Dictionary<Button, int>();

    [System.Obsolete]
    public void ShowChoices(DialogChoice[] choices, System.Action<int> callback)
    {
        Debug.Log("Showing choices...");

        onChoiceSelected = callback;
        buttonTweenIds.Clear();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < choices.Length && choices[i] != null)
            {
                Button btn = answerButtons[i];
                if (btn == null)
                {
                    Debug.LogWarning($"Button at index {i} is null.");
                    continue;
                }

                btn.gameObject.SetActive(true);
                btn.onClick.RemoveAllListeners();

                TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    int tweenId = AnimateButtonText(btnText, choices[i].playerChoice);
                    buttonTweenIds[btn] = tweenId;
                }
                else
                {
                    Debug.LogWarning($"No TMP_Text found on button {i}");
                }

                int index = i; // Important for correct capture
                btn.onClick.AddListener(() => {
                    // If animation is still playing, finish it instantly
                    if (buttonTweenIds.TryGetValue(btn, out int tweenId) && LeanTween.isTweening(tweenId))
                    {   
                        TMP_Text btnText2 = btn.GetComponentInChildren<TMP_Text>();
                        if (btnText2 != null)
                        {
                            btnText2.text = choices[index].playerChoice;
                        }
                        LeanTween.cancel(tweenId);
                        buttonTweenIds.Remove(btn);
                        return; // Don't invoke choice yet, just finish animation
                    }

                    // --- Custom logic: Only detect "mapname:scene_name" pattern ---
                    const string moveMapPrefix = "mapname:";
                    Debug.Log(choices[index].npcResponse);
                    int prefixIndex = choices[index].npcResponse.IndexOf(moveMapPrefix);
                    if (prefixIndex != -1)
                    {
                        int start = prefixIndex + moveMapPrefix.Length;
                        int end = choices[index].npcResponse.IndexOf(' ', start);
                        string mapName;
                        if (end == -1)
                            mapName = choices[index].npcResponse.Substring(start);
                        else
                            mapName = choices[index].npcResponse.Substring(start, end - start);

                        // if (!string.IsNullOrEmpty(mapName))
                        // {
                        //     if (moveMap == null)
                        //         moveMap = FindObjectOfType<MapMovementAnimation>();
                        //     if (moveMap != null)
                        //         moveMap.animationAndMoveMap(mapName);
                        // }
                    }
                    // --- End custom logic ---

                    onChoiceSelected?.Invoke(index);
                    HideChoices(); // Hide all buttons after a choice is made
                });
            }
            else
            {
                if (answerButtons[i] != null)
                    answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideChoices()
    {
        foreach (var btn in answerButtons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = "";
                }
                Destroy(btn.gameObject);
            }
        }
    }

    // Returns the LeanTween id so you can cancel/finish it
    private int AnimateButtonText(TMP_Text btnText, string fullText)
    {
        btnText.text = "";
        int len = fullText.Length;
        int counter = 0;

        int tweenId = LeanTween.value(btnText.gameObject, 0, len, 0.3f)
            .setOnUpdate((float val) =>
            {
                counter = Mathf.Clamp(Mathf.FloorToInt(val), 0, len);
                btnText.text = fullText.Substring(0, counter);
            })
            .setOnComplete(() =>
            {
                btnText.text = fullText;
            }).id;

        return tweenId;
    }
}
