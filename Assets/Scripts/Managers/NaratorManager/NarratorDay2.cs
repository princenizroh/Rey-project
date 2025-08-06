using UnityEngine;
using System.Collections;

public class NarratorDay2 : NarratorBase
{
    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {    
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        PlayCharacterAnimation(CharacterType.Father, "Sit");
        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(MoveCharacterToPosition(CharacterType.Baby, 0, 2f));
        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Bidan, 0));
        
        Debug.Log("Day 2 Morning finished! Moving to Afternoon...");
        yield return new WaitForSeconds(2f);
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 13.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Afternoon sequence.");
        yield return new WaitForSeconds(5f);
        
        Debug.Log("Day 2 Afternoon finished! Moving to Evening...");
        GoToNextTimeOfDay();
    }
    
    protected override IEnumerator PlayEveningSequence()
    {
        TimeManager.instance.TimeOfDay = 19.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Evening sequence.");
        yield return new WaitForSeconds(5f);
        
        Debug.Log("Day 2 Evening finished! Moving to Night...");
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Night sequence.");
        yield return new WaitForSeconds(5f);
        
        Debug.Log("Day 2 Night finished! Moving to Day 3...");
        GoToNextDay();
    }
}
