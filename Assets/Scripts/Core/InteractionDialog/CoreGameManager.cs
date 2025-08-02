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
/// - Keyboard input system for better user experience
///   * Space: Progress dialog and skip text animation
///   * Q, W, E: Select dialog choices (buttons show [Q], [W], [E] indicators)
///   * Escape: Skip cutscenes
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
/// - Choice buttons are automatically found by name in QuestionTemplate: Q, W, E
/// - Fallback: All choice buttons can also be assigned to answerButtons[] in inspector
/// - Dialog and question templates should be assigned to npcDialogThemplate and npcQuestionThemplate
/// - Assign npcNameText for displaying NPC names in 2D dialogs (3D dialogs ignore this)
/// - Assign backgroundFade image for cutscene fade transitions
/// - Use SetResponseIndex() to choose which response to use from dialogResponses array
/// - Use UseRandomResponse() for random NPC reactions
/// - Use SetResponseByCondition() for conditional responses based on game state
/// - Set cutsceneType in CoreGameDialog for fade transitions between dialogs
/// - Input Controls: Space to progress dialogs, Q/W/E to select choices, Escape to skip cutscenes
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
    public int stressvariable;
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
    private int currentChoiceResponseIndex = -1; // Which response in dialogResponses array is currently showing
    private int selectedChoiceIndex = -1; // Which choice was selected by the player
    private LTDescr dialogTween;
    private bool isShowingResponse = false;
    private bool isPlayingCutscene = false;
    private bool isTextAnimating = false;
    
    // Enhanced input throttling to prevent spam clicking and race conditions
    private float lastInputTime = 0f;
    private const float INPUT_COOLDOWN = 0.5f; // Increased cooldown to prevent spam
    private bool isProcessingInput = false; // Prevent multiple simultaneous input processing
    private bool isInDialogTransition = false; // Prevent input during dialog transitions
    private float lastDialogUpdateTime = 0f; // Track last dialog update time
    
    // Component safety tracking
    private Dictionary<string, TMP_Text> cachedTextComponents = new Dictionary<string, TMP_Text>();
    private bool componentsCached = false;
    
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

    #region Helper Methods for Dialog Text Assignment
    
    /// <summary>
    /// Helper method to update NPC name with enhanced error handling and spam protection
    /// </summary>
    public void UpdateNpcNameSafe(string npcName)
    {
        Debug.Log($"[SAFE-NAME] Updating NPC name to: '{npcName}'");
        
        if (string.IsNullOrEmpty(npcName))
        {
            Debug.LogWarning("[SAFE-NAME] NPC name is null or empty, skipping update");
            return;
        }
        
        if (dialogInstance == null)
        {
            Debug.LogWarning("[SAFE-NAME] Dialog instance is null, cannot update NPC name");
            return;
        }
        
        // Enhanced spam protection - prevent updates during processing or transitions
        if (isProcessingInput || isInDialogTransition)
        {
            Debug.Log("[SAFE-NAME] Currently processing input or in transition, deferring NPC name update");
            StartCoroutine(DeferredUpdateNpcName(npcName));
            return;
        }
        
        // Prevent rapid successive updates
        float currentTime = Time.time;
        if (currentTime - lastDialogUpdateTime < 0.1f)
        {
            Debug.LogWarning("[SAFE-NAME] Too rapid update attempt, skipping to prevent race condition");
            return;
        }
        lastDialogUpdateTime = currentTime;
        
        // Cache components on first use or if cache is invalid
        if (!componentsCached || !ValidateComponentCache())
        {
            CacheDialogComponents();
        }
        
        bool nameUpdated = false;
        
        // Method 1: Try cached DialogueName component first
        if (cachedTextComponents.ContainsKey("DialogueName"))
        {
            try
            {
                TMP_Text nameComponent = cachedTextComponents["DialogueName"];
                if (nameComponent != null && nameComponent.gameObject != null)
                {
                    // Double-check this is actually the name component and not text component
                    string componentPath = GetTransformPath(nameComponent.transform);
                    if (componentPath.ToLower().Contains("name") && !componentPath.ToLower().Contains("text"))
                    {
                        nameComponent.text = npcName;
                        Debug.Log($"[SAFE-NAME] ✓ Updated NPC name via cached DialogueName component: '{npcName}'");
                        nameUpdated = true;
                    }
                    else
                    {
                        Debug.LogError($"[SAFE-NAME] CRITICAL: Cached component path '{componentPath}' looks like a text component, not name!");
                        cachedTextComponents.Remove("DialogueName");
                        componentsCached = false;
                    }
                }
                else
                {
                    Debug.LogWarning("[SAFE-NAME] Cached DialogueName component is null or destroyed, clearing cache");
                    cachedTextComponents.Remove("DialogueName");
                    componentsCached = false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SAFE-NAME] Error updating cached DialogueName component: {e.Message}");
                cachedTextComponents.Remove("DialogueName");
                componentsCached = false;
            }
        }
        
        // Method 2: Try DialogPrefabController if caching failed
        if (!nameUpdated)
        {
            DialogPrefabController controller = dialogInstance.GetComponent<DialogPrefabController>();
            if (controller != null)
            {
                try
                {
                    controller.SetDialogName(npcName);
                    Debug.Log($"[SAFE-NAME] ✓ Updated NPC name via DialogPrefabController");
                    nameUpdated = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SAFE-NAME] Failed to update via DialogPrefabController: {e.Message}");
                }
            }
        }
        
        // Method 3: Direct search with enhanced validation
        if (!nameUpdated)
        {
            Transform nameTransform = dialogInstance.transform.Find("DialogueName");
            if (nameTransform != null)
            {
                TMP_Text nameText = nameTransform.GetComponent<TMP_Text>();
                if (nameText != null)
                {
                    // CRITICAL: Enhanced validation to ensure this is the name component
                    string transformName = nameTransform.name.ToLower();
                    string fullPath = GetTransformPath(nameTransform).ToLower();
                    
                    bool isNameComponent = (transformName.Contains("name") && !transformName.Contains("text") && !transformName.Contains("dialogue")) ||
                                          (fullPath.Contains("name") && !fullPath.Contains("text"));
                    
                    if (isNameComponent)
                    {
                        nameText.text = npcName;
                        Debug.Log($"[SAFE-NAME] ✓ Updated DialogueName directly: '{npcName}' -> '{nameTransform.name}'");
                        
                        // Cache this component for future use
                        cachedTextComponents["DialogueName"] = nameText;
                        nameUpdated = true;
                    }
                    else
                    {
                        Debug.LogError($"[SAFE-NAME] Component validation failed: '{nameTransform.name}' at path '{fullPath}' doesn't look like a name component");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SAFE-NAME] DialogueName transform found but no TMP_Text component");
                }
            }
            else
            {
                Debug.LogWarning($"[SAFE-NAME] DialogueName transform not found in dialog instance");
            }
        }
        
        if (!nameUpdated)
        {
            Debug.LogError($"[SAFE-NAME] ✗ CRITICAL FAILURE: Could not update NPC name: '{npcName}'");
            LogAllDialogComponents(); // Debug helper
        }
    }
    
    /// <summary>
    /// Deferred update for NPC name when processing is busy
    /// </summary>
    private IEnumerator DeferredUpdateNpcName(string npcName)
    {
        Debug.Log($"[SAFE-NAME] Deferring NPC name update: '{npcName}'");
        
        // Wait until processing is complete
        while (isProcessingInput || isInDialogTransition)
        {
            yield return new WaitForSeconds(0.05f);
        }
        
        // Wait a bit more for safety
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"[SAFE-NAME] Executing deferred NPC name update: '{npcName}'");
        UpdateNpcNameSafe(npcName);
    }
    
    /// <summary>
    /// Validate that cached components are still valid and point to correct objects
    /// </summary>
    private bool ValidateComponentCache()
    {
        if (!componentsCached || cachedTextComponents.Count == 0)
        {
            return false;
        }
        
        // Check if cached components are still valid
        var keysToRemove = new System.Collections.Generic.List<string>();
        
        foreach (var kvp in cachedTextComponents)
        {
            if (kvp.Value == null || kvp.Value.gameObject == null)
            {
                Debug.LogWarning($"[CACHE] Cached component '{kvp.Key}' is invalid, marking for removal");
                keysToRemove.Add(kvp.Key);
            }
        }
        
        // Remove invalid entries
        foreach (string key in keysToRemove)
        {
            cachedTextComponents.Remove(key);
        }
        
        if (keysToRemove.Count > 0)
        {
            Debug.LogWarning($"[CACHE] Removed {keysToRemove.Count} invalid cached components");
            componentsCached = false;
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Cache dialog components to prevent repeated searches and ensure correct component targeting
    /// </summary>
    private void CacheDialogComponents()
    {
        if (dialogInstance == null)
        {
            Debug.LogWarning("[CACHE] Dialog instance is null, cannot cache components");
            return;
        }
        
        Debug.Log("[CACHE] Caching dialog components...");
        cachedTextComponents.Clear();
        
        // Find and cache DialogueName component
        Transform nameTransform = dialogInstance.transform.Find("DialogueName");
        if (nameTransform != null)
        {
            TMP_Text nameText = nameTransform.GetComponent<TMP_Text>();
            if (nameText != null)
            {
                cachedTextComponents["DialogueName"] = nameText;
                Debug.Log($"[CACHE] ✓ Cached DialogueName component: {nameTransform.name}");
            }
        }
        
        // Find and cache DialogueText component
        Transform textTransform = dialogInstance.transform.Find("DialogueText");
        if (textTransform != null)
        {
            TMP_Text dialogText = textTransform.GetComponent<TMP_Text>();
            if (dialogText != null)
            {
                cachedTextComponents["DialogueText"] = dialogText;
                Debug.Log($"[CACHE] ✓ Cached DialogueText component: {textTransform.name}");
            }
        }
        
        componentsCached = true;
        Debug.Log($"[CACHE] Caching complete. {cachedTextComponents.Count} components cached.");
    }
    
    /// <summary>
    /// Clear cached components (call when dialog instance changes)
    /// </summary>
    private void ClearComponentCache()
    {
        cachedTextComponents.Clear();
        componentsCached = false;
        Debug.Log("[CACHE] Component cache cleared");
    }
    
    /// <summary>
    /// Debug helper to log all dialog components
    /// </summary>
    private void LogAllDialogComponents()
    {
        if (dialogInstance == null)
        {
            Debug.LogError("[DEBUG] Dialog instance is null, cannot log components");
            return;
        }
        
        Debug.Log("[DEBUG] === ALL DIALOG COMPONENTS ===");
        TMP_Text[] allTexts = dialogInstance.GetComponentsInChildren<TMP_Text>();
        Debug.Log($"[DEBUG] Found {allTexts.Length} TMP_Text components:");
        
        for (int i = 0; i < allTexts.Length; i++)
        {
            TMP_Text text = allTexts[i];
            string path = GetTransformPath(text.transform);
            Debug.Log($"[DEBUG]   [{i}] Path: {path}");
            Debug.Log($"[DEBUG]       Name: {text.transform.name}");
            Debug.Log($"[DEBUG]       Current Text: '{text.text}'");
            Debug.Log($"[DEBUG]       Parent: {text.transform.parent?.name ?? "None"}");
            Debug.Log($"[DEBUG]       ---");
        }
        
        Debug.Log("[DEBUG] === END DIALOG COMPONENTS ===");
    }
    
    /// <summary>
    /// Get the full path of a transform for debugging
    /// </summary>
    private string GetTransformPath(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;
        
        while (parent != null && parent != dialogInstance.transform)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    /// <summary>
    /// Helper method to update dialog text with enhanced error handling and spam protection
    /// </summary>
    public void UpdateDialogTextSafe(string dialogText)
    {
        Debug.Log($"[SAFE-TEXT] Updating dialog text to: '{dialogText}'");
        
        if (string.IsNullOrEmpty(dialogText))
        {
            Debug.LogWarning("[SAFE-TEXT] Dialog text is null or empty, skipping update");
            return;
        }
        
        if (dialogInstance == null)
        {
            Debug.LogWarning("[SAFE-TEXT] Dialog instance is null, cannot update dialog text");
            return;
        }
        
        // Enhanced spam protection - prevent updates during processing or transitions
        if (isProcessingInput || isInDialogTransition)
        {
            Debug.Log("[SAFE-TEXT] Currently processing input or in transition, deferring dialog text update");
            StartCoroutine(DeferredUpdateDialogText(dialogText));
            return;
        }
        
        // Prevent rapid successive updates
        float currentTime = Time.time;
        if (currentTime - lastDialogUpdateTime < 0.1f)
        {
            Debug.LogWarning("[SAFE-TEXT] Too rapid update attempt, skipping to prevent race condition");
            return;
        }
        lastDialogUpdateTime = currentTime;
        
        // Cache components on first use or if cache is invalid
        if (!componentsCached || !ValidateComponentCache())
        {
            CacheDialogComponents();
        }
        
        bool textUpdated = false;
        
        // Method 1: Try cached DialogueText component first
        if (cachedTextComponents.ContainsKey("DialogueText"))
        {
            try
            {
                TMP_Text textComponent = cachedTextComponents["DialogueText"];
                if (textComponent != null && textComponent.gameObject != null)
                {
                    // Double-check this is actually the dialog text component and not name component
                    string componentPath = GetTransformPath(textComponent.transform);
                    if ((componentPath.ToLower().Contains("text") || componentPath.ToLower().Contains("dialogue")) && !componentPath.ToLower().Contains("name"))
                    {
                        textComponent.text = dialogText;
                        Debug.Log($"[SAFE-TEXT] ✓ Updated dialog text via cached DialogueText component");
                        textUpdated = true;
                    }
                    else
                    {
                        Debug.LogError($"[SAFE-TEXT] CRITICAL: Cached component path '{componentPath}' looks like a name component, not text!");
                        cachedTextComponents.Remove("DialogueText");
                        componentsCached = false;
                    }
                }
                else
                {
                    Debug.LogWarning("[SAFE-TEXT] Cached DialogueText component is null or destroyed, clearing cache");
                    cachedTextComponents.Remove("DialogueText");
                    componentsCached = false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SAFE-TEXT] Error updating cached DialogueText component: {e.Message}");
                cachedTextComponents.Remove("DialogueText");
                componentsCached = false;
            }
        }
        
        // Method 2: Try DialogPrefabController if caching failed
        if (!textUpdated)
        {
            DialogPrefabController controller = dialogInstance.GetComponent<DialogPrefabController>();
            if (controller != null)
            {
                try
                {
                    controller.SetDialogText(dialogText);
                    Debug.Log($"[SAFE-TEXT] ✓ Updated dialog text via DialogPrefabController");
                    textUpdated = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[SAFE-TEXT] Failed to update via DialogPrefabController: {e.Message}");
                }
            }
        }
        
        // Method 3: Direct search with enhanced validation
        if (!textUpdated)
        {
            Transform textTransform = dialogInstance.transform.Find("DialogueText");
            if (textTransform != null)
            {
                TMP_Text textComponent = textTransform.GetComponent<TMP_Text>();
                if (textComponent != null)
                {
                    // CRITICAL: Enhanced validation to ensure this is the dialog text component
                    string transformName = textTransform.name.ToLower();
                    string fullPath = GetTransformPath(textTransform).ToLower();
                    
                    bool isTextComponent = ((transformName.Contains("text") || transformName.Contains("dialogue")) && !transformName.Contains("name")) ||
                                          (fullPath.Contains("text") && !fullPath.Contains("name"));
                    
                    if (isTextComponent)
                    {
                        textComponent.text = dialogText;
                        Debug.Log($"[SAFE-TEXT] ✓ Updated DialogueText directly: '{textTransform.name}'");
                        
                        // Cache this component for future use
                        cachedTextComponents["DialogueText"] = textComponent;
                        textUpdated = true;
                    }
                    else
                    {
                        Debug.LogError($"[SAFE-TEXT] Component validation failed: '{textTransform.name}' at path '{fullPath}' doesn't look like a dialog text component");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SAFE-TEXT] DialogueText transform found but no TMP_Text component");
                }
            }
            else
            {
                Debug.LogWarning($"[SAFE-TEXT] DialogueText transform not found in dialog instance");
            }
        }
        
        if (!textUpdated)
        {
            Debug.LogError($"[SAFE-TEXT] ✗ CRITICAL FAILURE: Could not update dialog text");
            LogAllDialogComponents(); // Debug helper
        }
    }
    
    /// <summary>
    /// Deferred update for dialog text when processing is busy
    /// </summary>
    private IEnumerator DeferredUpdateDialogText(string dialogText)
    {
        Debug.Log($"[SAFE-TEXT] Deferring dialog text update");
        
        // Wait until processing is complete
        while (isProcessingInput || isInDialogTransition)
        {
            yield return new WaitForSeconds(0.05f);
        }
        
        // Wait a bit more for safety
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"[SAFE-TEXT] Executing deferred dialog text update");
        UpdateDialogTextSafe(dialogText);
    }
    
    /// <summary>
    /// Helper method to update button text with proper error handling
    /// </summary>
    public void UpdateButtonTextSafe(string buttonName, string buttonText)
    {
        Debug.Log($"[SAFE] Updating button '{buttonName}' text to: '{buttonText}'");
        
        if (questionInstance == null)
        {
            Debug.LogWarning("[SAFE] Question instance is null!");
            return;
        }

        bool updated = false;
        
        // Method 1: Try DialogPrefabController first
        DialogPrefabController controller = questionInstance.GetComponent<DialogPrefabController>();
        if (controller != null)
        {
            Debug.Log($"[SAFE] Found DialogPrefabController, attempting to set button text...");
            controller.SetButtonText(buttonName, buttonText);
            Debug.Log($"[SAFE] ✓ Updated button via DialogPrefabController");
            updated = true;
        }
        
        // Method 2: Direct search for button (always try this as backup verification)
        Transform buttonTransform = questionInstance.transform.Find(buttonName);
        if (buttonTransform != null)
        {
            Button button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                TMP_Text btnText = button.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    Debug.Log($"[SAFE] Found button '{buttonName}' TMP_Text component: '{btnText.transform.name}' (current text: '{btnText.text}')");
                    btnText.text = buttonText;
                    Debug.Log($"[SAFE] ✓ Updated button '{buttonName}' directly to: '{btnText.text}'");
                    updated = true;
                }
                else
                {
                    Debug.LogWarning($"[SAFE] Button '{buttonName}' has no TMP_Text component!");
                    
                    // Debug button structure
                    Transform[] children = button.GetComponentsInChildren<Transform>();
                    Debug.Log($"[SAFE] Button '{buttonName}' children:");
                    for (int i = 0; i < children.Length; i++)
                    {
                        Component[] components = children[i].GetComponents<Component>();
                        string componentNames = "";
                        for (int j = 0; j < components.Length; j++)
                        {
                            componentNames += components[j].GetType().Name;
                            if (j < components.Length - 1) componentNames += ", ";
                        }
                        Debug.Log($"[SAFE]   - {children[i].name} (Components: {componentNames})");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[SAFE] Found transform '{buttonName}' but no Button component!");
            }
        }
        else
        {
            Debug.LogWarning($"[SAFE] Button transform '{buttonName}' not found!");
            
            // Debug all children in question instance
            Debug.Log($"[SAFE] Question instance children:");
            Transform[] allChildren = questionInstance.GetComponentsInChildren<Transform>();
            for (int i = 0; i < allChildren.Length; i++)
            {
                Debug.Log($"[SAFE]   - {allChildren[i].name}");
            }
        }
        
        if (!updated)
        {
            Debug.LogError($"[SAFE] ✗ FAILED to update button '{buttonName}' text: {buttonText}");
        }
    }
    
    /// <summary>
    /// Test method to manually verify dialog text assignments
    /// Call this from Unity Inspector or console to test your setup
    /// </summary>
    [ContextMenu("Test Dialog Text Assignment")]
    public void TestDialogTextAssignment()
    {
        Debug.Log("=== TESTING DIALOG TEXT ASSIGNMENT ===");
        
        // Test NPC name assignment
        UpdateNpcNameSafe("Test NPC Name");
        
        // Test dialog text assignment
        UpdateDialogTextSafe("This is a test dialog message");
        
        // Test button text assignment
        UpdateButtonTextSafe("Q", "[Q] Test Choice 1");
        UpdateButtonTextSafe("W", "[W] Test Choice 2");
        UpdateButtonTextSafe("E", "[E] Test Choice 3");
        
        Debug.Log("=== END DIALOG TEXT ASSIGNMENT TEST ===");
    }
    
    /// <summary>
    /// Debug method to inspect current dialog/choice data
    /// </summary>
    [ContextMenu("Debug Current Dialog Data")]
    public void DebugCurrentDialogData()
    {
        Debug.Log("=== DEBUGGING CURRENT DIALOG DATA ===");
        
        if (coreGameData == null)
        {
            Debug.LogError("CoreGameData is null!");
            return;
        }
        
        if (coreGameData.coreBlock == null || coreGameData.coreBlock.Length == 0)
        {
            Debug.LogError("CoreGameData has no blocks!");
            return;
        }
        
        Debug.Log($"CoreGameData has {coreGameData.coreBlock.Length} blocks");
        Debug.Log($"Current block index: {currentBlockIndex}");
        
        if (currentBlockIndex < coreGameData.coreBlock.Length)
        {
            var currentBlock = coreGameData.coreBlock[currentBlockIndex];
            if (currentBlock.Dialog != null)
            {
                Debug.Log($"Current dialog:");
                Debug.Log($"  - npcName: '{currentBlock.Dialog.npcName}'");
                Debug.Log($"  - dialogEntry: '{currentBlock.Dialog.dialogEntry}'");
                
                if (currentBlock.Dialog.choices != null)
                {
                    Debug.Log($"  - Has {currentBlock.Dialog.choices.Length} choices:");
                    for (int i = 0; i < currentBlock.Dialog.choices.Length; i++)
                    {
                        var choice = currentBlock.Dialog.choices[i];
                        if (choice != null)
                        {
                            Debug.Log($"    Choice {i}: '{choice.playerChoice}'");
                        }
                        else
                        {
                            Debug.LogWarning($"    Choice {i}: NULL");
                        }
                    }
                }
                else
                {
                    Debug.Log("  - No choices");
                }
            }
            else
            {
                Debug.Log("Current block has no dialog");
            }
        }
        
        Debug.Log("=== END DIALOG DATA DEBUG ===");
    }
    
    /// <summary>
    /// Test button array access and modification
    /// </summary>
    [ContextMenu("Test Button Array")]
    public void TestButtonArray()
    {
        Debug.Log("=== TESTING BUTTON ARRAY ACCESS ===");
        
        if (questionInstance == null)
        {
            Debug.LogError("Question instance is null! Cannot test buttons.");
            return;
        }
        
        Button[] buttonArray = questionInstance.GetComponentsInChildren<Button>();
        Debug.Log($"Found {buttonArray.Length} buttons in question instance");
        
        for (int i = 0; i < buttonArray.Length; i++)
        {
            Button btn = buttonArray[i];
            if (btn != null)
            {
                Debug.Log($"Button {i}: '{btn.name}' (GameObject: {btn.gameObject.name})");
                
                TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    string testText = $"[TEST{i}] Button {i} Array Test";
                    Debug.Log($"Setting button {i} text to: '{testText}'");
                    btnText.text = testText;
                    Debug.Log($"Button {i} text is now: '{btnText.text}'");
                }
                else
                {
                    Debug.LogError($"CRITICAL: Button {i} ({btn.name}) has no TMP_Text component!");
                }
            }
            else
            {
                Debug.LogWarning($"Button {i} in array is null!");
            }
        }
        
        Debug.Log("=== END BUTTON ARRAY TEST ===");
    }
    
    /// <summary>
    /// Test keyboard input functionality - simulates Q, W, E key presses
    /// </summary>
    [ContextMenu("Test Keyboard Input")]
    public void TestKeyboardInput()
    {
        Debug.Log("=== TESTING KEYBOARD INPUT ===");
        
        if (onChoiceSelected == null)
        {
            Debug.LogWarning("No choices are currently active. Please show choices first.");
            return;
        }
        
        Debug.Log("Testing keyboard input simulation...");
        
        // Test Q key (choice 0)
        Debug.Log("Simulating Q key press (choice 0):");
        SelectChoice(0);
        
        // Wait a moment, then test W key (choice 1) - you can uncomment these for manual testing
        // Debug.Log("Simulating W key press (choice 1):");
        // SelectChoice(1);
        
        // Debug.Log("Simulating E key press (choice 2):");
        // SelectChoice(2);
        
        Debug.Log("=== END KEYBOARD INPUT TEST ===");
    }
    
    /// <summary>
    /// Test method to simulate dialog responses with multiple entries
    /// </summary>
    [ContextMenu("Test Multiple Dialog Responses")]
    public void TestMultipleDialogResponses()
    {
        Debug.Log("=== TESTING MULTIPLE DIALOG RESPONSES ===");
        
        if (coreGameData == null)
        {
            Debug.LogError("CoreGameData is null! Cannot test responses.");
            return;
        }
        
        Debug.Log($"Current dialog response state:");
        Debug.Log($"  - isShowingResponse: {isShowingResponse}");
        Debug.Log($"  - selectedChoiceIndex: {selectedChoiceIndex}");
        Debug.Log($"  - currentChoiceResponseIndex: {currentChoiceResponseIndex}");
        
        if (isShowingResponse && selectedChoiceIndex >= 0)
        {
            var currentBlock = coreGameData.coreBlock[currentBlockIndex];
            if (currentBlock.Dialog?.choices != null && selectedChoiceIndex < currentBlock.Dialog.choices.Length)
            {
                var selectedChoice = currentBlock.Dialog.choices[selectedChoiceIndex];
                if (selectedChoice.dialogResponses != null)
                {
                    Debug.Log($"Selected choice '{selectedChoice.playerChoice}' has {selectedChoice.dialogResponses.Length} responses:");
                    for (int i = 0; i < selectedChoice.dialogResponses.Length; i++)
                    {
                        var response = selectedChoice.dialogResponses[i];
                        string indicator = (i == currentChoiceResponseIndex) ? " <- CURRENT" : "";
                        Debug.Log($"  Response {i}: '{response.NpcName}' says '{response.npcResponse}'{indicator}");
                    }
                    
                    Debug.Log("Press SPACE to advance to next response or continue to next block.");
                }
                else
                {
                    Debug.Log("Selected choice has no dialog responses.");
                }
            }
        }
        else
        {
            Debug.Log("Not currently showing responses. Select a choice first.");
        }
        
        Debug.Log("=== END MULTIPLE DIALOG RESPONSES TEST ===");
    }
    
    /// <summary>
    /// Debug current dialog progression state and attempt to recover from stuck states
    /// </summary>
    [ContextMenu("Debug Dialog Progression State")]
    public void DebugDialogProgressionState()
    {
        Debug.Log("=== DIALOG PROGRESSION STATE DEBUG ===");
        
        Debug.Log($"Dialog Manager State:");
        Debug.Log($"  - isPlayingCutscene: {isPlayingCutscene}");
        Debug.Log($"  - isTextAnimating: {isTextAnimating}");
        Debug.Log($"  - isShowingResponse: {isShowingResponse}");
        Debug.Log($"  - IsSequenceRunning: {IsSequenceRunning}");
        
        Debug.Log($"Block Information:");
        Debug.Log($"  - currentBlockIndex: {currentBlockIndex}");
        Debug.Log($"  - Total blocks: {(coreGameData?.coreBlock?.Length ?? 0)}");
        
        Debug.Log($"Choice State:");
        Debug.Log($"  - selectedChoiceIndex: {selectedChoiceIndex}");
        Debug.Log($"  - currentChoiceResponseIndex: {currentChoiceResponseIndex}");
        Debug.Log($"  - onChoiceSelected != null: {onChoiceSelected != null}");
        
        Debug.Log($"UI State:");
        Debug.Log($"  - dialogInstance != null: {dialogInstance != null}");
        Debug.Log($"  - questionInstance != null: {questionInstance != null}");
        
        if (coreGameData != null && currentBlockIndex < coreGameData.coreBlock.Length)
        {
            var currentBlock = coreGameData.coreBlock[currentBlockIndex];
            Debug.Log($"Current Block:");
            Debug.Log($"  - Type: {currentBlock.Type}");
            
            if (currentBlock.Dialog != null)
            {
                Debug.Log($"  - Dialog Type: {currentBlock.Dialog.dialogType}");
                Debug.Log($"  - Has Choices: {currentBlock.Dialog.choices != null && currentBlock.Dialog.choices.Length > 0}");
                if (currentBlock.Dialog.choices != null)
                {
                    Debug.Log($"  - Choice Count: {currentBlock.Dialog.choices.Length}");
                }
            }
        }
        
        Debug.Log("=== ATTEMPTING PROGRESSION ===");
        Debug.Log("Calling HandleDialogProgression() to see current behavior...");
        HandleDialogProgression();
        
        Debug.Log("=== END DIALOG PROGRESSION STATE DEBUG ===");
    }
    
    /// <summary>
    /// Force reset dialog state - use this if dialog gets stuck
    /// </summary>
    [ContextMenu("Force Reset Dialog State")]
    public void ForceResetDialogState()
    {
        Debug.Log("=== FORCE RESETTING DIALOG STATE ===");
        
        // Reset all dialog states
        isShowingResponse = false;
        isTextAnimating = false;
        isPlayingCutscene = false;
        selectedChoiceIndex = -1;
        currentChoiceResponseIndex = -1;
        onChoiceSelected = null;
        
        // Reset input throttling
        isProcessingInput = false;
        isInDialogTransition = false;
        lastInputTime = 0f;
        lastDialogUpdateTime = 0f;
        
        // Clear component cache
        ClearComponentCache();
        
        // Clear button states
        buttonTweenIds.Clear();
        
        // Stop any audio
        if (dialogAudioSource != null && dialogAudioSource.isPlaying)
        {
            dialogAudioSource.Stop();
        }
        
        // Cancel any active tweens
        if (dialogTween != null)
        {
            LeanTween.cancel(gameObject, dialogTween.id);
            dialogTween = null;
        }
        
        // Clear 3D dialogs
        ClearAll3DDialogs();
        
        // Hide any active choices
        HideChoices();
        
        Debug.Log("Dialog state has been reset. Try pressing SPACE or selecting a choice again.");
        Debug.Log("If still stuck, try 'Debug Dialog Progression State' to see what's happening.");
        
        Debug.Log("=== DIALOG STATE RESET COMPLETE ===");
    }
    
    /// <summary>
    /// Comprehensive dialog progression fix - call this if dialog gets stuck
    /// </summary>
    [ContextMenu("Fix Dialog Progression")]
    public void FixDialogProgression()
    {
        Debug.Log("=== ATTEMPTING TO FIX DIALOG PROGRESSION ===");
        
        if (coreGameData == null || coreGameData.coreBlock == null)
        {
            Debug.LogError("CoreGameData is null or has no blocks!");
            return;
        }
        
        if (currentBlockIndex >= coreGameData.coreBlock.Length)
        {
            Debug.Log("Already at end of game");
            FinishCoreGame();
            return;
        }
        
        var currentBlock = coreGameData.coreBlock[currentBlockIndex];
        Debug.Log($"Current block {currentBlockIndex}: Type={currentBlock.Type}");
        
        if (currentBlock.Type == CoreGameBlock.CoreType.Dialog && currentBlock.Dialog != null)
        {
            bool hasChoices = currentBlock.Dialog.choices != null && currentBlock.Dialog.choices.Length > 0;
            Debug.Log($"Dialog block has choices: {hasChoices}");
            
            if (!hasChoices)
            {
                Debug.Log("No choices - this dialog should auto-advance. Forcing continuation...");
                isShowingResponse = false;
                isTextAnimating = false;
                currentChoiceResponseIndex = -1;
                selectedChoiceIndex = -1;
                onChoiceSelected = null;
                
                // Force advance to next block
                ContinueToNextBlock();
            }
            else
            {
                Debug.Log($"Dialog has {currentBlock.Dialog.choices.Length} choices - waiting for user selection");
                
                // Check if we're stuck in response mode
                if (isShowingResponse)
                {
                    Debug.Log("Currently showing response - this might be the problem");
                    if (selectedChoiceIndex >= 0 && selectedChoiceIndex < currentBlock.Dialog.choices.Length)
                    {
                        var selectedChoice = currentBlock.Dialog.choices[selectedChoiceIndex];
                        if (selectedChoice.dialogResponses == null || selectedChoice.dialogResponses.Length == 0)
                        {
                            Debug.Log("Selected choice has no responses - forcing next block");
                            isShowingResponse = false;
                            currentChoiceResponseIndex = -1;
                            selectedChoiceIndex = -1;
                            ContinueToNextBlock();
                        }
                        else
                        {
                            Debug.Log($"Selected choice has {selectedChoice.dialogResponses.Length} responses, currentResponseIndex={currentChoiceResponseIndex}");
                            if (currentChoiceResponseIndex >= selectedChoice.dialogResponses.Length - 1)
                            {
                                Debug.Log("All responses shown - forcing next block");
                                isShowingResponse = false;
                                currentChoiceResponseIndex = -1;
                                selectedChoiceIndex = -1;
                                ContinueToNextBlock();
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"Current block is not a dialog or has no dialog data - forcing next block");
            ContinueToNextBlock();
        }
        
        Debug.Log("=== DIALOG PROGRESSION FIX COMPLETE ===");
    }
    
    /// <summary>
    /// Test text animation - call this to verify animation is working
    /// </summary>
    [ContextMenu("Test Text Animation")]
    public void TestTextAnimation()
    {
        Debug.Log("=== TESTING TEXT ANIMATION ===");
        
        if (dialogInstance == null)
        {
            Debug.LogError("No dialog instance found! Please create a dialog first.");
            return;
        }
        
        // Find the text component to animate
        TMP_Text dialogTextComponent = null;
        DialogPrefabController controller = dialogInstance.GetComponent<DialogPrefabController>();
        if (controller != null && controller.dialogueText != null)
        {
            dialogTextComponent = controller.dialogueText;
            Debug.Log("Found DialogueText component via DialogPrefabController for test animation");
        }
        else
        {
            Transform textTransform = dialogInstance.transform.Find("DialogueText");
            if (textTransform != null)
            {
                dialogTextComponent = textTransform.GetComponent<TMP_Text>();
                Debug.Log("Found DialogueText component via direct search for test animation");
            }
        }
        
        if (dialogTextComponent != null)
        {
            string testText = "This is a test of the typewriter text animation system. It should appear character by character with proper timing.";
            Debug.Log($"Starting test animation with text: '{testText}'");
            AnimateDialogText(testText, dialogTextComponent, null);
        }
        else
        {
            Debug.LogError("Could not find DialogueText component for animation test!");
        }
        
        Debug.Log("=== END TEXT ANIMATION TEST ===");
    }
    
    #endregion
    
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
        Debug.Log($"UpdateNpcNameDisplay called with: '{npcName}'");
        UpdateNpcNameSafe(npcName);
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

    #region Stress Modifier System
    
    /// <summary>
    /// Process all modifiers in dialog text and apply them
    /// Supported modifiers:
    /// - {+100stress}, {-50stress} - for stress changes
    /// - {animation:animationName} - for triggering animations
    /// - {scene:sceneName} - for changing scenes
    /// Returns the cleaned text without the modifiers
    /// </summary>
    /// <param name="dialogText">Original dialog text with potential modifiers</param>
    /// <returns>Clean dialog text without modifiers</returns>
    private string ProcessStressModifiers(string dialogText)
    {
        if (string.IsNullOrEmpty(dialogText))
        {
            return dialogText;
        }
        
        string processedText = dialogText;
        
        // Process stress modifiers: {+100stress}, {-50stress}
        processedText = ProcessStressModifiersInternal(processedText);
        
        // Process animation modifiers: {animation:animationName}
        processedText = ProcessAnimationModifiers(processedText);
        
        // Process scene modifiers: {scene:sceneName}
        processedText = ProcessSceneModifiers(processedText);
        
        // Clean up any double spaces that might result from removing modifiers
        processedText = System.Text.RegularExpressions.Regex.Replace(processedText, @"\s+", " ").Trim();
        
        return processedText;
    }
    
    /// <summary>
    /// Process stress modifiers in dialog text and apply them to stressvariable
    /// Modifiers format: {+100stress}, {-50stress}, etc.
    /// </summary>
    /// <param name="dialogText">Text containing stress modifiers</param>
    /// <returns>Text with stress modifiers removed</returns>
    private string ProcessStressModifiersInternal(string dialogText)
    {
        string processedText = dialogText;
        
        // Regex pattern to find stress modifiers: {+number stress} or {-number stress}
        System.Text.RegularExpressions.Regex stressPattern = 
            new System.Text.RegularExpressions.Regex(@"\{([+-]?\d+)stress\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        System.Text.RegularExpressions.MatchCollection matches = stressPattern.Matches(dialogText);
        
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (match.Success && match.Groups.Count > 1)
            {
                string valueStr = match.Groups[1].Value;
                if (int.TryParse(valueStr, out int stressValue))
                {
                    // Apply stress modifier to stressvariable
                    stressvariable += stressValue;
                    
                    Debug.Log($"[STRESS] Applied stress modifier: {stressValue} (Total stress: {stressvariable})");
                }
                else
                {
                    Debug.LogWarning($"[STRESS] Failed to parse stress value: '{valueStr}' from modifier: '{match.Value}'");
                }
                
                // Remove the modifier from the text
                processedText = processedText.Replace(match.Value, "");
            }
        }
        
        return processedText;
    }
    
    /// <summary>
    /// Process animation modifiers in dialog text and trigger animations
    /// Modifiers format: {animation:animationName}
    /// </summary>
    /// <param name="dialogText">Text containing animation modifiers</param>
    /// <returns>Text with animation modifiers removed</returns>
    private string ProcessAnimationModifiers(string dialogText)
    {
        string processedText = dialogText;
        
        // Regex pattern to find animation modifiers: {animation:animationName}
        System.Text.RegularExpressions.Regex animationPattern = 
            new System.Text.RegularExpressions.Regex(@"\{animation:([^}]+)\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        System.Text.RegularExpressions.MatchCollection matches = animationPattern.Matches(dialogText);
        
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (match.Success && match.Groups.Count > 1)
            {
                string animationName = match.Groups[1].Value.Trim();
                
                if (!string.IsNullOrEmpty(animationName))
                {
                    Debug.Log($"[ANIMATION] Triggering animation: '{animationName}'");
                    TriggerAnimation(animationName);
                }
                else
                {
                    Debug.LogWarning($"[ANIMATION] Empty animation name in modifier: '{match.Value}'");
                }
                
                // Remove the modifier from the text
                processedText = processedText.Replace(match.Value, "");
            }
        }
        
        return processedText;
    }
    
    /// <summary>
    /// Process scene modifiers in dialog text and trigger scene changes
    /// Modifiers format: {scene:sceneName}
    /// </summary>
    /// <param name="dialogText">Text containing scene modifiers</param>
    /// <returns>Text with scene modifiers removed</returns>
    private string ProcessSceneModifiers(string dialogText)
    {
        string processedText = dialogText;
        
        // Regex pattern to find scene modifiers: {scene:sceneName}
        System.Text.RegularExpressions.Regex scenePattern = 
            new System.Text.RegularExpressions.Regex(@"\{scene:([^}]+)\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        System.Text.RegularExpressions.MatchCollection matches = scenePattern.Matches(dialogText);
        
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (match.Success && match.Groups.Count > 1)
            {
                string sceneName = match.Groups[1].Value.Trim();
                
                if (!string.IsNullOrEmpty(sceneName))
                {
                    Debug.Log($"[SCENE] Triggering scene change to: '{sceneName}'");
                    TriggerSceneChange(sceneName);
                }
                else
                {
                    Debug.LogWarning($"[SCENE] Empty scene name in modifier: '{match.Value}'");
                }
                
                // Remove the modifier from the text
                processedText = processedText.Replace(match.Value, "");
            }
        }
        
        return processedText;
    }
    
    /// <summary>
    /// Trigger an animation by name
    /// Override this method to implement your specific animation logic
    /// </summary>
    /// <param name="animationName">Name of the animation to trigger</param>
    protected virtual void TriggerAnimation(string animationName)
    {
        Debug.Log($"[ANIMATION] TriggerAnimation called with: '{animationName}'");
        
        // Trigger event for external subscribers
        OnAnimationTriggered?.Invoke(animationName);
        
        // Example implementation - you can customize this based on your animation system
        try
        {
            // Method 1: Try to find an Animator component on this GameObject
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(animationName);
                Debug.Log($"[ANIMATION] ✓ Triggered animation '{animationName}' on local Animator");
                return;
            }
            
            // Method 2: Look for global animation manager or specific objects
            GameObject animationTarget = GameObject.Find("AnimationManager");
            if (animationTarget != null)
            {
                Animator targetAnimator = animationTarget.GetComponent<Animator>();
                if (targetAnimator != null)
                {
                    targetAnimator.SetTrigger(animationName);
                    Debug.Log($"[ANIMATION] ✓ Triggered animation '{animationName}' on AnimationManager");
                    return;
                }
            }
            
            // Method 3: Custom animation handling - you can add your own logic here
            HandleCustomAnimation(animationName);
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ANIMATION] Error triggering animation '{animationName}': {e.Message}");
        }
    }
    
    /// <summary>
    /// Handle custom animations that don't use Unity's Animator system
    /// Override this method for custom animation implementations
    /// </summary>
    /// <param name="animationName">Name of the custom animation</param>
    protected virtual void HandleCustomAnimation(string animationName)
    {
        Debug.Log($"[ANIMATION] HandleCustomAnimation called with: '{animationName}'");
        
        // Example custom animations
        switch (animationName.ToLower())
        {
            case "shake":
                StartCoroutine(ShakeAnimation());
                break;
                
            case "fade":
                StartCoroutine(FadeAnimation());
                break;
                
            case "bounce":
                StartCoroutine(BounceAnimation());
                break;
                
            default:
                Debug.LogWarning($"[ANIMATION] Unknown custom animation: '{animationName}'");
                break;
        }
    }
    
    /// <summary>
    /// Trigger a scene change by name
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    protected virtual void TriggerSceneChange(string sceneName)
    {
        Debug.Log($"[SCENE] TriggerSceneChange called with: '{sceneName}'");
        
        // Trigger event for external subscribers
        OnSceneChangeTriggered?.Invoke(sceneName);
        
        try
        {
            // Check if the scene exists in build settings
            if (UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(sceneName) >= 0)
            {
                Debug.Log($"[SCENE] ✓ Loading scene '{sceneName}' from build settings");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            else
            {
                // Try loading by exact name
                Debug.Log($"[SCENE] Attempting to load scene '{sceneName}' by name");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SCENE] Error loading scene '{sceneName}': {e.Message}");
            
            // Fallback: try async loading
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }
    
    /// <summary>
    /// Load scene asynchronously as a fallback
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log($"[SCENE] Attempting async load of scene: '{sceneName}'");
        
        var asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        
        if (asyncLoad != null)
        {
            while (!asyncLoad.isDone)
            {
                Debug.Log($"[SCENE] Loading scene '{sceneName}': {asyncLoad.progress * 100f:F1}%");
                yield return null;
            }
            
            Debug.Log($"[SCENE] ✓ Scene '{sceneName}' loaded successfully");
        }
        else
        {
            Debug.LogError($"[SCENE] ✗ Failed to start async load for scene '{sceneName}'");
        }
    }
    
    #region Example Custom Animations
    
    /// <summary>
    /// Example shake animation
    /// </summary>
    private IEnumerator ShakeAnimation()
    {
        Debug.Log("[ANIMATION] Starting shake animation");
        
        Vector3 originalPosition = transform.position;
        float duration = 0.5f;
        float magnitude = 0.1f;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            
            transform.position = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
        Debug.Log("[ANIMATION] Shake animation complete");
    }
    
    /// <summary>
    /// Example fade animation
    /// </summary>
    private IEnumerator FadeAnimation()
    {
        Debug.Log("[ANIMATION] Starting fade animation");
        
        if (backgroundFade != null)
        {
            backgroundFade.gameObject.SetActive(true);
            
            // Fade in
            LeanTween.value(backgroundFade.gameObject, 0f, 1f, 0.5f)
                .setOnUpdate((float alpha) =>
                {
                    Color color = backgroundFade.color;
                    color.a = alpha;
                    backgroundFade.color = color;
                });
            
            yield return new WaitForSeconds(0.5f);
            
            // Fade out
            LeanTween.value(backgroundFade.gameObject, 1f, 0f, 0.5f)
                .setOnUpdate((float alpha) =>
                {
                    Color color = backgroundFade.color;
                    color.a = alpha;
                    backgroundFade.color = color;
                })
                .setOnComplete(() =>
                {
                    backgroundFade.gameObject.SetActive(false);
                });
        }
        
        Debug.Log("[ANIMATION] Fade animation complete");
    }
    
    /// <summary>
    /// Example bounce animation
    /// </summary>
    private IEnumerator BounceAnimation()
    {
        Debug.Log("[ANIMATION] Starting bounce animation");
        
        Vector3 originalScale = transform.localScale;
        
        // Scale up
        LeanTween.scale(gameObject, originalScale * 1.2f, 0.2f)
            .setEase(LeanTweenType.easeOutBack);
        
        yield return new WaitForSeconds(0.2f);
        
        // Scale back to normal
        LeanTween.scale(gameObject, originalScale, 0.2f)
            .setEase(LeanTweenType.easeInBack);
        
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log("[ANIMATION] Bounce animation complete");
    }
    
    #endregion
    
    /// <summary>
    /// Get current stress value (for debugging or UI display)
    /// </summary>
    public int GetCurrentStress()
    {
        return stressvariable;
    }
    
    /// <summary>
    /// Set stress to a specific value (for debugging or save/load systems)
    /// </summary>
    public void SetStress(int value)
    {
        stressvariable = value;
        Debug.Log($"Stress set to: {stressvariable}");
    }
    
    /// <summary>
    /// Add stress value directly (alternative to using modifiers in text)
    /// </summary>
    public void AddStress(int value)
    {
        stressvariable += value;
        Debug.Log($"Added {value} stress (Total: {stressvariable})");
    }
    
    /// <summary>
    /// Test method for all modifier systems - can be called from Unity Editor
    /// </summary>
    [ContextMenu("Test All Modifier Systems")]
    public void TestStressModifierSystem()
    {
        Debug.Log("=== ALL MODIFIER SYSTEMS TEST ===");
        
        // Test various modifier formats
        string[] testTexts = {
            "Hello! {+100stress} This should add 100 stress.",
            "I'm feeling bad today. {-50stress} That should reduce stress by 50.",
            "Multiple modifiers {+25stress} in one {+10stress} sentence.",
            "Let's trigger an animation {animation:shake} and see what happens!",
            "This will change the scene {scene:MainMenu} after the dialog.",
            "Combined test: {+75stress} {animation:bounce} {scene:GameScene}",
            "No modifiers in this text.",
            "{+200stress} Modifier at the beginning.",
            "Modifier at the end {-75stress}",
            "Animation at start {animation:fade} with more text.",
            "Scene change {scene:Level2} in the middle of text.",
            "Invalid modifier {invalidstress} should be ignored.",
            "Case insensitive {+30STRESS} {ANIMATION:Shake} {SCENE:Menu} modifiers.",
            "{0stress} Zero modifier should work.",
            "Empty modifiers {animation:} {scene:} should be handled gracefully."
        };
        
        int initialStress = stressvariable;
        Debug.Log($"Initial stress: {initialStress}");
        
        foreach (string test in testTexts)
        {
            Debug.Log($"Testing: '{test}'");
            string processed = ProcessStressModifiers(test);
            Debug.Log($"Result: '{processed}' | Current stress: {stressvariable}");
            Debug.Log("---");
        }
        
        int finalStress = stressvariable;
        int totalChange = finalStress - initialStress;
        Debug.Log($"Final stress: {finalStress} (Change: {totalChange})");
        Debug.Log("=== END ALL MODIFIER SYSTEMS TEST ===");
    }
    
    /// <summary>
    /// Public property to access current stress value
    /// </summary>
    public int CurrentStress => stressvariable;
    
    /// <summary>
    /// Event triggered when an animation modifier is processed
    /// Subscribe to this to handle animations in external systems
    /// </summary>
    public System.Action<string> OnAnimationTriggered;
    
    /// <summary>
    /// Event triggered when a scene modifier is processed
    /// Subscribe to this to handle scene changes in external systems
    /// </summary>
    public System.Action<string> OnSceneChangeTriggered;
    
    /// <summary>
    /// Manually trigger an animation (can be called externally)
    /// </summary>
    /// <param name="animationName">Name of the animation to trigger</param>
    public void TriggerAnimationManual(string animationName)
    {
        Debug.Log($"[ANIMATION] Manual animation trigger: '{animationName}'");
        TriggerAnimation(animationName);
    }
    
    /// <summary>
    /// Manually trigger a scene change (can be called externally)
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void TriggerSceneChangeManual(string sceneName)
    {
        Debug.Log($"[SCENE] Manual scene change trigger: '{sceneName}'");
        TriggerSceneChange(sceneName);
    }
    
    /// <summary>
    /// Process a custom text string and apply all modifiers
    /// Useful for testing or processing text from external sources
    /// </summary>
    /// <param name="text">Text containing modifiers</param>
    /// <returns>Cleaned text with modifiers applied</returns>
    public string ProcessCustomText(string text)
    {
        Debug.Log($"[MODIFIERS] Processing custom text: '{text}'");
        string result = ProcessStressModifiers(text);
        Debug.Log($"[MODIFIERS] Processed result: '{result}'");
        return result;
    }
    
    /// <summary>
    /// Test spam protection during text animation
    /// </summary>
    [ContextMenu("Test Animation Spam Protection")]
    public void TestAnimationSpamProtection()
    {
        Debug.Log("=== TESTING ANIMATION SPAM PROTECTION ===");
        
        if (dialogInstance == null)
        {
            Debug.LogError("No dialog instance found! Please create a dialog first.");
            return;
        }
        
        // Start a long text animation
        string longText = "This is a very long text that will take some time to animate completely. We will test what happens when we spam space during this animation.";
        
        // Find dialog text component safely
        TMP_Text dialogTextComponent = null;
        DialogPrefabController controller = dialogInstance.GetComponent<DialogPrefabController>();
        if (controller != null && controller.dialogueText != null)
        {
            dialogTextComponent = controller.dialogueText;
        }
        else
        {
            Transform textTransform = dialogInstance.transform.Find("DialogueText");
            if (textTransform != null)
            {
                dialogTextComponent = textTransform.GetComponent<TMP_Text>();
            }
        }
        
        if (dialogTextComponent == null)
        {
            Debug.LogError("Could not find dialog text component for animation test!");
            return;
        }
        
        Debug.Log("Starting long text animation, then simulating spam...");
        AnimateDialogText(longText, dialogTextComponent, null);
        
        // Start spam test after animation begins
        StartCoroutine(SimulateAnimationSpam());
    }
    
    /// <summary>
    /// Simulate spam during text animation
    /// </summary>
    private IEnumerator SimulateAnimationSpam()
    {
        // Wait for animation to start
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("[ANIM-SPAM-TEST] Starting spam during text animation");
        
        for (int i = 0; i < 5; i++)
        {
            Debug.Log($"[ANIM-SPAM-TEST] Spam attempt #{i + 1} - isTextAnimating: {isTextAnimating}");
            
            if (isTextAnimating)
            {
                // Simulate HandleDialogProgression call during animation
                HandleDialogProgression();
            }
            
            // Very short delay between spam attempts
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("[ANIM-SPAM-TEST] Animation spam test complete");
        Debug.Log("=== END ANIMATION SPAM PROTECTION TEST ===");
    }
    
    /// <summary>
    /// Test method to simulate rapid space key presses (spam test)
    /// </summary>
    [ContextMenu("Test Spam Protection")]
    public void TestSpamProtection()
    {
        Debug.Log("=== TESTING SPAM PROTECTION ===");
        Debug.Log("Simulating rapid space key presses...");
        
        StartCoroutine(SimulateSpamInput());
    }
    
    /// <summary>
    /// Simulate rapid input to test spam protection
    /// </summary>
    private IEnumerator SimulateSpamInput()
    {
        Debug.Log("[SPAM-TEST] Starting spam simulation");
        
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"[SPAM-TEST] Simulated input #{i + 1}");
            
            // Check current state before each input
            Debug.Log($"[SPAM-TEST] Pre-input state: isProcessingInput={isProcessingInput}, isInDialogTransition={isInDialogTransition}");
            
            // Simulate HandleDialogProgression call
            if (!isProcessingInput && !isInDialogTransition)
            {
                float currentTime = Time.time;
                bool canProcessInput = (currentTime - lastInputTime) >= INPUT_COOLDOWN;
                bool canUpdateDialog = (currentTime - lastDialogUpdateTime) >= 0.2f;
                
                if (canProcessInput && canUpdateDialog)
                {
                    Debug.Log($"[SPAM-TEST] Input #{i + 1} ACCEPTED");
                    lastInputTime = currentTime;
                    lastDialogUpdateTime = currentTime;
                    isProcessingInput = true;
                    isInDialogTransition = true;
                    
                    // Simulate quick processing
                    yield return new WaitForSeconds(0.1f);
                    
                    // Reset flags
                    isProcessingInput = false;
                    yield return new WaitForSeconds(0.1f);
                    isInDialogTransition = false;
                }
                else
                {
                    Debug.Log($"[SPAM-TEST] Input #{i + 1} BLOCKED - cooldown not met");
                }
            }
            else
            {
                Debug.Log($"[SPAM-TEST] Input #{i + 1} BLOCKED - already processing");
            }
            
            // Very short delay between inputs to simulate spam
            yield return new WaitForSeconds(0.05f);
        }
        
        Debug.Log("[SPAM-TEST] Spam simulation complete");
        Debug.Log("=== END SPAM PROTECTION TEST ===");
    }
    
    /// <summary>
    /// Debug method to test component caching and separation
    /// </summary>
    [ContextMenu("Test Component Separation")]
    public void TestComponentSeparation()
    {
        Debug.Log("=== TESTING COMPONENT SEPARATION ===");
        
        if (dialogInstance == null)
        {
            Debug.LogError("Dialog instance is null! Create a dialog first.");
            return;
        }
        
        // Clear cache to force fresh component detection
        ClearComponentCache();
        
        // Test NPC name update
        Debug.Log("Testing NPC name update...");
        UpdateNpcNameSafe("TEST NPC NAME");
        
        // Wait a moment then test dialog text update
        Debug.Log("Testing dialog text update...");
        UpdateDialogTextSafe("TEST DIALOG TEXT - This should appear in a different component than the name");
        
        // Log all components for verification
        LogAllDialogComponents();
        
        Debug.Log("=== END COMPONENT SEPARATION TEST ===");
    }
    
    #endregion

    #region Dialog Handling

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
        
        // Process stress modifiers from the 3D dialog entry
        string processedDialogEntry = ProcessStressModifiers(dialog.dialogEntry);
        Debug.Log($"3D Dialog - Original: '{dialog.dialogEntry}'");
        Debug.Log($"3D Dialog - Processed: '{processedDialogEntry}'");
        
        // 3D dialogs don't need NPC name extraction - the 3D model represents the character
        // Extract and display NPC name if present in dialog entry
        // string npcName = ExtractNpcNameFromDialogText(dialog.dialogEntry);
        // if (!string.IsNullOrEmpty(npcName))
        // {
        //     UpdateNpcNameDisplay(npcName);
        // }
        
        AnimateDialogText(processedDialogEntry, tmp3D, dialog.audioDialogEntry);

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

    private void Show2DDialog(CoreGameDialog dialog)
    {
        Debug.Log($"=== Show2DDialog FIELD MAPPING DEBUG ===");
        Debug.Log($"CoreGameDialog.npcName = '{dialog.npcName}' -> should go to DialogueName");
        Debug.Log($"CoreGameDialog.dialogEntry = '{dialog.dialogEntry}' -> should go to DialogueText");
        
        // Validate the dialog data structure
        ValidateDialogData(dialog);
        
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
            
            // Clear component cache when new dialog instance is created
            ClearComponentCache();
        }
        
        // Validate the UI structure
        ValidateDialogUI();
        
        // Ensure BackgroundFade reference if not assigned
        EnsureBackgroundFadeReference();
        
        // Handle cutscene fade effect
        HandleCutsceneFade(dialog.cutsceneType);
        
        // CRITICAL: Assign NPC name to DialogueName component
        // Use NPC name from CoreGameDialog.npcName, or extract from dialog text as fallback
        string npcName = !string.IsNullOrEmpty(dialog.npcName) ? dialog.npcName : ExtractNpcNameFromDialogText(dialog.dialogEntry);
        Debug.Log($"Final npcName for DialogueName component: '{npcName}'");
        
        // Process stress modifiers from the initial dialog entry
        string processedDialogEntry = ProcessStressModifiers(dialog.dialogEntry);
        Debug.Log($"Original dialog entry: '{dialog.dialogEntry}'");
        Debug.Log($"Processed dialog entry: '{processedDialogEntry}'");
        
        if (!string.IsNullOrEmpty(npcName))
        {
            UpdateNpcNameDisplay(npcName);
        }
        else
        {
            Debug.LogWarning("NPC name is empty! DialogueName will not be updated.");
        }
        
        // CRITICAL: Animate dialog text instead of setting it instantly
        // Find the DialogueText component to animate
        TMP_Text dialogTextComponent = null;
        if (dialogInstance != null)
        {
            // Try DialogPrefabController first
            DialogPrefabController controller = dialogInstance.GetComponent<DialogPrefabController>();
            if (controller != null && controller.dialogueText != null)
            {
                dialogTextComponent = controller.dialogueText;
                Debug.Log("Found DialogueText component via DialogPrefabController for animation");
            }
            else
            {
                // Fallback to direct search
                Transform textTransform = dialogInstance.transform.Find("DialogueText");
                if (textTransform != null)
                {
                    dialogTextComponent = textTransform.GetComponent<TMP_Text>();
                    Debug.Log("Found DialogueText component via direct search for animation");
                }
            }
        }
        
        if (dialogTextComponent != null)
        {
            Debug.Log($"Starting dialog text animation for: '{processedDialogEntry}'");
            AnimateDialogText(processedDialogEntry, dialogTextComponent, dialog.audioDialogEntry);
        }
        else
        {
            Debug.LogError("Could not find DialogueText component for animation! Falling back to instant text.");
            UpdateDialogTextSafe(processedDialogEntry);
        }
        
        // Handle choices if any
        if (dialog.choices != null && dialog.choices.Length > 0)
        {
            ShowChoices(dialog.choices);
        }
    }

    private void ShowChoices(CoreGameDialogChoices[] choices)
    {
        GameObject questionBar = SummonQuestionBar();
        
        // If prefab failed, try creating a fallback question bar
        if (questionBar == null)
        {
            Debug.LogWarning("Prefab question bar failed, attempting to create fallback...");
            questionBar = CreateFallbackQuestionBar();
        }
        
        if (questionBar == null) 
        {
            Debug.LogError("Both prefab and fallback question bar creation failed!");
            return;
        }
        
        ShowChoicesWithButtons(choices, OnPlayerChoseResponse);
    }
    
    /// <summary>
    /// Integrated choice display system from PlayerAnswerManager
    /// UPDATED: Use button array approach for direct modification
    /// </summary>
    private void ShowChoicesWithButtons(CoreGameDialogChoices[] choices, System.Action<int> callback)
    {
        Debug.Log($"Showing {choices?.Length ?? 0} choices using button array approach...");

        onChoiceSelected = callback;
        buttonTweenIds.Clear();

        if (choices == null || choices.Length == 0)
        {
            Debug.LogWarning("No choices provided to ShowChoicesWithButtons!");
            return;
        }
        
        // Debug the choices data
        ValidateChoices(choices);
        
        // Validate the question UI structure
        ValidateQuestionUI();

        // Get all buttons from the question instance as an array
        Button[] buttonArray = null;
        if (questionInstance != null)
        {
            buttonArray = questionInstance.GetComponentsInChildren<Button>();
            Debug.Log($"Found {buttonArray.Length} buttons in question instance");
            
            // Debug what buttons we found
            for (int b = 0; b < buttonArray.Length; b++)
            {
                Debug.Log($"Button {b}: '{buttonArray[b].name}' (GameObject: {buttonArray[b].gameObject.name})");
            }
        }
        
        if (buttonArray == null || buttonArray.Length == 0)
        {
            Debug.LogError("No buttons found in question instance!");
            return;
        }

        // Show up to 3 choices on screen (or all choices if less than 3), limited by available buttons
        int choicesToShow = Mathf.Min(choices.Length, 3, buttonArray.Length);
        
        Debug.Log($"Total choices in data: {choices.Length}, UI will show: {choicesToShow} (Available buttons: {buttonArray.Length})");

        for (int i = 0; i < choicesToShow; i++)
        {
            if (choices[i] != null && i < buttonArray.Length)
            {
                Debug.Log($"Processing choice {i}: '{choices[i].playerChoice}' -> Button {i} ({buttonArray[i].name})");
                
                Button btn = buttonArray[i];
                btn.gameObject.SetActive(true);
                btn.onClick.RemoveAllListeners();

                // CRITICAL: Use choices[i].playerChoice directly from CoreGameDialogChoices data structure
                string choiceText = choices[i].playerChoice;
                
                // Handle empty playerChoice
                if (string.IsNullOrEmpty(choiceText))
                {
                    choiceText = $"Choice {i + 1}";
                    Debug.LogWarning($"Choice {i} has empty playerChoice field! Using fallback: '{choiceText}'");
                }
                
                // Add key indicator based on button index
                string keyIndicator = GetKeyIndicator(i);
                string buttonTextWithKey = $"{keyIndicator} {choiceText}";
                
                Debug.Log($"Setting button array[{i}] ({btn.name}) text to: '{buttonTextWithKey}'");
                
                // Direct TMP_Text assignment to button
                TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    Debug.Log($"Found TMP_Text in button {i}: '{btnText.transform.name}' (current: '{btnText.text}')");
                    btnText.text = buttonTextWithKey;
                    Debug.Log($"Button {i} updated to: '{btnText.text}'");
                }
                else
                {
                    Debug.LogError($"CRITICAL: Button array[{i}] ({btn.name}) has no TMP_Text component!");
                    
                    // Debug button structure
                    Transform[] children = btn.GetComponentsInChildren<Transform>();
                    Debug.Log($"Button {i} ({btn.name}) children:");
                    for (int c = 0; c < children.Length; c++)
                    {
                        Component[] components = children[c].GetComponents<Component>();
                        string componentNames = "";
                        for (int j = 0; j < components.Length; j++)
                        {
                            componentNames += components[j].GetType().Name;
                            if (j < components.Length - 1) componentNames += ", ";
                        }
                        Debug.Log($"  - {children[c].name} (Components: {componentNames})");
                    }
                }

                int index = i; // Important for correct capture
                btn.onClick.AddListener(() => {
                    // If animation is still playing, finish it instantly
                    if (buttonTweenIds.TryGetValue(btn, out int tweenId) && LeanTween.isTweening(tweenId))
                    {   
                        TMP_Text btnText2 = btn.GetComponentInChildren<TMP_Text>();
                        if (btnText2 != null)
                        {
                            string keyIndicator = GetKeyIndicator(index);
                            string choiceTextFinal = !string.IsNullOrEmpty(choices[index].playerChoice) ? choices[index].playerChoice : $"Choice {index + 1}";
                            btnText2.text = $"{keyIndicator} {choiceTextFinal}";
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
        }
        
        // Hide unused buttons
        for (int i = choicesToShow; i < buttonArray.Length; i++)
        {
            if (buttonArray[i] != null)
            {
                buttonArray[i].gameObject.SetActive(false);
                Debug.Log($"Hidden unused button {i}: {buttonArray[i].name}");
            }
        }
    }
    
    /// <summary>
    /// Hide choices and clean up buttons using button array approach
    /// </summary>
    private void HideChoices()
    {
        Debug.Log("Hiding choices using button array approach...");
        
        if (questionInstance != null)
        {
            Button[] buttonArray = questionInstance.GetComponentsInChildren<Button>();
            Debug.Log($"Found {buttonArray.Length} buttons to hide in question instance");
            
            for (int i = 0; i < buttonArray.Length; i++)
            {
                Button btn = buttonArray[i];
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                    {
                        Debug.Log($"Clearing button {i} ({btn.name}) text: was '{btnText.text}'");
                        btnText.text = "";
                    }
                    btn.gameObject.SetActive(false);
                    Debug.Log($"Hidden button {i}: {btn.name}");
                }
            }
        }
        
        // Also hide any buttons from the answerButtons array (fallback)
        if (answerButtons != null)
        {
            Debug.Log($"Also hiding {answerButtons.Length} buttons from answerButtons fallback array");
            for (int i = 0; i < answerButtons.Length; i++)
            {
                Button btn = answerButtons[i];
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                    {
                        btnText.text = "";
                    }
                    btn.gameObject.SetActive(false);
                }
            }
        }
        
        // Clear choice selection state
        onChoiceSelected = null;
        buttonTweenIds.Clear();
        
        Debug.Log("All buttons hidden and cleared");
    }
    
    /// <summary>
    /// Get key indicator for button based on index
    /// </summary>
    private string GetKeyIndicator(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0: return "[Q]";
            case 1: return "[W]";
            case 2: return "[E]";
            default: return $"[{buttonIndex + 1}]"; // Fallback for additional buttons
        }
    }
    
    /// <summary>
    /// Get choice button by name from the question instance, with fallback creation
    /// </summary>
    private Button GetChoiceButton(string buttonName)
    {
        if (questionInstance == null)
        {
            Debug.LogWarning("Question instance is null, cannot find button!");
            return null;
        }
        
        Transform buttonTransform = questionInstance.transform.Find(buttonName);
        if (buttonTransform != null)
        {
            Button button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                return button;
            }
            else
            {
                Debug.LogWarning($"GameObject '{buttonName}' found but has no Button component!");
            }
        }
        else
        {
            Debug.LogWarning($"Button '{buttonName}' not found as direct child of question instance!");
            
            // Try to find it deeper in the hierarchy
            Button[] allButtons = questionInstance.GetComponentsInChildren<Button>();
            foreach (Button btn in allButtons)
            {
                if (btn.transform.name.Contains(buttonName) || btn.transform.name.Equals(buttonName))
                {
                    Debug.Log($"Found button '{buttonName}' deeper in hierarchy: {btn.transform.name}");
                    return btn;
                }
            }
            
            Debug.LogWarning($"Button '{buttonName}' not found anywhere in question instance hierarchy!");
        }
        
        return null;
    }
    
    /// <summary>
    /// Create a fallback question bar programmatically if prefab fails
    /// </summary>
    private GameObject CreateFallbackQuestionBar()
    {
        Debug.Log("Creating fallback question bar programmatically...");
        
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found for fallback question bar!");
            return null;
        }
        
        // Create main container
        GameObject questionBar = new GameObject("FallbackQuestionBar");
        questionBar.transform.SetParent(canvas.transform, false);
        
        RectTransform questionRect = questionBar.AddComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0f, 0f);
        questionRect.anchorMax = new Vector2(1f, 0.3f);
        questionRect.offsetMin = Vector2.zero;
        questionRect.offsetMax = Vector2.zero;
        
        // Add background image
        Image background = questionBar.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.8f);
        
        // Create button container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(questionBar.transform, false);
        
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.3f);
        containerRect.anchorMax = new Vector2(0.9f, 0.7f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        // Add horizontal layout group
        HorizontalLayoutGroup layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        
        // Create Q, W, E buttons
        string[] buttonNames = { "Q", "W", "E" };
        for (int i = 0; i < buttonNames.Length; i++)
        {
            GameObject buttonObj = new GameObject(buttonNames[i]);
            buttonObj.transform.SetParent(buttonContainer.transform, false);
            
            // Add button component
            Button button = buttonObj.AddComponent<Button>();
            
            // Add button image
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TMP_Text buttonText = textObj.AddComponent<TMP_Text>();
            buttonText.text = $"[{buttonNames[i]}] Choice {i + 1}";
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Debug.Log($"Created fallback button: {buttonNames[i]}");
        }
        
        questionInstance = questionBar;
        Debug.Log("Fallback question bar created successfully!");
        
        return questionBar;
    }
    
    /// <summary>
    /// Initialize and clear all answer buttons
    /// </summary>
    private void InitializeButtons()
    {
        Debug.Log("Initializing buttons...");
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].gameObject.SetActive(false);
                answerButtons[i].onClick.RemoveAllListeners();
                
                // Find ALL TMP_Text components in the button and clear them
                TMP_Text[] allTexts = answerButtons[i].GetComponentsInChildren<TMP_Text>();
                foreach (var txt in allTexts)
                {
                    Debug.Log($"Clearing text component '{txt.transform.name}' in button {i}: was '{txt.text}'");
                    txt.text = "";
                }
                
                // Also try the direct approach
                TMP_Text btnText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = "";
                }
            }
        }
    }
    
    /// <summary>
    /// Validate choices for debugging purposes
    /// </summary>
    public void ValidateChoices(CoreGameDialogChoices[] choices)
    {
        if (choices == null)
        {
            Debug.LogError("Choices array is null!");
            return;
        }
        
        Debug.Log($"Validating {choices.Length} choices:");
        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i] == null)
            {
                Debug.LogError($"Choice {i} is null!");
            }
            else
            {
                Debug.Log($"Choice {i}: playerChoice='{choices[i].playerChoice}', hasResponses={choices[i].dialogResponses != null && choices[i].dialogResponses.Length > 0}");
            }
        }
    }

    /// <summary>
    /// Comprehensive data validation for debugging dialog/choice issues
    /// </summary>
    private void ValidateDialogData(CoreGameDialog dialog)
    {
        Debug.Log($"=== DIALOG DATA VALIDATION ===");
        Debug.Log($"CoreGameDialog.npcName: '{dialog.npcName}' (Length: {dialog.npcName?.Length ?? 0})");
        Debug.Log($"CoreGameDialog.dialogEntry: '{dialog.dialogEntry}' (Length: {dialog.dialogEntry?.Length ?? 0})");
        
        if (dialog.choices != null)
        {
            Debug.Log($"Dialog has {dialog.choices.Length} choices:");
            for (int i = 0; i < dialog.choices.Length; i++)
            {
                var choice = dialog.choices[i];
                if (choice != null)
                {
                    Debug.Log($"  Choice {i}:");
                    Debug.Log($"    - playerChoice: '{choice.playerChoice}' (Length: {choice.playerChoice?.Length ?? 0})");
                    
                    if (choice.dialogResponses != null)
                    {
                        Debug.Log($"    - Has {choice.dialogResponses.Length} responses:");
                        for (int j = 0; j < choice.dialogResponses.Length; j++)
                        {
                            var response = choice.dialogResponses[j];
                            if (response != null)
                            {
                                Debug.Log($"      Response {j}:");
                                Debug.Log($"        - NpcName: '{response.NpcName}' (Length: {response.NpcName?.Length ?? 0})");
                                Debug.Log($"        - npcResponse: '{response.npcResponse}' (Length: {response.npcResponse?.Length ?? 0})");
                            }
                            else
                            {
                                Debug.LogWarning($"      Response {j} is NULL!");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"    - Choice {i} has no dialogResponses array!");
                    }
                }
                else
                {
                    Debug.LogWarning($"  Choice {i} is NULL!");
                }
            }
        }
        else
        {
            Debug.Log("Dialog has no choices.");
        }
        
        Debug.Log($"=== END DIALOG DATA VALIDATION ===");
    }

    /// <summary>
    /// Validate UI components in dialog instance
    /// </summary>
    private void ValidateDialogUI()
    {
        Debug.Log($"=== DIALOG UI VALIDATION ===");
        
        if (dialogInstance == null)
        {
            Debug.LogError("dialogInstance is NULL!");
            return;
        }
        
        Debug.Log($"Dialog instance: {dialogInstance.name}");
        
        // Check for DialogueName component
        Transform dialogueNameTransform = dialogInstance.transform.Find("DialogueName");
        if (dialogueNameTransform != null)
        {
            TMP_Text dialogueNameText = dialogueNameTransform.GetComponent<TMP_Text>();
            if (dialogueNameText != null)
            {
                Debug.Log($"✓ DialogueName found: '{dialogueNameText.text}'");
            }
            else
            {
                Debug.LogWarning("DialogueName transform found but no TMP_Text component!");
            }
        }
        else
        {
            Debug.LogWarning("DialogueName transform not found!");
        }
        
        // Check for DialogueText component
        Transform dialogueTextTransform = dialogInstance.transform.Find("DialogueText");
        if (dialogueTextTransform != null)
        {
            TMP_Text dialogueTextComponent = dialogueTextTransform.GetComponent<TMP_Text>();
            if (dialogueTextComponent != null)
            {
                Debug.Log($"✓ DialogueText found: '{dialogueTextComponent.text}'");
            }
            else
            {
                Debug.LogWarning("DialogueText transform found but no TMP_Text component!");
            }
        }
        else
        {
            Debug.LogWarning("DialogueText transform not found!");
        }
        
        // List all TMP_Text components
        TMP_Text[] allTexts = dialogInstance.GetComponentsInChildren<TMP_Text>();
        Debug.Log($"All TMP_Text components in dialog instance ({allTexts.Length}):");
        for (int i = 0; i < allTexts.Length; i++)
        {
            Debug.Log($"  [{i}] {allTexts[i].transform.name}: '{allTexts[i].text}'");
        }
        
        Debug.Log($"=== END DIALOG UI VALIDATION ===");
    }

    /// <summary>
    /// Validate question bar UI components
    /// </summary>
    private void ValidateQuestionUI()
    {
        Debug.Log($"=== QUESTION UI VALIDATION ===");
        
        if (questionInstance == null)
        {
            Debug.LogError("questionInstance is NULL!");
            return;
        }
        
        Debug.Log($"Question instance: {questionInstance.name}");
        
        string[] buttonNames = { "Q", "W", "E" };
        foreach (string buttonName in buttonNames)
        {
            Button btn = GetChoiceButton(buttonName);
            if (btn != null)
            {
                TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    Debug.Log($"✓ Button {buttonName} found with text: '{btnText.text}' on component '{btnText.transform.name}'");
                }
                else
                {
                    Debug.LogWarning($"Button {buttonName} found but no TMP_Text component!");
                }
            }
            else
            {
                Debug.LogWarning($"Button {buttonName} not found!");
            }
        }
        
        Debug.Log($"=== END QUESTION UI VALIDATION ===");
    }

    /// <summary>
    /// Manual test method - can be called from Unity Editor for debugging
    /// </summary>
    [ContextMenu("Test Dialog System")]
    public void TestDialogSystem()
    {
        Debug.Log("=== MANUAL DIALOG SYSTEM TEST ===");
        
        if (coreGameData == null)
        {
            Debug.LogError("CoreGameData is null! Please assign a ScriptableObject.");
            return;
        }
        
        if (coreGameData.coreBlock == null || coreGameData.coreBlock.Length == 0)
        {
            Debug.LogError("CoreGameData has no blocks!");
            return;
        }
        
        Debug.Log($"CoreGameData has {coreGameData.coreBlock.Length} blocks");
        
        for (int i = 0; i < coreGameData.coreBlock.Length; i++)
        {
            var block = coreGameData.coreBlock[i];
            if (block.Dialog != null)
            {
                Debug.Log($"Block {i} - Dialog Block:");
                ValidateDialogData(block.Dialog);
            }
        }
        
        // Test UI components
        if (dialogInstance != null)
        {
            ValidateDialogUI();
        }
        else
        {
            Debug.Log("No dialog instance currently active");
        }
        
        if (questionInstance != null)
        {
            ValidateQuestionUI();
        }
        else
        {
            Debug.Log("No question instance currently active");
        }
        
        Debug.Log("=== END MANUAL DIALOG SYSTEM TEST ===");
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
        Debug.Log($"=== OnPlayerChoseResponse - Choice {choiceIndex} Selected ===");
        
        var currentBlock = coreGameData.coreBlock[currentBlockIndex];
        if (currentBlock.Dialog?.choices == null || choiceIndex >= currentBlock.Dialog.choices.Length)
        {
            Debug.LogError($"Invalid choice index {choiceIndex} or no choices available!");
            return;
        }
        
        var selectedChoice = currentBlock.Dialog.choices[choiceIndex];
        Debug.Log($"Selected choice: '{selectedChoice.playerChoice}'");
        
        // Hide choices first
        HideChoices();
        
        // Store which choice was selected
        selectedChoiceIndex = choiceIndex;
        
        // Check if there are dialog responses to show
        if (selectedChoice.dialogResponses != null && selectedChoice.dialogResponses.Length > 0)
        {
            Debug.Log($"Found {selectedChoice.dialogResponses.Length} dialog responses to display");
            
            // Start showing responses from index 0
            currentChoiceResponseIndex = 0;
            isShowingResponse = true;
            
            ShowDialogResponse(selectedChoice, 0);
        }
        else
        {
            Debug.Log("No dialog responses found, continuing to next block");
            // No responses, just continue to next block
            ContinueToNextBlock();
        }
    }

    /// <summary>
    /// Show a specific dialog response from the selected choice
    /// </summary>
    [Obsolete]
    private void ShowDialogResponse(CoreGameDialogChoices selectedChoice, int responseIndex)
    {
        if (selectedChoice.dialogResponses == null || responseIndex >= selectedChoice.dialogResponses.Length)
        {
            Debug.LogError($"Invalid response index {responseIndex} or no responses available!");
            ContinueToNextBlock();
            return;
        }
        
        var response = selectedChoice.dialogResponses[responseIndex];
        string npcName = response.NpcName;
        string npcResponse = response.npcResponse;
        AudioClip audioClip = selectedChoice.audioDialogResponse;
        
        // Process stress modifiers from the dialog response
        string processedResponse = ProcessStressModifiers(npcResponse);
        
        Debug.Log($"Showing dialog response {responseIndex + 1}/{selectedChoice.dialogResponses.Length}:");
        Debug.Log($"  - NPC Name: '{npcName}' -> should go to DialogueName");
        Debug.Log($"  - Original Response: '{npcResponse}'");
        Debug.Log($"  - Processed Response: '{processedResponse}' -> should go to DialogueText");
        
        // Get current dialog type from the current block
        var currentBlock = coreGameData.coreBlock[currentBlockIndex];
        
        // Show the response based on dialog type
        if (currentBlock.Dialog.dialogType == CoreGameDialog.DialogType.ThreeD)
        {
            Debug.Log("Displaying 3D dialog response");
            Show3DResponse(currentBlock.Dialog, processedResponse, audioClip);
        }
        else
        {
            Debug.Log("Displaying 2D dialog response");
            
            // Ensure dialog instance exists
            if (dialogInstance == null)
            {
                dialogInstance = SummonDialogBar();
                if (dialogInstance == null)
                {
                    Debug.LogError("Failed to create dialog bar for response!");
                    ContinueToNextBlock();
                    return;
                }
                
                // Clear component cache when new dialog instance is created
                ClearComponentCache();
            }
            
            // Update NPC name display
            if (!string.IsNullOrEmpty(npcName))
            {
                Debug.Log($"Updating NPC name to: '{npcName}'");
                UpdateNpcNameSafe(npcName);
            }
            else
            {
                Debug.LogWarning("NPC name from response is empty!");
            }
            
            // Animate dialog response text instead of setting it instantly
            TMP_Text dialogTextComponent = null;
            if (dialogInstance != null)
            {
                // Try DialogPrefabController first
                DialogPrefabController controller = dialogInstance.GetComponent<DialogPrefabController>();
                if (controller != null && controller.dialogueText != null)
                {
                    dialogTextComponent = controller.dialogueText;
                    Debug.Log("Found DialogueText component via DialogPrefabController for response animation");
                }
                else
                {
                    // Fallback to direct search
                    Transform textTransform = dialogInstance.transform.Find("DialogueText");
                    if (textTransform != null)
                    {
                        dialogTextComponent = textTransform.GetComponent<TMP_Text>();
                        Debug.Log("Found DialogueText component via direct search for response animation");
                    }
                }
            }
            
            if (dialogTextComponent != null)
            {
                Debug.Log($"Starting dialog response text animation for: '{processedResponse}'");
                AnimateDialogText(processedResponse, dialogTextComponent, audioClip);
            }
            else
            {
                Debug.LogError("Could not find DialogueText component for response animation! Falling back to instant text.");
                UpdateDialogTextSafe(processedResponse);
            }
        }
        
        // Store the current response index for progression
        currentChoiceResponseIndex = responseIndex;
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

        Canvas canvas = FindFirstObjectByType<Canvas>();
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

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return null;
        }

        if (npcQuestionThemplate == null)
        {
            Debug.LogError("npcQuestionThemplate is not assigned! Please assign the prefab in the inspector.");
            return null;
        }

        GameObject instance = null;
        
        try
        {
            instance = Instantiate(npcQuestionThemplate, canvas.transform, false);
            
            if (instance == null)
            {
                Debug.LogError("Failed to instantiate npcQuestionThemplate prefab!");
                return null;
            }
            
            // Check for broken script references
            MonoBehaviour[] scripts = instance.GetComponentsInChildren<MonoBehaviour>();
            int brokenScripts = 0;
            foreach (var script in scripts)
            {
                if (script == null)
                {
                    brokenScripts++;
                }
            }
            
            if (brokenScripts > 0)
            {
                Debug.LogWarning($"Question bar prefab has {brokenScripts} missing script references, but continuing with instantiation.");
            }
            
            // CRITICAL FIX: Assign the instance to questionInstance
            questionInstance = instance;
            
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
                Debug.Log("Question bar summoned and positioned!");
            }
            else
            {
                Debug.LogWarning("Question prefab has no RectTransform!");
            }
            
            Debug.Log($"questionInstance assigned: {questionInstance != null}");
            
            // Validate that we can find the expected buttons
            ValidateQuestionBarStructure(instance);
            
            return instance;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception while instantiating question bar: {e.Message}");
            if (instance != null)
            {
                DestroyImmediate(instance);
            }
            return null;
        }
    }
    
    /// <summary>
    /// Validate that the question bar has the expected button structure
    /// </summary>
    private void ValidateQuestionBarStructure(GameObject questionBar)
    {
        Debug.Log("=== QUESTION BAR STRUCTURE VALIDATION ===");
        
        if (questionBar == null)
        {
            Debug.LogError("Question bar is null!");
            return;
        }
        
        Debug.Log($"Question bar name: {questionBar.name}");
        
        // List all child objects
        Transform[] allChildren = questionBar.GetComponentsInChildren<Transform>();
        Debug.Log($"Question bar has {allChildren.Length} total transforms:");
        
        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform child = allChildren[i];
            Button button = child.GetComponent<Button>();
            TMP_Text text = child.GetComponent<TMP_Text>();
            
            string info = $"  [{i}] {child.name}";
            if (button != null) info += " [Button]";
            if (text != null) info += " [TMP_Text]";
            
            Debug.Log(info);
        }
        
        // Check for Q, W, E buttons specifically
        string[] expectedButtons = { "Q", "W", "E" };
        foreach (string buttonName in expectedButtons)
        {
            Transform buttonTransform = questionBar.transform.Find(buttonName);
            if (buttonTransform != null)
            {
                Button btn = buttonTransform.GetComponent<Button>();
                TMP_Text btnText = buttonTransform.GetComponentInChildren<TMP_Text>();
                
                Debug.Log($"✓ Found button '{buttonName}': Button={btn != null}, TMP_Text={btnText != null}");
            }
            else
            {
                Debug.LogWarning($"✗ Button '{buttonName}' not found as direct child!");
            }
        }
        
        Debug.Log("=== END QUESTION BAR STRUCTURE VALIDATION ===");
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

    private void Update()
    {
        float currentTime = Time.time;
        
        // Enhanced input throttling - prevent spam input with multiple checks
        if (isProcessingInput || isInDialogTransition)
        {
            return; // Don't process new input while already processing or transitioning
        }
        
        // Additional safety: ensure minimum time between inputs
        bool canProcessInput = (currentTime - lastInputTime) >= INPUT_COOLDOWN;
        
        // Extra safety: ensure minimum time between dialog updates
        bool canUpdateDialog = (currentTime - lastDialogUpdateTime) >= 0.2f;
        
        // Use Space key for dialog progression with enhanced protection
        if (Input.GetKeyDown(KeyCode.Space) && canProcessInput && canUpdateDialog)
        {
            // Set all protection flags immediately
            lastInputTime = currentTime;
            lastDialogUpdateTime = currentTime;
            isProcessingInput = true;
            isInDialogTransition = true;
            
            Debug.Log($"[INPUT] SPACE KEY PRESSED - Time: {currentTime:F2}, Starting HandleDialogProgression()");
            
            try
            {
                HandleDialogProgression();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[INPUT] Error in HandleDialogProgression: {e.Message}");
            }
            finally
            {
                // Always reset flags even if there's an error
                StartCoroutine(ResetInputProcessingFlags());
            }
        }
        
        // Handle choice input with enhanced throttling
        if (canProcessInput && !isInDialogTransition)
        {
            HandleChoiceInput();
        }
        
        // Escape key for cutscenes (no throttling needed)
        if (Input.GetKeyDown(KeyCode.Escape) && isPlayingCutscene)
        {
            SkipCutscene();
        }
    }
    
    /// <summary>
    /// Reset transition flag after text skip to prevent getting stuck
    /// </summary>
    private IEnumerator ResetTransitionFlagAfterSkip()
    {
        // Wait a short time for text completion to finish
        yield return new WaitForSeconds(0.1f);
        isInDialogTransition = false;
        Debug.Log("[PROGRESSION] Transition flag reset after text skip");
    }
    
    /// <summary>
    /// Reset input processing flags after a delay to prevent spam
    /// </summary>
    private IEnumerator ResetInputProcessingFlags()
    {
        // Wait for a minimum delay before allowing new input
        yield return new WaitForSeconds(0.2f);
        
        // Reset processing flags
        isProcessingInput = false;
        
        // Wait a bit longer before allowing dialog transitions
        yield return new WaitForSeconds(0.1f);
        isInDialogTransition = false;
        
        Debug.Log("[INPUT] Input processing flags reset - ready for new input");
    }
    
    /// <summary>
    /// Handle choice input using Q, W, E keys
    /// </summary>
    private void HandleChoiceInput()
    {
        // Only handle choice input if choices are currently visible
        if (onChoiceSelected == null) return;
        
        // Check for Q, W, E key presses for choice selection
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SelectChoice(0);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            SelectChoice(1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SelectChoice(2);
        }
    }
    
    /// <summary>
    /// Select a choice by index using keyboard input
    /// UPDATED: Use button array approach for consistency
    /// </summary>
    private void SelectChoice(int choiceIndex)
    {
        Debug.Log($"SelectChoice called with index {choiceIndex}");
        
        if (questionInstance == null)
        {
            Debug.LogWarning("Question instance is null, cannot select choice!");
            return;
        }
        
        // Get all buttons using the same array approach
        Button[] buttonArray = questionInstance.GetComponentsInChildren<Button>();
        Debug.Log($"Found {buttonArray.Length} buttons for choice selection");
        
        // Make sure the choice index is valid and within button array bounds
        if (choiceIndex >= 0 && choiceIndex < buttonArray.Length)
        {
            Button targetButton = buttonArray[choiceIndex];
            
            if (targetButton != null && targetButton.gameObject.activeInHierarchy)
            {
                Debug.Log($"Selecting choice {choiceIndex} - Button: {targetButton.name}");
                // Simulate button click
                targetButton.onClick.Invoke();
            }
            else
            {
                Debug.LogWarning($"Button at index {choiceIndex} is null or inactive!");
            }
        }
        else
        {
            Debug.LogWarning($"Choice index {choiceIndex} is out of range! Available buttons: {buttonArray.Length}");
        }
    }

    private void HandleDialogProgression()
    {
        Debug.Log("[PROGRESSION] === HandleDialogProgression CALLED ===");
        Debug.Log($"[PROGRESSION] Current state:");
        Debug.Log($"[PROGRESSION]   - isPlayingCutscene: {isPlayingCutscene}");
        Debug.Log($"[PROGRESSION]   - isTextAnimating: {isTextAnimating}");
        Debug.Log($"[PROGRESSION]   - isShowingResponse: {isShowingResponse}");
        Debug.Log($"[PROGRESSION]   - isProcessingInput: {isProcessingInput}");
        Debug.Log($"[PROGRESSION]   - isInDialogTransition: {isInDialogTransition}");
        Debug.Log($"[PROGRESSION]   - currentBlockIndex: {currentBlockIndex}/{(coreGameData?.coreBlock?.Length ?? 0)}");
        Debug.Log($"[PROGRESSION]   - selectedChoiceIndex: {selectedChoiceIndex}");
        Debug.Log($"[PROGRESSION]   - currentChoiceResponseIndex: {currentChoiceResponseIndex}");
        
        try
        {
            // Safety check: Ensure we have valid data
            if (coreGameData == null || coreGameData.coreBlock == null)
            {
                Debug.LogError("[PROGRESSION] CoreGameData is null or has no blocks!");
                return;
            }
            
            if (isPlayingCutscene) 
            {
                Debug.Log("[PROGRESSION] Blocked: Currently playing cutscene");
                return;
            }
            
            // If text is currently animating, skip to complete text and stop audio
            if (isTextAnimating)
            {
                Debug.Log("[PROGRESSION] Text is animating, skipping to complete...");
                
                // Set transition flag to prevent further input during completion
                isInDialogTransition = true;
                
                SkipTextAnimation();
                
                // Reset transition flag after a short delay
                StartCoroutine(ResetTransitionFlagAfterSkip());
                return;
            }
            
            if (currentBlockIndex >= coreGameData.coreBlock.Length)
            {
                Debug.Log("[PROGRESSION] Reached end of game, finishing...");
                FinishCoreGame();
                return;
            }
            
            var currentBlock = coreGameData.coreBlock[currentBlockIndex];
            Debug.Log($"[PROGRESSION] Current block type: {currentBlock.Type}");
            
            // If showing a choice response, handle multiple responses
            if (isShowingResponse)
            {
                Debug.Log("[PROGRESSION] Currently showing response, checking for more responses...");
                
                if (selectedChoiceIndex < 0 || currentBlock.Dialog?.choices == null || selectedChoiceIndex >= currentBlock.Dialog.choices.Length)
                {
                    Debug.LogError($"[PROGRESSION] Invalid selectedChoiceIndex {selectedChoiceIndex} or no choices available!");
                    // Reset state and continue
                    ResetDialogResponseState();
                    ContinueToNextBlock();
                    return;
                }
                
                var selectedChoice = currentBlock.Dialog.choices[selectedChoiceIndex];
                
                if (selectedChoice != null && selectedChoice.dialogResponses != null)
                {
                    int nextResponseIndex = currentChoiceResponseIndex + 1;
                    Debug.Log($"[PROGRESSION] Next response index would be: {nextResponseIndex} (total responses: {selectedChoice.dialogResponses.Length})");
                    
                    // Check if there are more responses to show
                    if (nextResponseIndex < selectedChoice.dialogResponses.Length)
                    {
                        Debug.Log($"[PROGRESSION] Showing next dialog response: {nextResponseIndex + 1}/{selectedChoice.dialogResponses.Length}");
                        
                        // Ensure we're not in a transition state
                        if (!isInDialogTransition)
                        {
                            isInDialogTransition = true;
                            ShowDialogResponse(selectedChoice, nextResponseIndex);
                        }
                        else
                        {
                            Debug.LogWarning("[PROGRESSION] Already in transition, skipping duplicate response display");
                        }
                        return;
                    }
                    else
                    {
                        Debug.Log("[PROGRESSION] All dialog responses shown, continuing to next block");
                        // All responses shown, continue to next block
                        ResetDialogResponseState();
                    }
                }
                else
                {
                    Debug.Log("[PROGRESSION] No more responses or selectedChoice is null, continuing to next block");
                    // No more responses, continue to next block
                    ResetDialogResponseState();
                }
                
                ClearAll3DDialogs(); // Clear 3D dialogs when continuing
                
                // Only destroy dialog instances if next block is not a 2D dialog
                if (!IsNext2DDialog())
                {
                    DestroyDialogInstances();
                }
                
                Debug.Log("[PROGRESSION] Continuing to next block after responses...");
                ContinueToNextBlock();
                return;
            }
            
            // If current block is dialog and has no choices, continue
            if (currentBlock.Type == CoreGameBlock.CoreType.Dialog)
            {
                var dialog = currentBlock.Dialog;
                if (dialog.choices == null || dialog.choices.Length == 0)
                {
                    Debug.Log("[PROGRESSION] Current dialog has no choices, continuing to next block...");
                    ClearAll3DDialogs(); // Clear 3D dialogs when continuing
                    
                    // Only destroy dialog instances if next block is not a 2D dialog
                    if (!IsNext2DDialog())
                    {
                        DestroyDialogInstances();
                    }
                    
                    ContinueToNextBlock();
                }
                else
                {
                    Debug.Log($"[PROGRESSION] Current dialog has {dialog.choices.Length} choices - waiting for user selection");
                }
            }
            else
            {
                Debug.Log($"[PROGRESSION] Current block is not a dialog (type: {currentBlock.Type})");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PROGRESSION] Exception in HandleDialogProgression: {e.Message}");
            Debug.LogError($"[PROGRESSION] Stack trace: {e.StackTrace}");
            
            // Reset state to prevent getting stuck
            ResetDialogResponseState();
        }
        finally
        {
            Debug.Log("[PROGRESSION] === END HandleDialogProgression ===");
        }
    }
    
    /// <summary>
    /// Reset dialog response state to prevent getting stuck
    /// </summary>
    private void ResetDialogResponseState()
    {
        Debug.Log("[PROGRESSION] Resetting dialog response state");
        isShowingResponse = false;
        currentChoiceResponseIndex = -1;
        selectedChoiceIndex = -1;
        isInDialogTransition = false;
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
        Debug.Log("[SKIP-ANIM] Skipping text animation...");
        
        // Prevent multiple simultaneous skip attempts
        if (!isTextAnimating)
        {
            Debug.Log("[SKIP-ANIM] No text animation running, nothing to skip");
            return;
        }
        
        // Set flag immediately to prevent race conditions
        bool wasTextAnimating = isTextAnimating;
        isTextAnimating = false;
        
        try
        {
            // Stop audio immediately
            if (dialogAudioSource != null && dialogAudioSource.isPlaying)
            {
                dialogAudioSource.Stop();
                Debug.Log("[SKIP-ANIM] Dialog audio stopped due to skip.");
            }
            
            // Complete the tween immediately
            if (dialogTween != null)
            {
                LeanTween.cancel(gameObject, dialogTween.id);
                dialogTween = null;
                Debug.Log("[SKIP-ANIM] Dialog tween cancelled");
            }
            
            // Only complete text display if we were actually animating
            if (wasTextAnimating)
            {
                // Force complete the text display with enhanced safety
                CompleteCurrentTextDisplay();
                Debug.Log("[SKIP-ANIM] Text animation skipped and completed successfully.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SKIP-ANIM] Error skipping text animation: {e.Message}");
            Debug.LogError($"[SKIP-ANIM] Stack trace: {e.StackTrace}");
        }
        finally
        {
            // Ensure the flag is always set to false, even if there's an error
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
        Debug.Log($"[COMPLETE-TEXT] Completing 2D text animation with: '{textToDisplay}'");
        
        // CRITICAL FIX: Use the same safe methods instead of generic GetComponentInChildren
        // This prevents targeting the wrong component during spam
        if (dialogInstance != null)
        {
            bool textCompleted = false;
            
            // Method 1: Try cached DialogueText component first
            if (cachedTextComponents.ContainsKey("DialogueText"))
            {
                try
                {
                    TMP_Text textComponent = cachedTextComponents["DialogueText"];
                    if (textComponent != null && textComponent.gameObject != null)
                    {
                        // Validate this is actually the dialog text component
                        string componentPath = GetTransformPath(textComponent.transform);
                        if ((componentPath.ToLower().Contains("text") || componentPath.ToLower().Contains("dialogue")) && !componentPath.ToLower().Contains("name"))
                        {
                            textComponent.text = textToDisplay;
                            Debug.Log($"[COMPLETE-TEXT] ✓ Completed text via cached DialogueText component");
                            textCompleted = true;
                        }
                        else
                        {
                            Debug.LogWarning($"[COMPLETE-TEXT] Cached component validation failed: '{componentPath}' looks like name component");
                            cachedTextComponents.Remove("DialogueText");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[COMPLETE-TEXT] Error with cached component: {e.Message}");
                    cachedTextComponents.Remove("DialogueText");
                }
            }
            
            // Method 2: Try DialogPrefabController if caching failed
            if (!textCompleted)
            {
                DialogPrefabController controller = dialogInstance.GetComponent<DialogPrefabController>();
                if (controller != null && controller.dialogueText != null)
                {
                    controller.dialogueText.text = textToDisplay;
                    Debug.Log($"[COMPLETE-TEXT] ✓ Completed text via DialogPrefabController");
                    textCompleted = true;
                }
            }
            
            // Method 3: Direct search with strict validation
            if (!textCompleted)
            {
                Transform textTransform = dialogInstance.transform.Find("DialogueText");
                if (textTransform != null)
                {
                    TMP_Text textComponent = textTransform.GetComponent<TMP_Text>();
                    if (textComponent != null)
                    {
                        // CRITICAL: Validate this is the dialog text component, not name
                        string transformName = textTransform.name.ToLower();
                        string fullPath = GetTransformPath(textTransform).ToLower();
                        
                        bool isTextComponent = ((transformName.Contains("text") || transformName.Contains("dialogue")) && !transformName.Contains("name")) ||
                                              (fullPath.Contains("text") && !fullPath.Contains("name"));
                        
                        if (isTextComponent)
                        {
                            textComponent.text = textToDisplay;
                            Debug.Log($"[COMPLETE-TEXT] ✓ Completed text via direct search");
                            textCompleted = true;
                        }
                        else
                        {
                            Debug.LogError($"[COMPLETE-TEXT] Component validation failed: '{transformName}' at '{fullPath}' is not dialog text");
                        }
                    }
                }
            }
            
            if (!textCompleted)
            {
                Debug.LogError($"[COMPLETE-TEXT] ✗ FAILED to complete text display - no valid dialog text component found");
                // Fallback to inspector-assigned dialogText if available
                if (dialogText != null)
                {
                    dialogText.text = textToDisplay;
                    Debug.Log($"[COMPLETE-TEXT] ✓ Completed text via inspector-assigned dialogText fallback");
                }
            }
        }
        else
        {
            Debug.LogWarning("[COMPLETE-TEXT] Dialog instance is null, cannot complete text");
            // Fallback to inspector-assigned dialogText if available
            if (dialogText != null)
            {
                dialogText.text = textToDisplay;
                Debug.Log($"[COMPLETE-TEXT] ✓ Completed text via inspector-assigned dialogText fallback");
            }
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
