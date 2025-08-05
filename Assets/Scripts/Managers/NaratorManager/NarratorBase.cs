using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

[System.Serializable]
public class AudioClipData
{
    public string clipName;
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = false;
}

public enum CharacterType
{
    Mother,
    Father,
    Bidan,
    Baby
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
    
    // Runtime references (initialized automatically)
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

    protected virtual void Awake()
    {
        InitializeAudioSystem();
        InitializeCharacterSystem();
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

    protected virtual void Start()
    {
        InitializeCharacterComponents();
    }
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
    private void InitializeCharacterComponents()
    {
        foreach (var characterData in charactersDataArray)
        {
            characterData.Initialize();
        }
    }

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

    protected void PlayAudio(string clipName)
    {
        if (audioDict.ContainsKey(clipName))
        {
            var clipData = audioDict[clipName];
            audioSource.clip = clipData.audioClip;
            audioSource.volume = clipData.volume;
            audioSource.loop = clipData.loop;
            audioSource.Play();
            Debug.Log($"Playing audio: {clipName}");
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
            
            Debug.Log($"Moved {characterType} to movement position {positionIndex}");
        }
        else
        {
            Debug.LogWarning($"Character '{characterType}' not found for movement!");
        }
    }
    private IEnumerator MoveObjectToPosition(Transform obj, Transform targetTransform, float duration)
    {
        if (obj == null || targetTransform == null)
        {
            Debug.LogError("Invalid object or target transform for movement");
            yield break;
        }

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
            Debug.Log($"{characterType} reached destination and switched to Idle");
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


    protected void PlayCharacterAnimation(CharacterType characterType, string animationName)
    {
        if (characterDict.TryGetValue(characterType, out CharacterData characterData))
        {
            if (characterData.animator != null)
            {
                if (characterType == CharacterType.Bidan)
                {
                    characterData.animator.Play(animationName);
                }
                else
                {
                    characterData.animator.SetTrigger(animationName);
                }

            }
        }
    }

    protected void SetupDay1NightObjects()
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
}
