using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.Cinemachine;

public enum NarratorDay
{
    Day1, Day2, Day3, Day4, Day5, Day6, Day7, Day8, Day9, Day10, Day11, Day12, Day13, Day14, Helper, DayMainMenu
}

public enum TimeOfDay
{
    Morning, Afternoon, Evening, Night
}

public enum CharacterType
{
    Mother, Father, Bidan, Baby, Object, Ghost
}

public enum CharacterTarget
{
    Mother, Father, Bidan, Baby, Object, Ghost
}

[System.Serializable]
public class AudioClipData
{
    public string clipName;
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = false;
}

[System.Serializable]
public class CharacterData
{
    [Header("Character Info")]
    public CharacterType characterType;
    public GameObject characterObject;
    
    [Header("Positions")]
    public Transform[] spawnPositions;
    public Transform[] movementPositions;
    
    [System.NonSerialized] public NavMeshAgent agent;
    [System.NonSerialized] public Animator animator;
    
    public void Initialize()
    {
        if (characterObject != null)
        {
            agent = characterObject.GetComponent<NavMeshAgent>();
            animator = characterObject.GetComponentInChildren<Animator>();
        }
    }
    
    public bool HasValidSpawnPosition(int index)
    {
        return spawnPositions != null && 
               index >= 0 && 
               index < spawnPositions.Length && 
               spawnPositions[index] != null;
    }
    
    public bool HasValidMovementPosition(int index)
    {
        return movementPositions != null && 
               index >= 0 && 
               index < movementPositions.Length && 
               movementPositions[index] != null;
    }
}

[System.Serializable]
public class UIElements
{
    public TextMeshProUGUI narratorText;
    public Image backgroundImage;
    public CanvasGroup canvasGroup;
}

[System.Serializable]
public class GameObjects
{
    [Header("Day 1 Setup")]
    public GameObject[] activeObjects;
    public GameObject[] inActiveObjects;    
}

public abstract class NarratorBase : MonoBehaviour
{
    [Header("Camera Control")]
    [SerializeField] protected CinemachineCamera cinemachineCamera;
    [Header("UI Elements")]
    [SerializeField] protected UIElements uiElements;

    [Header("Game Objects")]
    [SerializeField] protected GameObjects gameObjects;   

    [Header("Core Manager")]
    [SerializeField] protected CoreGameManager dialogGameManager;
    
    // SaveFileManager will be auto-found, no need for Inspector assignment
    protected SaveFileManager saveFileManager;

    [Header("Characters")]
    [SerializeField] protected CharacterData[] charactersDataArray;

    [Header("Audio Clips")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClipData[] audioClips;

    private Dictionary<string, AudioClipData> audioDict;
    private Dictionary<CharacterType, CharacterData> characterDict;

#region Unity Lifecycle 
    protected virtual void Awake()
    {
        InitializeAudioSystem();
        InitializeCharacterSystem();
        InitializeSaveFileManager();
    }

    protected virtual void Start()
    {
        InitializeCharacterComponents();
    }
#endregion
#region Initialization
    private void InitializeAudioSystem()
    {
        audioDict = new Dictionary<string, AudioClipData>();
        foreach (var clipData in audioClips)
        {
            if (!string.IsNullOrEmpty(clipData.clipName))
            {
                audioDict[clipData.clipName] = clipData;
            }
        }
    }
    private void InitializeCharacterSystem()
    {
        characterDict = new Dictionary<CharacterType, CharacterData>();

        foreach (var characterData in charactersDataArray)
        {
            if (characterData != null)
            {
                characterDict[characterData.characterType] = characterData;
            }
        }
    }
    
    private void InitializeSaveFileManager()
    {
        // Auto-find SaveFileManager in scene
        if (saveFileManager == null)
        {
            saveFileManager = FindFirstObjectByType<SaveFileManager>();
        }
        
        if (saveFileManager == null)
        {
            Debug.LogWarning("[NarratorBase] SaveFileManager not found in scene. Auto-save will be disabled.");
        }
        else
        {
            Debug.Log("[NarratorBase] SaveFileManager auto-found and initialized.");
        }
    }
    
    private void InitializeCharacterComponents()
    {
        foreach (var characterData in charactersDataArray)
        {
            characterData.Initialize();
        }
    }
#endregion
#region Sequence Detection
    public virtual TimeOfDay GetFirstAvailableTimeOfDay()
    {
        System.Type thisType = this.GetType();
        
        var morningMethod = thisType.GetMethod("PlayMorningSequence", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (morningMethod != null && morningMethod.DeclaringType != typeof(NarratorBase))
        {
            return TimeOfDay.Morning;
        }
        
        var afternoonMethod = thisType.GetMethod("PlayAfternoonSequence", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (afternoonMethod != null && afternoonMethod.DeclaringType != typeof(NarratorBase))
        {
            return TimeOfDay.Afternoon;
        }
        
        var eveningMethod = thisType.GetMethod("PlayEveningSequence", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (eveningMethod != null && eveningMethod.DeclaringType != typeof(NarratorBase))
        {
            return TimeOfDay.Evening;
        }
        
        return TimeOfDay.Night;
    }
    
    public virtual TimeOfDay GetNextAvailableTimeOfDay(TimeOfDay currentTime)
    {
        System.Type thisType = this.GetType();
        
        for (int i = (int)currentTime + 1; i <= (int)TimeOfDay.Night; i++)
        {
            TimeOfDay checkTime = (TimeOfDay)i;
            string methodName = $"Play{checkTime}Sequence";
            
            var method = thisType.GetMethod(methodName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null && method.DeclaringType != typeof(NarratorBase))
            {
                return checkTime;
            }
        }
        
        return TimeOfDay.Morning;
    }
    
    public virtual bool HasTimeOfDaySequence(TimeOfDay timeOfDay)
    {
        System.Type thisType = this.GetType();
        string methodName = $"Play{timeOfDay}Sequence";
        
        var method = thisType.GetMethod(methodName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return method != null && method.DeclaringType != typeof(NarratorBase);
    }
#endregion
#region Abstract Methods
    [System.Obsolete]
    public IEnumerator StartNarration()
    {
        yield return StartCoroutine(Narrate());
    }
    
    [System.Obsolete]
    protected virtual IEnumerator Narrate()
    {
        ResetUIState();
        
        TimeOfDay targetTime = NarratorManager.Instance.currentTime;
        
        if (!HasTimeOfDaySequence(targetTime))
        {
            Debug.LogWarning($"{this.GetType().Name} does not have {targetTime}Sequence implemented. Finding next available sequence...");
            
            TimeOfDay nextAvailable = GetNextAvailableTimeOfDay(targetTime);
            if (nextAvailable != TimeOfDay.Morning || HasTimeOfDaySequence(TimeOfDay.Morning))
            {
                NarratorManager.Instance.currentTime = nextAvailable;
                targetTime = nextAvailable;
            }
            else
            {
                GoToNextDay();
                yield break;
            }
        }
        
        switch (targetTime)
        {
            case TimeOfDay.Morning:
                yield return StartCoroutine(PlayMorningSequence());
                break;
            case TimeOfDay.Afternoon:
                yield return StartCoroutine(PlayAfternoonSequence());
                break;
            case TimeOfDay.Evening:
                yield return StartCoroutine(PlayEveningSequence());
                break;
            case TimeOfDay.Night:
                yield return StartCoroutine(PlayNightSequence());
                break;
        }
    }
    [System.Obsolete]
    protected virtual IEnumerator PlayMorningSequence()
    {
        yield return null;
    }
    
    [System.Obsolete]
    protected virtual IEnumerator PlayAfternoonSequence()
    {
        yield return null;
    }
    
    [System.Obsolete]
    protected virtual IEnumerator PlayEveningSequence()
    {
        yield return null;
    }
    
    [System.Obsolete]
    protected virtual IEnumerator PlayNightSequence()
    {
        yield return null;
    }
#endregion

#region UI Management
    protected void ResetUIState()
    {
    }
    protected void CloseEyes()
    { 
        Color newColor = Color.black;
        newColor.a = 1f; 
        uiElements.backgroundImage.color = newColor;
        uiElements.canvasGroup.alpha = 1f; 
    }

    protected void FadeOpenEyes()
    {
        StartCoroutine(FadeEyesCoroutine(1f, 0f, 2f)); 
    }

    protected void FadeCloseEyes()
    {
        StartCoroutine(FadeEyesCoroutine(0f, 1f, 2f)); 
    }

    private IEnumerator FadeEyesCoroutine(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color currentColor = uiElements.backgroundImage.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            
            currentColor.a = currentAlpha;
            uiElements.backgroundImage.color = currentColor;
            
            uiElements.canvasGroup.alpha = Mathf.Lerp(1f, 1f, normalizedTime);
            
            yield return null; 
        }
        
        currentColor.a = endAlpha;
        uiElements.backgroundImage.color = currentColor;
       
    }
#endregion
#region Audio Management
    protected void PlayAudio(string clipName)
    {
        if (audioDict.ContainsKey(clipName))
        {
            var clipData = audioDict[clipName];
            audioSource.clip = clipData.audioClip;
            audioSource.volume = clipData.volume;
            audioSource.loop = clipData.loop;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"Audio clip '{clipName}' not found!");
        }
    }
    
    private void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private IEnumerator PlayAudioForDuration(string clipName, float duration)
    {
        PlayAudio(clipName);
        yield return new WaitForSeconds(duration);
        StopAudio();
    }

    protected IEnumerator FadeOutAudio(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume; 
    }
#endregion

#region Character Management
    protected void SetCharacterSpawn(CharacterType characterType, int spawnIndex)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData characterData))
        {
            if (characterData.HasValidSpawnPosition(spawnIndex))
            {
                SetCharacterPosition(characterData.characterObject, characterData.spawnPositions[spawnIndex]);
            }
        }
    }
    private void SetCharacterPosition(GameObject character, Transform targetTransform)
    {
        if (character == null || targetTransform == null) return;

        character.SetActive(false);
        character.transform.position = targetTransform.position;
        character.transform.rotation = targetTransform.rotation;
        character.SetActive(true);
    }

    protected void PlayCharacterAnimation(CharacterType characterType, string animationName)
    {
        if (!EnsureCharacterInitialized(characterType))
        {
            Debug.LogError($"Failed to initialize {characterType} for animation!");
            return;
        }
        
        if (characterDict.TryGetValue(characterType, out CharacterData characterData))
        {
            if (characterData.animator != null)
            {
                if (!characterData.characterObject.activeInHierarchy)
                {
                    Debug.LogWarning($"{characterType} GameObject is not active!");
                    return;
                }
                
                if (!characterData.animator.enabled)
                {
                    Debug.LogWarning($"{characterType} Animator is not enabled!");
                    return;
                }
                
                Debug.Log($"Playing animation '{animationName}' for {characterType}");
                
                if (characterType == CharacterType.Bidan)
                {
                    characterData.animator.Play(animationName);
                }
                if (characterType == CharacterType.Mother)
                {
                    characterData.animator.Play(animationName); 
                }
                if (characterType == CharacterType.Father)
                {
                    characterData.animator.Play(animationName);
                }
                if (characterType == CharacterType.Ghost)
                {
                    characterData.animator.Play(animationName);
                }
                else
                {
                    characterData.animator.SetTrigger(animationName);
                }
            }
            else
            {
                Debug.LogError($"Animator for {characterType} is still null after initialization!");
            }
        }
        else
        {
            Debug.LogError($"Character data for {characterType} not found!");
        }
    }

    private bool EnsureCharacterInitialized(CharacterType characterType)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData characterData))
        {
            if (characterData.animator == null || characterData.agent == null)
            {
                Debug.Log($"Re-initializing {characterType}");
                
                bool wasActive = characterData.characterObject.activeInHierarchy;
                if (!wasActive)
                {
                    characterData.characterObject.SetActive(true);
                }
                
                characterData.Initialize();
                
                if (!wasActive)
                {
                    characterData.characterObject.SetActive(wasActive);
                }
            }
            
            return characterData.animator != null;
        }
        
        return false;
    }

    protected IEnumerator MoveAgentToTarget(CharacterType characterType, Transform target)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData character))
        {
            character.agent.SetDestination(target.position);

            while (character.agent.pathPending)
            {
                yield return null;
            }

            PlayCharacterAnimation(characterType, "Walk");

            while (character.agent.remainingDistance > character.agent.stoppingDistance)
            {
                yield return null;
            }

            PlayCharacterAnimation(characterType, "Idle");
        }
    }

    private IEnumerator MoveObjectToPosition(Transform obj, Transform targetTransform, float duration)
    {
        var startPos = obj.position;
        var targetPos = targetTransform.position;
        var startRot = obj.rotation;
        var targetRot = targetTransform.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            obj.position = Vector3.Lerp(startPos, targetPos, t);
            obj.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        obj.position = targetPos;
        obj.rotation = targetRot;
    }
#endregion
#region Movement Management
    protected IEnumerator MoveCharacterToPosition(CharacterType characterType, int positionIndex, float duration = 1f)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData character))
        {
            if (!character.HasValidMovementPosition(positionIndex))
            {
                Debug.LogError($"Invalid movement position index {positionIndex} for {characterType}");
                yield break;
            }

            Transform targetTransform = character.movementPositions[positionIndex];
            yield return StartCoroutine(MoveObjectToPosition(character.characterObject.transform, targetTransform, duration));
        }
    }

    protected IEnumerator MoveAgentToMovementPosition(CharacterType characterType, int positionIndex)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData character))
        {
            if (!character.HasValidMovementPosition(positionIndex))
            {
                yield break;
            }
            Transform target = character.movementPositions[positionIndex];
            yield return StartCoroutine(MoveAgentToTarget(characterType, target));
        }
    }

    protected void EnableNavMeshAgent(CharacterType characterType)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData character))
        {
            if (character.agent != null)
            {
                character.agent.enabled = true;
            }
        }
    }

    protected void DisableNavMeshAgent(CharacterType characterType)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData character))
        {
            if (character.agent != null)
            {
                character.agent.enabled = false;
            }
        }
    }
#endregion
#region GameObject Management
    protected void AppearObjects()
    {
        SetObjectsActive(gameObjects.activeObjects, true);
        SetObjectsActive(gameObjects.inActiveObjects, false);
    }

    protected void SetObjectsActive(GameObject[] objects, bool active)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    protected GameObject SpawnChargeMeter(Canvas targetCanvas = null)
    {
        GameObject chargeMeterPrefab = Resources.Load<GameObject>("ChargeMeter");
        
        if (chargeMeterPrefab == null)
        {
            return null;
        }

        GameObject chargeMeterInstance;
        
        if (targetCanvas != null)
        {
            chargeMeterInstance = Instantiate(chargeMeterPrefab, targetCanvas.transform);
            
            RectTransform rectTransform = chargeMeterInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
            }
            
        }
        else
        {
            chargeMeterInstance = Instantiate(chargeMeterPrefab, Vector3.zero, Quaternion.identity);
        }

        return chargeMeterInstance;
    }

    protected GameObject SpawnChargeMeterByCanvasName(string canvasName)
    {
        Canvas targetCanvas = FindCanvasByName(canvasName);
        
        if (targetCanvas == null)
        {
            Debug.LogWarning($"Canvas with name '{canvasName}' not found! Spawning in world space instead.");
            return SpawnChargeMeter(null);
        }
        
        return SpawnChargeMeter(targetCanvas);
    }

    private Canvas FindCanvasByName(string canvasName)
    {
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.name.Equals(canvasName, System.StringComparison.OrdinalIgnoreCase))
            {
                return canvas;
            }
        }
        
        return null;
    }

    [System.Obsolete]
    protected void GoToNextTimeOfDay()
    {
        // Auto-save before transitioning to next time of day
        AutoSaveProgress();
        
        if (NarratorManager.Instance != null)
        {
            NarratorManager.Instance.NextTimeOfDay();
        }
    }

    [System.Obsolete]
    protected void GoToNextDay()
    {
        // Auto-save before transitioning to next day
        AutoSaveProgress();
        
        if (NarratorManager.Instance != null)
        {
            NarratorManager.Instance.NextDay();
        }
    }

    [System.Obsolete]
    protected void GoToSpecificNarrator(NarratorDay day, TimeOfDay time)
    {
        if (NarratorManager.Instance != null)
        {
            NarratorManager.Instance.ChangeNarrator(day, time);
        }
    }

#endregion
#region CameraManagement
    protected IEnumerator SetCameraPanRangeFront()
    {
        var panTilt = cinemachineCamera.GetComponent<CinemachinePanTilt>();
        panTilt.PanAxis.Range = new Vector2(0f, 180f);
        yield return null;
    }

    protected IEnumerator SetCameraPanRangeBack()
    {
        var panTilt = cinemachineCamera.GetComponent<CinemachinePanTilt>();
        panTilt.PanAxis.Range = new Vector2(180f, 360f);
        yield return null;
    }

    protected IEnumerator SetCameraPanRangeRight()
    {
        var panTilt = cinemachineCamera.GetComponent<CinemachinePanTilt>();
        panTilt.PanAxis.Range = new Vector2(90f, 270f);
        yield return null;
    }

    protected IEnumerator SetCameraPanRangeLeft()
    {
        var panTilt = cinemachineCamera.GetComponent<CinemachinePanTilt>();
        panTilt.PanAxis.Range = new Vector2(-90f, 90f);
        yield return null;
    }
#endregion

#region Save Management
    /// <summary>
    /// Auto-save game progress after completing a sequence
    /// </summary>
    protected void AutoSaveProgress()
    {
        if (saveFileManager == null)
        {
            Debug.LogWarning("[NarratorBase] SaveFileManager not assigned, skipping auto-save.");
            return;
        }

        try
        {
            // Update day progress based on current narrator
            if (NarratorManager.Instance != null)
            {
                int currentDayNumber = (int)NarratorManager.Instance.currentDay + 1; // Convert enum to 1-based day number
                
                // Update ScriptableObject day value before saving
                UpdateSaveDataDay(currentDayNumber);
                
                // Save current progress to JSON
                saveFileManager.SaveToCoreGameSavesJSON();
                
                Debug.Log($"[NarratorBase] Auto-saved progress: Day {currentDayNumber}, Time: {NarratorManager.Instance.currentTime}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NarratorBase] Auto-save failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// Update the day value in SaveFileManager's target ScriptableObject
    /// </summary>
    private void UpdateSaveDataDay(int dayNumber)
    {
        // Access the SaveFileManager's target ScriptableObject through reflection or direct access
        // This assumes SaveFileManager has a public getter or we add one
        var saveDataField = saveFileManager.GetType().GetField("targetSaveObject", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (saveDataField != null)
        {
            var coreGameSaves = saveDataField.GetValue(saveFileManager);
            if (coreGameSaves != null)
            {
                // Update day field using reflection
                var dayField = coreGameSaves.GetType().GetField("day");
                if (dayField != null)
                {
                    dayField.SetValue(coreGameSaves, dayNumber);
                    Debug.Log($"[NarratorBase] Updated save data day to: {dayNumber}");
                }
            }
        }
    }
    
    /// <summary>
    /// Manual save method that can be called from child classes
    /// </summary>
    protected void ManualSave()
    {
        AutoSaveProgress();
    }
    
    /// <summary>
    /// Save with custom day number (useful for special cases)
    /// </summary>
    protected void SaveWithDay(int dayNumber)
    {
        if (saveFileManager == null)
        {
            Debug.LogWarning("[NarratorBase] SaveFileManager not assigned, skipping save.");
            return;
        }

        try
        {
            UpdateSaveDataDay(dayNumber);
            saveFileManager.SaveToCoreGameSavesJSON();
            Debug.Log($"[NarratorBase] Saved progress with custom day: {dayNumber}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NarratorBase] Custom save failed: {e.Message}");
        }
    }
#endregion

#region Head Tracking Management
    /// <summary>
    /// Set head target for specific character using HeadTarget enum
    /// </summary>
    protected void SetHeadTarget(CharacterType characterType, string targetName)
    {
        var headTrackingManager = FindFirstObjectByType(System.Type.GetType("HeadTrackingManager"));
        if (headTrackingManager != null)
        {
            // Get HeadTarget enum type
            var headTargetType = System.Type.GetType("HeadTarget");
            if (headTargetType != null)
            {
                var targetEnum = System.Enum.Parse(headTargetType, targetName);
                var method = headTrackingManager.GetType().GetMethod("SetHeadTarget", new System.Type[] { typeof(CharacterType), headTargetType });
                if (method != null)
                {
                    method.Invoke(headTrackingManager, new object[] { characterType, targetEnum });
                }
            }
        }
        else
        {
            Debug.LogWarning("[NarratorBase] HeadTrackingManager not found in scene!");
        }
    }
    
    /// <summary>
    /// Helper methods for easier calling
    /// </summary>
    protected void SetHeadTargetCamera(CharacterType characterType)
    {
        SetHeadTarget(characterType, "Camera");
    }
    
    protected void SetHeadTargetMother(CharacterType characterType)
    {
        SetHeadTarget(characterType, "Mother");
    }
    
    protected void SetHeadTargetFather(CharacterType characterType)
    {
        SetHeadTarget(characterType, "Father");
    }
    
    protected void SetHeadTargetBaby(CharacterType characterType)
    {
        SetHeadTarget(characterType, "Baby");
    }
    
    protected void SetHeadTargetBidan(CharacterType characterType)
    {
        SetHeadTarget(characterType, "Bidan");
    }
    
    /// <summary>
    /// Disable head tracking for specific character
    /// </summary>
    protected void DisableHeadTracking(CharacterType characterType)
    {
        SetHeadTarget(characterType, "None");
    }
    
    /// <summary>
    /// Enable/disable head tracking for all characters
    /// </summary>
    protected void EnableGlobalHeadTracking(bool enable)
    {
        var headTrackingManager = FindFirstObjectByType(System.Type.GetType("HeadTrackingManager"));
        if (headTrackingManager != null)
        {
            var method = headTrackingManager.GetType().GetMethod("EnableGlobalHeadTracking");
            if (method != null)
            {
                method.Invoke(headTrackingManager, new object[] { enable });
            }
        }
    }
    
    /// <summary>
    /// Set multiple characters to look at same target
    /// </summary>
    protected void SetMultipleHeadTargetsCamera(CharacterType[] characters)
    {
        foreach (var character in characters)
        {
            SetHeadTargetCamera(character);
        }
    }
#endregion
}
