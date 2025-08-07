using UnityEngine;
using System.Collections;

public class NarratorDay5 : NarratorBase
{
    // Day 5 - Baby Blues Phase Day 2
    // Mother's condition worsens, supernatural elements increase
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 13.0f; // Afternoon
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        SetCharacterSpawn(CharacterType.Mother, 3); // Work room
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 5\nBaby Blues Phase - Day 2\nSiang Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);

        // Seq1 Lapar
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day5/Seq1Lapar", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother comes angry and stressed
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
        // Seq2 IbuMarah
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day5/Seq2IbuMarah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq3 IbuMarah (continued anger)
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day5/Seq3IbuMarah", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq4 Penyesalan
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day5/Seq4Penyesalan", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        FadeCloseEyes(); // Baby sleeps from exhaustion
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 20.0f; // Night
        SetCharacterSpawn(CharacterType.Baby, 4);
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nGangguan Supernatural Meningkat";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Heavy rain and supernatural disturbance
        PlayAudio("rain_heavy");
        PlayAudio("wind_strong");
        
        yield return new WaitForSeconds(2f);
        FadeOpenEyes(); 
        
        // Seq5 GangguanSetanHujan
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day5/Seq5GangguanSetanHujan", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq6 Muak
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day5/Seq6Muak", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 6
        Debug.Log("Day 5 finished! Moving to Day 6...");
        GoToNextDay();
    }
}
