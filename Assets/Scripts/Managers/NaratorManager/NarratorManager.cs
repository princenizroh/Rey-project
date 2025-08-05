using UnityEngine;
using System.Collections.Generic;

public class NarratorManager : MonoBehaviour
{
    public static NarratorManager Instance;
    public NarratorDay currentDay;
    public TimeOfDay currentTime;

    [Header("Narrators")]
    [SerializeField] private NarratorBase[] dayNarrators;
    private Dictionary<NarratorDay, NarratorBase> narratorDict;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNarrators();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeNarrators()
    {
        narratorDict = new Dictionary<NarratorDay, NarratorBase>();

        for (int i = 0; i < dayNarrators.Length && i < 14; i++)
        {
            if (dayNarrators[i] != null)
            {
                narratorDict[(NarratorDay)i] = dayNarrators[i];
            }
        }
    }

    [System.Obsolete]
    public void Start()
    {
        StartNarration(currentDay, currentTime);
    }

    [System.Obsolete]
    public void StartNarration(NarratorDay day, TimeOfDay time)
    {
        currentDay = day;
        currentTime = time;

        if (narratorDict.TryGetValue(day, out NarratorBase narrator))
        {
            StartCoroutine(narrator.Narrate());
        }
        else
        {
            Debug.LogWarning($"Narrator for {day} not found!");
        }
    }


    [System.Obsolete]
    public void ChangeNarrator(NarratorDay newDay, TimeOfDay newTime)
    {
        StartNarration(newDay, newTime);
    }

    [System.Obsolete]
    public void NextDay()
    {
        if ((int)currentDay < 13) // Day14 is the last (index 13)
        {
            StartNarration(currentDay + 1, TimeOfDay.Morning);
        }
    }
    [System.Obsolete]
    
    public void NextTimeOfDay()
    {
        TimeOfDay nextTime = currentTime + 1;
        if ((int)nextTime > 4) // Midnight is the last (index 4)
        {
            NextDay();
        }
        else
        {
            StartNarration(currentDay, nextTime);
        }
    }
}
