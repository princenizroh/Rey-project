using UnityEngine;
using System.Collections;

public class NarratorDay3WithRaycast : NarratorBase
{
    [Header("Raycast Interaction")]
    public RaycastObjectCam raycastCamera;
    
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
    
    /// <summary>
    /// Wait for player to stare at a raycast object (raycastStatus becomes true) and then press E
    /// </summary>
    /// <param name="objectName">Name of the object to wait for interaction with</param>
    /// <returns></returns>
    private IEnumerator WaitForRaycastInteraction(string objectName = "")
    {
        bool interactionCompleted = false;
        bool wasStaring = false;
        
        while (!interactionCompleted)
        {
            // Check if player is currently staring at a raycast object
            if (raycastCamera.raycastStatus)
            {
                wasStaring = true;
                
                // Wait for player to press E while staring
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Function executed when interaction happens
                    Debug.Log($"Player interacted with raycast object: {objectName}");
                    interactionCompleted = true;
                }
            }
            else
            {
                // Player stopped staring, reset the staring flag
                wasStaring = false;
            }
            
            yield return null; // Wait one frame
        }
    }

    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {
        CloseEyes();
        DisableNavMeshAgent(CharacterType.Mother);
        DisableNavMeshAgent(CharacterType.Father);
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 8.00f; 
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0);  
        SetCharacterSpawn(CharacterType.Father, 0);    
        SetCharacterSpawn(CharacterType.Baby, 0);
        PlayCharacterAnimation(CharacterType.Father, "Sit");
        PlayCharacterAnimation(CharacterType.Mother, "Idle");
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Day 3\nPagi Kedua";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        bool seq0Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq0PagiKedua", 
            () => { seq0Complete = true; });
        yield return new WaitUntil(() => seq0Complete);
        
        yield return new WaitForSeconds(0.5f);

        FadeOpenEyes();

        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq1PagiKedua", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);

        // Wait for player to interact with an object before continuing
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Look at an object and press E to continue...";
        yield return StartCoroutine(WaitForRaycastInteraction("Morning Object"));
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Execute function when interaction happens
        {
            // Function here - player successfully interacted
            Debug.Log("Morning interaction completed - continuing story");
        }

        FadeCloseEyes();

        yield return new WaitForSeconds(2f);
        SetCharacterSpawn(CharacterType.Baby, 1); 
        SetCharacterSpawn(CharacterType.Father, 1);
        SetCharacterSpawn(CharacterType.Mother, 1);

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(SetCameraPanRangeFront());
        FadeOpenEyes(); 

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 0));
        SetCharacterSpawn(CharacterType.Father, 2); 
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 1));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 1));

        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq2KepergianAyah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 2));
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 2));
        

        // Seq3 Mandi - Another interaction point
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq3Mandi", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);

        // Wait for another interaction
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Look at the bathroom area and press E...";
        yield return StartCoroutine(WaitForRaycastInteraction("Bathroom"));
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Execute function when interaction happens
        {
            // Function here - bathroom interaction completed
            Debug.Log("Bathroom interaction completed");
        }

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
        
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq4Lapar", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        // Wait for interaction during afternoon
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Look at the hungry baby and press E...";
        yield return StartCoroutine(WaitForRaycastInteraction("Baby"));
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Execute function when interaction happens
        {
            // Function here - baby interaction completed
            Debug.Log("Baby hunger interaction completed");
        }
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 3));
        yield return new WaitForSeconds(1f);
        
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq5Rewel", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
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

        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq6Mengompol", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Wait for interaction before mother moves
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Look at the wet spot and press E...";
        yield return StartCoroutine(WaitForRaycastInteraction("Wet Spot"));
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Execute function when interaction happens
        {
            // Function here - wet spot interaction completed
            Debug.Log("Wet spot interaction completed");
        }
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 3));
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq7Kesal", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
        yield return new WaitForSeconds(1f);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(SetCameraPanRangeFront());
        SetCharacterSpawn(CharacterType.Baby, 1); 
        SetCharacterSpawn(CharacterType.Father, 3);
        SetCharacterSpawn(CharacterType.Mother, 1);
        yield return new WaitForSeconds(2f);
        FadeOpenEyes(); 

        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 3));
        
        bool seq8Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq8AyahPulang", 
            () => { seq8Complete = true; });
        yield return new WaitUntil(() => seq8Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Wait for interaction before mother moves to greet father
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Look at father and press E to greet him...";
        yield return StartCoroutine(WaitForRaycastInteraction("Father"));
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Execute function when interaction happens
        {
            // Function here - father greeting interaction completed
            Debug.Log("Father greeting interaction completed");
        }
        
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
        bool seq9Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq9MenyambutAyah", 
            () => { seq9Complete = true; });
        yield return new WaitUntil(() => seq9Complete);

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
        yield return StartCoroutine(SetCameraPanRangeFront());
        TimeManager.instance.TimeOfDay = 1.0f;
        SetCharacterSpawn(CharacterType.Baby, 1);
        SetCharacterSpawn(CharacterType.Mother, 4);
        SetCharacterSpawn(CharacterType.Father, 4);

        yield return new WaitForSeconds(1f);
        FadeOpenEyes();
        yield return new WaitForSeconds(1f);

        bool seq10Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq10MakanMalam",
            () => { seq10Complete = true; });
        yield return new WaitUntil(() => seq10Complete);

        yield return new WaitForSeconds(1f);

        // Wait for interaction during dinner
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Look at the dinner table and press E...";
        yield return StartCoroutine(WaitForRaycastInteraction("Dinner Table"));
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Execute function when interaction happens
        {
            // Function here - dinner table interaction completed
            Debug.Log("Dinner table interaction completed");
        }

        bool seq11Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq11ButuhSusu",
            () => { seq11Complete = true; });
        yield return new WaitUntil(() => seq11Complete);

        yield return new WaitForSeconds(1f);
        EnableNavMeshAgent(CharacterType.Father);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 3));

        bool seq12Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq12SusuBotol",
            () => { seq12Complete = true; });
        yield return new WaitUntil(() => seq12Complete);
        yield return new WaitForSeconds(1f);

        FadeCloseEyes();
        yield return new WaitForSeconds(2f);

        StartCoroutine(PlayMidnightSequence());
        yield break;
    }
    
    [System.Obsolete]
    protected IEnumerator PlayMidnightSequence()
    {
        CloseEyes();
        EnableNavMeshAgent(CharacterType.Mother);
        EnableNavMeshAgent(CharacterType.Father);
        
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 1.0f; 
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 5);
        SetCharacterSpawn(CharacterType.Father, 5);
        
        yield return new WaitForSeconds(3f);
        FadeOpenEyes(); 
        yield return new WaitForSeconds(3f);

        // Wait for final interaction before waking up
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Look at the crying baby and press E...";
        yield return StartCoroutine(WaitForRaycastInteraction("Crying Baby"));
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Execute function when interaction happens
        {
            // Function here - crying baby interaction completed
            Debug.Log("Crying baby interaction completed - parents will wake up");
        }
         
        bool seq13Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day3/Seq13Terbangun", 
            () => { seq13Complete = true; });
        yield return new WaitUntil(() => seq13Complete);
        yield return new WaitForSeconds(1f);

        SetCharacterSpawn(CharacterType.Mother, 6);
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 3));
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        GoToNextDay();
    }
}
