using UnityEngine;
using System.Collections;

public class NarratorDay12 : NarratorBase
{
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 13.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 1);
        SetCharacterSpawn(CharacterType.Father, 1);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 12\nPertolongan Pertama";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Seq1 MemanggilPertolongan
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq1MemanggilPertolongan", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother comes to baby
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        // Seq2 MelakukanPertolongan
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq2MelakukanPertolongan", 
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
        yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 1.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nHarapan dan Syukur";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        // Seq3 RasaSyukur
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq3RasaSyukur", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq4 HarapanKedepan
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq4HarapanKedepan", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        // Final day, end narration
        
    }
}
