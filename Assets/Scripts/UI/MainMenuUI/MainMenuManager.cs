using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Button References")]
    public GameObject startButton;
    public GameObject continueButton;
    public GameObject optionsButton;
    public GameObject creditsButton;
    public GameObject quitButton;
    public GameObject backButton;
    public GameObject creditsSprite;
    public GameObject optionsSprite;
    public TMP_Text startGameButtonText;
    public TMP_Text resolution;
    public TMP_Text volume;
    public CoreGameSaves targetScriptableObject;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float stairDelay = 0.1f; // Delay between each button animation
    [SerializeField] private float hoverMoveDistance = 20f;
    [SerializeField] private float hoverDuration = 0.3f;
    [SerializeField] private float clickMovePercentage = 150f; // Percentage to move all buttons when clicked
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType hoverEaseType = LeanTweenType.easeOutQuad;
    
    [Header("Sprite Animation Settings")]
    [SerializeField] private float spriteAnimationDuration = 0.8f;
    [SerializeField] private float spriteDelayOffset = 0.3f;
    [SerializeField] private LeanTweenType spriteEnterEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType spriteExitEase = LeanTweenType.easeInBack;
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private bool useFadeAnimation = true;
    [SerializeField] private float scaleAnimationDuration = 0.6f;
    [SerializeField] private float fadeAnimationDuration = 0.5f;

    [Header("Save File Settings")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Settings Control")]
    [SerializeField] private int currentResolutionIndex = 0;
    [SerializeField] private int currentVolumeLevel = 60;
    [SerializeField] private int volumeMin = 0;
    [SerializeField] private int volumeMax = 100;
    [SerializeField] private int volumeStep = 5;
    
    // Available resolutions
    private Resolution[] availableResolutions;
    private string[] resolutionStrings;

    private GameObject[] buttons;
    private Vector3[] originalPositions;
    private bool isAnimating = false;
    private bool buttonsVisible = true;
    
    // Sprite animation states
    private bool creditsVisible = false;
    private bool optionsVisible = false;
    private Vector3 creditsOffScreenPos;
    private Vector3 optionsOffScreenPos;

    // Save file detection
    [System.Serializable]
    public class SaveData
    {
        public int day;
        public int mother_stress_level;
        
        public SaveData()
        {
            day = 0;
            mother_stress_level = 0;
        }
    }

    void Start()
    {
        InitializeButtons();
        InitializeSprites();
        SetupButtonEvents();
        InitializeSettings();
        CheckSaveFileAndUpdateContinueButton();
        // Start with buttons visible
        ShowButtons();
    }

    void InitializeButtons()
    {
        // Store all buttons in array for easy iteration (excluding back button)
        buttons = new GameObject[] { startButton, continueButton, optionsButton, creditsButton, quitButton };
        
        // Store original positions
        originalPositions = new Vector3[buttons.Length];
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                originalPositions[i] = buttons[i].transform.localPosition;
            }
        }

        // Initialize back button (hidden initially)
        if (backButton != null)
        {
            backButton.SetActive(false);
        }
    }

    void InitializeSprites()
    {
        // Initialize credits sprite
        if (creditsSprite != null)
        {
            creditsOffScreenPos = new Vector3(1000f, creditsSprite.transform.localPosition.y, creditsSprite.transform.localPosition.z);
            creditsSprite.transform.localPosition = creditsOffScreenPos;
            creditsSprite.SetActive(false);
            
            // Initialize scale and alpha if using animations
            if (useScaleAnimation)
            {
                creditsSprite.transform.localScale = Vector3.zero;
            }
            if (useFadeAnimation)
            {
                CanvasGroup creditsCanvasGroup = creditsSprite.GetComponent<CanvasGroup>();
                if (creditsCanvasGroup == null)
                {
                    creditsCanvasGroup = creditsSprite.AddComponent<CanvasGroup>();
                }
                creditsCanvasGroup.alpha = 0f;
            }
        }

        // Initialize options sprite
        if (optionsSprite != null)
        {
            optionsOffScreenPos = new Vector3(3000f, optionsSprite.transform.localPosition.y, optionsSprite.transform.localPosition.z);
            optionsSprite.transform.localPosition = optionsOffScreenPos;
            optionsSprite.SetActive(false);
            
            // Initialize scale and alpha if using animations
            if (useScaleAnimation)
            {
                optionsSprite.transform.localScale = Vector3.zero;
            }
            if (useFadeAnimation)
            {
                CanvasGroup optionsCanvasGroup = optionsSprite.GetComponent<CanvasGroup>();
                if (optionsCanvasGroup == null)
                {
                    optionsCanvasGroup = optionsSprite.AddComponent<CanvasGroup>();
                }
                optionsCanvasGroup.alpha = 0f;
            }
        }
    }

    void InitializeSettings()
    {
        // Initialize available resolutions
        InitializeResolutions();
        
        // Initialize volume
        UpdateVolumeDisplay();
        
        // Setup interactive controls
        SetupVolumeScrollWheel();
        SetupResolutionClickDetection();
        SetupVolumeClickDetection();
        
        LogDebug("Settings initialized with interactive controls");
    }

    void InitializeResolutions()
    {
        // Get all available resolutions
        availableResolutions = Screen.resolutions;
        resolutionStrings = new string[availableResolutions.Length];
        
        // Convert resolutions to strings
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution res = availableResolutions[i];
            resolutionStrings[i] = $"{res.width} x {res.height}";
        }
        
        // Find current resolution index
        Resolution currentRes = Screen.currentResolution;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i].width == currentRes.width && 
                availableResolutions[i].height == currentRes.height)
            {
                currentResolutionIndex = i;
                break;
            }
        }
        
        // Update resolution display
        UpdateResolutionDisplay();
        
        LogDebug($"Found {availableResolutions.Length} available resolutions, current: {resolutionStrings[currentResolutionIndex]}");
    }

    void SetupVolumeScrollWheel()
    {
        if (volume != null)
        {
            // We need to add EventTrigger to the volume text's GameObject, not the text component itself
            GameObject volumeObject = volume.gameObject;
            
            // Add EventTrigger component if it doesn't exist
            EventTrigger volumeEventTrigger = volumeObject.GetComponent<EventTrigger>();
            if (volumeEventTrigger == null)
            {
                volumeEventTrigger = volumeObject.AddComponent<EventTrigger>();
            }

            // Add scroll event
            EventTrigger.Entry scrollEntry = new EventTrigger.Entry();
            scrollEntry.eventID = EventTriggerType.Scroll;
            scrollEntry.callback.AddListener((data) => {
                PointerEventData pointerData = data as PointerEventData;
                if (pointerData != null)
                {
                    OnVolumeScroll(pointerData.scrollDelta.y);
                }
            });
            volumeEventTrigger.triggers.Add(scrollEntry);
            
            LogDebug("Volume scroll wheel setup complete");
        }
    }

    void SetupButtonEvents()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                Button buttonComponent = buttons[i].GetComponent<Button>();
                if (buttonComponent != null)
                {
                    // Remove any existing listeners
                    buttonComponent.onClick.RemoveAllListeners();
                    
                    // Add specific click events based on button type
                    if (buttons[i] == startButton)
                    {
                        buttonComponent.onClick.AddListener(() => OnStartButtonClick());
                    }
                    else if (buttons[i] == continueButton)
                    {
                        buttonComponent.onClick.AddListener(() => OnContinueButtonClick());
                    }
                    else if (buttons[i] == creditsButton)
                    {
                        buttonComponent.onClick.AddListener(() => ShowCredits());
                    }
                    else if (buttons[i] == optionsButton)
                    {
                        buttonComponent.onClick.AddListener(() => ShowOptions());
                    }
                    else
                    {
                        // For other buttons, use the generic submenu behavior
                        buttonComponent.onClick.AddListener(() => MoveToSubmenu());
                    }
                }

                // Add hover events
                EventTrigger eventTrigger = buttons[i].GetComponent<EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = buttons[i].AddComponent<EventTrigger>();
                }

                // Mouse enter event
                EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
                pointerEnter.eventID = EventTriggerType.PointerEnter;
                int buttonIndex = i; // Capture the index for the lambda
                pointerEnter.callback.AddListener((data) => { OnButtonHover(buttonIndex, true); });
                eventTrigger.triggers.Add(pointerEnter);

                // Mouse exit event
                EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((data) => { OnButtonHover(buttonIndex, false); });
                eventTrigger.triggers.Add(pointerExit);
            }
        }

        // Setup back button event
        if (backButton != null)
        {
            Button backButtonComponent = backButton.GetComponent<Button>();
            if (backButtonComponent != null)
            {
                backButtonComponent.onClick.RemoveAllListeners();
                backButtonComponent.onClick.AddListener(() => ReturnToMainMenu());
            }

            // Add hover events for back button
            EventTrigger backEventTrigger = backButton.GetComponent<EventTrigger>();
            if (backEventTrigger == null)
            {
                backEventTrigger = backButton.AddComponent<EventTrigger>();
            }

            // Mouse enter event for back button
            EventTrigger.Entry backPointerEnter = new EventTrigger.Entry();
            backPointerEnter.eventID = EventTriggerType.PointerEnter;
            backPointerEnter.callback.AddListener((data) => { OnBackButtonHover(true); });
            backEventTrigger.triggers.Add(backPointerEnter);

            // Mouse exit event for back button
            EventTrigger.Entry backPointerExit = new EventTrigger.Entry();
            backPointerExit.eventID = EventTriggerType.PointerExit;
            backPointerExit.callback.AddListener((data) => { OnBackButtonHover(false); });
            backEventTrigger.triggers.Add(backPointerExit);
        }
    }

    public void ShowButtons()
    {
        if (isAnimating || buttonsVisible) return;
        
        isAnimating = true;
        buttonsVisible = true;

        // Counter to track completed animations
        int completedAnimations = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                // Set initial position (off-screen to the left)
                Vector3 startPos = originalPositions[i] + Vector3.left * 1000f;
                buttons[i].transform.localPosition = startPos;
                buttons[i].SetActive(true);

                // Animate to original position with stair delay
                float delay = i * stairDelay;
                
                LeanTween.moveLocal(buttons[i], originalPositions[i], animationDuration)
                    .setDelay(delay)
                    .setEase(easeType)
                    .setOnComplete(() => {
                        completedAnimations++;
                        if (completedAnimations >= buttons.Length)
                        {
                            isAnimating = false;
                        }
                    });

                // Add a slight scale animation for extra polish
                buttons[i].transform.localScale = Vector3.zero;
                LeanTween.scale(buttons[i], Vector3.one, animationDuration * 0.8f)
                    .setDelay(delay + 0.1f)
                    .setEase(LeanTweenType.easeOutBack);
            }
            else
            {
                // Count null buttons as completed immediately
                completedAnimations++;
            }
        }

        // If all buttons are null, complete immediately
        if (completedAnimations >= buttons.Length)
        {
            isAnimating = false;
        }
    }

    public void HideButtons()
    {
        if (isAnimating || !buttonsVisible) return;
        
        isAnimating = true;
        buttonsVisible = false;

        // Counter to track completed animations
        int completedAnimations = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                // Calculate delay - start button exits first, then cascade
                float delay = i * stairDelay;
                Vector3 endPos = originalPositions[i] + Vector3.left * 1000f;

                // Move animation
                LeanTween.moveLocal(buttons[i], endPos, animationDuration)
                    .setDelay(delay)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() => {
                        buttons[i].SetActive(false);
                        completedAnimations++;
                        if (completedAnimations >= buttons.Length)
                        {
                            isAnimating = false;
                            // Automatically show buttons again after a short delay
                            StartCoroutine(RestartAnimation());
                        }
                    });

                // Scale down animation
                LeanTween.scale(buttons[i], Vector3.zero, animationDuration * 0.8f)
                    .setDelay(delay + 0.1f)
                    .setEase(LeanTweenType.easeInBack);
            }
            else
            {
                // Count null buttons as completed immediately
                completedAnimations++;
            }
        }

        // If all buttons are null, complete immediately
        if (completedAnimations >= buttons.Length)
        {
            isAnimating = false;
            StartCoroutine(RestartAnimation());
        }
    }

    private IEnumerator RestartAnimation()
    {
        yield return new WaitForSeconds(1f);
        ShowButtons();
    }

    void OnButtonHover(int buttonIndex, bool isHovering)
    {
        if (isAnimating || buttonIndex >= buttons.Length || buttons[buttonIndex] == null || !buttonsVisible) return;

        // Cancel any existing hover animation for this button
        LeanTween.cancel(buttons[buttonIndex]);

        // Calculate target position - only change X position
        Vector3 currentPos = buttons[buttonIndex].transform.localPosition;
        Vector3 targetPos = isHovering ? 
            new Vector3(originalPositions[buttonIndex].x + hoverMoveDistance, currentPos.y, currentPos.z) : 
            new Vector3(originalPositions[buttonIndex].x, currentPos.y, currentPos.z);

        LeanTween.moveLocal(buttons[buttonIndex], targetPos, hoverDuration)
            .setEase(hoverEaseType);
    }

    void OnBackButtonHover(bool isHovering)
    {
        if (isAnimating || backButton == null) return;

        // Cancel any existing hover animation for back button
        LeanTween.cancel(backButton);

        // Get current position and calculate target
        Vector3 currentPos = backButton.transform.localPosition;
        Vector3 targetPos = isHovering ? 
            new Vector3(currentPos.x + hoverMoveDistance, currentPos.y, currentPos.z) : 
            new Vector3(50f, currentPos.y, currentPos.z); // Return to x=50

        LeanTween.moveLocal(backButton, targetPos, hoverDuration)
            .setEase(hoverEaseType);
    }

    public void ShowCredits()
    {
        if (isAnimating || creditsVisible) return;
        
        MoveToSubmenu(); // First move main buttons and show back button
        
        // Show credits sprite animation
        if (creditsSprite != null)
        {
            creditsVisible = true;
            creditsSprite.SetActive(true);
            
            // Start from right side (x = 1000) and animate to x = 450
            Vector3 startPos = new Vector3(1000f, creditsSprite.transform.localPosition.y, creditsSprite.transform.localPosition.z);
            Vector3 targetPos = new Vector3(450f, 0, 0);
            
            creditsSprite.transform.localPosition = startPos;
            
            // Position animation
            LeanTween.moveLocal(creditsSprite, targetPos, spriteAnimationDuration)
                .setDelay(spriteDelayOffset) // Delay to let main menu buttons move first
                .setEase(spriteEnterEase);
            
            // Scale animation
            if (useScaleAnimation)
            {
                creditsSprite.transform.localScale = Vector3.zero;
                LeanTween.scale(creditsSprite, Vector3.one, scaleAnimationDuration)
                    .setDelay(spriteDelayOffset + 0.1f)
                    .setEase(LeanTweenType.easeOutBack);
            }
            
            // Fade animation
            if (useFadeAnimation)
            {
                CanvasGroup creditsCanvasGroup = creditsSprite.GetComponent<CanvasGroup>();
                if (creditsCanvasGroup != null)
                {
                    creditsCanvasGroup.alpha = 0f;
                    LeanTween.alphaCanvas(creditsCanvasGroup, 1f, fadeAnimationDuration)
                        .setDelay(spriteDelayOffset + 0.2f)
                        .setEase(LeanTweenType.easeOutQuad);
                }
            }
        }
    }

    public void ShowOptions()
    {
        if (isAnimating || optionsVisible) return;
        
        MoveToSubmenu(); // First move main buttons and show back button
        
        // Show options sprite animation
        if (optionsSprite != null)
        {
            optionsVisible = true;
            optionsSprite.SetActive(true);
            
            // Start from right side (x = 3000) and animate to x = 540
            Vector3 startPos = new Vector3(3000f, optionsSprite.transform.localPosition.y, optionsSprite.transform.localPosition.z);
            Vector3 targetPos = new Vector3(540f, optionsSprite.transform.localPosition.y, optionsSprite.transform.localPosition.z);
            
            optionsSprite.transform.localPosition = startPos;
            
            // Position animation
            LeanTween.moveLocal(optionsSprite, targetPos, spriteAnimationDuration)
                .setDelay(spriteDelayOffset) // Delay to let main menu buttons move first
                .setEase(spriteEnterEase);
            
            // Scale animation
            if (useScaleAnimation)
            {
                optionsSprite.transform.localScale = Vector3.zero;
                LeanTween.scale(optionsSprite, Vector3.one, scaleAnimationDuration)
                    .setDelay(spriteDelayOffset + 0.1f)
                    .setEase(LeanTweenType.easeOutBack);
            }
            
            // Fade animation
            if (useFadeAnimation)
            {
                CanvasGroup optionsCanvasGroup = optionsSprite.GetComponent<CanvasGroup>();
                if (optionsCanvasGroup != null)
                {
                    optionsCanvasGroup.alpha = 0f;
                    LeanTween.alphaCanvas(optionsCanvasGroup, 1f, fadeAnimationDuration)
                        .setDelay(spriteDelayOffset + 0.2f)
                        .setEase(LeanTweenType.easeOutQuad);
                }
            }
        }
    }

    public void HideCredits()
    {
        if (!creditsVisible || creditsSprite == null) return;
        
        creditsVisible = false;
        
        // Animate credits sprite back to x = 1000 (right side)
        Vector3 exitPos = new Vector3(1000f, creditsSprite.transform.localPosition.y, creditsSprite.transform.localPosition.z);
        
        // Position animation
        LeanTween.moveLocal(creditsSprite, exitPos, spriteAnimationDuration)
            .setEase(spriteExitEase)
            .setOnComplete(() => {
                creditsSprite.SetActive(false);
                // Reset transforms for next time
                if (useScaleAnimation)
                {
                    creditsSprite.transform.localScale = Vector3.zero;
                }
                if (useFadeAnimation)
                {
                    CanvasGroup creditsCanvasGroup = creditsSprite.GetComponent<CanvasGroup>();
                    if (creditsCanvasGroup != null)
                    {
                        creditsCanvasGroup.alpha = 0f;
                    }
                }
            });
        
        // Scale animation
        if (useScaleAnimation)
        {
            LeanTween.scale(creditsSprite, Vector3.zero, scaleAnimationDuration * 0.8f)
                .setDelay(0.1f)
                .setEase(LeanTweenType.easeInBack);
        }
        
        // Fade animation
        if (useFadeAnimation)
        {
            CanvasGroup creditsCanvasGroup = creditsSprite.GetComponent<CanvasGroup>();
            if (creditsCanvasGroup != null)
            {
                LeanTween.alphaCanvas(creditsCanvasGroup, 0f, fadeAnimationDuration * 0.7f)
                    .setEase(LeanTweenType.easeInQuad);
            }
        }
    }

    public void HideOptions()
    {
        if (!optionsVisible || optionsSprite == null) return;
        
        optionsVisible = false;
        
        // Animate options sprite back to x = 3000 (right side)
        Vector3 exitPos = new Vector3(3000f, optionsSprite.transform.localPosition.y, optionsSprite.transform.localPosition.z);
        
        // Position animation
        LeanTween.moveLocal(optionsSprite, exitPos, spriteAnimationDuration)
            .setEase(spriteExitEase)
            .setOnComplete(() => {
                optionsSprite.SetActive(false);
                // Reset transforms for next time
                if (useScaleAnimation)
                {
                    optionsSprite.transform.localScale = Vector3.zero;
                }
                if (useFadeAnimation)
                {
                    CanvasGroup optionsCanvasGroup = optionsSprite.GetComponent<CanvasGroup>();
                    if (optionsCanvasGroup != null)
                    {
                        optionsCanvasGroup.alpha = 0f;
                    }
                }
            });
        
        // Scale animation
        if (useScaleAnimation)
        {
            LeanTween.scale(optionsSprite, Vector3.zero, scaleAnimationDuration * 0.8f)
                .setDelay(0.1f)
                .setEase(LeanTweenType.easeInBack);
        }
        
        // Fade animation
        if (useFadeAnimation)
        {
            CanvasGroup optionsCanvasGroup = optionsSprite.GetComponent<CanvasGroup>();
            if (optionsCanvasGroup != null)
            {
                LeanTween.alphaCanvas(optionsCanvasGroup, 0f, fadeAnimationDuration * 0.7f)
                    .setEase(LeanTweenType.easeInQuad);
            }
        }
    }

    public void MoveToSubmenu()
    {
        if (isAnimating) return;
        
        isAnimating = true;
        buttonsVisible = false; // Set buttons as not visible in main menu state

        // Move all main buttons to x -900
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                Vector3 targetPos = new Vector3(-900f, originalPositions[i].y, originalPositions[i].z);

                LeanTween.moveLocal(buttons[i], targetPos, animationDuration)
                    .setEase(easeType);
            }
        }

        // Show and animate back button from left side to x 50
        if (backButton != null)
        {
            backButton.SetActive(true);
            
            // Start back button off-screen to the left
            Vector3 backStartPos = new Vector3(-1000f, backButton.transform.localPosition.y, backButton.transform.localPosition.z);
            backButton.transform.localPosition = backStartPos;

            // Animate back button to x=50
            Vector3 backTargetPos = new Vector3(50f, backButton.transform.localPosition.y, backButton.transform.localPosition.z);
            LeanTween.moveLocal(backButton, backTargetPos, animationDuration)
                .setEase(easeType)
                .setDelay(0.2f) // Small delay to let main buttons start moving first
                .setOnComplete(() => {
                    isAnimating = false;
                });
        }
        else
        {
            isAnimating = false;
        }
    }

    public void ReturnToMainMenu()
    {
        if (isAnimating) return;
        
        isAnimating = true;

        // Hide any visible sprites first
        if (creditsVisible)
        {
            HideCredits();
        }
        if (optionsVisible)
        {
            HideOptions();
        }

        // Hide back button by moving it off-screen to the left
        if (backButton != null)
        {
            Vector3 backExitPos = new Vector3(-1000f, backButton.transform.localPosition.y, backButton.transform.localPosition.z);
            LeanTween.moveLocal(backButton, backExitPos, animationDuration)
                .setEase(easeType)
                .setOnComplete(() => {
                    backButton.SetActive(false);
                });
        }

        // Counter to track completed animations
        int completedAnimations = 0;

        // Return all main buttons to their original positions
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                LeanTween.moveLocal(buttons[i], originalPositions[i], animationDuration)
                    .setEase(easeType)
                    .setDelay(0.1f) // Small delay to let back button start exiting first
                    .setOnComplete(() => {
                        completedAnimations++;
                        if (completedAnimations >= buttons.Length)
                        {
                            isAnimating = false;
                            buttonsVisible = true; // Reset the buttons visible state
                        }
                    });
            }
            else
            {
                // Count null buttons as completed immediately
                completedAnimations++;
            }
        }

        // If all buttons are null, complete immediately
        if (completedAnimations >= buttons.Length)
        {
            isAnimating = false;
            buttonsVisible = true;
        }
    }

    // Public methods to manually trigger animations
    public void TriggerShowAnimation()
    {
        ShowButtons();
    }

    public void TriggerMoveToSubmenu()
    {
        MoveToSubmenu();
    }

    public void TriggerReturnToMainMenu()
    {
        ReturnToMainMenu();
    }

    public void TriggerHideAnimation()
    {
        HideButtons();
    }

    // Method to reset all animations
    public void ResetAnimations()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                LeanTween.cancel(buttons[i]);
                buttons[i].transform.localPosition = originalPositions[i];
                buttons[i].transform.localScale = Vector3.one;
                buttons[i].SetActive(true);
            }
        }
        
        // Reset sprites
        if (creditsSprite != null)
        {
            LeanTween.cancel(creditsSprite);
            creditsSprite.transform.localPosition = creditsOffScreenPos;
            creditsSprite.SetActive(false);
            creditsVisible = false;
            
            // Reset scale and alpha
            if (useScaleAnimation)
            {
                creditsSprite.transform.localScale = Vector3.zero;
            }
            if (useFadeAnimation)
            {
                CanvasGroup creditsCanvasGroup = creditsSprite.GetComponent<CanvasGroup>();
                if (creditsCanvasGroup != null)
                {
                    creditsCanvasGroup.alpha = 0f;
                }
            }
        }
        
        if (optionsSprite != null)
        {
            LeanTween.cancel(optionsSprite);
            optionsSprite.transform.localPosition = optionsOffScreenPos;
            optionsSprite.SetActive(false);
            optionsVisible = false;
            
            // Reset scale and alpha
            if (useScaleAnimation)
            {
                optionsSprite.transform.localScale = Vector3.zero;
            }
            if (useFadeAnimation)
            {
                CanvasGroup optionsCanvasGroup = optionsSprite.GetComponent<CanvasGroup>();
                if (optionsCanvasGroup != null)
                {
                    optionsCanvasGroup.alpha = 0f;
                }
            }
        }
        
        if (backButton != null)
        {
            LeanTween.cancel(backButton);
            backButton.SetActive(false);
        }
        
        isAnimating = false;
        buttonsVisible = true;
    }

    /// <summary>
    /// Check for save file and update continue button state accordingly
    /// </summary>
    private void CheckSaveFileAndUpdateContinueButton()
    {
        if (continueButton == null)
        {
            LogDebug("Continue button is not assigned!");
            return;
        }

        try
        {
            string saveFilePath = GetSaveFilePath();
            bool hasSaveFile = false;
            
            if (File.Exists(saveFilePath))
            {
                LogDebug($"Save file found at: {saveFilePath}");
                
                // Read and parse the JSON file
                string jsonContent = File.ReadAllText(saveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
                
                if (saveData != null)
                {
                    LogDebug($"Save data loaded - Day: {saveData.day}, Mother Stress: {saveData.mother_stress_level}");
                    
                    // Check if save data has meaningful progress (day > 0 or mother_stress_level > 0)
                    if (saveData.day > 0 || saveData.mother_stress_level > 0)
                    {
                        hasSaveFile = true;
                        LogDebug("Valid save data found - enabling continue button");
                    }
                    else
                    {
                        LogDebug("Save data has no progress - disabling continue button");
                    }
                }
                else
                {
                    LogDebug("Failed to parse save data - disabling continue button");
                }
            }
            else
            {
                LogDebug($"No save file found at: {saveFilePath} - disabling continue button");
            }
            
            // Update continue button state
            UpdateContinueButtonState(hasSaveFile);
        }
        catch (System.Exception e)
        {
            LogDebug($"Error checking save file: {e.Message} - disabling continue button");
            UpdateContinueButtonState(false);
        }
    }

    /// <summary>
    /// Update the continue button's interactivity and visual state
    /// </summary>
    private void UpdateContinueButtonState(bool isEnabled)
    {
        if (continueButton == null) return;

        // Get button component
        Button buttonComponent = continueButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.interactable = isEnabled;
        }

        // Get CanvasGroup for opacity control
        CanvasGroup canvasGroup = continueButton.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = continueButton.AddComponent<CanvasGroup>();
        }

        // Set opacity based on state
        canvasGroup.alpha = isEnabled ? 1f : 0.5f;

        LogDebug($"Continue button state updated - Enabled: {isEnabled}, Opacity: {canvasGroup.alpha}");
    }

    /// <summary>
    /// Get the full path to the save file in MyGames/Rey/saves
    /// </summary>
    private string GetSaveFilePath()
    {
        // Get the user's Documents folder
        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        
        // Build the path: Documents/My Games/Rey/saves/save_data.json
        string saveFilePath = Path.Combine(documentsPath, "My Games", "Rey", "saves", "save_data.json");
        
        return saveFilePath;
    }

    /// <summary>
    /// Public method to manually refresh the continue button state
    /// </summary>
    [ContextMenu("Refresh Continue Button State")]
    public void RefreshContinueButtonState()
    {
        CheckSaveFileAndUpdateContinueButton();
    }

    /// <summary>
    /// Create a test save file for testing purposes
    /// </summary>
    [ContextMenu("Create Test Save File")]
    public void CreateTestSaveFile()
    {
        try
        {
            string saveFilePath = GetSaveFilePath();
            string directoryPath = Path.GetDirectoryName(saveFilePath);
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                LogDebug($"Created save directory: {directoryPath}");
            }
            
            // Create test save data
            SaveData testSave = new SaveData();
            testSave.day = 3;
            testSave.mother_stress_level = 5;
            
            string jsonContent = JsonUtility.ToJson(testSave, true);
            File.WriteAllText(saveFilePath, jsonContent);
            
            LogDebug($"Test save file created at: {saveFilePath}");
            LogDebug($"Content: {jsonContent}");
            
            // Refresh the continue button state
            CheckSaveFileAndUpdateContinueButton();
        }
        catch (System.Exception e)
        {
            LogDebug($"Error creating test save file: {e.Message}");
        }
    }

    /// <summary>
    /// Create an empty save file for testing
    /// </summary>
    [ContextMenu("Create Empty Save File")]
    public void CreateEmptySaveFile()
    {
        try
        {
            string saveFilePath = GetSaveFilePath();
            string directoryPath = Path.GetDirectoryName(saveFilePath);
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                LogDebug($"Created save directory: {directoryPath}");
            }
            
            // Create empty save data (day=0, mother_stress_level=0)
            SaveData emptySave = new SaveData();
            
            string jsonContent = JsonUtility.ToJson(emptySave, true);
            File.WriteAllText(saveFilePath, jsonContent);
            
            LogDebug($"Empty save file created at: {saveFilePath}");
            LogDebug($"Content: {jsonContent}");
            
            // Refresh the continue button state
            CheckSaveFileAndUpdateContinueButton();
        }
        catch (System.Exception e)
        {
            LogDebug($"Error creating empty save file: {e.Message}");
        }
    }

    /// <summary>
    /// Delete the save file for testing
    /// </summary>
    [ContextMenu("Delete Save File")]
    public void DeleteSaveFile()
    {
        try
        {
            string saveFilePath = GetSaveFilePath();
            
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                LogDebug($"Save file deleted: {saveFilePath}");
                
                // Refresh the continue button state
                CheckSaveFileAndUpdateContinueButton();
            }
            else
            {
                LogDebug($"No save file to delete at: {saveFilePath}");
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"Error deleting save file: {e.Message}");
        }
    }

    /// <summary>
    /// Check current save file status and log information
    /// </summary>
    [ContextMenu("Check Save File Status")]
    public void CheckSaveFileStatus()
    {
        string saveFilePath = GetSaveFilePath();
        
        LogDebug($"Save file path: {saveFilePath}");
        
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonContent = File.ReadAllText(saveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
                
                LogDebug("✓ Save file exists and is readable");
                LogDebug($"Content: {jsonContent}");
                
                if (saveData != null)
                {
                    LogDebug($"Parsed data - Day: {saveData.day}, Mother Stress: {saveData.mother_stress_level}");
                    
                    if (saveData.day > 0 || saveData.mother_stress_level > 0)
                    {
                        LogDebug("→ Would show 'Continue' button");
                    }
                    else
                    {
                        LogDebug("→ Would show 'Start Game' button");
                    }
                }
                else
                {
                    LogDebug("✗ Failed to parse save data");
                }
            }
            catch (System.Exception e)
            {
                LogDebug($"✗ Error reading save file: {e.Message}");
            }
        }
        else
        {
            LogDebug("✗ Save file does not exist");
            LogDebug("→ Would show 'Start Game' button");
        }
    }

    /// <summary>
    /// Helper method for debug logging
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MainMenuManager] {message}");
        }
    }

    #region Settings Control Methods

    /// <summary>
    /// Update the resolution display text
    /// </summary>
    private void UpdateResolutionDisplay()
    {
        if (resolution != null && resolutionStrings != null && currentResolutionIndex >= 0 && currentResolutionIndex < resolutionStrings.Length)
        {
            resolution.text = $"< {resolutionStrings[currentResolutionIndex]} >";
        }
    }

    /// <summary>
    /// Update the volume display text
    /// </summary>
    private void UpdateVolumeDisplay()
    {
        if (volume != null)
        {
            volume.text = $"< {currentVolumeLevel} >";
        }
    }

    /// <summary>
    /// Handle volume scroll wheel input
    /// </summary>
    private void OnVolumeScroll(float scrollDelta)
    {
        if (scrollDelta > 0)
        {
            IncreaseVolume();
        }
        else if (scrollDelta < 0)
        {
            DecreaseVolume();
        }
    }

    /// <summary>
    /// Handle resolution scroll wheel input
    /// </summary>
    private void OnResolutionScroll(float scrollDelta)
    {
        if (scrollDelta > 0)
        {
            NextResolution();
        }
        else if (scrollDelta < 0)
        {
            PreviousResolution();
        }
    }

    /// <summary>
    /// Cycle to the next resolution
    /// </summary>
    public void NextResolution()
    {
        if (availableResolutions != null && availableResolutions.Length > 0)
        {
            currentResolutionIndex = (currentResolutionIndex + 1) % availableResolutions.Length;
            UpdateResolutionDisplay();
            ApplyResolution();
            LogDebug($"Resolution changed to: {resolutionStrings[currentResolutionIndex]}");
        }
    }

    /// <summary>
    /// Cycle to the previous resolution
    /// </summary>
    public void PreviousResolution()
    {
        if (availableResolutions != null && availableResolutions.Length > 0)
        {
            currentResolutionIndex--;
            if (currentResolutionIndex < 0)
            {
                currentResolutionIndex = availableResolutions.Length - 1;
            }
            UpdateResolutionDisplay();
            ApplyResolution();
            LogDebug($"Resolution changed to: {resolutionStrings[currentResolutionIndex]}");
        }
    }

    /// <summary>
    /// Apply the currently selected resolution
    /// </summary>
    private void ApplyResolution()
    {
        if (availableResolutions != null && currentResolutionIndex >= 0 && currentResolutionIndex < availableResolutions.Length)
        {
            Resolution selectedResolution = availableResolutions[currentResolutionIndex];
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
            LogDebug($"Applied resolution: {selectedResolution.width}x{selectedResolution.height}");
        }
    }

    /// <summary>
    /// Increase volume level
    /// </summary>
    public void IncreaseVolume()
    {
        currentVolumeLevel = Mathf.Clamp(currentVolumeLevel + volumeStep, volumeMin, volumeMax);
        UpdateVolumeDisplay();
        ApplyVolume();
        LogDebug($"Volume increased to: {currentVolumeLevel}");
    }

    /// <summary>
    /// Decrease volume level
    /// </summary>
    public void DecreaseVolume()
    {
        currentVolumeLevel = Mathf.Clamp(currentVolumeLevel - volumeStep, volumeMin, volumeMax);
        UpdateVolumeDisplay();
        ApplyVolume();
        LogDebug($"Volume decreased to: {currentVolumeLevel}");
    }

    /// <summary>
    /// Apply the current volume level to AudioListener
    /// </summary>
    private void ApplyVolume()
    {
        // Convert 0-100 range to 0-1 range for AudioListener
        float normalizedVolume = currentVolumeLevel / 100f;
        AudioListener.volume = normalizedVolume;
        LogDebug($"Applied volume: {normalizedVolume:F2} (AudioListener.volume)");
    }

    /// <summary>
    /// Add click detection and scroll wheel support to resolution text
    /// </summary>
    public void SetupResolutionClickDetection()
    {
        if (resolution != null)
        {
            GameObject resolutionObject = resolution.gameObject;
            
            // Add EventTrigger if it doesn't exist
            EventTrigger resolutionEventTrigger = resolutionObject.GetComponent<EventTrigger>();
            if (resolutionEventTrigger == null)
            {
                resolutionEventTrigger = resolutionObject.AddComponent<EventTrigger>();
            }

            // Add click event
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => {
                PointerEventData pointerData = data as PointerEventData;
                if (pointerData != null)
                {
                    OnResolutionClick(pointerData);
                }
            });
            resolutionEventTrigger.triggers.Add(clickEntry);

            // Add scroll event for resolution
            EventTrigger.Entry scrollEntry = new EventTrigger.Entry();
            scrollEntry.eventID = EventTriggerType.Scroll;
            scrollEntry.callback.AddListener((data) => {
                PointerEventData pointerData = data as PointerEventData;
                if (pointerData != null)
                {
                    OnResolutionScroll(pointerData.scrollDelta.y);
                }
            });
            resolutionEventTrigger.triggers.Add(scrollEntry);
            
            LogDebug("Resolution click detection and scroll wheel setup complete");
        }
    }

    /// <summary>
    /// Add click detection to volume text for arrow simulation
    /// </summary>
    public void SetupVolumeClickDetection()
    {
        if (volume != null)
        {
            GameObject volumeObject = volume.gameObject;
            
            // Add EventTrigger if it doesn't exist
            EventTrigger volumeEventTrigger = volumeObject.GetComponent<EventTrigger>();
            if (volumeEventTrigger == null)
            {
                volumeEventTrigger = volumeObject.AddComponent<EventTrigger>();
            }

            // Add click event
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => {
                PointerEventData pointerData = data as PointerEventData;
                if (pointerData != null)
                {
                    OnVolumeClick(pointerData);
                }
            });
            volumeEventTrigger.triggers.Add(clickEntry);
            
            LogDebug("Volume click detection setup complete");
        }
    }

    /// <summary>
    /// Handle resolution text clicks to simulate arrow buttons
    /// </summary>
    private void OnResolutionClick(PointerEventData pointerData)
    {
        if (resolution == null) return;

        // Get the local position of the click relative to the text
        Vector2 localPoint;
        RectTransform rectTransform = resolution.rectTransform;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerData.position, pointerData.pressEventCamera, out localPoint))
        {
            // Calculate if click was on left half or right half of the entire text
            float normalizedX = (localPoint.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width;
            
            if (normalizedX < 0.5f) // Left half = Previous
            {
                PreviousResolution();
            }
            else // Right half = Next
            {
                NextResolution();
            }
            
            LogDebug($"Resolution click at normalized X: {normalizedX:F2}");
        }
    }

    /// <summary>
    /// Handle volume text clicks to simulate arrow buttons
    /// </summary>
    private void OnVolumeClick(PointerEventData pointerData)
    {
        if (volume == null) return;

        // Get the local position of the click relative to the text
        Vector2 localPoint;
        RectTransform rectTransform = volume.rectTransform;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerData.position, pointerData.pressEventCamera, out localPoint))
        {
            // Calculate if click was on left half or right half of the entire text
            float normalizedX = (localPoint.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width;
            
            if (normalizedX < 0.5f) // Left half = Decrease
            {
                DecreaseVolume();
            }
            else // Right half = Increase
            {
                IncreaseVolume();
            }
            
            LogDebug($"Volume click at normalized X: {normalizedX:F2}");
        }
    }

    /// <summary>
    /// Setup all interactive controls for settings
    /// </summary>
    [ContextMenu("Setup Settings Controls")]
    public void SetupSettingsControls()
    {
        SetupResolutionClickDetection();
        SetupVolumeClickDetection();
        LogDebug("All settings controls setup complete");
    }

    /// <summary>
    /// Reset settings to default values
    /// </summary>
    [ContextMenu("Reset Settings to Default")]
    public void ResetSettingsToDefault()
    {
        // Reset volume to default
        currentVolumeLevel = 60;
        UpdateVolumeDisplay();
        ApplyVolume();
        
        // Reset resolution to current screen resolution
        InitializeResolutions();
        
        LogDebug("Settings reset to default values");
    }

    #endregion

    public void onExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Handle start button click - resets ScriptableObject data if save file exists
    /// </summary>
    public void OnStartButtonClick()
    {
        LogDebug("Start button clicked");
        
        // Check if save data exists
        string saveFilePath = GetSaveFilePath();
        bool hasSaveData = false;
        
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonContent = File.ReadAllText(saveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
                
                if (saveData != null && (saveData.day > 0 || saveData.mother_stress_level > 0))
                {
                    hasSaveData = true;
                    LogDebug("Save data found, will reset ScriptableObject to start fresh");
                }
            }
            catch (System.Exception e)
            {
                LogDebug($"Error reading save data: {e.Message}");
            }
        }
        
        // Reset ScriptableObject data if save data exists (to start fresh)
        if (hasSaveData && targetScriptableObject != null)
        {
            ResetScriptableObjectData();
        }
        else if (targetScriptableObject != null)
        {
            LogDebug("No save data found or already at default values");
        }
        else
        {
            LogDebug("Warning: targetScriptableObject is not assigned!");
        }
        
        // Continue with normal start game flow
        StartGame();
    }

    /// <summary>
    /// Reset the CoreGameSaves ScriptableObject to default values
    /// </summary>
    private void ResetScriptableObjectData()
    {
        if (targetScriptableObject == null)
        {
            LogDebug("Cannot reset ScriptableObject - targetScriptableObject is null!");
            return;
        }
        
        LogDebug($"Resetting ScriptableObject data - Before: Day={targetScriptableObject.day}, Stress={targetScriptableObject.mother_stress_level}");
        
        // Reset all values to 0
        targetScriptableObject.day = 0;
        targetScriptableObject.mother_stress_level = 0;
        
        // Mark as dirty for Unity to save changes in editor
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(targetScriptableObject);
        #endif
        
        LogDebug($"ScriptableObject data reset - After: Day={targetScriptableObject.day}, Stress={targetScriptableObject.mother_stress_level}");
    }

    /// <summary>
    /// Start the game (placeholder method - implement your game start logic here)
    /// </summary>
    private void StartGame()
    {
        LogDebug("Starting game...");
        // TODO: Add your game start logic here
        // For example: SceneManager.LoadScene("GameScene");
        
        // For now, just move to submenu to show the button worked
        MoveToSubmenu();
    }

    /// <summary>
    /// Context menu method to test start button behavior
    /// </summary>
    [ContextMenu("Test Start Button Click")]
    public void TestStartButtonClick()
    {
        OnStartButtonClick();
    }

    /// <summary>
    /// Handle continue button click - loads save data into ScriptableObject
    /// </summary>
    public void OnContinueButtonClick()
    {
        LogDebug("Continue button clicked");
        
        string saveFilePath = GetSaveFilePath();
        
        if (!File.Exists(saveFilePath))
        {
            LogDebug("No save file found for continue - this should not happen!");
            return;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(saveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
            
            if (saveData != null && targetScriptableObject != null)
            {
                LogDebug($"Loading save data into ScriptableObject - Day: {saveData.day}, Stress: {saveData.mother_stress_level}");
                
                // Load save data into ScriptableObject
                targetScriptableObject.day = saveData.day;
                targetScriptableObject.mother_stress_level = saveData.mother_stress_level;
                
                // Mark as dirty for Unity to save changes in editor
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(targetScriptableObject);
                #endif
                
                LogDebug("Save data loaded into ScriptableObject successfully");
                
                // Continue the game with loaded data
                ContinueGame();
            }
            else
            {
                LogDebug("Failed to parse save data or targetScriptableObject is null!");
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"Error loading save data: {e.Message}");
        }
    }

    /// <summary>
    /// Continue the game with loaded save data (placeholder method)
    /// </summary>
    private void ContinueGame()
    {
        LogDebug("Continuing game with loaded save data...");
        // TODO: Add your game continue logic here
        // For example: SceneManager.LoadScene("GameScene");
        
        // For now, just move to submenu to show the button worked
        MoveToSubmenu();
    }

    /// <summary>
    /// Context menu method to test continue button behavior
    /// </summary>
    [ContextMenu("Test Continue Button Click")]
    public void TestContinueButtonClick()
    {
        OnContinueButtonClick();
    }

    /// <summary>
    /// Show current ScriptableObject values for debugging
    /// </summary>
    [ContextMenu("Show ScriptableObject Values")]
    public void ShowScriptableObjectValues()
    {
        if (targetScriptableObject != null)
        {
            LogDebug($"Current ScriptableObject values - Day: {targetScriptableObject.day}, Mother Stress Level: {targetScriptableObject.mother_stress_level}");
        }
        else
        {
            LogDebug("targetScriptableObject is not assigned!");
        }
    }

    /// <summary>
    /// Manually reset ScriptableObject values (for testing)
    /// </summary>
    [ContextMenu("Reset ScriptableObject Values")]
    public void ManualResetScriptableObject()
    {
        ResetScriptableObjectData();
    }

    public void StartGameNew()
    {
        // Reset ScriptableObject data
        if (targetScriptableObject != null)
        {
            targetScriptableObject.day = 1;
            targetScriptableObject.mother_stress_level = 0;
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(targetScriptableObject);
            #endif
        }

        // Load the next scene (replace "GameScene" with your actual scene name)
        UnityEngine.SceneManagement.SceneManager.LoadScene("Builder House");
    }

    void OnDestroy()
    {
        // Clean up any running tweens
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                LeanTween.cancel(buttons[i]);
            }
        }
        
        // Clean up sprite tweens
        if (creditsSprite != null)
        {
            LeanTween.cancel(creditsSprite);
        }
        
        if (optionsSprite != null)
        {
            LeanTween.cancel(optionsSprite);
        }
        
        if (backButton != null)
        {
            LeanTween.cancel(backButton);
        }
    }
}
