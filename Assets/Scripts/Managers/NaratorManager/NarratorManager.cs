using UnityEngine;

public enum NarratorDay
{
    Day1, Day2, Day3, Day4, Day5, Day6, Day7, Day8, Day9, Day10, Day11, Day12, Day13, Day14
}

public enum TimeOfDay
{
    Morning, Afternoon, Evening, Night, Midnight
}
public class NarratorManager : MonoBehaviour
{
    public static NarratorManager Instance;
    public NarratorDay currentDay;
    public TimeOfDay currentTime;

    public NarratorDay1 day1Narrator;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        StartNarration(NarratorDay.Day1, TimeOfDay.Night);
    }

    public void StartNarration(NarratorDay day, TimeOfDay time)
    {
        currentDay = day;
        currentTime = time;

        switch (day)
        {
          case NarratorDay.Day1:
                NarrationDay1();
                break;
        }
    }

    private void NarrationDay1()
    {
        StartCoroutine(day1Narrator.Narrate());
    }

    private void ChangeNarrator(NarratorDay newDay, TimeOfDay newTime)
    {
        
    }
}
