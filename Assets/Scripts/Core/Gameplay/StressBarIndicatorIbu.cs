using UnityEngine;
using UnityEngine.UI;

public class StressBarIndicatorIbu : MonoBehaviour
{
    [Header("UI References")]
    private Image stressBarFillImage;
    private Image indicatorStressImage; // Fill image inside IndicatorStress GameObject
    
    [Header("Save Data")]
    [SerializeField] private CoreGameSaves saveData;
    [SerializeField] private string saveDataPath = "Saves/coregamesaves"; // Path in Resources folder
    
    [Header("Stress Settings")]
    [SerializeField] private float maxStressLevel = 1000f; // Updated to 1000 scale
    [SerializeField] private bool enableAutoStressIncrease = false;
    [SerializeField] private float stressRate;
    
    [Header("Base Stress Colors (Day 1-12 - Brighter)")]
    [SerializeField] private Color lowStressColor = new Color(0f, 1f, 0f, 1f);         // 0-200 stress (Green)
    [SerializeField] private Color mediumLowStressColor = new Color(0.5f, 1f, 0f, 1f); // 200-400 stress (Yellow-Green)
    [SerializeField] private Color mediumStressColor = new Color(1f, 1f, 0f, 1f);      // 400-600 stress (Yellow)
    [SerializeField] private Color mediumHighStressColor = new Color(1f, 0.5f, 0f, 1f); // 600-800 stress (Orange)
    [SerializeField] private Color highStressColor = new Color(1f, 0f, 0f, 1f);        // 800-1000 stress (Red)
    [SerializeField] private Color maxStressColor = new Color(0.6f, 0f, 0f, 1f);       // 1000+ stress (Dark Red)
    
    [Header("Dark Stress Colors (Day 13+ - Darker)")]
    [SerializeField] private Color darkLowStressColor = new Color(0f, 0.7f, 0f, 1f);         // 0-200 stress (Dark Green)
    [SerializeField] private Color darkMediumLowStressColor = new Color(0.3f, 0.7f, 0f, 1f); // 200-400 stress (Dark Yellow-Green)
    [SerializeField] private Color darkMediumStressColor = new Color(0.7f, 0.7f, 0f, 1f);    // 400-600 stress (Dark Yellow)
    [SerializeField] private Color darkMediumHighStressColor = new Color(0.7f, 0.3f, 0f, 1f); // 600-800 stress (Dark Orange)
    [SerializeField] private Color darkHighStressColor = new Color(0.7f, 0f, 0f, 1f);        // 800-1000 stress (Dark Red)
    [SerializeField] private Color darkMaxStressColor = new Color(0.4f, 0f, 0f, 1f);         // 1000+ stress (Very Dark Red)
    
    [Header("Day-based Settings")]
    [SerializeField] private int darkDayThreshold = 13; // Day 13+ uses darker colors
    
    [Header("Outline Colors")]
    [SerializeField] private Color lowStressOutline = new Color(0f, 1f, 0f, 0.3f);     // Low stress outline
    [SerializeField] private Color mediumStressOutline = new Color(1f, 1f, 0f, 0.5f);  // Medium stress outline
    [SerializeField] private Color highStressOutline = new Color(1f, 0.5f, 0f, 0.7f);  // High stress outline
    [SerializeField] private Color maxStressOutline = new Color(0.8f, 0f, 0f, 0.9f);   // Max stress outline
    
    [Header("Outline Settings")]
    [SerializeField] private float outlineThickness = 2f;
    
    // Component references
    private Outline outlineComponent;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    void Start()
    {
        InitializeComponents();
        LoadSaveData();
    }
    
    /// <summary>
    /// Initialize UI components and add Outline component if needed
    /// </summary>
    private void InitializeComponents()
    {
        // Find the stress bar fill image in Background GameObject (for outline)
        stressBarFillImage = GameObject.Find("Background").GetComponent<Image>();
        if (stressBarFillImage == null)
        {
            LogError("Background GameObject not found in the scene or doesn't have an Image component.");
        }
        else
        {
            // Add or get Outline component for day-based visual effects
            outlineComponent = stressBarFillImage.GetComponent<Outline>();
            if (outlineComponent == null)
            {
                outlineComponent = stressBarFillImage.gameObject.AddComponent<Outline>();
                LogDebug("Added Outline component to Background");
            }
            
            // Initialize outline settings
            outlineComponent.effectDistance = new Vector2(outlineThickness, outlineThickness);
            outlineComponent.useGraphicAlpha = true;
        }
        
        // Find the indicator stress image (for fill color and amount)
        GameObject indicatorStressGO = GameObject.Find("IndicatorStress");
        if (indicatorStressGO != null)
        {
            indicatorStressImage = indicatorStressGO.GetComponent<Image>();
            if (indicatorStressImage == null)
            {
                LogError("IndicatorStress GameObject found but doesn't have an Image component.");
            }
            else
            {
                LogDebug("Found IndicatorStress Image component");
            }
        }
        else
        {
            LogError("IndicatorStress GameObject not found in the scene.");
        }
    }
    
    /// <summary>
    /// Load save data from Resources or assigned ScriptableObject
    /// </summary>
    private void LoadSaveData()
    {
        // If no save data is assigned, try to load from Resources
        if (saveData == null)
        {
            saveData = Resources.Load<CoreGameSaves>(saveDataPath);
            if (saveData == null)
            {
                LogError($"CoreGameSaves not found at Resources/{saveDataPath}");
                LogError("Please assign CoreGameSaves ScriptableObject in inspector or place it in Resources folder");
                return;
            }
            else
            {
                LogDebug($"Loaded CoreGameSaves from Resources: {saveDataPath}");
            }
        }
        
        LogDebug($"Save data loaded - Day: {saveData.day}, Mother Stress: {saveData.mother_stress_level}");
        
        // Initial update
        UpdateStressBar();
        UpdateDayAndStressBasedColors();
    }

    void Update()
    {
        if (saveData == null || indicatorStressImage == null)
            return;
            
        // Optional: Auto-increase stress for testing
        if (enableAutoStressIncrease)
        {
            saveData.mother_stress_level += (int)(stressRate * Time.deltaTime);
            // Don't clamp here - allow stress to go above 1000 for testing
            // The UI will handle clamping the fill amount to 1.0
        }
        
        // Update UI
        UpdateStressBar();
        UpdateDayAndStressBasedColors();
    }
    
    /// <summary>
    /// Update the stress bar fill amount based on save data
    /// Scaling: 0.1 fill = 100 stress, 1.0 fill = 1000 stress
    /// </summary>
    private void UpdateStressBar()
    {
        if (saveData == null)
            return;
            
        // Calculate fill amount - 100 stress = 0.1 fill, 1000 stress = 1.0 fill
        // Formula: fillAmount = stress / 1000
        float fillAmount = Mathf.Clamp01(saveData.mother_stress_level / 1000f);
        
        // Update IndicatorStress fill amount (primary stress display)
        if (indicatorStressImage != null)
        {
            indicatorStressImage.fillAmount = fillAmount;
        }
        
        // Also update Background if it exists (backup/secondary display)
        if (stressBarFillImage != null)
        {
            stressBarFillImage.fillAmount = fillAmount;
        }
        
        // Debug log to show fill calculation
        LogDebug($"Stress: {saveData.mother_stress_level} = Fill: {fillAmount:F3} ({fillAmount * 100:F1}%) [Scale: stress/1000]");
    }
    
    /// <summary>
    /// Update both fill color and outline color based on current day and stress level
    /// Day 13+ uses darker color palette regardless of stress level
    /// Stress level affects color progression within the day's palette and fill amount (0.0-1.0)
    /// </summary>
    private void UpdateDayAndStressBasedColors()
    {
        if (saveData == null)
            return;
            
        int currentDay = saveData.day;
        int currentStress = saveData.mother_stress_level;
        bool useDarkPalette = currentDay >= darkDayThreshold;
        
        // Update fill color based on day and stress
        UpdateDayAndStressBasedFillColor(currentStress, useDarkPalette);
        
        // Update outline color based on stress
        UpdateStressBasedOutlineColor(currentStress);
        
        LogDebug($"Day {currentDay}, Stress {currentStress} - Using {(useDarkPalette ? "DARK" : "BRIGHT")} color palette");
    }
    
    /// <summary>
    /// Update fill color based on day and stress level
    /// Day 13+ uses darker colors, stress affects progression within that palette
    /// </summary>
    private void UpdateDayAndStressBasedFillColor(int stressLevel, bool useDarkPalette)
    {
        if (indicatorStressImage == null)
            return;
            
        Color fillColor;
        string colorInfo;
        
        // Select color palette based on day
        Color lowColor = useDarkPalette ? darkLowStressColor : lowStressColor;
        Color mediumLowColor = useDarkPalette ? darkMediumLowStressColor : mediumLowStressColor;
        Color mediumColor = useDarkPalette ? darkMediumStressColor : mediumStressColor;
        Color mediumHighColor = useDarkPalette ? darkMediumHighStressColor : mediumHighStressColor;
        Color highColor = useDarkPalette ? darkHighStressColor : highStressColor;
        Color maxColor = useDarkPalette ? darkMaxStressColor : maxStressColor;
        
        string paletteType = useDarkPalette ? "DARK" : "BRIGHT";
        
        // Determine color based on stress level (same ranges, different palettes)
        if (stressLevel <= 200)
        {
            fillColor = lowColor;
            colorInfo = $"{paletteType} Green (0-200 stress)";
        }
        else if (stressLevel <= 400)
        {
            float t = (stressLevel - 200f) / 200f;
            fillColor = Color.Lerp(lowColor, mediumLowColor, t);
            colorInfo = $"{paletteType} Yellow-Green (200-400 stress)";
        }
        else if (stressLevel <= 600)
        {
            float t = (stressLevel - 400f) / 200f;
            fillColor = Color.Lerp(mediumLowColor, mediumColor, t);
            colorInfo = $"{paletteType} Yellow (400-600 stress)";
        }
        else if (stressLevel <= 800)
        {
            float t = (stressLevel - 600f) / 200f;
            fillColor = Color.Lerp(mediumColor, mediumHighColor, t);
            colorInfo = $"{paletteType} Orange (600-800 stress)";
        }
        else if (stressLevel < 1000)
        {
            float t = (stressLevel - 800f) / 200f;
            fillColor = Color.Lerp(mediumHighColor, highColor, t);
            colorInfo = $"{paletteType} Red (800-999 stress)";
        }
        else
        {
            fillColor = maxColor;
            colorInfo = $"{paletteType} Dark Red (1000+ stress)";
        }
        
        // Apply fill color
        indicatorStressImage.color = fillColor;
        
        LogDebug($"Applied {colorInfo}");
    }
    
    /// <summary>
    /// Update outline color based on stress level
    /// </summary>
    private void UpdateStressBasedOutlineColor(int stressLevel)
    {
        if (outlineComponent == null)
            return;
            
        Color outlineColor;
        string stressRange;
        
        if (stressLevel <= 300)
        {
            // 0-300: Green outline
            outlineColor = lowStressOutline;
            stressRange = "0-300 (Green Outline)";
        }
        else if (stressLevel <= 600)
        {
            // 300-600: Yellow outline
            outlineColor = mediumStressOutline;
            stressRange = "300-600 (Yellow Outline)";
        }
        else if (stressLevel < 1000)
        {
            // 600-999: Orange outline
            outlineColor = highStressOutline;
            stressRange = "600-999 (Orange Outline)";
        }
        else
        {
            // 1000+: Dark Red outline
            outlineColor = maxStressOutline;
            stressRange = "1000+ (Dark Red Outline)";
        }
        
        // Apply outline color
        outlineComponent.effectColor = outlineColor;
        outlineComponent.enabled = true;
        
        LogDebug($"Stress {stressLevel} - Applied outline color for range {stressRange}");
    }
    
    /// <summary>
    /// Manually refresh the stress bar and colors (useful after save data changes)
    /// </summary>
    [ContextMenu("Refresh Stress Bar")]
    public void RefreshStressBar()
    {
        if (saveData != null)
        {
            UpdateStressBar();
            UpdateDayAndStressBasedColors();
            LogDebug($"Stress bar refreshed - Day: {saveData.day}, Stress: {saveData.mother_stress_level}");
        }
        else
        {
            LogError("Cannot refresh - save data is null");
        }
    }
    
    /// <summary>
    /// Set stress level directly (for testing or external systems)
    /// Allows stress levels above 1000 for testing purposes
    /// </summary>
    public void SetStressLevel(int newStressLevel)
    {
        if (saveData != null)
        {
            saveData.mother_stress_level = Mathf.Max(0, newStressLevel); // Only ensure it's not negative
            UpdateStressBar();
            UpdateDayAndStressBasedColors();
            LogDebug($"Stress level set to: {saveData.mother_stress_level}");
        }
    }
    
    /// <summary>
    /// Set day directly (for testing or external systems)
    /// </summary>
    public void SetDay(int newDay)
    {
        if (saveData != null)
        {
            saveData.day = Mathf.Max(1, newDay);
            UpdateDayAndStressBasedColors(); // Colors are now day AND stress-based
            LogDebug($"Day set to: {saveData.day}");
        }
    }
    
    /// <summary>
    /// Get current stress level
    /// </summary>
    public int GetCurrentStressLevel()
    {
        return saveData != null ? saveData.mother_stress_level : 0;
    }
    
    /// <summary>
    /// Get current day
    /// </summary>
    public int GetCurrentDay()
    {
        return saveData != null ? saveData.day : 1;
    }
    
    /// <summary>
    /// Test method to cycle through different stress levels and days to see color changes
    /// </summary>
    [ContextMenu("Test Day and Stress Color Cycle")]
    public void TestDayAndStressColorCycle()
    {
        if (saveData == null)
        {
            LogError("Cannot test - save data is null");
            return;
        }
        
        int[] testDays = { 1, 5, 12, 13, 15, 20 };
        int[] testStressLevels = { 0, 200, 400, 600, 800, 1000 };
        
        LogDebug("=== Testing Day and Stress-based Color Cycle ===");
        
        foreach (int testDay in testDays)
        {
            LogDebug($"--- Testing Day {testDay} ---");
            saveData.day = testDay;
            
            foreach (int testStress in testStressLevels)
            {
                saveData.mother_stress_level = testStress;
                UpdateDayAndStressBasedColors();
            }
        }
        
        LogDebug("=== End Day and Stress Color Cycle Test ===");
    }
    
    /// <summary>
    /// Test method to show how day affects colors even at 0 stress
    /// </summary>
    [ContextMenu("Test Day Influence on Zero Stress")]
    public void TestDayInfluenceOnZeroStress()
    {
        if (saveData == null)
        {
            LogError("Cannot test - save data is null");
            return;
        }
        
        // Set stress to 0 and test different days
        saveData.mother_stress_level = 0;
        
        LogDebug("=== Testing Day Influence on Zero Stress ===");
        LogDebug("Stress level set to 0 - observing day-based color changes:");
        
        // Test early days (bright colors)
        for (int day = 1; day <= 12; day++)
        {
            saveData.day = day;
            UpdateDayAndStressBasedColors();
            if (day == 1 || day == 6 || day == 12)
            {
                LogDebug($"Day {day}: Using BRIGHT palette even at 0 stress");
            }
        }
        
        // Test late days (dark colors)
        for (int day = 13; day <= 20; day++)
        {
            saveData.day = day;
            UpdateDayAndStressBasedColors();
            if (day == 13 || day == 15 || day == 20)
            {
                LogDebug($"Day {day}: Using DARK palette even at 0 stress");
            }
        }
        
        LogDebug("=== End Day Influence Test ===");
    }
    
    /// <summary>
    /// Test method to verify fill amounts work correctly with the new scaling
    /// 0.1 fill = 100 stress, 1.0 fill = 1000 stress
    /// </summary>
    [ContextMenu("Test Fill Amount Calculation")]
    public void TestFillAmountCalculation()
    {
        if (saveData == null)
        {
            LogError("Cannot test - save data is null");
            return;
        }
        
        int[] testStressLevels = { 0, 100, 250, 500, 750, 1000, 1200, 1500 };
        
        LogDebug("=== Testing Fill Amount Calculation (New Scale: stress/1000) ===");
        
        foreach (int testStress in testStressLevels)
        {
            saveData.mother_stress_level = testStress;
            UpdateStressBar();
            
            float expectedFill = Mathf.Clamp01(testStress / 1000f);
            LogDebug($"Stress {testStress}: Expected Fill = {expectedFill:F3} ({expectedFill * 100:F1}%)");
        }
        
        LogDebug("=== Fill Scale Examples ===");
        LogDebug("100 stress = 0.1 fill (10%)");
        LogDebug("500 stress = 0.5 fill (50%)");
        LogDebug("1000 stress = 1.0 fill (100%)");
        LogDebug("=== End Fill Amount Test ===");
    }
    
    /// <summary>
    /// Quick test to set stress to 1000 and verify bar is full
    /// </summary>
    [ContextMenu("Test 1000 Stress Level")]
    public void Test1000StressLevel()
    {
        if (saveData == null)
        {
            LogError("Cannot test - save data is null");
            return;
        }
        
        LogDebug("=== Testing 1000 Stress Level ===");
        SetStressLevel(1000);
        LogDebug($"Stress set to 1000, Fill Amount should be 1.0 (100%)");
        LogDebug("=== End 1000 Stress Test ===");
    }
    
    /// <summary>
    /// Quick test to set stress to various levels around 1000
    /// </summary>
    [ContextMenu("Test Critical Stress Levels")]
    public void TestCriticalStressLevels()
    {
        if (saveData == null)
        {
            LogError("Cannot test - save data is null");
            return;
        }
        
        int[] criticalLevels = { 999, 1000, 1001, 1500 };
        
        LogDebug("=== Testing Critical Stress Levels ===");
        foreach (int stress in criticalLevels)
        {
            SetStressLevel(stress);
            LogDebug($"--- Stress {stress} tested ---");
        }
        LogDebug("=== End Critical Stress Test ===");
    }
    
    #region Logging Helpers
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[StressBarIbu] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[StressBarIbu] {message}");
    }
    
    #endregion
}
