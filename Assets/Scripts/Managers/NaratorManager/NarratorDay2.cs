using UnityEngine;
using System.Collections;

public class NarratorDay2 : NarratorBase
{
    
    [System.Obsolete]
    public override IEnumerator Narrate()
    {
        ResetUIState();

        switch (NarratorManager.Instance.currentTime)
        {
            case TimeOfDay.Morning:
                yield return StartCoroutine(PlayMorningSequence());
                break;
            case TimeOfDay.Afternoon:
                yield return StartCoroutine(PlayAfternoonSequence());
                break;
            case TimeOfDay.Evening:
                yield return StartCoroutine(PlayEveningSequence());
                break;
            case TimeOfDay.Night:
                yield return StartCoroutine(PlayNightSequence());
                break;
        }
    }

    [System.Obsolete]
    private IEnumerator PlayMorningSequence()
    {
        TimeManager.instance.TimeOfDay = 8.0f;
        yield return null;
    }
    [System.Obsolete]
    private IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 13.0f;
        yield return null;
    }

    private IEnumerator PlayEveningSequence()
    {
        TimeManager.instance.TimeOfDay = 19.0f;
        yield return null;
    }

    [System.Obsolete]
    private IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f;
        yield return null;
    }
}
