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
        
        yield return new WaitForSeconds(1f);
        
        // Move back to bedroom for bathing
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        // Seq3 Mandi
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq3Mandi", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
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
        
        // Seq4 Lapar
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq4Lapar", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq5 Rewel
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq5Rewel", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq6 Mengompol
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq6Mengompol", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq7 Kesal
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq7Kesal", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
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
        
        // Seq8 AyahPulang
        bool seq8Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq8AyahPulang", 
            () => { seq8Complete = true; });
        yield return new WaitUntil(() => seq8Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother approaches
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));
        
        // Seq9 MenyambutAyah
        bool seq9Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq9MenyambutAyah", 
            () => { seq9Complete = true; });
        yield return new WaitUntil(() => seq9Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to kitchen for dinner
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 2));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 2));
        
        // Seq10 MakanMalam
        bool seq10Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq10MakanMalam", 
            () => { seq10Complete = true; });
        yield return new WaitUntil(() => seq10Complete);
        
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
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq11 ButuhSusu
        bool seq11Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq11ButuhSusu", 
            () => { seq11Complete = true; });
        yield return new WaitUntil(() => seq11Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq12 SusuBotol
        bool seq12Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq12SusuBotol", 
            () => { seq12Complete = true; });
        yield return new WaitUntil(() => seq12Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq13 Terbangun
        bool seq13Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq13Terbangun", 
            () => { seq13Complete = true; });
        yield return new WaitUntil(() => seq13Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 4
        Debug.Log("Day 3 finished! Moving to Day 4...");
        GoToNextDay();
    }
}
