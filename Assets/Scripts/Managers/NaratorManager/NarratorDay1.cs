using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class AudioClipData
{
    public string clipName;
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = false;
}
public class NarratorDay1 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narratorText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CoreGameManager coregame;

    [Header("Movement System")]
    [SerializeField] private Transform babyObject; 
    [SerializeField] private Transform motherObject; 
    [SerializeField] private Transform fatherObject; 
    
    [Header("Story Positions")]
    [SerializeField] private Transform[] storyPositions;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClipData[] audioClips;

    private System.Collections.Generic.Dictionary<string, AudioClipData> audioDict;

    private void Awake()
    {
        // Build dictionary dari array
        audioDict = new System.Collections.Generic.Dictionary<string, AudioClipData>();
        foreach (var clipData in audioClips)
        {
            audioDict[clipData.clipName] = clipData;
        }
    }
    [System.Obsolete]
    public IEnumerator Narrate()
    {
        // Set the background image for Day 1 black
        ResetUIState();

        switch (NarratorManager.Instance.currentTime)
        {
            case TimeOfDay.Night:
                yield return StartCoroutine(PlayNightSequence());
                break;
        }
    }

    private void ResetUIState()
    {
    }

    [System.Obsolete]
    private IEnumerator PlayNightSequence()
    {
        // CloseEyes();
        // Memainkan animasi ibu sedang duduk, ayah sedang duduk
        Debug.Log("Playing sitting animations for mother and father.");
        PlayCharacterAnimation("mother", "Sit");
        PlayCharacterAnimation("father", "Sit");
        Debug.Log("Playing narration for Day 1 Night sequence.");
        
        yield return new WaitForSeconds(1f);
        narratorText.text = "Day 1\nKelahiran";
        yield return new WaitForSeconds(5f);
        narratorText.gameObject.SetActive(false);

        bool seq1Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/DalamPerut/Seq1DalamPerut", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);

        yield return new WaitForSeconds(0.3f);

        PlayAudio("baby_crying");

        yield return new WaitForSeconds(1f);

        bool seq2Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/DalamPerut/Seq2Terlahir", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);

        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 4f)); 
        }
        yield return new WaitForSeconds(3f);
      
        bool seq3Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq3Kesehatan", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);

        yield return new WaitForSeconds(1f);

        bool seq4Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq4Kesadaran", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);

        yield return new WaitForSeconds(1f);

        yield return new WaitForSeconds(0.5f);
        Debug.Log("Opening eyes");
        FadeOpenEyes();

        bool seq5Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq5MembukaMata", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);

        yield return new WaitForSeconds(1f);
        // pergeseran bayi makin dekat ke ibu
        
        // Memainkan animasi
        yield return new WaitForSeconds(2f);

        bool seq6Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq6Makanan", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);

        yield return new WaitForSeconds(1f);

        Debug.Log("Closing eyes");
        FadeCloseEyes();

        yield return new WaitForSeconds(1f);
        bool seq7Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq7Nama", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        yield return new WaitForSeconds(1f);

        Debug.Log("Sequence 7 complete, closing narration.");
    }

    private void CloseEyes()
    { 
        Color newColor = Color.black;
        newColor.a = 1f; 
        backgroundImage.color = newColor;
        canvasGroup.alpha = 1f; 
    }

    private void FadeOpenEyes()
    {
        StartCoroutine(FadeEyesCoroutine(1f, 0f, 2f)); 
    }

    private void FadeCloseEyes()
    {
        StartCoroutine(FadeEyesCoroutine(0f, 1f, 2f)); 
    }

    private IEnumerator FadeEyesCoroutine(float startAlpha, float endAlpha, float duration)
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
        
        // Ensure final values are set exactly
        currentColor.a = endAlpha;
        backgroundImage.color = currentColor;
        
        Debug.Log($"Eye fade complete: Alpha = {endAlpha}");
    }

    private IEnumerator FadeOutAudio(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume; // Reset for next use
    }

    private void PlayAudio(string clipName)
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
    private IEnumerator MoveObjectToPosition(Transform obj, int positionIndex, float duration = 1f)
    {
        if (obj == null || positionIndex >= storyPositions.Length) 
        {
            Debug.LogError($"Invalid object or position index: {positionIndex}");
            yield break;
        }
        
        Vector3 startPos = obj.position;
        Vector3 targetPos = storyPositions[positionIndex].position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        obj.position = targetPos;
        Debug.Log($"Moved {obj.name} to position {positionIndex}");
    }

    // Character animation triggers
    private void PlayCharacterAnimation(string characterName, string animationName)
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
}
