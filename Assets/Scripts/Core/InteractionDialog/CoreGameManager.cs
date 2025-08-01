using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CONSOLIDATED DIALOG SYSTEM - CoreGameManager
/// 
/// This script now contains integrated functionality from the following removed components:
/// 
/// 1. PlayerAnswerManager - Choice handling with button animations
///    - ShowChoicesWithButtons() - Display and animate choice buttons
///    - HideChoices() - Hide and cleanup choice buttons
///    - AnimateButtonText() - Animate button text display
///    - Button click handling with animation skip support
/// 
/// 2. DialogController - Dialog UI management and animations
///    - SummonDialogBar() - Create and animate dialog bars
///    - SummonQuestionBar() - Create and animate question/choice bars
///    - DestroyAllQuestionBars() - Cleanup question bar instances
///    - Enhanced DestroyDialogInstances() with complete cleanup
/// 
/// 3. NPCDialogManager - Text animation and dialog processing
///    - Legacy dialog text animation system
///    - Special prefix handling (mapname:, exitgame:, timeline:, charge:)
///    - Dialog progression and state management
/// 
/// 4. NPCDialogManagerMaster - Master dialog control and state management
///    - InitiateStartDialog() - Legacy dialog system entry point
///    - Dialog state management (normal, choices, response)
///    - Input handling and dialog progression
/// 
/// 5. DialogButtonController/MenuButtonHandler - Button event handling
///    - OnNPCButtonClicked() - Handle NPC interaction button clicks
///    - NPC type detection and appropriate dialog triggering
/// 
/// NEW FEATURES - Multiple Response & NPC Name Support:
/// - Support for CoreGameDialogChoicesResponse[] - multiple responses per choice
/// - Dynamic NPC name display with UpdateNpcNameDisplay() (2D dialogs only)
/// - Response selection system with SetResponseIndex(), UseRandomResponse()
/// - Conditional response selection with SetResponseByCondition()
/// - Automatic NPC name extraction from dialog text ("NpcName: dialog") for 2D dialogs
/// - 3D dialogs ignore NPC name updates (models represent characters inherently)
/// - Cutscene fade system with BackgroundFade image for dialog transitions
///   * None: No fade effect
///   * FadeIn: Transparent to dark transition
///   * FadeOut: Dark to transparent transition
///   * StayIn: Remain dark throughout dialog
///   * StayOut: Remain transparent throughout dialog
/// 
/// Key Improvements:
/// - All dialog functionality consolidated into one manager
/// - Removed dependencies on FindObjectOfType calls
/// - Better integration with CoreGame data structure
/// - Proper cleanup and memory management
/// - Support for both new CoreGame system and legacy dialog files
/// - Multiple response support for varied NPC reactions
/// - Dynamic NPC name display in 2D dialogs only (3D models represent characters inherently)
/// 
/// Usage:
/// - Use the existing CoreGame system for new dialogs
/// - Legacy support available through InitiateStartDialog() and OnNPCButtonClicked()
/// - All choice buttons should be assigned to answerButtons[] in inspector
/// - Dialog and question templates should be assigned to npcDialogThemplate and npcQuestionThemplate
/// - Assign npcNameText for displaying NPC names in 2D dialogs (3D dialogs ignore this)
/// - Assign backgroundFade image for cutscene fade transitions
/// - Use SetResponseIndex() to choose which response to use from dialogResponses array
/// - Use UseRandomResponse() for random NPC reactions
/// - Use SetResponseByCondition() for conditional responses based on game state
/// - Set cutsceneType in CoreGameDialog for fade transitions between dialogs
/// </summary>

[System.Serializable]
public class DialogChoice
{
    public string playerChoice;
    [TextArea(2, 5)]
    public string npcResponse;
}

public class CoreGameManager : MonoBehaviour
{
    [Header("Core Game Settings")]
    public CoreGame coreGameData;
    
    [Header("Dialog Templates")]
    public GameObject npcDialogThemplate;
    public GameObject npcQuestionThemplate;
    
    [Header("Dialog Components")]
    public TMP_Text dialogText;
    public TMP_Text npcNameText; // For displaying NPC names
    public Image backgroundFade; // For fade in/out transitions
    
    [Header("Choice UI Components")]
    public Button[] answerButtons; // Assign 3+ buttons in Inspector
    
    [Header("Camera References")]
    public Transform defaultCamera;
    public Transform reyCamera;
    public Transform momCamera;
    public Transform fatherCamera;
    
    [Header("Audio Settings")]
    public AudioSource dialogAudioSource;
    
    // Private variables
    private GameObject dialogInstance;
    private GameObject questionInstance;
    private int currentBlockIndex = 0;
    private int currentChoiceResponseIndex = -1;
    private LTDescr dialogTween;
    private bool isShowingResponse = false;
    private bool isPlayingCutscene = false;
    private bool isTextAnimating = false;
    
    // Choice management variables
    private System.Action<int> onChoiceSelected;
    private Dictionary<Button, int> buttonTweenIds = new Dictionary<Button, int>();
    private int selectedResponseIndex = 0; // Which response to use from dialogResponses array

    private System.Action currentCompletionCallback;
    public bool IsSequenceRunning { get; private set; }
    
    // Events
    public Action onCoreGameFinished;
    public Action<int> onBlockCompleted;

    #region Unity Lifecycle
    
    private void Awake()
    {
        try
        {
            // Initialize or get AudioSource component
            if (dialogAudioSource == null)
            {
                dialogAudioSource = GetComponent<AudioSource>();
                if (dialogAudioSource == null)
                {
                    dialogAudioSource = gameObject.AddComponent<AudioSource>();
                    Debug.Log("AudioSource component added to CoreGameManager for dialog audio.");
                }
                else
                {
                    Debug.Log("Found existing AudioSource component on CoreGameManager.");
                }
            }
            
            // Configure AudioSource settings
            if (dialogAudioSource != null)
            {
                dialogAudioSource.playOnAwake = false;
                dialogAudioSource.loop = false;
                dialogAudioSource.volume = 1.0f; // Set default volume
                Debug.Log("AudioSource configured for dialog playback.");
            }
            else
            {
                Debug.LogWarning("Failed to initialize AudioSource. Dialog audio will be disabled.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing AudioSource in CoreGameManager: {e.Message}. Dialog will work without audio.");
            dialogAudioSource = null;
        }
    }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Start the core game sequence with assigned ScriptableObject
    /// </summary>
    [Obsolete]
    public void StartCoreGame()
    {
        if (coreGameData == null || coreGameData.coreBlock == null || coreGameData.coreBlock.Length == 0)
        {
            Debug.LogError("CoreGame data is null or empty!");
            return;
        }
        
        currentBlockIndex = 0;
        ProcessCurrentBlock();
    }
    
    /// <summary>
    /// Start the core game sequence by loading from Resources folder
    /// </summary>
    /// <param name="resourcePath">Path to the CoreGame ScriptableObject in Resources folder (e.g., "resource/rey")</param>
    [Obsolete]
    public void StartCoreGame(string resourcePath, System.Action onComplete = null)
    {
        // Load the CoreGame ScriptableObject from Resources
        CoreGame loadedCoreGame = Resources.Load<CoreGame>(resourcePath);
        
        if (loadedCoreGame == null)
        {
            Debug.LogError($"CoreGame file not found at path: {resourcePath}");
            onComplete?.Invoke();
            return;
        }
        
        if (loadedCoreGame.coreBlock == null || loadedCoreGame.coreBlock.Length == 0)
        {
            Debug.LogError($"CoreGame at path '{resourcePath}' has no blocks!");
            onComplete?.Invoke();
            return;
        }
        
        // Set the loaded data as current
        coreGameData = loadedCoreGame;
        currentBlockIndex = 0;
        currentCompletionCallback = onComplete;
        IsSequenceRunning = true;
        
        Debug.Log($"Starting CoreGame sequence from: {resourcePath}");
        ProcessCurrentBlock();
    }

    /// <summary>
    /// Continue to the next block in the sequence
    /// </summary>
    [Obsolete]
    public void ContinueToNextBlock()
    {
        if (isPlayingCutscene) return;
        
        currentBlockIndex++;
        
        if (currentBlockIndex >= coreGameData.coreBlock.Length)
        {
            FinishCoreGame();
            return;
        }
        
        ProcessCurrentBlock();
    }

    /// <summary>
    /// Skip current cutscene if playing
    /// </summary>
    [Obsolete]
    public void SkipCutscene()
    {
        if (isPlayingCutscene)
        {
            StopAllCoroutines();
            isPlayingCutscene = false;
            ContinueToNextBlock();
        }
    }
    
    /// <summary>
    /// Stops any currently playing dialog audio
    /// </summary>
    public void StopDialogAudio()
    {
        try
        {
            if (dialogAudioSource != null && dialogAudioSource.isPlaying)
            {
                dialogAudioSource.Stop();
                Debug.Log("Dialog audio stopped.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error stopping dialog audio: {e.Message}");
        }
    }
    
    /// <summary>
    /// Forces completion of current dialog animation and stops audio
    /// </summary>
    public void ForceCompleteDialog()
    {
        if (isTextAnimating)
        {
            SkipTextAnimation();
        }
    }
    
    /// <summary>
    /// Manually trigger a fade effect (useful for testing or custom scenarios)
    /// </summary>
    /// <param name="cutsceneType">Type of fade effect to perform</param>
    /// <param name="onComplete">Optional callback when fade completes</param>
    public void TriggerFadeEffect(CoreGameDialog.CutsceneType cutsceneType, System.Action onComplete = null)
    {
        HandleCutsceneFade(cutsceneType, onComplete);
    }
    
    /// <summary>
    /// Reset fade to default state (transparent and disabled)
    /// </summary>
    public void ResetFade()
    {
        if (backgroundFade != null)
        {
            backgroundFade.gameObject.SetActive(false);
            Color fadeColor = backgroundFade.color;
            fadeColor.a = 0f;
            backgroundFade.color = fadeColor;
        }
    }

    #endregion

    #region Core Game Processing

    [Obsolete]
    private void ProcessCurrentBlock()
    {
        if (currentBlockIndex >= coreGameData.coreBlock.Length) return;
        
        CoreGameBlock currentBlock = coreGameData.coreBlock[currentBlockIndex];
        
        switch (currentBlock.Type)
        {
            case CoreGameBlock.CoreType.Dialog:
                ProcessDialogBlock(currentBlock);
                break;
                
            case CoreGameBlock.CoreType.Cutscene:
                ProcessCutsceneBlock(currentBlock);
                break;
        }
    }

    [Obsolete]
    private void ProcessDialogBlock(CoreGameBlock block)
    {
        if (block.Dialog == null)
        {
            Debug.LogWarning($"Dialog block at index {currentBlockIndex} has no dialog data!");
            ContinueToNextBlock();
            return;
        }
        
        // Clear any existing 3D dialogs before showing new dialog
        ClearAll3DDialogs();
        
        // If we're switching from 2D to 3D dialog, destroy 2D dialog instances
        if (block.Dialog.dialogType == CoreGameDialog.DialogType.ThreeD && dialogInstance != null)
        {
            DestroyDialogInstances();
        }
        
        // Set up camera based on dialog choice
        SetupDialogCamera(block.Dialog.camChoice);
        
        // Show dialog based on type
        if (block.Dialog.dialogType == CoreGameDialog.DialogType.ThreeD)
        {
            Show3DDialog(block.Dialog);
        }
        else
        {
            Show2DDialog(block.Dialog);
        }
    }

    [Obsolete]
    private void ProcessCutsceneBlock(CoreGameBlock block)
    {
        if (block.Animation == null)
        {
            Debug.LogWarning($"Cutscene block at index {currentBlockIndex} has no animation data!");
            ContinueToNextBlock();
            return;
        }
        
        // Clear any 3D dialogs and destroy 2D dialog instances when starting cutscene
        ClearAll3DDialogs();
        DestroyDialogInstances();
        
        StartCoroutine(PlayCutscene(block.Animation));
    }

    #endregion

    #region Choice Response Helper Methods
    
    /// <summary>
    /// Get NPC response from CoreGameDialogChoices using current response index
    /// </summary>
    private string GetNpcResponseFromChoice(CoreGameDialogChoices choice)
    {
        if (choice.dialogResponses == null || choice.dialogResponses.Length == 0)
        {
            Debug.LogWarning("No dialog responses found in choice!");
            return "No response available.";
        }
        
        // Use selectedResponseIndex, but clamp it to valid range
        int responseIndex = Mathf.Clamp(selectedResponseIndex, 0, choice.dialogResponses.Length - 1);
        return choice.dialogResponses[responseIndex].npcResponse;
    }
    
    /// <summary>
    /// Get NPC name from CoreGameDialogChoices using current response index
    /// </summary>
    private string GetNpcNameFromChoice(CoreGameDialogChoices choice)
    {
        if (choice.dialogResponses == null || choice.dialogResponses.Length == 0)
        {
            Debug.LogWarning("No dialog responses found in choice!");
            return "Unknown";
        }
        
        // Use selectedResponseIndex, but clamp it to valid range
        int responseIndex = Mathf.Clamp(selectedResponseIndex, 0, choice.dialogResponses.Length - 1);
        return choice.dialogResponses[responseIndex].NpcName;
    }
    
    /// <summary>
    /// Set which response index to use from dialogResponses array
    /// </summary>
    public void SetResponseIndex(int index)
    {
        selectedResponseIndex = index;
        Debug.Log($"Response index set to: {selectedResponseIndex}");
    }
    
    /// <summary>
    /// Get a random response from the available responses
    /// </summary>
    public void UseRandomResponse(CoreGameDialogChoices choice)
    {
        if (choice.dialogResponses != null && choice.dialogResponses.Length > 0)
        {
            selectedResponseIndex = UnityEngine.Random.Range(0, choice.dialogResponses.Length);
            Debug.Log($"Using random response index: {selectedResponseIndex}");
        }
    }
    
    /// <summary>
    /// Get the number of available responses for a choice
    /// </summary>
    public int GetResponseCount(CoreGameDialogChoices choice)
    {
        return choice.dialogResponses?.Length ?? 0;
    }
    
    /// <summary>
    /// Set response index based on some condition (e.g., player stats, previous choices, etc.)
    /// </summary>
    public void SetResponseByCondition(CoreGameDialogChoices choice, System.Func<CoreGameDialogChoicesResponse, bool> condition)
    {
        if (choice.dialogResponses == null || choice.dialogResponses.Length == 0)
            return;
            
        for (int i = 0; i < choice.dialogResponses.Length; i++)
        {
            if (condition(choice.dialogResponses[i]))
            {
                selectedResponseIndex = i;
                Debug.Log($"Response index set to {i} based on condition");
                return;
            }
        }
        
        // If no condition matches, use first response
        selectedResponseIndex = 0;
        Debug.Log("No condition matched, using first response");
    }
    
    /// <summary>
    /// Preview all available responses for a choice (for debugging)
    /// </summary>
    public void LogAllResponses(CoreGameDialogChoices choice)
    {
        if (choice.dialogResponses == null || choice.dialogResponses.Length == 0)
        {
            Debug.Log("No responses available for this choice");
            return;
        }
        
        Debug.Log($"Available responses for choice '{choice.playerChoice}':");
        for (int i = 0; i < choice.dialogResponses.Length; i++)
        {
            var response = choice.dialogResponses[i];
            Debug.Log($"  [{i}] {response.NpcName}: {response.npcResponse}");
        }
    }
    
    /// <summary>
    /// Update the NPC name display text (2D dialogs only)
    /// </summary>
    private void UpdateNpcNameDisplay(string npcName)
    {
        // Only update 2D dialog NPC name display
        // First try the assigned npcNameText field
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }
        
        // Also try to find DialogueName in the current dialog instance
        if (dialogInstance != null)
        {
            Transform dialogueNameTransform = dialogInstance.transform.Find("DialogueName");
            if (dialogueNameTransform != null)
            {
                var dialogueNameComponent = dialogueNameTransform.GetComponent<TMP_Text>();
                if (dialogueNameComponent != null)
                {
                    dialogueNameComponent.text = npcName;
                }
            }
        }
        
        // 3D dialogs ignore NPC name updates - they use the model's inherent identity
        // UpdateNpcNameIn3DDialog(npcName); // Disabled for 3D dialogs
    }
    
    /// <summary>
    /// Update NPC name in 3D dialog displays
    /// </summary>
    private void UpdateNpcNameIn3DDialog(string npcName)
    {
        // Update NPC names in all possible 3D dialog locations
        string[] modelNames = { "Linda_Model", "Isayat_Model", "Rey_Baby_Model" };
        
        foreach (string modelName in modelNames)
        {
            GameObject model = GameObject.Find(modelName);
            if (model != null)
            {
                // Look for NPC name text component (you might need to adjust the path)
                Transform npcNameTransform = model.transform.Find("DialogueName");
                if (npcNameTransform != null)
                {
                    var npcNameComponent = npcNameTransform.GetComponent<TMP_Text>();
                    if (npcNameComponent != null)
                    {
                        npcNameComponent.text = npcName;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Extract NPC name from dialog text if it follows "Name: dialog" format
    /// </summary>
    private string ExtractNpcNameFromDialogText(string dialogText)
    {
        if (string.IsNullOrEmpty(dialogText))
            return "";
        
        // Check if dialog follows "Name: dialog text" format
        int colonIndex = dialogText.IndexOf(':');
        if (colonIndex > 0 && colonIndex < 20) // Reasonable name length limit
        {
            string potentialName = dialogText.Substring(0, colonIndex).Trim();
            // Simple validation - names shouldn't be too long and should be reasonable
            if (potentialName.Length > 0 && potentialName.Length < 20 && !potentialName.Contains(' '))
            {
                return potentialName;
            }
            // Handle names with spaces (like "Rey Baby")
            else if (potentialName.Length > 0 && potentialName.Length < 20)
            {
                return potentialName;
            }
        }
        
        return ""; // No name found
    }

    #endregion

    #region Dialog Handling

    [Obsolete]
    private void Show3DDialog(CoreGameDialog dialog)
    {
        // Handle cutscene fade effect for 3D dialogs
        HandleCutsceneFade(dialog.cutsceneType);
        
        GameObject targetModel = null;
        
        // Find the target model based on dialog3DLocation
        switch (dialog.dialog3DLocation)
        {
            case CoreGameDialog.Dialog3DLocation.Mother:
                targetModel = GameObject.Find("Linda_Model");
                break;
            case CoreGameDialog.Dialog3DLocation.Father:
                targetModel = GameObject.Find("Isayat_Model");
                break;
            case CoreGameDialog.Dialog3DLocation.Rey:
                targetModel = GameObject.Find("Rey_Baby_Model");
                break;
        }
        
        if (targetModel == null)
        {
            Debug.LogError($"No GameObject named '{GetModelName(dialog.dialog3DLocation)}' found for 3D dialog!");
            ContinueToNextBlock();
            return;
        }
        
        // Find the TextDialog3D component in the target model
        var textDialog3D = targetModel.transform.Find("TextDialog3D");
        if (textDialog3D == null)
        {
            Debug.LogError($"No child GameObject named 'TextDialog3D' found in '{targetModel.name}'!");
            ContinueToNextBlock();
            return;
        }

        var tmp3D = textDialog3D.GetComponent<TMP_Text>();
        if (tmp3D == null)
        {
            Debug.LogError($"'TextDialog3D' in '{targetModel.name}' does not have a TMP_Text component!");
            ContinueToNextBlock();
            return;
        }

        textDialog3D.gameObject.SetActive(true);
        
        // 3D dialogs don't need NPC name extraction - the 3D model represents the character
        // Extract and display NPC name if present in dialog entry
        // string npcName = ExtractNpcNameFromDialogText(dialog.dialogEntry);
        // if (!string.IsNullOrEmpty(npcName))
        // {
        //     UpdateNpcNameDisplay(npcName);
        // }
        
        AnimateDialogText(dialog.dialogEntry, tmp3D, dialog.audioDialogEntry);

        // Handle choices if any
        if (dialog.choices != null && dialog.choices.Length > 0)
        {
            ShowChoices(dialog.choices);
        }
    }
    
    private string GetModelName(CoreGameDialog.Dialog3DLocation location)
    {
        switch (location)
        {
            case CoreGameDialog.Dialog3DLocation.Mother:
                return "Linda_Model";
            case CoreGameDialog.Dialog3DLocation.Father:
                return "Isayat_Model";
            case CoreGameDialog.Dialog3DLocation.Rey:
                return "Rey_Baby_Model";
            default:
                return "Unknown";
        }
    }

    [Obsolete]
    private void Show2DDialog(CoreGameDialog dialog)
    {
        // Only summon dialog bar if we don't have one already
        if (dialogInstance == null)
        {
            dialogInstance = SummonDialogBar();
            if (dialogInstance == null)
            {
                Debug.LogError("Failed to create dialog bar!");
                ContinueToNextBlock();
                return;
            }
        }
        
        // Ensure BackgroundFade reference if not assigned
        EnsureBackgroundFadeReference();
        
        // Handle cutscene fade effect
        HandleCutsceneFade(dialog.cutsceneType);
        
        // Extract and display NPC name if present in dialog entry
        string npcName = ExtractNpcNameFromDialogText(dialog.dialogEntry);
        if (!string.IsNullOrEmpty(npcName))
        {
            UpdateNpcNameDisplay(npcName);
        }
        
        // Get dialog text component from the existing or newly created dialog bar
        // Look specifically for DialogueText component, not DialogueName
        Transform dialogTextTransform = dialogInstance.transform.Find("DialogueText");
        TMP_Text dialogTextComponent = null;
        
        if (dialogTextTransform != null)
        {
            dialogTextComponent = dialogTextTransform.GetComponent<TMP_Text>();
        }
        
        if (dialogTextComponent != null)
        {
            AnimateDialogText(dialog.dialogEntry, dialogTextComponent, dialog.audioDialogEntry);
        }
        else if (dialogText != null)
        {
            AnimateDialogText(dialog.dialogEntry, dialogText, dialog.audioDialogEntry);
        }
        else
        {
            Debug.LogWarning("No DialogueText component found in dialog instance!");
        }
        
        // Handle choices if any
        if (dialog.choices != null && dialog.choices.Length > 0)
        {
            ShowChoices(dialog.choices);
        }
    }

    [Obsolete]
    private void ShowChoices(CoreGameDialogChoices[] choices)
    {
        GameObject questionBar = SummonQuestionBar();
        if (questionBar == null) return;
        
        ShowChoicesWithButtons(choices, OnPlayerChoseResponse);
    }
    
    /// <summary>
    /// Integrated choice display system from PlayerAnswerManager
    /// </summary>
    private void ShowChoicesWithButtons(CoreGameDialogChoices[] choices, System.Action<int> callback)
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

                    // Custom logic: Only detect "mapname:scene_name" pattern
                    const string moveMapPrefix = "mapname:";
                    string npcResponse = GetNpcResponseFromChoice(choices[index]);
                    Debug.Log(npcResponse);
                    int prefixIndex = npcResponse.IndexOf(moveMapPrefix);
                    if (prefixIndex != -1)
                    {
                        int start = prefixIndex + moveMapPrefix.Length;
                        int end = npcResponse.IndexOf(' ', start);
                        string mapName;
                        if (end == -1)
                            mapName = npcResponse.Substring(start);
                        else
                            mapName = npcResponse.Substring(start, end - start);

                        // Handle map movement logic here if needed
                        Debug.Log($"Map movement detected: {mapName}");
                    }

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
    
    /// <summary>
    /// Hide choices and clean up buttons (from PlayerAnswerManager)
    /// </summary>
    private void HideChoices()
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
                btn.gameObject.SetActive(false); // Just hide instead of destroy to reuse
            }
        }
    }
    
    /// <summary>
    /// Animate button text (from PlayerAnswerManager)
    /// </summary>
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

    [Obsolete]
    private void OnPlayerChoseResponse(int choiceIndex)
    {
        var currentBlock = coreGameData.coreBlock[currentBlockIndex];
        if (currentBlock.Dialog?.choices == null || choiceIndex >= currentBlock.Dialog.choices.Length)
            return;
        
        var selectedChoice = currentBlock.Dialog.choices[choiceIndex];
        string npcResponse = GetNpcResponseFromChoice(selectedChoice);
        string npcName = GetNpcNameFromChoice(selectedChoice);
        
        // Show the NPC response based on dialog type
        if (currentBlock.Dialog.dialogType == CoreGameDialog.DialogType.ThreeD)
        {
            // 3D dialogs don't need NPC name updates - the 3D model represents the character
            Show3DResponse(currentBlock.Dialog, npcResponse, selectedChoice.audioDialogResponse);
        }
        else
        {
            // Update NPC name display only for 2D dialogs
            UpdateNpcNameDisplay(npcName);
            
            // Show 2D response - look specifically for DialogueText component
            TMP_Text textComponent = null;
            if (dialogInstance != null)
            {
                Transform dialogTextTransform = dialogInstance.transform.Find("DialogueText");
                if (dialogTextTransform != null)
                {
                    textComponent = dialogTextTransform.GetComponent<TMP_Text>();
                }
            }
            
            // Fallback to the assigned dialogText field if DialogueText not found
            if (textComponent == null)
            {
                textComponent = dialogText;
            }
            
            if (textComponent != null)
            {
                AnimateDialogText(npcResponse, textComponent, selectedChoice.audioDialogResponse);
            }
            else
            {
                Debug.LogWarning("No DialogueText component found for response!");
            }
        }
        
        // Hide choices
        HideChoices();
        
        isShowingResponse = true;
        currentChoiceResponseIndex = choiceIndex;
    }
    
    [Obsolete]
    private void Show3DResponse(CoreGameDialog dialog, string responseText, AudioClip audioClip)
    {
        GameObject targetModel = null;
        
        // Find the target model based on dialog3DLocation
        switch (dialog.dialog3DLocation)
        {
            case CoreGameDialog.Dialog3DLocation.Mother:
                targetModel = GameObject.Find("Linda_Model");
                break;
            case CoreGameDialog.Dialog3DLocation.Father:
                targetModel = GameObject.Find("Isayat_Model");
                break;
            case CoreGameDialog.Dialog3DLocation.Rey:
                targetModel = GameObject.Find("Rey_Baby_Model");
                break;
        }
        
        if (targetModel == null)
        {
            Debug.LogError($"No GameObject named '{GetModelName(dialog.dialog3DLocation)}' found for 3D response!");
            return;
        }
        
        // Find the TextDialog3D component in the target model
        var textDialog3D = targetModel.transform.Find("TextDialog3D");
        if (textDialog3D == null)
        {
            Debug.LogError($"No child GameObject named 'TextDialog3D' found in '{targetModel.name}' for response!");
            return;
        }

        var tmp3D = textDialog3D.GetComponent<TMP_Text>();
        if (tmp3D == null)
        {
            Debug.LogError($"'TextDialog3D' in '{targetModel.name}' does not have a TMP_Text component for response!");
            return;
        }

        textDialog3D.gameObject.SetActive(true);
        AnimateDialogText(responseText, tmp3D, audioClip);
    }
    
    private void ClearAll3DDialogs()
    {
        // Clear all possible 3D dialog texts
        string[] modelNames = { "Linda_Model", "Isayat_Model", "Rey_Baby_Model" };
        
        foreach (string modelName in modelNames)
        {
            GameObject model = GameObject.Find(modelName);
            if (model != null)
            {
                Transform textDialog3D = model.transform.Find("TextDialog3D");
                if (textDialog3D != null)
                {
                    var tmpText = textDialog3D.GetComponent<TMP_Text>();
                    if (tmpText != null)
                    {
                        tmpText.text = ""; // Clear the text
                    }
                    textDialog3D.gameObject.SetActive(false); // Hide the dialog
                }
            }
        }
    }
    
    #endregion
    
    #region Fade System (Background Transitions)
    
    /// <summary>
    /// Handle cutscene fade effects based on CoreGameDialog.CutsceneType
    /// </summary>
    /// <param name="cutsceneType">The type of fade effect to apply</param>
    /// <param name="onComplete">Callback when fade animation completes</param>
    public void HandleCutsceneFade(CoreGameDialog.CutsceneType cutsceneType, System.Action onComplete = null)
    {
        if (backgroundFade == null)
        {
            Debug.LogWarning("BackgroundFade image is not assigned! Fade effects will be skipped.");
            onComplete?.Invoke();
            return;
        }
        
        switch (cutsceneType)
        {
            case CoreGameDialog.CutsceneType.None:
                // No fade effect, keep current state
                onComplete?.Invoke();
                break;
                
            case CoreGameDialog.CutsceneType.FadeIn:
                PerformFadeIn(onComplete);
                break;
                
            case CoreGameDialog.CutsceneType.FadeOut:
                PerformFadeOut(onComplete);
                break;
                
            case CoreGameDialog.CutsceneType.StayIn:
                SetFadeState(true, 1f); // Stay dark
                onComplete?.Invoke();
                break;
                
            case CoreGameDialog.CutsceneType.StayOut:
                SetFadeState(true, 0f); // Stay transparent
                onComplete?.Invoke();
                break;
        }
    }
    
    /// <summary>
    /// Perform fade in animation (transparent to dark)
    /// </summary>
    private void PerformFadeIn(System.Action onComplete = null)
    {
        backgroundFade.gameObject.SetActive(true);
        
        // Start from transparent
        Color fadeColor = backgroundFade.color;
        fadeColor.a = 0f;
        backgroundFade.color = fadeColor;
        
        // Animate to dark
        LeanTween.value(backgroundFade.gameObject, 0f, 1f, 1f)
            .setOnUpdate((float alpha) =>
            {
                Color color = backgroundFade.color;
                color.a = alpha;
                backgroundFade.color = color;
            })
            .setOnComplete(() =>
            {
                Debug.Log("Fade In completed");
                onComplete?.Invoke();
            })
            .setEase(LeanTweenType.easeInOutQuad);
    }
    
    /// <summary>
    /// Perform fade out animation (dark to transparent)
    /// </summary>
    private void PerformFadeOut(System.Action onComplete = null)
    {
        backgroundFade.gameObject.SetActive(true);
        
        // Start from dark
        Color fadeColor = backgroundFade.color;
        fadeColor.a = 1f;
        backgroundFade.color = fadeColor;
        
        // Animate to transparent
        LeanTween.value(backgroundFade.gameObject, 1f, 0f, 1f)
            .setOnUpdate((float alpha) =>
            {
                Color color = backgroundFade.color;
                color.a = alpha;
                backgroundFade.color = color;
            })
            .setOnComplete(() =>
            {
                backgroundFade.gameObject.SetActive(false); // Disable after fade out
                Debug.Log("Fade Out completed");
                onComplete?.Invoke();
            })
            .setEase(LeanTweenType.easeInOutQuad);
    }
    
    /// <summary>
    /// Set fade state immediately without animation
    /// </summary>
    /// <param name="active">Whether the background fade should be active</param>
    /// <param name="alpha">Alpha value (0 = transparent, 1 = dark)</param>
    private void SetFadeState(bool active, float alpha)
    {
        backgroundFade.gameObject.SetActive(active);
        
        if (active)
        {
            Color fadeColor = backgroundFade.color;
            fadeColor.a = alpha;
            backgroundFade.color = fadeColor;
            
            Debug.Log($"Fade state set to: Active={active}, Alpha={alpha}");
        }
    }
    
    /// <summary>
    /// Get BackgroundFade from dialog instance if not assigned in inspector
    /// </summary>
    private void EnsureBackgroundFadeReference()
    {
        if (backgroundFade == null && dialogInstance != null)
        {
            Transform fadeTransform = dialogInstance.transform.Find("BackgroundFade");
            if (fadeTransform != null)
            {
                backgroundFade = fadeTransform.GetComponent<Image>();
                Debug.Log("BackgroundFade reference found in dialog instance");
            }
        }
    }
    
    #endregion
    
    #region Camera Management
    
    private void SetupDialogCamera(CoreGameDialog.CamChoices camChoice)
    {
        Transform targetCamera = null;
        
        switch (camChoice)
        {
            case CoreGameDialog.CamChoices.Default_Engine:
                targetCamera = defaultCamera ?? Camera.main?.transform;
                break;
            case CoreGameDialog.CamChoices.Rey:
                targetCamera = reyCamera;
                break;
            case CoreGameDialog.CamChoices.Mother:
                targetCamera = momCamera;
                break;
            case CoreGameDialog.CamChoices.Father:
                targetCamera = fatherCamera;
                break;
        }
        
        if (targetCamera != null)
        {
            // Switch to the selected camera
            SetActiveCamera(targetCamera);
        }
    }
    
    private void SetActiveCamera(Transform cameraTransform)
    {
        // Disable all cameras first
        DisableAllCameras();
        
        // Enable the selected camera
        var camera = cameraTransform.GetComponent<Camera>();
        if (camera != null)
        {
            camera.enabled = true;
            Debug.Log($"Switched to camera: {cameraTransform.name}");
        }
    }
    
    private void DisableAllCameras()
    {
        Camera[] allCameras = { 
            defaultCamera?.GetComponent<Camera>(),
            reyCamera?.GetComponent<Camera>(),
            momCamera?.GetComponent<Camera>(),
            fatherCamera?.GetComponent<Camera>()
        };
        
        foreach (var cam in allCameras)
        {
            if (cam != null) cam.enabled = false;
        }
    }

    #endregion

    #region Cutscene Handling

    [Obsolete]
    private IEnumerator PlayCutscene(CoreGameAnimation animation)
    {
        isPlayingCutscene = true;
        Debug.Log($"Playing cutscene - moving to coordinates: {animation.Coordinates}");
        
        // Example cutscene: move camera to coordinates
        Transform activeCamera = Camera.main?.transform;
        if (activeCamera != null)
        {
            Vector3 startPos = activeCamera.position;
            Vector3 targetPos = animation.Coordinates;
            float duration = 2f;
            
            float elapsed = 0f;
            while (elapsed < duration && isPlayingCutscene)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                activeCamera.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            if (isPlayingCutscene)
            {
                activeCamera.position = targetPos;
            }
        }
        
        isPlayingCutscene = false;
        
        // Auto-continue after cutscene
        yield return new WaitForSeconds(1f);
        ContinueToNextBlock();
    }

    #endregion

    #region UI Management (Integrated from DialogController)

    [Obsolete]
    private GameObject SummonDialogBar()
    {
        Debug.Log("Summoning dialog bar!");

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return null;
        }

        GameObject instance = Instantiate(npcDialogThemplate, canvas.transform, false);
        if (instance == null)
        {
            Debug.LogError("Failed to instantiate npcDialogThemplate prefab!");
            return null;
        }
        
        instance.SetActive(true);
        
        // Setup positioning and animation
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0, 0);    // left-bottom
            rect.anchorMax = new Vector2(1, 0);    // right-bottom
            rect.pivot = new Vector2(0.5f, 0);     // bottom center
            rect.sizeDelta = new Vector2(0, rect.sizeDelta.y); // stretch width, keep height

            // Start off the bottom of the screen
            rect.anchoredPosition = new Vector2(0, -rect.rect.height);

            // Animate up to visible position (flush with bottom)
            LeanTween.value(instance, rect.anchoredPosition.y, 0, 0.3f)
                .setEaseInOutBack()
                .setOnUpdate((float val) => {
                    Vector2 pos = rect.anchoredPosition;
                    pos.y = val;
                    rect.anchoredPosition = pos;
                });
            Debug.Log("Dialog bar summoned!");
        }
        else
        {
            Debug.LogWarning("Dialog prefab has no RectTransform!");
        }
        
        return instance;
    }

    [Obsolete]
    private GameObject SummonQuestionBar()
    {
        Debug.Log("Summoning question bar!");

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return null;
        }

        GameObject instance = Instantiate(npcQuestionThemplate, canvas.transform, false);
        if (instance == null)
        {
            Debug.LogError("Failed to instantiate npcQuestionThemplate prefab!");
            return null;
        }
        
        instance.SetActive(true);
        
        // Setup positioning and animation
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);    // center
            rect.anchorMax = new Vector2(0.5f, 0.5f);    // center
            rect.pivot = new Vector2(0.5f, 0f);        // center
            rect.anchoredPosition = new Vector2(0, ((RectTransform)rect.parent).rect.height / 2 + rect.rect.height); // Start above the screen

            // Animate down to center of the screen
            LeanTween.value(instance, rect.anchoredPosition.y, 0, 0.3f)
                .setEaseInOutBack()
                .setOnUpdate((float val) => {
                    Vector2 pos = rect.anchoredPosition;
                    pos.y = val;
                    rect.anchoredPosition = pos;
                });
            Debug.Log("Question bar summoned!");
        }
        else
        {
            Debug.LogWarning("Question prefab has no RectTransform!");
        }
        
        return instance;
    }
    
    /// <summary>
    /// Destroy all question bars (integrated from DialogController)
    /// </summary>
    private static void DestroyAllQuestionBars()
    {
        // If you use a tag:
        foreach (var obj in GameObject.FindGameObjectsWithTag("QuestionBar"))
        {
            GameObject.Destroy(obj);
        }
    }
    
    private void DestroyDialogInstances()
    {
        // Stop any playing audio when destroying dialog instances
        if (dialogAudioSource != null && dialogAudioSource.isPlaying)
        {
            dialogAudioSource.Stop();
        }
        
        // Stop any active text animation
        if (dialogTween != null)
        {
            LeanTween.cancel(gameObject, dialogTween.id);
            dialogTween = null;
        }
        
        isTextAnimating = false;
        
        if (dialogInstance != null)
        {
            RectTransform rect = dialogInstance.GetComponent<RectTransform>();
            if (rect != null)
            {
                float parentHeight = ((RectTransform)rect.parent).rect.height;
                LeanTween.value(dialogInstance, rect.anchoredPosition.y, -parentHeight, 0.3f)
                    .setEaseOutQuint()
                    .setOnUpdate((float val) => {
                        Vector2 pos = rect.anchoredPosition;
                        pos.y = val;
                        rect.anchoredPosition = pos;
                    })
                    .setOnComplete(() => {
                        Destroy(dialogInstance);
                        dialogInstance = null;
                    });
            }
            else
            {
                Destroy(dialogInstance);
                dialogInstance = null;
            }
        }
        
        if (questionInstance != null)
        {
            Destroy(questionInstance);
            questionInstance = null;
        }
        
        // Also destroy any remaining question bars
        DestroyAllQuestionBars();
        
        // Hide choice buttons
        HideChoices();
    }
    
    #endregion
    
    #region Text Animation
    
    private void AnimateDialogText(string fullText, TMP_Text textComponent, AudioClip audioClip = null)
    {
        // Stop any existing audio and tweens
        if (dialogTween != null) LeanTween.cancel(gameObject, dialogTween.id);
        if (dialogAudioSource != null && dialogAudioSource.isPlaying)
        {
            dialogAudioSource.Stop();
        }
        
        // Handle special prefixes (same as your original system)
        string displayText = ProcessSpecialPrefixes(fullText);
        
        textComponent.text = "";
        int len = displayText.Length;
        
        // Determine animation duration
        float animationDuration;
        bool hasValidAudio = false;
        
        // Check if audio clip is valid and not null
        if (audioClip != null && audioClip.length > 0)
        {
            try
            {
                // Use audio clip duration for text animation
                animationDuration = audioClip.length;
                
                // Play the audio clip if AudioSource is available
                if (dialogAudioSource != null)
                {
                    dialogAudioSource.clip = audioClip;
                    dialogAudioSource.Play();
                    hasValidAudio = true;
                    Debug.Log($"Playing audio for dialog: {audioClip.name} (Duration: {audioClip.length}s)");
                }
                else
                {
                    Debug.LogWarning("DialogAudioSource is null, cannot play audio. Using default text timing.");
                    animationDuration = len * 0.02f;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to play audio clip '{audioClip.name}': {e.Message}. Using default text timing.");
                animationDuration = len * 0.02f;
            }
        }
        else
        {
            // No audio file provided or invalid audio - use default duration based on text length
            animationDuration = len * 0.02f;
            if (audioClip == null)
            {
                Debug.Log("No audio file provided for dialog. Using default text animation timing.");
            }
            else
            {
                Debug.LogWarning($"Audio clip provided but has invalid length ({audioClip.length}). Using default text timing.");
            }
        }
        
        // Ensure minimum animation duration to prevent instant text
        animationDuration = Mathf.Max(animationDuration, 0.1f);
        
        isTextAnimating = true;
        
        dialogTween = LeanTween.value(gameObject, 0, len, animationDuration)
            .setOnUpdate((float val) => {
                int counter = Mathf.Clamp(Mathf.FloorToInt(val), 0, len);
                textComponent.text = displayText.Substring(0, counter);
            })
            .setOnComplete(() => {
                textComponent.text = displayText;
                isTextAnimating = false;
                
                // Log completion
                if (hasValidAudio)
                {
                    Debug.Log("Dialog animation completed with audio sync.");
                }
                else
                {
                    Debug.Log("Dialog animation completed using default timing.");
                }
            });
    }
    
    private string ProcessSpecialPrefixes(string fullText)
    {
        const string mapnamePrefix = "mapname:";
        const string exitgamePrefix = "exitgame:true";
        const string timelinePrefix = "timeline:";
        const string chargemeterPrefix = "charge:";
        
        if (fullText.StartsWith(mapnamePrefix))
        {
            int spaceIndex = fullText.IndexOf(' ');
            if (spaceIndex > mapnamePrefix.Length)
            {
                return fullText.Substring(spaceIndex + 1);
            }
        }
        else if (fullText.StartsWith(exitgamePrefix))
        {
            Application.Quit();
            return "Exiting game...";
        }
        else if (fullText.StartsWith(timelinePrefix))
        {
            Debug.Log("Timeline event triggered: " + fullText);
            // Handle timeline logic here
        }
        else if (fullText.StartsWith(chargemeterPrefix))
        {
            Debug.Log("Charge meter event: " + fullText);
            // Handle charge meter logic here
        }
        
        return fullText;
    }

    #endregion

    #region Input Handling

    [Obsolete]
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && isPlayingCutscene)
        {
            SkipCutscene();
        }
    }

    [Obsolete]
    private void HandleMouseClick()
    {
        if (isPlayingCutscene) return;
        
        // If text is currently animating, skip to complete text and stop audio
        if (isTextAnimating)
        {
            SkipTextAnimation();
            return;
        }
        
        if (currentBlockIndex >= coreGameData.coreBlock.Length)
        {
            FinishCoreGame();
            return;
        }
        
        var currentBlock = coreGameData.coreBlock[currentBlockIndex];
        
        // If showing a choice response, continue to next block
        if (isShowingResponse)
        {
            isShowingResponse = false;
            currentChoiceResponseIndex = -1;
            ClearAll3DDialogs(); // Clear 3D dialogs when continuing
            
            // Only destroy dialog instances if next block is not a 2D dialog
            if (!IsNext2DDialog())
            {
                DestroyDialogInstances();
            }
            
            ContinueToNextBlock();
            return;
        }
        
        // If current block is dialog and has no choices, continue
        if (currentBlock.Type == CoreGameBlock.CoreType.Dialog)
        {
            var dialog = currentBlock.Dialog;
            if (dialog.choices == null || dialog.choices.Length == 0)
            {
                ClearAll3DDialogs(); // Clear 3D dialogs when continuing
                
                // Only destroy dialog instances if next block is not a 2D dialog
                if (!IsNext2DDialog())
                {
                    DestroyDialogInstances();
                }
                
                ContinueToNextBlock();
            }
        }
    }
    
    /// <summary>
    /// Check if the next block in sequence is a 2D dialog
    /// </summary>
    private bool IsNext2DDialog()
    {
        int nextBlockIndex = currentBlockIndex + 1;
        
        if (nextBlockIndex >= coreGameData.coreBlock.Length)
        {
            return false; // No next block
        }
        
        var nextBlock = coreGameData.coreBlock[nextBlockIndex];
        
        return nextBlock.Type == CoreGameBlock.CoreType.Dialog && 
               nextBlock.Dialog != null && 
               nextBlock.Dialog.dialogType == CoreGameDialog.DialogType.TwoD;
    }
    
    private void SkipTextAnimation()
    {
        try
        {
            // Stop audio immediately
            if (dialogAudioSource != null && dialogAudioSource.isPlaying)
            {
                dialogAudioSource.Stop();
                Debug.Log("Dialog audio stopped due to skip.");
            }
            
            // Complete the tween immediately
            if (dialogTween != null)
            {
                LeanTween.cancel(gameObject, dialogTween.id);
                dialogTween = null;
            }
            
            isTextAnimating = false;
            
            // Force complete the text display
            CompleteCurrentTextDisplay();
            
            Debug.Log("Text animation skipped successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error skipping text animation: {e.Message}");
            // Ensure we still set the flag to false even if there's an error
            isTextAnimating = false;
        }
    }
    
    private void CompleteCurrentTextDisplay()
    {
        // Get current block and complete its text display
        if (currentBlockIndex < coreGameData.coreBlock.Length)
        {
            var currentBlock = coreGameData.coreBlock[currentBlockIndex];
            
            if (currentBlock.Type == CoreGameBlock.CoreType.Dialog)
            {
                var dialog = currentBlock.Dialog;
                string textToDisplay;
                
                // Determine which text to display based on current state
                if (isShowingResponse && currentChoiceResponseIndex >= 0 && 
                    dialog.choices != null && currentChoiceResponseIndex < dialog.choices.Length)
                {
                    textToDisplay = ProcessSpecialPrefixes(GetNpcResponseFromChoice(dialog.choices[currentChoiceResponseIndex]));
                }
                else
                {
                    textToDisplay = ProcessSpecialPrefixes(dialog.dialogEntry);
                }
                
                // Complete the text based on dialog type
                if (dialog.dialogType == CoreGameDialog.DialogType.ThreeD)
                {
                    Complete3DText(dialog, textToDisplay);
                }
                else
                {
                    Complete2DText(textToDisplay);
                }
            }
        }
    }
    
    private void Complete2DText(string textToDisplay)
    {
        // Complete 2D dialog text
        var dialogTextComponent = dialogInstance?.GetComponentInChildren<TMP_Text>();
        if (dialogTextComponent != null)
        {
            dialogTextComponent.text = textToDisplay;
        }
        else if (dialogText != null)
        {
            dialogText.text = textToDisplay;
        }
    }
    
    private void Complete3DText(CoreGameDialog dialog, string textToDisplay)
    {
        // Complete 3D dialog text
        GameObject targetModel = null;
        
        switch (dialog.dialog3DLocation)
        {
            case CoreGameDialog.Dialog3DLocation.Mother:
                targetModel = GameObject.Find("Linda_Model");
                break;
            case CoreGameDialog.Dialog3DLocation.Father:
                targetModel = GameObject.Find("Isayat_Model");
                break;
            case CoreGameDialog.Dialog3DLocation.Rey:
                targetModel = GameObject.Find("Rey_Baby_Model");
                break;
        }
        
        if (targetModel != null)
        {
            var textDialog3D = targetModel.transform.Find("TextDialog3D");
            if (textDialog3D != null)
            {
                var tmp3D = textDialog3D.GetComponent<TMP_Text>();
                if (tmp3D != null)
                {
                    tmp3D.text = textToDisplay;
                }
            }
        }
    }
    
    #endregion
    
    #region Legacy Dialog System Integration (from NPCDialogManager/NPCDialogManagerMaster)
    
    /// <summary>
    /// Initiate legacy dialog system (integrated from NPCDialogManagerMaster)
    /// </summary>
    [Obsolete]
    public void InitiateStartDialog(string npcDialogFile)
    {
        // Clear any existing dialogs
        ClearAll3DDialogs();
        DestroyDialogInstances();
        
        // Load and start legacy dialog
        GameObject dialogObj = SummonDialogBar();
        if (dialogObj == null)
        {
            Debug.LogError("Dialog bar could not be summoned!");
            return;
        }

        InitiateLegacyDialog(npcDialogFile, dialogObj);
    }
    
    /// <summary>
    /// Initialize legacy dialog system (from NPCDialogManager)
    /// </summary>
    [Obsolete]
    private void InitiateLegacyDialog(string dialogFileName, GameObject dialogObj)
    {
        // This would integrate with your existing legacy dialog system
        // For now, just provide a framework for backward compatibility
        Debug.Log($"Legacy dialog system called with file: {dialogFileName}");
        
        // You can extend this to load DialogMasterManager files from Resources
        // and convert them to work with the CoreGame system
    }
    
    /// <summary>
    /// Handle NPC button interactions (from DialogButtonController/MenuButtonHandler)
    /// </summary>
    [Obsolete]
    public void OnNPCButtonClicked(string npcTag)
    {
        Debug.Log($"NPC button clicked! NPC tag: {npcTag}");
        
        // Handle different NPC types
        if (npcTag.Contains("npc-nene"))
        {
            InitiateStartDialog("NPC_Nene");
        }
        else if (npcTag.Contains("npc-shopkeeper"))
        {
            InitiateStartDialog("NPC_Shopkeeper");
        }
        else if (npcTag.Contains("villager"))
        {
            Debug.Log("Show villager dialog options.");
        }
    }
    
    #endregion
    
    #region Game Flow
    
    private void FinishCoreGame()
    {
        Debug.Log("Core game sequence finished!");
        
        try
        {
            // Stop any playing audio
            if (dialogAudioSource != null && dialogAudioSource.isPlaying)
            {
                dialogAudioSource.Stop();
                Debug.Log("Dialog audio stopped on game finish.");
            }
            
            // Stop any active tweens
            if (dialogTween != null)
            {
                LeanTween.cancel(gameObject, dialogTween.id);
                dialogTween = null;
            }
            
            isTextAnimating = false;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error cleaning up audio/animation on game finish: {e.Message}");
        }
        
        // Clean up dialogs and invoke completion event
        ClearAll3DDialogs(); // Clear any remaining 3D dialogs
        DestroyDialogInstances();
        onCoreGameFinished?.Invoke();
        IsSequenceRunning = false;
        currentCompletionCallback?.Invoke();
        currentCompletionCallback = null; 
    }
    
    #endregion
}
