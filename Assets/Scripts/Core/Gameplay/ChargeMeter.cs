using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ChargeMeter : MonoBehaviour
{
    [Header("UI References")]
    private TMP_Text spaceSpamIndicator;
    private Image chargeMeterFillImage;
    
    [Header("Charge Settings")]
    private float chargeLevel = 0;
    [SerializeField] private float chargeRate = 10f;
    [SerializeField] private float pullbackThreshold = 25f; // Percentage when pullback starts
    [SerializeField] private float pullbackRate = 5f; // How fast it pulls back
    [SerializeField] private float maxChargeLevel = 100f;
    
    [Header("Input Settings")]
    private float lastSpacePress = 0f;
    private float spacePressCooldown = 0.1f;

    void Start()
    {
        spaceSpamIndicator = GameObject.Find("TextSpam").GetComponent<TMP_Text>();
        if (spaceSpamIndicator == null)
        {
            Debug.LogError("SpaceSpamIndicator not found in the scene.");
        }

        chargeMeterFillImage = GameObject.Find("Indicator").GetComponent<Image>();
        if (chargeMeterFillImage == null)   
        {
            Debug.LogError("ChargeMeterFill not found in the scene.");
        }
    }

    public void changeChargeRate(float newChargeRate)
    {
        chargeRate = newChargeRate;
        Debug.Log("Charge rate changed to: " + chargeRate);
    }

    public void changePullbackThreshold(float newThreshold)
    {
        pullbackThreshold = Mathf.Clamp(newThreshold, 0f, 100f);
        Debug.Log("Pullback threshold changed to: " + pullbackThreshold + "%");
    }

    public void changePullbackRate(float newPullbackRate)
    {
        pullbackRate = newPullbackRate;
        Debug.Log("Pullback rate changed to: " + pullbackRate);
    }

    public void resetChargeLevel()
    {
        chargeLevel = 0;
        Debug.Log("Charge level reset to: " + chargeLevel);
    }

    /// <summary>
    /// Get current charge level as percentage (0-100)
    /// </summary>
    public float GetChargePercentage()
    {
        return (chargeLevel / maxChargeLevel) * 100f;
    }

    /// <summary>
    /// Check if charge is above pullback threshold
    /// </summary>
    public bool IsInPullbackZone()
    {
        return GetChargePercentage() >= pullbackThreshold;
    }

    void Update()
    {
        // Handle Space key press with cooldown
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastSpacePress + spacePressCooldown)
        {
            lastSpacePress = Time.time;
            
            // Visual feedback
            if (spaceSpamIndicator != null)
            {
                spaceSpamIndicator.fontSize = 50;
            }
            
            // Add charge
            chargeLevel += chargeRate;
            
            // Clamp to max level
            if (chargeLevel > maxChargeLevel)
            {
                chargeLevel = maxChargeLevel;
            }
            
            Debug.Log($"Space pressed! Charge level: {GetChargePercentage():F1}%");
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            // Visual feedback reset
            if (spaceSpamIndicator != null)
            {
                spaceSpamIndicator.fontSize = 60;
            }
        }

        // Apply pullback when above threshold
        if (IsInPullbackZone())
        {
            float pullbackAmount = pullbackRate * Time.deltaTime;
            chargeLevel -= pullbackAmount;
            
            // Don't let it go below 0
            if (chargeLevel < 0)
            {
                chargeLevel = 0;
            }
        }

        // Update visual meter
        if (chargeMeterFillImage != null)
        {
            chargeMeterFillImage.fillAmount = chargeLevel / maxChargeLevel;
        }

        // Check if meter is full
        if (chargeLevel >= maxChargeLevel)
        {
            Debug.Log("Charge meter full! Starting core game...");
            
            // Find and start core game
            CoreGameManager coreGameManager = FindFirstObjectByType<CoreGameManager>();
            if (coreGameManager != null)
            {
                coreGameManager.StartCoreGame("Rey", null);
            }
            else
            {
                Debug.LogError("CoreGameManager not found in the scene.");
            }
            
            // Deactivate and reset
            gameObject.SetActive(false);
            chargeLevel = 0f;
        }
    }
}
