using UnityEngine;
using System.Collections;

public class NarratorDay9 : NarratorBase
{
    // Day 9 - Depresi Postpartum Day 1
    // Multiple POV - severe emotional breakdown and supernatural manifestation
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0); // Bedroom - severe depression
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 9\nDepresi Postpartum - Day 1\nSiang Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);

        // Seq1 Breakdownemosi
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day9/Seq1Breakdownemosi", 
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
        
        // Supernatural manifestation at its peak
        PlayAudio("supernatural_manifestation");
        PlayAudio("reality_distortion");
        
        yield return new WaitForSeconds(2f);
        
        // Seq2 Manifestasisupranatural
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day9/Seq2Manifestasisupranatural", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 4f)); 
        }
        
        yield return new WaitForSeconds(3f);
        
        // Auto progression to Day 10
        Debug.Log("Day 9 finished! Moving to Day 10...");
        GoToNextDay();
    }
}
