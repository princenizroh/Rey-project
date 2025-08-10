using UnityEngine;
using System.Collections;

public class NarratorDay4 : NarratorBase
{    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        DisableNavMeshAgent(CharacterType.Ghost);
        // SetObjectsActive(gameObjects.activeObjects, true);
        CloseEyes();
        StartCoroutine(SwitchLights.Instance.SwitchToDark());
        yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 13.0f; 
        SetCharacterSpawn(CharacterType.Baby, 0);   
        SetCharacterSpawn(CharacterType.Mother, 0); 
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Day 4\n Tempat Berbeda";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq1TempatBerbeda", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);



        // PlayCharacterAnimation(CharacterType.Object, "OpenTheDoor");
        
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));

        StartCoroutine(SwitchLights.Instance.SwitchToBright());
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby)); 

        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq2IbuMarah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq3GangguanTelpon", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        StartCoroutine(ResetHeadTracking());
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));
        
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq4Telephone", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        FadeCloseEyes(); 
        yield return new WaitForSeconds(5f);

        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq5Kerja", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayEveningSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeRight());
        // AppearObjects();
        TimeManager.instance.TimeOfDay = 18.0f; 
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        yield return new WaitForSeconds(3f);
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq6Lapar", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq7IbuMarahLagi", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
        yield return new WaitForSeconds(1f);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        SetCharacterSpawn(CharacterType.Baby, 1);
        SetCharacterSpawn(CharacterType.Mother, 1);
        yield return new WaitForSeconds(2f);
        FadeOpenEyes(); 
        
        bool seq8Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq8Stres", 
            () => { seq8Complete = true; });
        yield return new WaitUntil(() => seq8Complete);
        
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

        // Mother focuses on baby, Ghost appears mysteriously
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby));

        yield return new WaitForSeconds(1f);
        
        // PlayAudio("wind_light");
        
        yield return new WaitForSeconds(2f);
        FadeOpenEyes(); 
        
        yield return new WaitForSeconds(1f);
        
        // Mother notices Ghost movement
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Ghost));
        
        yield return StartCoroutine(MoveCharacterToPosition(CharacterType.Ghost, 0, 0.5f));
        bool seq9Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq9GangguanSetanRingan", 
            () => { seq9Complete = true; });
        yield return new WaitUntil(() => seq9Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother looks forward while moving to respond
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Object));
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        // Mother looks back at Ghost after moving
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Ghost));
        
        bool seq10Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq10Maaf", 
            () => { seq10Complete = true; });
        yield return new WaitUntil(() => seq10Complete);
        
        // if (audioSource != null && audioSource.isPlaying)
        // {
        //     StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        // }
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextDay();
    }
}
