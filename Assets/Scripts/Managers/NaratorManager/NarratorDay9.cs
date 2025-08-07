using UnityEngine;
using System.Collections;

public class NarratorDay9 : NarratorBase
{
    // Day 9 - Postpartum Depression Begins
    // Mother's condition becomes clinical, baby's environment deteriorates
    
    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 8.0f; // Morning
        SetCharacterSpawn(CharacterType.Baby, 4);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 1);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 9\nPostpartum Depression Begins\nPagi Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Seq1 Lapar
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day9/Seq1Lapar", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
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
        SetCharacterSpawn(CharacterType.Baby, 4);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 1);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Siang Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Father comes to help
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 4));
        
        // Seq2 AyahDatang
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day9/Seq2AyahDatang", 
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
        uiElements.narratorText.text = "Malam Hari\nKegelapan Mengintai";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        // Seq3 Sendirian
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day9/Seq3Sendirian", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq4 GangguanSetanMalam
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day9/Seq4GangguanSetanMalam", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 10
        Debug.Log("Day 9 finished! Moving to Day 10...");
        GoToNextDay();
    }
}
