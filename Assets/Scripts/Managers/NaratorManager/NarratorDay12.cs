using UnityEngine;
using System.Collections;

public class NarratorDay12 : NarratorBase
{
    // Day 12 - Ayah's Return - Critical Discovery
    // Choice point: Father's reaction determines the path
    private bool fatherAngerChoice = false; // This could be set by player choice system
    
    [System.Obsolete]
    protected override IEnumerator PlayEveningSequence()
    {
        TimeManager.instance.TimeOfDay = 0.75f; // Evening
        AppearObjects();
        SetCharacterSpawn(CharacterType.Father, 6);  // Outside door
        SetCharacterSpawn(CharacterType.Mother, 4);  // Baby's room - catatonic
        SetCharacterSpawn(CharacterType.Baby, 0);    // Parents' room - neglected
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 12\nAyah's Return\nCritical Discovery";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        // Seq1 KepulanganAyah
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq1KepulanganAyah", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Father enters and discovers chaos
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 1)); // Living room
        
        // Seq2 Berantakan
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq2Berantakan", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        // CHOICE POINT - This would be determined by player choice system
        // For now, we'll implement both paths
        
        if (fatherAngerChoice)
        {
            // Path A - Father Angry
            yield return StartCoroutine(PlayAngryPath());
        }
        else
        {
            // Path B - Father Concerned  
            yield return StartCoroutine(PlayConcernedPath());
        }
        
        // Auto progression to Day 13
        Debug.Log("Day 12 finished! Moving to Day 13...");
        GoToNextDay();
    }
    
    [System.Obsolete]
    private IEnumerator PlayAngryPath()
    {
        // Seq3A KemarahanAyah
        bool seq3AComplete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq3AKemarahanAyah", 
            () => { seq3AComplete = true; });
        yield return new WaitUntil(() => seq3AComplete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to parents' room to find Rey
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 0));
        
        // Seq4A MencariIbu (Angry version)
        bool seq4AComplete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq4AMencariIbu", 
            () => { seq4AComplete = true; });
        yield return new WaitUntil(() => seq4AComplete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to find mother
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 4));
        
        // Seq5A MenemukanIbu (Angry version)
        bool seq5AComplete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq5AMenemukanIbu", 
            () => { seq5AComplete = true; });
        yield return new WaitUntil(() => seq5AComplete);
    }
    
    [System.Obsolete]
    private IEnumerator PlayConcernedPath()
    {
        // Seq3B Khawatir
        bool seq3BComplete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq3BKhawatir", 
            () => { seq3BComplete = true; });
        yield return new WaitUntil(() => seq3BComplete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to parents' room to find Rey
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 0));
        
        // Seq4B MencariIbu (Concerned version)
        bool seq4BComplete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq4BMencariIbu", 
            () => { seq4BComplete = true; });
        yield return new WaitUntil(() => seq4BComplete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to find mother
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Father, 4));
        
        // Seq5B MenemukanIbu (Concerned version)
        bool seq5BComplete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq5BMenemukanIbu", 
            () => { seq5BComplete = true; });
        yield return new WaitUntil(() => seq5BComplete);
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        // Day 12 ends with evening sequence
        yield return null;
    }
}
