
using UnityEngine;
using System.Collections;

public class NarratorDay7 : NarratorBase
{
    // Day 7 - Pre-Depression Phase
    // Condition deteriorates significantly, supernatural intensifies
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 0.5f; // Afternoon
        AppearObjects();
        SetCharacterSpawn(CharacterType.Mother, 0); // Bedroom - severe withdrawal
        SetCharacterSpawn(CharacterType.Baby, 4);   // Baby's room
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 7\nPre-Depression Phase\nSiang Hari";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);

        // Seq1 Lapar
        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq1Lapar", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq2 Sendirian
        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq2Sendirian", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Mother returns
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Mother, 4));
        
        // Seq3 IbuPulang
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq3IbuPulang", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq4 Kelaparan
        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq4Kelaparan", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);
        
        FadeCloseEyes(); // Baby sleeps
        yield return new WaitForSeconds(2f);
        
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f; // Night
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Malam Hari\nManifestasi Supernatural";
        yield return new WaitForSeconds(4f);
        uiElements.narratorText.gameObject.SetActive(false);
        
        // Strong supernatural presence
        PlayAudio("supernatural_presence");
        PlayAudio("whispers_dark");
        
        yield return new WaitForSeconds(2f);
        
        FadeOpenEyes(); // Baby wakes up
        yield return new WaitForSeconds(1f);
        
        // Seq5 SosokMenyeramkan
        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq5SosokMenyeramkan", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);
        
        yield return new WaitForSeconds(1f);
        
        // Seq6 Khawatir
        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day7/Seq6Khawatir", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 3f)); 
        }
        
        yield return new WaitForSeconds(2f);
        
        // Auto progression to Day 8
        Debug.Log("Day 7 finished! Moving to Day 8...");
        GoToNextDay();
    }
}
