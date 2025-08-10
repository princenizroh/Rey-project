using UnityEngine;
using System.Collections;

public class NarratorDay1 : NarratorBase
{
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f;
        AppearObjects();
        DisableNavMeshAgent(CharacterType.Mother);
        DisableNavMeshAgent(CharacterType.Father);
        SetCharacterSpawn(CharacterType.Mother, 0);  
        SetCharacterSpawn(CharacterType.Father, 0);    
        SetCharacterSpawn(CharacterType.Bidan, 0);   
        SetCharacterSpawn(CharacterType.Baby, 0);    
        SetCharacterSpawn(CharacterType.Object, 0);
        CloseEyes(); 
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        PlayCharacterAnimation(CharacterType.Father, "Sit");
        PlayCharacterAnimation(CharacterType.Bidan, "Idle");
        
        // Enable head tracking - all characters focus on baby (POV camera)
        HeadTrackingManager.Instance.SetHeadTarget(CharacterType.Mother, HeadTarget.Camera, 1f);
        HeadTrackingManager.Instance.SetHeadTarget(CharacterType.Father, HeadTarget.Camera, 1f);
        HeadTrackingManager.Instance.SetHeadTarget(CharacterType.Bidan, HeadTarget.Camera, 1f);

        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 1\nKelahiran";
        yield return new WaitForSeconds(5f); 
        uiElements.narratorText.gameObject.SetActive(false);

        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/Seq1DalamPerut", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);

        yield return new WaitForSeconds(0.3f);
        
        PlayAudio("baby_crying");

        yield return new WaitForSeconds(1f);

        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/Seq2Terlahir", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);

        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 4f)); 
        }
        yield return new WaitForSeconds(3f);
      
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/Seq3Kesehatan", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);

        yield return new WaitForSeconds(1f);

        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/Seq4Kesadaran", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);

        yield return new WaitForSeconds(1f);

        yield return new WaitForSeconds(0.5f);
        FadeOpenEyes();
        
        // When baby opens eyes - parents look at each other in amazement
        HeadTrackingManager.Instance.SetHeadTarget(CharacterType.Mother, HeadTarget.Father, 1f);
        HeadTrackingManager.Instance.SetHeadTarget(CharacterType.Father, HeadTarget.Mother, 1f);
        
        yield return new WaitForSeconds(2f);
        
        // Then back to looking at baby
        HeadTrackingManager.Instance.SetHeadTarget(CharacterType.Mother, HeadTarget.Camera, 1f);
        HeadTrackingManager.Instance.SetHeadTarget(CharacterType.Father, HeadTarget.Camera, 1f);
        
        yield return new WaitForSeconds(28f);
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/Seq5MembukaMata", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(MoveCharacterToPosition(CharacterType.Baby, 0, 2f));
        
        yield return new WaitForSeconds(2f);

        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/Seq6Makanan", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);

        yield return new WaitForSeconds(1f);
        
        // Disable head tracking during movement for natural look
        HeadTrackingManager.Instance.DisableHeadTracking(CharacterType.Bidan);
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Bidan, 0));
        FadeCloseEyes();

        yield return new WaitForSeconds(1f);
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/Seq7Nama", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        SetCharacterSpawn(CharacterType.Object, 1);

        // Disable all head tracking before transition
        HeadTrackingManager.Instance.EnableGlobalHeadTracking(false);
        
        yield return new WaitForSeconds(2f);
        GoToNextDay();
    }

}
