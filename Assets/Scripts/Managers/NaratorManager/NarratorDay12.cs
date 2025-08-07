using UnityEngine;
using System.Collections;

public class NarratorDay12 : NarratorBase
{
    // Day 12 - Hope and Recovery Begin
    // Father takes action, professional help is sought, supernatural activity subsides
    
    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 8.0f; // Morning
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 12\nHope and Recovery Begin\nPagi Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Seq1 PagiBaruHarapan
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq1PagiBaruHarapan", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq2 AyahAmbilTindakan
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq2AyahAmbilTindakan", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeFront());
        TimeManager.instance.TimeOfDay = 13.0f; // Afternoon
        SetCharacterSpawn(CharacterType.Baby, 1);
        SetCharacterSpawn(CharacterType.Mother, 1);
        SetCharacterSpawn(CharacterType.Father, 1);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Siang Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);

        // Seq3 BantuanProfesional
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq3BantuanProfesional", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq4 IbuMulaiPulih
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq4IbuMulaiPulih", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 20.0f; // Night
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nKetenangan Kembali";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Peaceful night, supernatural activity fading
        PlayAudio("peaceful_night");
        
        FadeOpenEyes(); 
        yield return new WaitForSeconds(1f);
        
        // Seq5 MalamTenang
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq5MalamTenang", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq6 HarapanMasaDepan
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq6HarapanMasaDepan", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Final sequence - credits or next chapter
        // Seq7 Epilog
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day12/Seq7Epilog", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 5f)); 
        }
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(3f);
        
        // Story completed or transition to next chapter
        Debug.Log("Day 12 finished! Story completed!");
        
        // Could transition to credits, next chapter, or end game
        // GoToNextDay(); // If continuing to Day 13+
        // Or show credits/ending sequence
        
        yield return new WaitForSeconds(2f);
        uiElements.narratorText.gameObject.SetActive(true);
        uiElements.narratorText.text = "Chapter 1: The First 12 Days\nCompleted";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);
    }
}
