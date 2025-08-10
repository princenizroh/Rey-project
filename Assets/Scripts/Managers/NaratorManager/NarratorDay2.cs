using UnityEngine;
using System.Collections;

public class NarratorDay2 : NarratorBase
{
    public RaycastObjectCam raycastCamera;

    [System.Obsolete]
    void Start()
    {
        // Find the raycast camera if not assigned
        if (raycastCamera == null)
        {
            raycastCamera = FindObjectOfType<RaycastObjectCam>();
            if (raycastCamera == null)
            {
                Debug.LogError("RaycastObjectCam not found! Please assign it in the inspector.");
            }
        }
    }

    [System.Obsolete]
    private IEnumerator WaitForRaycastInteraction(string code = "")
    {
        bool interactionCompleted = false;
        bool wasStaring = false;
        
        while (!interactionCompleted)
        {
            if (raycastCamera.raycastStatus)
            {
                wasStaring = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (code == "interaksi_ortu")
                    {
                        bool seq11Complete = false;
                        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq11Memasak", 
                            () => { seq11Complete = true; });
                        yield return new WaitUntil(() => seq11Complete);
                    }
                    // Set interaction as completed after the dialog finishes
                    interactionCompleted = true;
                }
            }
            else
            {
                wasStaring = false;
            }
            
            yield return null;
        }
    }


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
        
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq3Mandi", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
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
        
        bool seq9Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq9AyahPulang", 
            () => { seq9Complete = true; });
        yield return new WaitUntil(() => seq9Complete);
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
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

        yield return StartCoroutine(WaitForRaycastInteraction("interaksi_ortu"));
        
        // bool seq11Complete = false;
        // dialogGameManager.StartCoreGame("GameData/Dialog/Day2/Seq11Memasak", 
        //     () => { seq11Complete = true; });
        // yield return new WaitUntil(() => seq11Complete);

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
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextDay();
    }
}
