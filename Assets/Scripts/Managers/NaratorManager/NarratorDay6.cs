using UnityEngine;
using System.Collections;

public class NarratorDay6 : NarratorBase
{
    // Day 6 - Baby Blues Escalation
    // Mother starts showing signs of deeper psychological issues
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0); // Bedroom - showing withdrawal
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 6\nBaby Blues Escalation\nSiang Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);

        // Seq1 Keraguan
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day6/Seq1Keraguan", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq2 Heran
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day6/Seq2Heran", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq3 Gangguan
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day6/Seq3Gangguan", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f; // Night
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nSosok Mendekat";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq4 Sosok
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day6/Seq4Sosok", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq5 Mendekat
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day6/Seq5Mendekat", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq6 Khawatir
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day6/Seq6Khawatir", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 7
        Debug.Log("Day 6 finished! Moving to Day 7...");
        GoToNextDay();
    }
}
