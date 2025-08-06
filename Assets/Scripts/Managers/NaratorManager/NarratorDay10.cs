using UnityEngine;
using System.Collections;

public class NarratorDay10 : NarratorBase
{
    // Day 10 - Depresi Postpartum Day 2 (Multiple POV)
    // Father still working overtime, mother's condition deteriorates further
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0); // Bedroom - severe state
        SetCharacterSpawn(CharacterType.Baby, 0);   // Back to parents' room
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 10\nDepresi Postpartum - Day 2\nSiang Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        // Sound effects of chaos
        PlayAudio("objects_falling");
        PlayAudio("plates_breaking");
        
        yield return new WaitForSeconds(2f);
        
        FadeOpenEyes(); // Baby wakes up to chaos
        yield return new WaitForSeconds(1f);

        // Seq1 KeanehanIbu
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day10/Seq1KeanehanIbu", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 2f)); 
        }
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f; // Night
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nCurhatan Patah Hati";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Mother's emotional breakdown and paranoid thoughts
        PlayAudio("crying_breakdown");
        PlayAudio("furniture_thrown");
        
        yield return new WaitForSeconds(2f);
        
        // Seq2 Curhatan
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day10/Seq2Curhatan", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        yield return new WaitForSeconds(3f);
        
        // Auto progression to Day 11 (Psikosis begins)
        Debug.Log("Day 10 finished! Moving to Day 11 - Psikosis Postpartum...");
        GoToNextDay();
    }
}
