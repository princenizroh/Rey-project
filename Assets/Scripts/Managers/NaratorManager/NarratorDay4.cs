using UnityEngine;
using System.Collections;

public class NarratorDay4 : NarratorBase
{    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 13.0f; 
        SetCharacterSpawn(CharacterType.Baby, 0);   
        SetCharacterSpawn(CharacterType.Mother, 0); 
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 4\n Tempat Berbeda";
        yield return new WaitForSeconds(2f);
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
        
        // Seq4 Telephone
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq4Telephone", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq5 Kerja
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq5Kerja", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        FadeCloseEyes(); // Baby eventually sleeps
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayEveningSequence()
    {
        CloseEyes();
        yield return StartCoroutine(SetCameraPanRangeBack());
        TimeManager.instance.TimeOfDay = 18.0f; // Evening
        SetCharacterSpawn(CharacterType.Baby, 4);
        SetCharacterSpawn(CharacterType.Mother, 3);
        uiElements.narratorText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Sore Hari";
        yield return new WaitForSeconds(3f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq6 Lapar
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq6Lapar", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother stressed with baby
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
        // Seq7 IbuMarahLagi
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq7IbuMarahLagi", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Move to parent room for Seq8
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Mother, 0);
        yield return new WaitForSeconds(2f);
        FadeOpenEyes(); 
        
        // Seq8 Stres
        bool seq8Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq8Stres", 
            () => { seq8Complete = true; });
        yield return new WaitUntil(() => seq8Complete);
        
        FadeCloseEyes(); // Baby sleeps
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
        uiElements.narratorText.text = "Malam Hari\nGangguan Supernatural Pertama";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Baby alone in dark room - supernatural disturbance begins
        PlayAudio("wind_light");
        
        yield return new WaitForSeconds(2f);
        FadeOpenEyes(); 
        
        // Seq9 GangguanSetanRingan
        bool seq9Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq9GangguanSetanRingan", 
            () => { seq9Complete = true; });
        yield return new WaitUntil(() => seq9Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Parents come to help
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
        // Seq10 Maaf
        bool seq10Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day4/Seq10Maaf", 
            () => { seq10Complete = true; });
        yield return new WaitUntil(() => seq10Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        FadeCloseEyes(); 
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 5
        Debug.Log("Day 4 finished! Moving to Day 5...");
        GoToNextDay();
    }
}
