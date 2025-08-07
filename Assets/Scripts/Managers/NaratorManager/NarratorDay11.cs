using UnityEngine;
using System.Collections;

public class NarratorDay11 : NarratorBase
{
    // Day 11 - Father Begins Intervention
    // Father realizes the severity and starts taking action
    
    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 8.0f; // Morning
        SetCharacterSpawn(CharacterType.Baby, 4);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 11\nFather's Intervention\nPagi Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Seq1 AyahSadar
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq1AyahSadar", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Father comes to baby
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 4));
        
        // Seq2 PerhatianAyah
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq2PerhatianAyah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeFront());
        TimeManager.instance.TimeOfDay = 13.0f; // Afternoon
        SetCharacterSpawn(CharacterType.Baby, 1);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 1);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Siang Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Seq3 AyahMerawat
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq3AyahMerawat", 
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
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nHarapan Muncul";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        // Seq4 BicaraAyahIbu
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq4BicaraAyahIbu", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq5 RencanaAyah
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq5RencanaAyah", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 12
        Debug.Log("Day 11 finished! Moving to Day 12...");
        GoToNextDay();
    }
}
