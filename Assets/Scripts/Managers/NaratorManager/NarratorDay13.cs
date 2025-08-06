using UnityEngine;
using System.Collections;

public class NarratorDay13 : NarratorBase
{
    [System.Obsolete]
    public override IEnumerator Narrate()
    {
        ResetUIState();

        switch (NarratorManager.Instance.currentTime)
        {
            case TimeOfDay.Night:
                yield return StartCoroutine(PlayNightSequence());
                break;
        }
    }
    private IEnumerator PlayNightSequence()
    {
        yield return null;
    }
}
