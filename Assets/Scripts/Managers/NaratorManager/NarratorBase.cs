using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.AI;

[System.Serializable]
public class AudioClipData
{
    public string clipName;
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = false;
}

public abstract class NarratorBase : MonoBehaviour
{

    [Header("Day 1 Setup")]
    [SerializeField] protected GameObject[] activeObjects;   
    [SerializeField] protected GameObject[] inActiveObjects; 

    [Header("UI Elements")]
    [SerializeField] protected TextMeshProUGUI narratorText;
    [SerializeField] protected Image backgroundImage;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] protected CoreGameManager dialogGameManager;

    [Header("Object Move")]
    [SerializeField] protected GameObject babyObject; 
    [SerializeField] protected GameObject motherObject; 
    [SerializeField] protected GameObject fatherObject;
    [SerializeField] protected GameObject bidanObject;
    
    [Header("Story Positions")]
    [SerializeField] protected Transform[] storyPositions;

    [Header("Initial Spawn Positions")]
    [SerializeField] protected Transform[] motherSpawnPositions;   
    [SerializeField] protected Transform[] fatherSpawnPositions;     
    [SerializeField] protected Transform[] bidanSpawnPositions;  
    [SerializeField] protected Transform[] babySpawnPositions;

    [Header("Audio Clips")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClipData[] audioClips;

    protected NavMeshAgent bidanAgent;
    protected NavMeshAgent fatherAgent;
    protected NavMeshAgent motherAgent;
    protected System.Collections.Generic.Dictionary<string, AudioClipData> audioDict;

    protected void Start()
    {
        bidanAgent = bidanObject.GetComponent<NavMeshAgent>();
        fatherAgent = fatherObject.GetComponent<NavMeshAgent>();
        motherAgent = motherObject.GetComponent<NavMeshAgent>();
    }
    protected void Awake()
    {
        // Build dictionary dari array
        audioDict = new System.Collections.Generic.Dictionary<string, AudioClipData>();
        foreach (var clipData in audioClips)
        {
            audioDict[clipData.clipName] = clipData;
        }
    }

    protected void ResetUIState()
    {
    }
    protected void CloseEyes()
    { 
        Color newColor = Color.black;
        newColor.a = 1f; 
        backgroundImage.color = newColor;
        canvasGroup.alpha = 1f; 
    }

    protected void FadeOpenEyes()
    {
        StartCoroutine(FadeEyesCoroutine(1f, 0f, 2f)); 
    }

    protected void FadeCloseEyes()
    {
        StartCoroutine(FadeEyesCoroutine(0f, 1f, 2f)); 
    }

    protected IEnumerator FadeEyesCoroutine(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color currentColor = backgroundImage.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            
            currentColor.a = currentAlpha;
            backgroundImage.color = currentColor;
            
            canvasGroup.alpha = Mathf.Lerp(1f, 1f, normalizedTime);
            
            yield return null; 
        }
        
        currentColor.a = endAlpha;
        backgroundImage.color = currentColor;
        
        Debug.Log($"Eye fade complete: Alpha = {endAlpha}");
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
    
    protected void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    protected IEnumerator PlayAudioForDuration(string clipName, float duration)
    {
        PlayAudio(clipName);
        yield return new WaitForSeconds(duration);
        StopAudio();
    }
    protected IEnumerator MoveObjectToPosition(Transform obj, int positionIndex, float duration = 1f)
    {
        if (obj == null || positionIndex >= storyPositions.Length) 
        {
            Debug.LogError($"Invalid object or position index: {positionIndex}");
            yield break;
        }
        
        Vector3 startPos = obj.position;
        Vector3 targetPos = storyPositions[positionIndex].position;
        
        // Tambah rotation handling
        Quaternion startRot = obj.rotation;
        Quaternion targetRot = storyPositions[positionIndex].rotation; // Ambil rotation dari marker
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            obj.position = Vector3.Lerp(startPos, targetPos, t);
            obj.rotation = Quaternion.Lerp(startRot, targetRot, t); // Lerp rotation juga
            
            yield return null;
        }
        
        obj.position = targetPos;
        obj.rotation = targetRot; // Set exact final rotation
        Debug.Log($"Moved {obj.name} to position {positionIndex}");
    }

    protected void PlayCharacterAnimation(string characterName, string animationName)
    {
        switch (characterName.ToLower())
        {
            case "mother":
                if (motherObject != null)
                {
                    Animator motherAnim = motherObject.GetComponentInChildren<Animator>();
                    if (motherAnim != null)
                        motherAnim.SetTrigger(animationName);
                }
                break;
                
            case "father":
                if (fatherObject != null)
                {
                    Animator fatherAnim = fatherObject.GetComponentInChildren<Animator>();
                    if (fatherAnim != null)
                        fatherAnim.SetTrigger(animationName);
                }
                break;
        }
    }


    protected void SetupDay1NightObjects()
    {
        foreach (GameObject obj in activeObjects)
        {
            if (obj != null) obj.SetActive(true);
        }

        foreach (GameObject obj in inActiveObjects)
        {
            if (obj != null) obj.SetActive(false);
            
        }
    }

    protected void SetCharacterSpawn(string characterName, int spawnIndex)
    {
        Transform targetTransform = null;
        GameObject characterObject = null;
        
        switch (characterName.ToLower())
        {
            case "mother":
                if (spawnIndex < motherSpawnPositions.Length && motherSpawnPositions[spawnIndex] != null)
                {
                    targetTransform = motherSpawnPositions[spawnIndex];
                    characterObject = motherObject;
                }
                break;
            case "father":
                if (spawnIndex < fatherSpawnPositions.Length && fatherSpawnPositions[spawnIndex] != null)
                {
                    targetTransform = fatherSpawnPositions[spawnIndex];
                    characterObject = fatherObject;
                }
                break;
            case "bidan":
                if (spawnIndex < bidanSpawnPositions.Length && bidanSpawnPositions[spawnIndex] != null)
                {
                    targetTransform = bidanSpawnPositions[spawnIndex];
                    characterObject = bidanObject;
                }
                break;
            case "baby":
                if (spawnIndex < babySpawnPositions.Length && babySpawnPositions[spawnIndex] != null)
                {
                    targetTransform = babySpawnPositions[spawnIndex];
                    characterObject = babyObject;
                }
                break;
        }
        
        if (targetTransform != null && characterObject != null)
        {            
            // Disable GameObject untuk reset semua component
            characterObject.SetActive(false);
            
            // Set posisi dan rotasi saat disabled
            characterObject.transform.position = targetTransform.position;
            characterObject.transform.rotation = targetTransform.rotation;
            
            // Enable kembali GameObject
            characterObject.SetActive(true);
            
            Debug.Log($"Set {characterName} spawn to position {spawnIndex} at {targetTransform.position}");
        }
        else
        {
            Debug.LogWarning($"Failed to set spawn for {characterName} at index {spawnIndex}");
        }
    }
}
