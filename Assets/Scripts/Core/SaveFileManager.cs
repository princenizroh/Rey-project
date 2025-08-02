using UnityEngine;
using System.IO;

public class SaveFileManager : MonoBehaviour
{
    [Header("Save Configuration")]
    [SerializeField] private string savesFolderPath = "Saves";
    [SerializeField] private string saveFileName = "save_data";
    [SerializeField] private CoreGameSaves targetSaveObject;
    
    [Header("Auto Restore")]
    [SerializeField] private bool restoreOnStart = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Serializable class to match JSON structure
    [System.Serializable]
    public class SaveData
    {
        public int day;
        public int mother_stress_level;
        
        // Constructor for easy initialization
        public SaveData()
        {
            day = 1;
            mother_stress_level = 0;
        }
        
        public SaveData(int day, int motherStress)
        {
            this.day = day;
            this.mother_stress_level = motherStress;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (restoreOnStart)
        {
            RestoreSaveFromJSON();
        }
    }

    /// <summary>
    /// Restore save data from JSON file in Resources folder
    /// </summary>
    [ContextMenu("Restore Save From JSON")]
    public void RestoreSaveFromJSON()
    {
        if (targetSaveObject == null)
        {
            LogError("Target CoreGameSaves ScriptableObject is not assigned!");
            return;
        }

        try
        {
            // Construct the full path for Resources.Load
            string resourcePath = $"{savesFolderPath}/{saveFileName}";
            
            // Load the JSON file from Resources
            TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
            
            if (jsonFile == null)
            {
                LogError($"Save file not found at Resources/{resourcePath}.json");
                CreateDefaultSaveFile();
                return;
            }

            // Parse JSON data
            string jsonContent = jsonFile.text;
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
            
            if (saveData == null)
            {
                LogError("Failed to parse JSON data. Creating default save.");
                CreateDefaultSaveFile();
                return;
            }

            // Apply data to ScriptableObject
            ApplySaveDataToScriptableObject(saveData);
            
            LogDebug($"✓ Save restored successfully from {resourcePath}.json");
            LogDebug($"  - Day: {saveData.day}");
            LogDebug($"  - Mother Stress Level: {saveData.mother_stress_level}");
        }
        catch (System.Exception e)
        {
            LogError($"Error restoring save: {e.Message}");
            CreateDefaultSaveFile();
        }
    }
    
    /// <summary>
    /// Apply loaded save data to the target ScriptableObject
    /// </summary>
    private void ApplySaveDataToScriptableObject(SaveData saveData)
    {
        if (targetSaveObject == null)
        {
            LogError("Target ScriptableObject is null!");
            return;
        }
        
        targetSaveObject.day = saveData.day;
        targetSaveObject.mother_stress_level = saveData.mother_stress_level;
        
        // Mark as dirty for Unity to save changes in editor
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(targetSaveObject);
        #endif
        
        LogDebug("ScriptableObject updated with save data");
    }
    
    /// <summary>
    /// Create a default save file if none exists
    /// </summary>
    private void CreateDefaultSaveFile()
    {
        try
        {
            SaveData defaultSave = new SaveData();
            string jsonContent = JsonUtility.ToJson(defaultSave, true);
            
            // For Resources folder, we need to save to StreamingAssets or persistent data path
            // Since Resources is read-only at runtime, we'll save to persistent data path
            string persistentPath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(persistentPath))
            {
                Directory.CreateDirectory(persistentPath);
            }
            
            string filePath = Path.Combine(persistentPath, $"{saveFileName}.json");
            File.WriteAllText(filePath, jsonContent);
            
            LogDebug($"Default save file created at: {filePath}");
            LogDebug("Note: For runtime loading, place save files in Resources folder manually.");
            
            // Also apply default values to ScriptableObject
            ApplySaveDataToScriptableObject(defaultSave);
        }
        catch (System.Exception e)
        {
            LogError($"Failed to create default save file: {e.Message}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Save current ScriptableObject data to JSON (for testing/debugging)
    /// </summary>
    [ContextMenu("Save Current Data to JSON")]
    public void SaveCurrentDataToJSON()
    {
        if (targetSaveObject == null)
        {
            LogError("Target CoreGameSaves ScriptableObject is not assigned!");
            return;
        }
        
        try
        {
            // Create save data from current ScriptableObject
            SaveData currentData = new SaveData(targetSaveObject.day, targetSaveObject.mother_stress_level);
            
            // Convert to JSON
            string jsonContent = JsonUtility.ToJson(currentData, true);
            
            // Save to persistent data path
            string persistentPath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(persistentPath))
            {
                Directory.CreateDirectory(persistentPath);
            }
            
            string filePath = Path.Combine(persistentPath, $"{saveFileName}.json");
            File.WriteAllText(filePath, jsonContent);
            
            LogDebug($"✓ Current data saved to: {filePath}");
            LogDebug($"  - Day: {currentData.day}");
            LogDebug($"  - Mother Stress Level: {currentData.mother_stress_level}");
        }
        catch (System.Exception e)
        {
            LogError($"Error saving current data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Save CoreGameSaves data to coregamesaves.json in persistent data path
    /// This creates a specifically named file for the CoreGameSaves ScriptableObject
    /// </summary>
    [ContextMenu("Save to CoreGameSaves.json")]
    public void SaveToCoreGameSavesJSON()
    {
        if (targetSaveObject == null)
        {
            LogError("Target CoreGameSaves ScriptableObject is not assigned!");
            return;
        }
        
        try
        {
            // Create save data from current ScriptableObject
            SaveData currentData = new SaveData(targetSaveObject.day, targetSaveObject.mother_stress_level);
            
            // Convert to JSON with pretty formatting
            string jsonContent = JsonUtility.ToJson(currentData, true);
            
            // Save to persistent data path with specific filename
            string persistentPath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(persistentPath))
            {
                Directory.CreateDirectory(persistentPath);
            }
            
            string filePath = Path.Combine(persistentPath, "coregamesaves.json");
            File.WriteAllText(filePath, jsonContent);
            
            LogDebug($"✓ CoreGameSaves data saved to: {filePath}");
            LogDebug($"JSON Content:\n{jsonContent}");
            LogDebug($"To use in Resources folder, copy this file to: Assets/Resources/Saves/coregamesaves.json");
            
            // Also try to save to StreamingAssets if it exists (accessible at runtime)
            SaveToStreamingAssets(jsonContent, "coregamesaves.json");
            
        }
        catch (System.Exception e)
        {
            LogError($"Error saving to CoreGameSaves.json: {e.Message}");
        }
    }
    
    /// <summary>
    /// Save to StreamingAssets folder (if it exists) for runtime access
    /// </summary>
    private void SaveToStreamingAssets(string jsonContent, string fileName)
    {
        try
        {
            string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "Saves");
            
            // Only try to save if StreamingAssets exists
            if (Directory.Exists(Application.streamingAssetsPath))
            {
                if (!Directory.Exists(streamingAssetsPath))
                {
                    Directory.CreateDirectory(streamingAssetsPath);
                }
                
                string filePath = Path.Combine(streamingAssetsPath, fileName);
                File.WriteAllText(filePath, jsonContent);
                
                LogDebug($"✓ Also saved to StreamingAssets: {filePath}");
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"Could not save to StreamingAssets: {e.Message}");
        }
    }
    
    /// <summary>
    /// Load specifically from coregamesaves.json file
    /// </summary>
    [ContextMenu("Load from CoreGameSaves.json")]
    public void LoadFromCoreGameSavesJSON()
    {
        if (targetSaveObject == null)
        {
            LogError("Target CoreGameSaves ScriptableObject is not assigned!");
            return;
        }
        
        try
        {
            // First try Resources folder
            string resourcePath = $"{savesFolderPath}/coregamesaves";
            TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
            
            if (jsonFile != null)
            {
                // Parse JSON data from Resources
                string jsonContent = jsonFile.text;
                SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
                
                if (saveData != null)
                {
                    ApplySaveDataToScriptableObject(saveData);
                    LogDebug($"✓ CoreGameSaves loaded from Resources/{resourcePath}.json");
                    LogDebug($"  - Day: {saveData.day}");
                    LogDebug($"  - Mother Stress Level: {saveData.mother_stress_level}");
                    return;
                }
            }
            
            // Fallback to persistent data path
            LoadCoreGameSavesFromPersistentPath();
            
        }
        catch (System.Exception e)
        {
            LogError($"Error loading CoreGameSaves.json: {e.Message}");
            LoadCoreGameSavesFromPersistentPath();
        }
    }
    
    /// <summary>
    /// Load coregamesaves.json from persistent data path
    /// </summary>
    private void LoadCoreGameSavesFromPersistentPath()
    {
        try
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, "Saves");
            string filePath = Path.Combine(persistentPath, "coregamesaves.json");
            
            if (!File.Exists(filePath))
            {
                LogError($"coregamesaves.json not found at: {filePath}");
                LogDebug("Use 'Save to CoreGameSaves.json' to create the file first.");
                return;
            }
            
            string jsonContent = File.ReadAllText(filePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
            
            if (saveData == null)
            {
                LogError("Failed to parse coregamesaves.json from persistent path.");
                return;
            }
            
            ApplySaveDataToScriptableObject(saveData);
            
            LogDebug($"✓ CoreGameSaves loaded from persistent path: {filePath}");
            LogDebug($"  - Day: {saveData.day}");
            LogDebug($"  - Mother Stress Level: {saveData.mother_stress_level}");
            
        }
        catch (System.Exception e)
        {
            LogError($"Error loading coregamesaves.json from persistent path: {e.Message}");
        }
    }
    
    /// <summary>
    /// Load save data from persistent data path (alternative to Resources)
    /// </summary>
    [ContextMenu("Load from Persistent Data Path")]
    public void LoadFromPersistentDataPath()
    {
        if (targetSaveObject == null)
        {
            LogError("Target CoreGameSaves ScriptableObject is not assigned!");
            return;
        }
        
        try
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, "Saves");
            string filePath = Path.Combine(persistentPath, $"{saveFileName}.json");
            
            if (!File.Exists(filePath))
            {
                LogError($"Save file not found at: {filePath}");
                CreateDefaultSaveFile();
                return;
            }
            
            string jsonContent = File.ReadAllText(filePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
            
            if (saveData == null)
            {
                LogError("Failed to parse JSON data from persistent path.");
                return;
            }
            
            ApplySaveDataToScriptableObject(saveData);
            
            LogDebug($"✓ Save loaded from persistent path: {filePath}");
            LogDebug($"  - Day: {saveData.day}");
            LogDebug($"  - Mother Stress Level: {saveData.mother_stress_level}");
        }
        catch (System.Exception e)
        {
            LogError($"Error loading from persistent data path: {e.Message}");
        }
    }
    
    /// <summary>
    /// Reset ScriptableObject to default values
    /// </summary>
    [ContextMenu("Reset to Default Values")]
    public void ResetToDefaultValues()
    {
        if (targetSaveObject == null)
        {
            LogError("Target CoreGameSaves ScriptableObject is not assigned!");
            return;
        }
        
        SaveData defaultSave = new SaveData();
        ApplySaveDataToScriptableObject(defaultSave);
        
        LogDebug("ScriptableObject reset to default values");
    }
    
    /// <summary>
    /// Get current save data as JSON string (for debugging)
    /// </summary>
    public string GetCurrentSaveAsJSON()
    {
        if (targetSaveObject == null)
        {
            LogError("Target CoreGameSaves ScriptableObject is not assigned!");
            return "{}";
        }
        
        SaveData currentData = new SaveData(targetSaveObject.day, targetSaveObject.mother_stress_level);
        return JsonUtility.ToJson(currentData, true);
    }
    
    /// <summary>
    /// Validate save file exists in Resources folder
    /// </summary>
    [ContextMenu("Validate Resources Save File")]
    public void ValidateResourcesSaveFile()
    {
        string resourcePath = $"{savesFolderPath}/{saveFileName}";
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        
        if (jsonFile != null)
        {
            LogDebug($"✓ Save file found at Resources/{resourcePath}.json");
            LogDebug($"Content preview:\n{jsonFile.text}");
        }
        else
        {
            LogError($"✗ Save file not found at Resources/{resourcePath}.json");
            LogDebug("Make sure to place your JSON save file in the Resources folder!");
        }
    }
    
    /// <summary>
    /// Validate that coregamesaves.json exists in Resources folder
    /// </summary>
    [ContextMenu("Validate CoreGameSaves.json in Resources")]
    public void ValidateCoreGameSavesInResources()
    {
        string resourcePath = $"{savesFolderPath}/coregamesaves";
        TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);
        
        if (jsonFile != null)
        {
            LogDebug($"✓ coregamesaves.json found at Resources/{resourcePath}.json");
            LogDebug($"Content:\n{jsonFile.text}");
            
            // Also validate the JSON structure
            try
            {
                SaveData testData = JsonUtility.FromJson<SaveData>(jsonFile.text);
                if (testData != null)
                {
                    LogDebug($"✓ JSON structure is valid:");
                    LogDebug($"  - Day: {testData.day}");
                    LogDebug($"  - Mother Stress Level: {testData.mother_stress_level}");
                }
                else
                {
                    LogError("✗ JSON structure is invalid - could not parse SaveData");
                }
            }
            catch (System.Exception e)
            {
                LogError($"✗ JSON parsing error: {e.Message}");
            }
        }
        else
        {
            LogError($"✗ coregamesaves.json not found at Resources/{savesFolderPath}/coregamesaves.json");
            LogDebug("Use 'Save to CoreGameSaves.json' and manually copy the file to Resources folder.");
            
            // Check if file exists in persistent data path
            string persistentPath = Path.Combine(Application.persistentDataPath, "Saves", "coregamesaves.json");
            if (File.Exists(persistentPath))
            {
                LogDebug($"Found coregamesaves.json in persistent data path: {persistentPath}");
                LogDebug("Copy this file to Assets/Resources/Saves/ folder for Resources loading.");
            }
        }
    }
    
    /// <summary>
    /// Show all available save files in persistent data path
    /// </summary>
    [ContextMenu("List All Save Files")]
    public void ListAllSaveFiles()
    {
        try
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(persistentPath))
            {
                LogDebug("No save files found - Saves directory doesn't exist in persistent data path.");
                return;
            }
            
            string[] jsonFiles = Directory.GetFiles(persistentPath, "*.json");
            
            if (jsonFiles.Length == 0)
            {
                LogDebug("No JSON save files found in persistent data path.");
                return;
            }
            
            LogDebug($"Found {jsonFiles.Length} save file(s) in {persistentPath}:");
            
            foreach (string filePath in jsonFiles)
            {
                string fileName = Path.GetFileName(filePath);
                long fileSize = new FileInfo(filePath).Length;
                string lastModified = File.GetLastWriteTime(filePath).ToString("yyyy-MM-dd HH:mm:ss");
                
                LogDebug($"  - {fileName} ({fileSize} bytes, modified: {lastModified})");
                
                // Show preview for small files
                if (fileSize < 1000)
                {
                    try
                    {
                        string content = File.ReadAllText(filePath);
                        LogDebug($"    Preview: {content.Replace("\n", " ").Replace("\r", "")}");
                    }
                    catch (System.Exception e)
                    {
                        LogDebug($"    Could not read file: {e.Message}");
                    }
                }
            }
            
            // Also check StreamingAssets
            string streamingPath = Path.Combine(Application.streamingAssetsPath, "Saves");
            if (Directory.Exists(streamingPath))
            {
                string[] streamingFiles = Directory.GetFiles(streamingPath, "*.json");
                if (streamingFiles.Length > 0)
                {
                    LogDebug($"\nAlso found {streamingFiles.Length} file(s) in StreamingAssets/Saves:");
                    foreach (string filePath in streamingFiles)
                    {
                        string fileName = Path.GetFileName(filePath);
                        LogDebug($"  - {fileName}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            LogError($"Error listing save files: {e.Message}");
        }
    }
    
    /// <summary>
    /// Copy save files from persistent data path to a easily accessible location
    /// (Desktop or Documents folder for manual copying to Resources)
    /// </summary>
    [ContextMenu("Export Save Files to Desktop")]
    public void ExportSaveFilesToDesktop()
    {
        try
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(persistentPath))
            {
                LogError("No save files to export - Saves directory doesn't exist.");
                return;
            }
            
            // Create export folder on Desktop
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string exportPath = Path.Combine(desktopPath, "Unity_Save_Files_Export");
            
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            
            string[] jsonFiles = Directory.GetFiles(persistentPath, "*.json");
            int exportedCount = 0;
            
            foreach (string sourceFile in jsonFiles)
            {
                string fileName = Path.GetFileName(sourceFile);
                string destinationFile = Path.Combine(exportPath, fileName);
                
                File.Copy(sourceFile, destinationFile, true);
                exportedCount++;
                
                LogDebug($"Exported: {fileName}");
            }
            
            if (exportedCount > 0)
            {
                LogDebug($"✓ Exported {exportedCount} save file(s) to: {exportPath}");
                LogDebug("You can now manually copy these files to Assets/Resources/Saves/ folder.");
                
                // Try to open the folder (Windows only)
                #if UNITY_EDITOR_WIN
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", exportPath);
                }
                catch
                {
                    // Ignore if can't open folder
                }
                #endif
            }
            else
            {
                LogDebug("No JSON files found to export.");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Error exporting save files: {e.Message}");
        }
    }
    
    #region Logging Helpers
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[RestoreSaves] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[RestoreSaves] {message}");
    }
    
    #endregion
    
    #region Public Getters/Setters
    
    /// <summary>
    /// Change the saves folder path at runtime
    /// </summary>
    public void SetSavesFolderPath(string newPath)
    {
        savesFolderPath = newPath;
        LogDebug($"Saves folder path changed to: {newPath}");
    }
    
    /// <summary>
    /// Change the save file name at runtime
    /// </summary>
    public void SetSaveFileName(string newFileName)
    {
        saveFileName = newFileName;
        LogDebug($"Save file name changed to: {newFileName}");
    }
    
    /// <summary>
    /// Get current saves folder path
    /// </summary>
    public string GetSavesFolderPath()
    {
        return savesFolderPath;
    }
    
    /// <summary>
    /// Get current save file name
    /// </summary>
    public string GetSaveFileName()
    {
        return saveFileName;
    }
    
    /// <summary>
    /// Set target ScriptableObject at runtime
    /// </summary>
    public void SetTargetSaveObject(CoreGameSaves newTarget)
    {
        targetSaveObject = newTarget;
        LogDebug($"Target ScriptableObject changed to: {(newTarget ? newTarget.name : "null")}");
    }
    
    #endregion
}
