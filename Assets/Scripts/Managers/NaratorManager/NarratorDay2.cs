using UnityEngine;
using System.Collections;

public class NarratorDay2 : NarratorBase
{
    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {    
        TimeManager.instance.TimeOfDay = 8.0f;
        // AppearObjects(); 
        uiElements.narratorText.text = "Day 2\nHari Pertamaku";
        Debug.Log("Playing narration for Day 2 Morning sequence.");
        yield return new WaitForSeconds(1f);

    }
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 13.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Afternoon sequence.");
        yield return null;
    }
    
    protected override IEnumerator PlayEveningSequence()
    {
        TimeManager.instance.TimeOfDay = 19.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Evening sequence.");
        yield return null;
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Night sequence.");
        yield return null;
    }

    
}
