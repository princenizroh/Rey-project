using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NarratorDay12 : NarratorBase
{
    [Header("Choice UI Elements - Assign in Inspector")]
    [SerializeField] private GameObject choicePanel; 
    [SerializeField] private Button angryChoiceButton; 
    [SerializeField] private Button concernedChoiceButton; 
    [SerializeField] private Animator animator;
    
    private bool choiceMade = false;
    private bool selectedAngryPath = true; 
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        saveFileManager.UpdateCoreGameSaves(11, 1);
        saveFileManager.SaveToLocalMyGamesFolder();
        
        AppearObjects();
        // yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 13.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);

        animator.Play("OpenTheDoor"); 
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Day 12\nKekacauan";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        SetRaycastContext("Day12", "Afternoon");
        this.EnableRaycastInteraction();

        // Wait for CORRECT parent interaction - loop until player interacts with Father (Mulyono)
        bool correctInteraction = false;
        while (!correctInteraction)
        {
            yield return StartCoroutine(WaitForRaycastInteraction((characterIdentity) => {

                if (characterIdentity == "Object") 
                {
                    correctInteraction = true; // Exit the loop
                }
            }, "Day12", "Afternoon"));
            
            // Small delay before allowing next interaction attempt
            if (!correctInteraction)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Disable raycast interaction system after player made correct choice
        this.DisableRaycastInteraction();
        // animator.Play("OpenTheDoor"); 
        // PlayCharacterAnimation(CharacterType.Object, "OpenTheDoor");
        yield return new WaitForSeconds(1f);

        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq1KepulanganAyah", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);


        // SetRaycastContext("Day12", "Afternoon");

        // // Enable raycast interaction system for player choice
        // this.EnableRaycastInteraction();
        //
        // // Wait for CORRECT parent interaction - loop until player interacts with Father (Mulyono)
        // bool correctInteraction = false;
        // while (!correctInteraction)
        // {
        //     yield return StartCoroutine(WaitForRaycastInteraction((characterIdentity) => {
        //
        //         if (characterIdentity == "Object") 
        //         {
        //             correctInteraction = true; // Exit the loop
        //         }
        //     }, "Day12", "Afternoon"));
        //     
        //     // Small delay before allowing next interaction attempt
        //     if (!correctInteraction)
        //     {
        //         yield return new WaitForSeconds(0.5f);
        //     }
        // }
        //
        // // Disable raycast interaction system after player made correct choice
        // this.DisableRaycastInteraction();

        // PlayCharacterAnimation(CharacterType.Object, "OpenTheDoor");
        
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq2Berantakan", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(ShowChoiceUI());
        
        if (selectedAngryPath)
        {
            yield return StartCoroutine(PlayAngryTimeline());
        }
        else
        {
            yield return StartCoroutine(PlayConcernedTimeline());
        }
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    protected override void Start()
    {
        Animator animator = GetComponent<Animator>();
    //     base.Start(); 
    //     
    //     if (angryChoiceButton != null)
    //     {
    //         angryChoiceButton.onClick.AddListener(() => OnChoiceMade(true));
    //     }
    //     if (concernedChoiceButton != null)
    //     {
    //         concernedChoiceButton.onClick.AddListener(() => OnChoiceMade(false));
    //     }
    //     
    //     if (choicePanel != null)
    //     {
    //         choicePanel.SetActive(false);
    //     }
    }
    
    private IEnumerator ShowChoiceUI()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
            choiceMade = false;
            
            yield return new WaitUntil(() => choiceMade);
            
            choicePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Choice panel not assigned! Defaulting to angry path.");
            selectedAngryPath = true;
            choiceMade = true;
        }
    }
    
    private void OnChoiceMade(bool isAngryPath)
    {
        selectedAngryPath = isAngryPath;
        choiceMade = true;
        
        Debug.Log($"Player chose: {(isAngryPath ? "Angry" : "Concerned")} path");
    }
    
    [System.Obsolete]
    private IEnumerator PlayAngryTimeline()
    {
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq3AKemarahanAyah", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq4AMencariIbu", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq5AMenemukanIbu", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
    }
    
    [System.Obsolete]
    private IEnumerator PlayConcernedTimeline()
    {
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq3BKhawatir", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq4BMencariIbu", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq5BMenemukanIbu", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
    }
    
    // [System.Obsolete]
    // protected override IEnumerator PlayNightSequence()
    // {
    //     CloseEyes();
    //     yield return StartCoroutine(SetCameraPanRangeLeft());
    //     TimeManager.instance.TimeOfDay = 1.0f;
    //     SetCharacterSpawn(CharacterType.Baby, 0);
    //     SetCharacterSpawn(CharacterType.Mother, 0);
    //     SetCharacterSpawn(CharacterType.Father, 0);
    //     uiElements.narratorText.gameObject.SetActive(true);
    //     
    //     yield return new WaitForSeconds(1f);
    //     uiElements.narratorText.text = "Malam Hari\nMenjaga";
    //     yield return new WaitForSeconds(2f);
    //     uiElements.narratorText.gameObject.SetActive(false);
    //     
    //     FadeOpenEyes(); 
    //     yield return new WaitForSeconds(1f);
    //     
    //     // Seq6 Menjaga (both timelines converge here)
    //     bool seq6Complete = false;
    //     dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq6Menjaga", 
    //         () => { seq6Complete = true; });
    //     yield return new WaitUntil(() => seq6Complete);
    //     
    //     FadeCloseEyes(); 
    //     yield return new WaitForSeconds(2f);
    //     
    //     // Final day transition to Day 13-14 (no dialog)
    //     GoToNextDay();
    // }
}
