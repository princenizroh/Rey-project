using UnityEngine;
using System.Collections;

public class NarratorDay3 : NarratorBase
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
        uiElements.narratorText.text = "Day 3\nPagi Kedua";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        // Seq1 PagiKedua
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq1PagiKedua", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to living room for departure
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 1));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));
        
        // Seq2 KepergianAyah
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq2KepergianAyah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        // Father leaves
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 2));
        
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
        
        // Seq3 Rewel
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq3Rewel", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
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
        
        // Seq5 Mengompol
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq5Mengompol", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to living room for father's return
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Baby, 1));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 1));
        
        // Seq6 AyahPulang
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq6AyahPulang", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother approaches
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));
        
        // Seq7 IbuMenghampiriAyah
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq7IbuMenghampiriAyah", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
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
        
        // Move to living room for dinner discussion
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 1));
        
        // Seq8 PersiapanBesok
        bool seq8Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq8PersiapanBesok", 
            () => { seq8Complete = true; });
        yield return new WaitUntil(() => seq8Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq9 BuatinSusu
        bool seq9Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq9BuatinSusu", 
            () => { seq9Complete = true; });
        yield return new WaitUntil(() => seq9Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        // Add midnight sequence
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
        
        // Seq10 TerbangunTerakhir
        bool seq10Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq10TerbangunTerakhir", 
            () => { seq10Complete = true; });
        yield return new WaitUntil(() => seq10Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 4
        Debug.Log("Day 3 finished! Moving to Day 4...");
        GoToNextDay();
    }
}
