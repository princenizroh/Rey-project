using UnityEngine;
using System.Collections;

public class NarratorDay2 : NarratorBase
{
    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {
        TimeManager.instance.TimeOfDay = 0.25f; // Morning
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0);  
        SetCharacterSpawn(CharacterType.Father, 0);    
        SetCharacterSpawn(CharacterType.Baby, 0);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 2\nHari Pertamaku";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        // Seq1 PagiPertama
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq1PagiPertama", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to living room for departure
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 1));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));
        
        // Seq2 KeberangkatanAyah
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq2KeberangkatanAyah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Father leaves
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 2));
        
        // Seq3 Mandi
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq3Mandi", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Move back to bedroom
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        // Seq4 SelesaiMandi
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq4SelesaiMandi", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Siang Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq5 Lapar
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq5Lapar", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        FadeCloseEyes(); // Baby sleeps again
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayEveningSequence()
    {
        TimeManager.instance.TimeOfDay = 0.75f; // Evening
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Sore Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq6 Mengompol
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq6Mengompol", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to living room for father's return
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Baby, 1));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 1));
        
        // Seq7 AyahPulang
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq7AyahPulang", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother approaches
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));
        
        // Seq8 MenyambutAyah
        bool seq8Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq8MenyambutAyah", 
            () => { seq8Complete = true; });
        yield return new WaitUntil(() => seq8Complete);
        
        FadeCloseEyes(); // Scene transition
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f; // Night
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Move to kitchen for cooking
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 2));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 2));
        
        // Seq9 Memasak
        bool seq9Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq9Memasak", 
            () => { seq9Complete = true; });
        yield return new WaitUntil(() => seq9Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq10 BuatinSusu
        bool seq10Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq10BuatinSusu", 
            () => { seq10Complete = true; });
        yield return new WaitUntil(() => seq10Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        // Add midnight sequence instead of going to next day
        StartCoroutine(PlayMidnightSequence());
        yield break;
    }
    
    [System.Obsolete]
    protected IEnumerator PlayMidnightSequence()
    {
        TimeManager.instance.TimeOfDay = 0.0f; // Midnight
        
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.text = "Tengah Malam";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq11 Terbangun
        bool seq11Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq11Terbangun", 
            () => { seq11Complete = true; });
        yield return new WaitUntil(() => seq11Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 3
        Debug.Log("Day 2 finished! Moving to Day 3...");
        GoToNextDay();
    }
}
