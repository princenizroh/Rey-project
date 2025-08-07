using UnityEngine;
using System.Collections;

public class NarratorDay8 : NarratorBase
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
        uiElements.narratorText.text = "Day 8\nIbu Marah Besar";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes();
        yield return new WaitForSeconds(1f);

        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq1Lapar", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq2MencariIbu", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq3MarahBesar", 
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
        yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 1.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Ghost, 0);

        yield return new WaitForSeconds(1f);
        // PlayAudio("supernatural_intense");
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq4GangguanSetanSangatParah", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day8/Seq5Keputusasaan", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextDay();
    }
}
