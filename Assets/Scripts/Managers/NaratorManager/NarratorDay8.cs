using UnityEngine;
using System.Collections;

public class NarratorDay8 : NarratorBase
{
    // Day 8 - Final Baby Blues Phase / Pre-Postpartum Depression
    // Complete breakdown, most critical supernatural encounter
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 13.0f; // Afternoon
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        SetCharacterSpawn(CharacterType.Mother, 0); // Bedroom - complete withdrawal
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 8\nFinal Baby Blues Phase\nSiang Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);

        // Seq1 Lapar
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq1Lapar", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Baby realizes mother is not coming
        // Seq2 KemanaIbu
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq2KemanaIbu", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother finally comes in complete breakdown
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
        // Seq3 IbuMarahBesar
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq3IbuMarahBesar", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 20.0f; // Night
        SetCharacterSpawn(CharacterType.Baby, 4);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nGangguan Supernatural Memuncak";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Intense supernatural disturbance
        PlayAudio("supernatural_intense");
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        // Seq4 GangguanSetanKuat
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq4GangguanSetanKuat", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq5 Keputusasaan
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq5Keputusasaan", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 9
        Debug.Log("Day 8 finished! Moving to Day 9...");
        GoToNextDay();
    }
}
