using UnityEngine;
using System.Collections;

public class NarratorDay4 : NarratorBase
{
    // Day 4 starts with afternoon (Baby Blues Phase Day 1)
    // Baby moved to separate room, Mother working from home
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 3); // Work room
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's new room
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 4\nBaby Blues Phase - Day 1\nSiang Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); // Baby wakes up in new room
        yield return new WaitForSeconds(1f);

        // Seq1 TempatBerbeda
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq1TempatBerbeda", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother comes from work room stressed
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
        // Seq2 IbuMarah
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq2IbuMarah", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq3 GangguanTelpon
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq3GangguanTelpon", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother goes back to work room
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 3));
        
        // Seq4 ReyDiletakkanKembali
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq4ReyDiletakkanKembali", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq5 Overwhelmed
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq5Overwhelmed", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        FadeCloseEyes(); // Baby eventually sleeps
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayEveningSequence()
    {
        TimeManager.instance.TimeOfDay = 0.75f; // Evening
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Sore Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq6 DeadlinePressure
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq6DeadlinePressure", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to parents' room for breakdown
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 0));
        
        // Seq7 MenyerahSementara
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq7MenyerahSementara", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f; // Night
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nGangguan Supernatural Pertama";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Baby alone in dark room - supernatural disturbance begins
        PlayAudio("wind_light");
        
        yield return new WaitForSeconds(2f);
        
        // Seq6 GangguanSetanRingan (supernatural sequence)
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq6GangguanSetanRingan", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 5
        Debug.Log("Day 4 finished! Moving to Day 5...");
        GoToNextDay();
    }
}
