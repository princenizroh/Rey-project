using UnityEngine;
using System.Collections;

public class NarratorDay7 : NarratorBase
{
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 13.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 7\nSendirian";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes();
        yield return new WaitForSeconds(1f);

        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq1Lapar", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq2Sendirian", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayEveningSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 18.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        yield return new WaitForSeconds(1f);
        
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq3IbuPulang", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        
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
        yield return new WaitForSeconds(1f);
        
        
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq4Kelaparan", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq5SosokMenyeramkan", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));

        yield return new WaitForSeconds(1f);
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq6Khawatir", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);

        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextDay();
    }
}
