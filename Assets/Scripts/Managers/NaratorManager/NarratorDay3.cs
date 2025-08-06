using UnityEngine;
using System.Collections;

public class NarratorDay3 : NarratorBase
{
    [System.Obsolete]
    protected override IEnumerator PlayMorningSequence()
    {
        Debug.Log("Day 3 Morning sequence");
        yield return new WaitForSeconds(3f);
        Debug.Log("Day 3 Morning finished! Moving to Afternoon...");
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayAfternoonSequence()
    {
        Debug.Log("Day 3 Afternoon sequence");
        yield return new WaitForSeconds(3f);
        Debug.Log("Day 3 Afternoon finished! Moving to Evening...");
        GoToNextTimeOfDay();
    }
    
    protected override IEnumerator PlayEveningSequence()
    {
        Debug.Log("Day 3 Evening sequence");
        yield return new WaitForSeconds(3f);
        Debug.Log("Day 3 Evening finished! Moving to Night...");
        GoToNextTimeOfDay();
    }
    
    [System.Obsolete]
    protected override IEnumerator PlayNightSequence()
    {
        Debug.Log("Day 3 Night sequence");
        yield return new WaitForSeconds(3f);
        Debug.Log("Day 3 Night finished! Moving to Day 4...");
        GoToNextDay();
    }
}
