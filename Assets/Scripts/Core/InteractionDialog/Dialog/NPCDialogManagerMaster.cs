using System;
using UnityEngine;

public class NPCDialogManagerMaster : MonoBehaviour
{
    private NPCDialogManager npcDialogManager;
    private bool npcFirstTimeTalk = true;
    private bool dialogChoicesShown = false;

    private string dialogFileName;
    private string fallbackAnswerFile; 

    private enum DialogState
    {
        NormalDialog,
        ShowingChoices,
        ShowingResponse
    }
    private DialogState currentState = DialogState.NormalDialog;

    [System.Obsolete]
    public void InitiateStartDialog(string npcDialogFile)
    {
        if (!npcFirstTimeTalk) return;

        var dialogController = FindObjectOfType<DialogController>();
        var interactionMenu = FindObjectOfType<RightClickToOpenMenu>();

        GameObject dialogObj = dialogController.summonDialogBar();
        if (dialogObj == null)
        {
            Debug.LogError("Dialog bar could not be summoned!");
            return;
        }

        npcDialogManager = dialogObj.GetComponent<NPCDialogManager>();
        if (npcDialogManager == null)
        {
            Debug.LogError("NPCDialogManager component not found on dialogObj!");
            return;
        }

        npcDialogManager.InitiateDialog(npcDialogFile);
        if (interactionMenu != null)
        {
            interactionMenu.manualDestroyMenu();
        }

        npcFirstTimeTalk = true;
    }

    public bool isShowingResponse = false;
    internal Action onDialogFinished;

    [System.Obsolete]
    private void Update()
    {
        if (npcDialogManager == null || !Input.GetMouseButtonDown(0)) return;

        // If at end of dialog, close it
        if (npcDialogManager.GetCurrentIndex() >= npcDialogManager.dialogSequence.dialogBlocks.Length)
        {
            var dialogController = FindObjectOfType<DialogController>();
            if (dialogController != null) dialogController.DestroyDialogInstance();
            onDialogFinished?.Invoke();
            npcDialogManager = null;
            return;
        }

        // If showing a response to a choice, continue to next dialog
        if (isShowingResponse)
        {
            npcDialogManager.currentDialogIndex++; // <-- ADVANCE TO NEXT BLOCK
            npcDialogManager.ShowCurrentDialog();
            isShowingResponse = false; // <-- RESET FLAG
            return;
        }

        // Normal dialog progression
        var currentBlock = npcDialogManager.dialogSequence.dialogBlocks[npcDialogManager.GetCurrentIndex()];
        if (currentBlock.choices == null || currentBlock.choices.Length == 0)
        {
            npcDialogManager.ContinueDialog();
        }
    }

    // Call this from PlayerAnswerManager or NPCDialogManager when a choice is made:
    public void OnChoiceMade()
    {
        dialogChoicesShown = false; // Allow next click to continue dialog
    }
    
}
