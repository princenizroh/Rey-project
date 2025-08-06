using UnityEngine;
using System.Collections;

public class NarratorDay8 : NarratorBase
{
    // Day 8 - Final Baby Blues Phase / Pre-Postpartum Depression
    // Complete breakdown, most critical supernatural encounter
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0); // Bedroom - complete withdrawal
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        
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
        
        FadeCloseEyes(); // Baby sleeps from trauma
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f; // Night
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nGangguan Supernatural Terparah";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Most intense supernatural encounter
        PlayAudio("supernatural_intense");
        PlayAudio("demon_presence");
        PlayAudio("wind_howling");
        
        yield return new WaitForSeconds(2f);
        
        // Seq4 GangguanSetanSangatParah
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq4GangguanSetanSangatParah", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 5f)); 
        }
        
        yield return new WaitForSeconds(3f);
        
        // Auto progression to Day 9 (Postpartum Depression begins)
        Debug.Log("Day 8 finished! Moving to Day 9 - Postpartum Depression begins...");
        GoToNextDay();
    }
}
