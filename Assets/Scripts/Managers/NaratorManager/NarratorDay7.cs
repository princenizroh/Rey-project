
using UnityEngine;
using System.Collections;

public class NarratorDay7 : NarratorBase
{
    // Day 7 - Pre-Depression Phase
    // Condition deteriorates significantly, supernatural intensifies
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0); // Bedroom - severe withdrawal
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 7\nPre-Depression Phase\nSiang Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);

        // Seq1 Memburuk
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq1Memburuk", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f; // Night
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nManifestasi Supernatural";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Strong supernatural presence
        PlayAudio("supernatural_presence");
        PlayAudio("whispers_dark");
        
        yield return new WaitForSeconds(2f);
        
        // Seq2 Gangguansupranatural
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq2Gangguansupranatural", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 8
        Debug.Log("Day 7 finished! Moving to Day 8...");
        GoToNextDay();
    }
}
