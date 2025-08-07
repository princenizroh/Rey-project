using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenuButtonAnimation : MonoBehaviour
{
    [Header("Button References")]
    public GameObject startButton;
    public GameObject continueButton;
    public GameObject optionsButton;
    public GameObject creditsButton;
    public GameObject quitButton;
    public GameObject backButton;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float stairDelay = 0.1f; // Delay between each button animation
    [SerializeField] private float hoverMoveDistance = 20f;
    [SerializeField] private float hoverDuration = 0.3f;
    [SerializeField] private float clickMovePercentage = 150f; // Percentage to move all buttons when clicked
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType hoverEaseType = LeanTweenType.easeOutQuad;

    private GameObject[] buttons;
    private Vector3[] originalPositions;
    private bool isAnimating = false;
    private bool buttonsVisible = true;

    void Start()
    {
        InitializeButtons();
        SetupButtonEvents();
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

    void SetupButtonEvents()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                Button buttonComponent = buttons[i].GetComponent<Button>();
                if (buttonComponent != null)
                {
                    // Add click event to move all buttons to the left and show back button
                    buttonComponent.onClick.AddListener(() => MoveToSubmenu());
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
        isAnimating = false;
        buttonsVisible = true;
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
    }
}
