using UnityEngine;
using System.Collections;

public class NarratorDay11 : NarratorBase
{
    // Day 11 - Psikosis Postpartum - Critical Phase
    // Mother's disconnection from reality, severe neglect
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 5); // Stairs area - dangerous location
        SetCharacterSpawn(CharacterType.Baby, 0);   // Parents' room
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 11\nPsikosis Postpartum\nCritical Phase";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);

        // Seq1 BerbicaraAneh
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq1BerbicaraAneh", 
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
        uiElements.narratorText.text = "Malam Hari\nKelaparan Ekstrim";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Phone ringing - external world trying to reach
        PlayAudio("phone_ringing_continuous");
        
        yield return new WaitForSeconds(2f);

        // Seq2 Kelaparan
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq2Kelaparan", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq3 Kelaparan (continuation)
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq3Kelaparan", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        yield return new WaitForSeconds(3f);
        
        // Auto progression to Day 12 (Father's return)
        Debug.Log("Day 11 finished! Moving to Day 12 - Father returns...");
        GoToNextDay();
    }
}
