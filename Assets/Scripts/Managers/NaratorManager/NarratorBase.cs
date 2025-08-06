using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public enum NarratorDay
{
    Day1, Day2, Day3, Day4, Day5, Day6, Day7, Day8, Day9, Day10, Day11, Day12, Day13, Day14, Helper
}

public enum TimeOfDay
{
    Morning, Afternoon, Evening, Night
}

public enum CharacterType
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
    [Header("UI Elements")]
    [SerializeField] protected UIElements uiElements;

    [Header("Game Objects")]
    [SerializeField] protected GameObjects gameObjects;   

    [Header("Core Manager")]
    [SerializeField] protected CoreGameManager dialogGameManager;

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
    private void InitializeCharacterComponents()
    {
        foreach (var characterData in charactersDataArray)
        {
            characterData.Initialize();
        }
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
        switch (NarratorManager.Instance.currentTime)
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
        else
        {
            Debug.LogWarning($"Character '{characterType}' not found for movement!");
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
        else
        {
            Debug.LogWarning($"Character '{characterType}' not found for movement!");
        }
    }

    protected IEnumerator MoveAgentToMovementPosition(CharacterType characterType, int positionIndex)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData character))
        {
            if (!character.HasValidMovementPosition(positionIndex))
            {
                Debug.LogWarning($"Invalid movement position index {positionIndex} for {characterType}");
                yield break;
            }
            Transform target = character.movementPositions[positionIndex];
            yield return StartCoroutine(MoveAgentToTarget(characterType, target));
        }
        else
        {
            Debug.LogWarning($"Character '{characterType}' not found!");
        }
    }
#endregion
#region GameObject Management
    protected void AppearObjects()
    {
        SetObjectsActive(gameObjects.activeObjects, true);
        SetObjectsActive(gameObjects.inActiveObjects, false);
    }

    private void SetObjectsActive(GameObject[] objects, bool active)
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
            Debug.LogError("ChargeMeter prefab not found in Resources folder!");
            return null;
        }

        GameObject chargeMeterInstance;
        
        if (targetCanvas != null)
        {
            // Spawn dengan parent Canvas yang spesifik
            chargeMeterInstance = Instantiate(chargeMeterPrefab, targetCanvas.transform);
            
            // Set positioning untuk UI element
            RectTransform rectTransform = chargeMeterInstance.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
            }
            
            Debug.Log($"ChargeMeter spawned in Canvas: {targetCanvas.name}");
        }
        else
        {
            // Spawn di world space
            chargeMeterInstance = Instantiate(chargeMeterPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("ChargeMeter spawned in world space");
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
        if (NarratorManager.Instance != null)
        {
            NarratorManager.Instance.NextTimeOfDay();
        }
    }

    [System.Obsolete]
    protected void GoToNextDay()
    {
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
}
