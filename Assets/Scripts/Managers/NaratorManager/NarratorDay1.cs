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
        CloseEyes();
        yield return new WaitForSeconds(1f);
        PlayAudio("day1_night");
        narratorText.text = "Day 1 (Kelahiran)";
        yield return new WaitForSeconds(3f);
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
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        yield return new WaitForSeconds(1f);
        PlayAudio("clock_ticking");
        yield return new WaitForSeconds(4f); 
    
        PlayAudio("medicine_sound");
        yield return new WaitForSeconds(1.5f);
      
        bool seq3Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/Kamar/Seq3Kesehatan", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);

        yield return new WaitForSeconds(1f);

        bool seq4Complete = false;
        coregame.StartCoreGame("GameData/Dialog/Day1/Kamar/Seq4KesadaranBayi", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);

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
        
    }

    private void FadeCloseEyes()
    {
        
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

}
