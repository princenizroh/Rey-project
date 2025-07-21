using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogManager : MonoBehaviour
{
    public DialogMasterManager dialogSequence;
    public GameObject dialogBar;
    public TMP_Text npcName;
    public TMP_Text dialogText;
    public Image npcImage;

    public int currentDialogIndex = 0;

    private LTDescr dialogTween;

    [System.Obsolete]
    public void InitiateDialog(string dialogFileName)
    {
        DialogMasterManager usedDialog = Resources.Load<DialogMasterManager>("NpcDialog&Question/" + dialogFileName);
        if (usedDialog == null)
        {
            Debug.LogError("Dialog file not found: " + dialogFileName);
            return;
        }

        dialogSequence = usedDialog;
        currentDialogIndex = 0;
        ShowCurrentDialog();
    }

    [System.Obsolete]
    public void ShowCurrentDialog()
    {
        if (dialogSequence == null || dialogSequence.dialogBlocks.Length <= currentDialogIndex)
        {
            Debug.Log("Dialog ended or not initialized.");

            try
            {
                // PlayerMovement_Indoor playerVerticalMovement = FindObjectOfType<PlayerMovement_Indoor>();
                // if (playerVerticalMovement != null)
                //     playerVerticalMovement.AllowMovement();
            }
            catch
            {
                var dialogController = FindObjectOfType<DialogController>();
                if (dialogController != null)
                    dialogController.DestroyDialogInstance();

                DialogController.DestroyAllQuestionBars(); // <-- Add this line

                return;
            }

        }

        DialogBlock block = dialogSequence.dialogBlocks[currentDialogIndex];

        npcName.text = block.npcName;
        AnimateDialogText(block.npcDialog); // <-- Animate the dialog text
        npcImage.sprite = block.npcImage;

        if (block.choices != null && block.choices.Length > 0)
        {
            var dialogController = FindObjectOfType<DialogController>();
            if (dialogController != null)
                dialogController.summonQuestionBar();

            var playerAnswerManager = FindObjectOfType<PlayerAnswerManager>();
            if (playerAnswerManager != null)
                playerAnswerManager.ShowChoices(block.choices, OnPlayerChoseResponse);
        }
        else
        {
            var playerAnswerManager = FindObjectOfType<PlayerAnswerManager>();
            if (playerAnswerManager != null)
                playerAnswerManager.HideChoices();
        }
    }

    [System.Obsolete]
    public void ContinueDialog()
    {
        // Only advance if we're not showing a choice response
        if (dialogSequence.dialogBlocks[currentDialogIndex].choices == null || 
            dialogSequence.dialogBlocks[currentDialogIndex].choices.Length == 0)
        {
            currentDialogIndex++;
        }
        
        ShowCurrentDialog();
    }

    [System.Obsolete]
    private void OnPlayerChoseResponse(int choiceIndex)
    {
        if (dialogSequence == null || currentDialogIndex < 0 || currentDialogIndex >= dialogSequence.dialogBlocks.Length)
            return;

        DialogBlock block = dialogSequence.dialogBlocks[currentDialogIndex];
        if (block.choices == null || choiceIndex < 0 || choiceIndex >= block.choices.Length)
            return;

        DialogChoice choice = block.choices[choiceIndex];
        AnimateDialogText(choice.npcResponse); // <-- Animate the response text

        var playerAnswerManager = FindObjectOfType<PlayerAnswerManager>();
        if (playerAnswerManager != null)
            playerAnswerManager.HideChoices();

        var nameDialogManager = FindObjectOfType<NPCDialogManagerMaster>();
        if (nameDialogManager != null)
        {
            nameDialogManager.isShowingResponse = true; // <-- ADD THIS LINE
            nameDialogManager.OnChoiceMade();
        }

        // var dialogController = FindObjectOfType<DialogController>();
        // if (dialogController != null)
        //     dialogController.DestroyDialogInstance();

        // Don't immediately advance to next dialog - wait for player click
        // Just show the response and let Update() handle the next click
        // currentDialogIndex++; // Removed this line
    }

    private void AnimateDialogText(string fullText)
    {
        if (dialogTween != null) LeanTween.cancel(gameObject, dialogTween.id);

        // Check for "mapname:<scene_name> " prefix and remove it if present
        string displayText = fullText;
        const string prefix = "mapname:";
        const string exitgameprefix = "exitgame:true";
        if (fullText.StartsWith(prefix))
        {
            int spaceIndex = fullText.IndexOf(' ');
            if (spaceIndex > prefix.Length)
            {
                displayText = fullText.Substring(spaceIndex + 1);
            }
        } else if (fullText.StartsWith(exitgameprefix))
        {
            displayText = "Exiting game...";
            Application.Quit();
        }

        dialogText.text = "";
        int len = displayText.Length;
        int counter = 0;

        dialogTween = LeanTween.value(gameObject, 0, len, 0.5f)
            .setOnUpdate((float val) =>
            {
                counter = Mathf.Clamp(Mathf.FloorToInt(val), 0, len);
                dialogText.text = displayText.Substring(0, counter);
            })
            .setOnComplete(() =>
            {
                dialogText.text = displayText;
            });
    }

    public int GetCurrentIndex()
    {
        return currentDialogIndex;
    }

}
