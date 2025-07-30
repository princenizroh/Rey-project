using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CoreGameManager : MonoBehaviour
{
    [Header("Core Game Settings")]
    public CoreGame coreGameData;
    
    [Header("Dialog Templates")]
    public GameObject npcDialogThemplate;
    public GameObject npcQuestionThemplate;
    
    [Header("Dialog Components")]
    public TMP_Text dialogText;
    
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
    public void StartCoreGame(string resourcePath)
    {
        // Load the CoreGame ScriptableObject from Resources
        CoreGame loadedCoreGame = Resources.Load<CoreGame>(resourcePath);
        
        if (loadedCoreGame == null)
        {
            Debug.LogError($"CoreGame file not found at path: {resourcePath}");
            return;
        }
        
        if (loadedCoreGame.coreBlock == null || loadedCoreGame.coreBlock.Length == 0)
        {
            Debug.LogError($"CoreGame at path '{resourcePath}' has no blocks!");
            return;
        }
        
        // Set the loaded data as current
        coreGameData = loadedCoreGame;
        currentBlockIndex = 0;
        
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

    #region Dialog Handling

    [Obsolete]
    private void Show3DDialog(CoreGameDialog dialog)
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
        
        // Get dialog text component from the existing or newly created dialog bar
        var dialogTextComponent = dialogInstance.GetComponentInChildren<TMP_Text>();
        if (dialogTextComponent != null)
        {
            AnimateDialogText(dialog.dialogEntry, dialogTextComponent, dialog.audioDialogEntry);
        }
        else if (dialogText != null)
        {
            AnimateDialogText(dialog.dialogEntry, dialogText, dialog.audioDialogEntry);
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
        
        var playerAnswerManager = FindObjectOfType<PlayerAnswerManager>();
        if (playerAnswerManager != null)
        {
            // Convert CoreGameDialogChoices to DialogChoice format
            DialogChoice[] dialogChoices = new DialogChoice[choices.Length];
            for (int i = 0; i < choices.Length; i++)
            {
                dialogChoices[i] = new DialogChoice
                {
                    playerChoice = choices[i].playerChoice,
                    npcResponse = choices[i].npcResponse
                };
            }
            
            playerAnswerManager.ShowChoices(dialogChoices, OnPlayerChoseResponse);
        }
    }

    [Obsolete]
    private void OnPlayerChoseResponse(int choiceIndex)
    {
        var currentBlock = coreGameData.coreBlock[currentBlockIndex];
        if (currentBlock.Dialog?.choices == null || choiceIndex >= currentBlock.Dialog.choices.Length)
            return;
        
        var selectedChoice = currentBlock.Dialog.choices[choiceIndex];
        
        // Show the NPC response based on dialog type
        if (currentBlock.Dialog.dialogType == CoreGameDialog.DialogType.ThreeD)
        {
            // Show 3D response
            Show3DResponse(currentBlock.Dialog, selectedChoice.npcResponse, selectedChoice.audioDialogResponse);
        }
        else
        {
            // Show 2D response
            var textComponent = dialogInstance?.GetComponentInChildren<TMP_Text>() ?? dialogText;
            if (textComponent != null)
            {
                AnimateDialogText(selectedChoice.npcResponse, textComponent, selectedChoice.audioDialogResponse);
            }
        }
        
        // Hide choices
        var playerAnswerManager = FindObjectOfType<PlayerAnswerManager>();
        playerAnswerManager?.HideChoices();
        
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

    #region UI Management

    [Obsolete]
    private GameObject SummonDialogBar()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return null;
        }
        
        GameObject instance = Instantiate(npcDialogThemplate, canvas.transform, false);
        if (instance == null) return null;
        
        instance.SetActive(true);
        
        // Setup positioning and animation
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(0, -rect.rect.height);
            
            // Animate up
            LeanTween.value(instance, rect.anchoredPosition.y, 0, 0.3f)
                .setEaseInOutBack()
                .setOnUpdate((float val) => {
                    Vector2 pos = rect.anchoredPosition;
                    pos.y = val;
                    rect.anchoredPosition = pos;
                });
        }
        
        return instance;
    }

    [Obsolete]
    private GameObject SummonQuestionBar()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return null;
        
        GameObject instance = Instantiate(npcQuestionThemplate, canvas.transform, false);
        if (instance == null) return null;
        
        instance.SetActive(true);
        
        // Setup positioning and animation
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, ((RectTransform)rect.parent).rect.height / 2 + rect.rect.height);
            
            // Animate down
            LeanTween.value(instance, rect.anchoredPosition.y, 0, 0.3f)
                .setEaseInOutBack()
                .setOnUpdate((float val) => {
                    Vector2 pos = rect.anchoredPosition;
                    pos.y = val;
                    rect.anchoredPosition = pos;
                });
        }
        
        return instance;
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
                    textToDisplay = ProcessSpecialPrefixes(dialog.choices[currentChoiceResponseIndex].npcResponse);
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
    }
    
    #endregion
}
