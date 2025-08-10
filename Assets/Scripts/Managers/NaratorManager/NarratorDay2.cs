using UnityEngine;
using System.Collections;

public class NarratorDay2 : NarratorBase
{
    /// <summary>
    /// Handle dialog interaction with parents - specific to Day2
    /// Returns true if correct interaction (Seq12AAyah), false if wrong (Seq12BIbu)
    /// </summary>
    private IEnumerator HandleOrangTuaDialog()
    {
        // TODO: Implement proper choice detection logic
        // For now, simulate random choice for testing
        
        // Simulate player choice - in real implementation this would be based on
        // which character/object player is looking at when pressing E
        bool isCorrectChoice = UnityEngine.Random.value > 0.5f; // Temporary random for demo
        
        if (isCorrectChoice)
        {
            // Player chose Father (Seq12AAyah) - CORRECT choice
            bool seq12AComplete = false;
            dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq12AAyah", 
                () => { seq12AComplete = true; });
            yield return new WaitUntil(() => seq12AComplete);
            
            Debug.Log("[NarratorDay2] Correct choice - Seq12AAyah played");
            lastInteractionResult = true; // Set flag for correct choice
        }
        else
        {
            // Player chose Mother (Seq12BIbu) - WRONG choice  
            bool seq12BComplete = false;
            dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq12AIbu", 
                () => { seq12BComplete = true; });
            yield return new WaitUntil(() => seq12BComplete);
            
            Debug.Log("[NarratorDay2] Wrong choice - Seq12BIbu played, will repeat");
            lastInteractionResult = false; // Set flag for wrong choice
        }
    }
    
    private bool lastInteractionResult = false;
    


    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {
        CloseEyes();
        DisableNavMeshAgent(CharacterType.Father);
        DisableNavMeshAgent(CharacterType.Mother);
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 8.00f; 
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0);  
        SetCharacterSpawn(CharacterType.Father, 0);    
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Object, 1);
        PlayCharacterAnimation(CharacterType.Father, "Sitting_Talking");
        PlayCharacterAnimation(CharacterType.Mother, "Idle");
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby));
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Day 2\nHari Pertamaku";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        bool seq0Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq0PagiPertama", 
            () => { seq0Complete = true; });
        yield return new WaitUntil(() => seq0Complete);
        yield return new WaitForSeconds(0.5f);
        FadeOpenEyes();
        yield return new WaitForSeconds(2f);
        PlayCharacterAnimation(CharacterType.Mother, "Angry");
        yield return new WaitForSeconds(1f);
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq1PagiPertama", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);

        FadeCloseEyes();
        
        StartCoroutine(ResetHeadTracking());
        yield return new WaitForSeconds(2f);
        SetCharacterSpawn(CharacterType.Baby, 1); 
        SetCharacterSpawn(CharacterType.Father, 1);
        SetCharacterSpawn(CharacterType.Mother, 1);

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(SetCameraPanRangeFront());
        FadeOpenEyes(); 

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 0));
        SetCharacterSpawn(CharacterType.Father, 2); 
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 1));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));
        PlayCharacterAnimation(CharacterType.Father, "Left Turn");
        PlayCharacterAnimation(CharacterType.Mother, "Right Turn");
        
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq2KeberangkatanAyah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 2));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 2));
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby));
        
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq3Mandi", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        StartCoroutine(ResetHeadTracking());
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby));
        yield return StartCoroutine(SetCameraPanRangeRight());
        SetCharacterSpawn(CharacterType.Baby, 2); 
        SetCharacterSpawn(CharacterType.Mother, 2);
        yield return new WaitForSeconds(2f);

        FadeOpenEyes(); 

        yield return new WaitForSeconds(1f);
        
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq4SelesaiMandi", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 13.0f; 
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 3); 
        
        yield return new WaitForSeconds(1f);
        
        FadeOpenEyes(); 

        yield return new WaitForSeconds(1f);
        
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq5Lapar", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 3));
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby));
        
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq6Rewel", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayEveningSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 18.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 3);
        yield return new WaitForSeconds(1f);
        
        FadeOpenEyes();
        yield return new WaitForSeconds(1f);
        
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq7Mengompol", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 3));
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby));
        bool seq8Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq8Kesal", 
            () => { seq8Complete = true; });
        yield return new WaitUntil(() => seq8Complete);
        
        yield return new WaitForSeconds(1f);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(SetCameraPanRangeFront());
        SetCharacterSpawn(CharacterType.Baby, 1);
        SetCharacterSpawn(CharacterType.Mother, 1);
        SetCharacterSpawn(CharacterType.Father, 3);
        FadeOpenEyes();

        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 3));
        StartCoroutine(SetHeadTarget(CharacterType.Father, CharacterTarget.Baby));
        
        bool seq9Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq9AyahPulang", 
            () => { seq9Complete = true; });
        yield return new WaitUntil(() => seq9Complete);
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Father, 0.2f));
        StartCoroutine(SetHeadTarget(CharacterType.Father, CharacterTarget.Mother, 0.2f));
        
        bool seq10Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq10MenyambutAyah", 
            () => { seq10Complete = true; });
        yield return new WaitUntil(() => seq10Complete);

        yield return new WaitForSeconds(1f);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(4f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        CloseEyes();
        DisableNavMeshAgent(CharacterType.Mother);
        DisableNavMeshAgent(CharacterType.Father);
        yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 20.0f; 
        SetCharacterSpawn(CharacterType.Baby, 3);
        SetCharacterSpawn(CharacterType.Mother, 4);
        SetCharacterSpawn(CharacterType.Father, 4);


        yield return new WaitForSeconds(1f);
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Repeat interaction until player makes correct choice (Seq12AAyah)
        bool interactionComplete = false;
        while (!interactionComplete)
        {
            // Wait for player to interact with parents using raycast
            yield return StartCoroutine(WaitForRaycastInteraction(() => {
                // This will be called when player presses E while looking at interactable object
                StartCoroutine(HandleOrangTuaDialog());
            }));
            
            // Wait for dialog to complete
            yield return new WaitForSeconds(0.5f);
            
            // Check result - if correct choice (Seq12AAyah), exit loop
            if (lastInteractionResult)
            {
                interactionComplete = true;
                Debug.Log("[NarratorDay2] Correct interaction completed, continuing story");
            }
            else
            {
                Debug.Log("[NarratorDay2] Wrong choice, repeating interaction");
                yield return new WaitForSeconds(1f); // Small delay before allowing next interaction
            }
        }

        yield return new WaitForSeconds(1f);
        
        
        bool seq12Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq12BuatinSusu", 
            () => { seq12Complete = true; });
        yield return new WaitUntil(() => seq12Complete);
        
        yield return new WaitForSeconds(1f);
        
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 4));
        
        
        bool seq13Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq13SusuBotol", 
            () => { seq13Complete = true; });
        yield return new WaitUntil(() => seq13Complete);
        
        yield return new WaitForSeconds(1f);
        FadeCloseEyes();
        yield return new WaitForSeconds(4f);

        StartCoroutine(PlayMidnightSequence());
        yield break;
    }
    
    [System.Obsolete]
    protected IEnumerator PlayMidnightSequence()
    {
        CloseEyes();
        DisableNavMeshAgent(CharacterType.Mother);
        DisableNavMeshAgent(CharacterType.Father);
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 1.0f; 
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 5);
        SetCharacterSpawn(CharacterType.Father, 5);
        yield return new WaitForSeconds(3f);
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        bool seq14Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq14Terbangun", 
            () => { seq14Complete = true; });
        yield return new WaitUntil(() => seq14Complete);
        
        SetCharacterSpawn(CharacterType.Mother, 6);
        EnableNavMeshAgent(CharacterType.Mother);
        yield return new WaitForSeconds(1f);
        
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 3));
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby));
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextDay();
    }
}
