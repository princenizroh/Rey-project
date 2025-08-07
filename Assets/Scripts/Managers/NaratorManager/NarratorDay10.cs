using UnityEngine;
using System.Collections;

public class NarratorDay10 : NarratorBase
{
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeLeft());
        TimeManager.instance.TimeOfDay = 13.0f;
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 1);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 10\nSendirian Lagi";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Seq1 Sendirian
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day10/Seq1Sendirian", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq2 AyahKhawatir
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day10/Seq2AyahKhawatir", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
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
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nPuncak Gangguan";
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // PlayAudio("supernatural_peak");
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        // Seq3 GangguanSetanPuncak
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day10/Seq3GangguanSetanPuncak", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq4 Keputusasaan
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day10/Seq4Keputusasaan", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextDay();
    }
}
