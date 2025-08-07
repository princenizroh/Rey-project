using UnityEngine;
using System.Collections;

public class NarratorDay6 : NarratorBase
{
    // Day 6 - Baby Blues Escalation
    // Mother starts showing signs of deeper psychological issues
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 13.0f; // Afternoon
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        SetCharacterSpawn(CharacterType.Mother, 0); // Bedroom - showing withdrawal
        uiElements.narratorText.gameObject.SetActive(true);
        
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
        
        FadeCloseEyes(); // Baby sleeps due to fear
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayEveningSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 18.0f; // Evening
        SetCharacterSpawn(CharacterType.Baby, 4);
        SetCharacterSpawn(CharacterType.Mother, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Sore Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); // Baby still sleeping
        yield return new WaitForSeconds(1f);
        
        // Mother comes to check
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
        // Seq2 Heran
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day6/Seq2Heran", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
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
        uiElements.narratorText.text = "Malam Hari\nSosok Mendekat";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq3 Gangguan
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day6/Seq3Gangguan", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
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
        
        // Mother comes to help
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
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
