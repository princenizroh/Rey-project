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
        Debug.Log($"=== NarratorManager.StartNarration({day}, {time}) ===");
        currentDay = day;
        currentTime = time;

        if (narratorDict.TryGetValue(day, out NarratorBase narrator))
        {
            Debug.Log($"Found narrator for {day}: {narrator.name}");
            
            // Check if the requested time sequence is available
            if (narrator.HasTimeOfDaySequence(time))
            {
                Debug.Log($"Starting {day} {time} sequence");
                StartCoroutine(narrator.StartNarration());
            }
            else
            {
                Debug.LogWarning($"{day} does not have {time} sequence. Finding first available...");
                TimeOfDay firstAvailable = narrator.GetFirstAvailableTimeOfDay();
                currentTime = firstAvailable;
                Debug.Log($"Starting {day} with first available time: {firstAvailable}");
                StartCoroutine(narrator.StartNarration());
            }
        }
        else
        {
            Debug.LogError($"Narrator for {day} not found in dictionary!");
            Debug.Log($"Available narrators: {string.Join(", ", narratorDict.Keys)}");
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
        if ((int)currentDay < 13) 
        {
            NarratorDay nextDay = currentDay + 1;

            if (narratorDict.TryGetValue(nextDay, out NarratorBase nextNarrator))
            {
                TimeOfDay firstAvailableTime = nextNarrator.GetFirstAvailableTimeOfDay();
                StartNarration(nextDay, firstAvailableTime);
                Debug.Log($"Starting {nextDay} with first available time: {firstAvailableTime}");
            }
            else
            {
                Debug.LogError($"Narrator for {nextDay} not found!");
            }
        }
        else
        {
            Debug.Log("Story completed - reached final day!");
        }
    }
    
    [System.Obsolete]
    public void NextTimeOfDay()
    {
        if (narratorDict.TryGetValue(currentDay, out NarratorBase currentNarrator))
        {
            TimeOfDay nextTime = currentNarrator.GetNextAvailableTimeOfDay(currentTime);
            
            // If nextTime is Morning, it means no more sequences for current day
            if (nextTime == TimeOfDay.Morning && currentTime != TimeOfDay.Night)
            {
                // Skip to next day instead
                NextDay();
            }
            else if (nextTime == TimeOfDay.Morning && currentTime == TimeOfDay.Night)
            {
                // End of current day, go to next day
                NextDay();
            }
            else
            {
                // Stay on current day, move to next available time
                StartNarration(currentDay, nextTime);
                Debug.Log($"Moving to next available time: {nextTime} on {currentDay}");
            }
        }
        else
        {
            Debug.LogError($"Current narrator for {currentDay} not found!");
        }
    }
}
