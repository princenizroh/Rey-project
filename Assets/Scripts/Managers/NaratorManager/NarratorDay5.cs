using UnityEngine;
using System.Collections;

public class NarratorDay5 : NarratorBase
{
    // Day 5 - Baby Blues Phase Day 2
    // Mother's condition worsens, supernatural elements increase
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 3); // Work room
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        
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
        
        // Seq1 IbuMarah (note: duplicate name in storyboard, should be Seq2)
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day5/Seq1IbuMarah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        FadeCloseEyes(); // Baby sleeps from exhaustion
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f; // Night
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nGangguan Supernatural Meningkat";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Heavy rain and supernatural disturbance
        PlayAudio("rain_heavy");
        PlayAudio("wind_strong");
        
        yield return new WaitForSeconds(2f);
        
        // Seq5 GangguanSetanHujan
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day5/Seq5GangguanSetanHujan", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 6
        Debug.Log("Day 5 finished! Moving to Day 6...");
        GoToNextDay();
    }
}
