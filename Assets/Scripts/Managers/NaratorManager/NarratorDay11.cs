using UnityEngine;
using System.Collections;

public class NarratorDay11 : NarratorBase
{
    [SerializeField] protected Rigidbody rigidbodyIbu;

    // protected override void Awake()
    // {
    //     rigidbodyIbu = GetComponent<Rigidbody>();
    //     if (rigidbodyIbu == null)
    //     {
    //         Debug.LogError("Rigidbody Ibu is not assigned in the inspector.");
    //     }
    // }
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        saveFileManager.UpdateCoreGameSaves(10, 1);
        saveFileManager.SaveToLocalMyGamesFolder();
        
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 13.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        // SetFreezePosition(true);
        
        PlayCharacterAnimation(CharacterType.Mother, "Sitting_Sexy");
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Day 11\nKehilangan";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq1BerbicaraAneh", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        EnableNavMeshAgent(CharacterType.Mother);
        // SetFreezePosition(false);
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq2Selamat", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        saveFileManager.UpdateCoreGameSaves(10, 3);
        saveFileManager.SaveToLocalMyGamesFolder();
        
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 1.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        
        // Mother looks at baby with concern about hunger and survival
        StartCoroutine(SetHeadTarget(CharacterType.Mother, CharacterTarget.Baby));
        
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day11/Seq3Kelaparan", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(2f);
        
        GoToNextDay();
    }

    public void SetFreezePosition(bool freeze)
    {
        if (rigidbodyIbu == null) return;

        if (freeze)
        {
            rigidbodyIbu.constraints |= RigidbodyConstraints.FreezePositionX |
                              RigidbodyConstraints.FreezePositionY |
                              RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            rigidbodyIbu.constraints &= ~RigidbodyConstraints.FreezePositionX;
            rigidbodyIbu.constraints &= ~RigidbodyConstraints.FreezePositionY;
            rigidbodyIbu.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        }
    }
}
