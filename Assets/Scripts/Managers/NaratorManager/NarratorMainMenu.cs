using UnityEngine;
using System.Collections;

public class NarratorMainMenu : NarratorBase
{
    [Header("Main Menu Settings")]
    [SerializeField] private bool autoPlayOnStart = true;
    [SerializeField] private float delayBeforePlay = 1f;

    protected override void Start()
    {
        base.Start();
        
        if (autoPlayOnStart)
        {
            StartCoroutine(DelayedMainMenuPlay());
        }
    }

    private IEnumerator DelayedMainMenuPlay()
    {
        yield return new WaitForSeconds(delayBeforePlay);
        PlayMainMenuSequence();
    }

    /// <summary>
    /// Main function to play character animations based on current save day
    /// Call this from UI buttons or other scripts
    /// </summary>
    [ContextMenu("Play Main Menu Sequence")]
    public void PlayMainMenuSequence()
    {
        if (saveFileManager == null)
        {
            Debug.LogWarning("[NarratorMainMenu] SaveFileManager not found. Using default Day 1 animations.");
            PlayDay1MainMenuAnimation();
            return;
        }

        // Get current day from save data
        int currentDay = GetCurrentSaveDay();
        
        Debug.Log($"[NarratorMainMenu] Playing animations for Day {currentDay}");
        
        // Play animations based on day
        PlayAnimationsForDay(currentDay);
    }

    /// <summary>
    /// Get current day from SaveFileManager
    /// </summary>
    private int GetCurrentSaveDay()
    {
        try
        {
            // Access SaveFileManager's target ScriptableObject using reflection
            var saveDataField = saveFileManager.GetType().GetField("targetSaveObject", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (saveDataField != null)
            {
                var coreGameSaves = saveDataField.GetValue(saveFileManager);
                if (coreGameSaves != null)
                {
                    // Get day field using reflection
                    var dayField = coreGameSaves.GetType().GetField("day");
                    if (dayField != null)
                    {
                        int day = (int)dayField.GetValue(coreGameSaves);
                        return Mathf.Clamp(day, 1, 14); // Clamp between Day 1-14
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NarratorMainMenu] Error getting save day: {e.Message}");
        }
        
        return 1; // Default to Day 1 if error
    }

    /// <summary>
    /// Play character animations based on specific day
    /// </summary>
    private void PlayAnimationsForDay(int day)
    {
        // Ensure all characters are in proper main menu positions first
        SetupMainMenuPositions();
        
        switch (day)
        {
            case 1:
                PlayDay1MainMenuAnimation();
                break;
            case 2:
                PlayDay2MainMenuAnimation();
                break;
            case 3:
                PlayDay3MainMenuAnimation();
                break;
            case 4:
                PlayDay4MainMenuAnimation();
                break;
            case 5:
                PlayDay5MainMenuAnimation();
                break;
            case 6:
                PlayDay6MainMenuAnimation();
                break;
            case 7:
                PlayDay7MainMenuAnimation();
                break;
            case 8:
                PlayDay8MainMenuAnimation();
                break;
            case 9:
                PlayDay9MainMenuAnimation();
                break;
            case 10:
                PlayDay10MainMenuAnimation();
                break;
            case 11:
                PlayDay11MainMenuAnimation();
                break;
            case 12:
                PlayDay12MainMenuAnimation();
                break;
            case 13:
                PlayDay13MainMenuAnimation();
                break;
            case 14:
                PlayDay14MainMenuAnimation();
                break;
            default:
                Debug.LogWarning($"[NarratorMainMenu] Unknown day: {day}. Using Day 1 animations.");
                PlayDay1MainMenuAnimation();
                break;
        }
    }

    private void SetupMainMenuPositions()
    {
        SetCharacterSpawn(CharacterType.Mother, 0);
        SetCharacterSpawn(CharacterType.Father, 0);
        SetCharacterSpawn(CharacterType.Baby, 0);
        SetCharacterSpawn(CharacterType.Object, 0);

    }

    #region Day-Specific Animation Methods
    
    private void PlayDay1MainMenuAnimation()
    {
        
        SetObjectsActive(gameObjects.activeObjects, true); 
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        PlayCharacterAnimation(CharacterType.Father, "Sitting");
        PlayCharacterAnimation(CharacterType.Bidan, "Idle");
        
        SetCharacterSpawn(CharacterType.Object, 0); 
        
        Debug.Log("[NarratorMainMenu] Playing Day 1 animations: Birth scene");
    }

    private void PlayDay2MainMenuAnimation()
    {
        SetObjectsActive(gameObjects.inActiveObjects, false);
        PlayCharacterAnimation(CharacterType.Mother, "Angry");
        PlayCharacterAnimation(CharacterType.Father, "Sitting_Talking");
        PlayCharacterAnimation(CharacterType.Baby, "Idle");
        
        SetCharacterSpawn(CharacterType.Object, 1);
        
        Debug.Log("[NarratorMainMenu] Playing Day 2 animations: First day home");
    }

    private void PlayDay3MainMenuAnimation()
    {
        // Day 3: Growing baby - more active
        PlayCharacterAnimation(CharacterType.Mother, "Happy");
        PlayCharacterAnimation(CharacterType.Father, "Idle");
        PlayCharacterAnimation(CharacterType.Baby, "Active");
        
        Debug.Log("[NarratorMainMenu] Playing Day 3 animations: Growing baby");
    }

    private void PlayDay4MainMenuAnimation()
    {
        // Day 4: Family bonding
        PlayCharacterAnimation(CharacterType.Mother, "Playing");
        PlayCharacterAnimation(CharacterType.Father, "Happy");
        PlayCharacterAnimation(CharacterType.Baby, "Happy");
        
        Debug.Log("[NarratorMainMenu] Playing Day 4 animations: Family bonding");
    }

    private void PlayDay5MainMenuAnimation()
    {
        // Day 5: Learning phase
        PlayCharacterAnimation(CharacterType.Mother, "Teaching");
        PlayCharacterAnimation(CharacterType.Father, "Watching");
        PlayCharacterAnimation(CharacterType.Baby, "Learning");
        
        Debug.Log("[NarratorMainMenu] Playing Day 5 animations: Learning phase");
    }

    private void PlayDay6MainMenuAnimation()
    {
        // Day 6: Developing skills
        PlayCharacterAnimation(CharacterType.Mother, "Encouraging");
        PlayCharacterAnimation(CharacterType.Father, "Proud");
        PlayCharacterAnimation(CharacterType.Baby, "Crawling");
        
        Debug.Log("[NarratorMainMenu] Playing Day 6 animations: Developing skills");
    }

    private void PlayDay7MainMenuAnimation()
    {
        // Day 7: Week milestone
        PlayCharacterAnimation(CharacterType.Mother, "Celebrating");
        PlayCharacterAnimation(CharacterType.Father, "Celebrating");
        PlayCharacterAnimation(CharacterType.Baby, "Smiling");
        
        Debug.Log("[NarratorMainMenu] Playing Day 7 animations: Week milestone");
    }

    private void PlayDay8MainMenuAnimation()
    {
        // Day 8: New challenges
        PlayCharacterAnimation(CharacterType.Mother, "Concerned");
        PlayCharacterAnimation(CharacterType.Father, "Helping");
        PlayCharacterAnimation(CharacterType.Baby, "Fussy");
        
        Debug.Log("[NarratorMainMenu] Playing Day 8 animations: New challenges");
    }

    private void PlayDay9MainMenuAnimation()
    {
        // Day 9: Adaptation
        PlayCharacterAnimation(CharacterType.Mother, "Adapting");
        PlayCharacterAnimation(CharacterType.Father, "Supporting");
        PlayCharacterAnimation(CharacterType.Baby, "Growing");
        
        Debug.Log("[NarratorMainMenu] Playing Day 9 animations: Adaptation");
    }

    private void PlayDay10MainMenuAnimation()
    {
        // Day 10: Progress
        PlayCharacterAnimation(CharacterType.Mother, "Proud");
        PlayCharacterAnimation(CharacterType.Father, "Amazed");
        PlayCharacterAnimation(CharacterType.Baby, "Achieving");
        
        Debug.Log("[NarratorMainMenu] Playing Day 10 animations: Progress");
    }

    private void PlayDay11MainMenuAnimation()
    {
        // Day 11: Advanced development
        PlayCharacterAnimation(CharacterType.Mother, "Guiding");
        PlayCharacterAnimation(CharacterType.Father, "Observing");
        PlayCharacterAnimation(CharacterType.Baby, "Advanced");
        
        Debug.Log("[NarratorMainMenu] Playing Day 11 animations: Advanced development");
    }

    private void PlayDay12MainMenuAnimation()
    {
        // Day 12: Near completion
        PlayCharacterAnimation(CharacterType.Mother, "Emotional");
        PlayCharacterAnimation(CharacterType.Father, "Reflective");
        PlayCharacterAnimation(CharacterType.Baby, "Mature");
        
        Debug.Log("[NarratorMainMenu] Playing Day 12 animations: Near completion");
    }

    private void PlayDay13MainMenuAnimation()
    {
        // Day 13: Final preparations
        PlayCharacterAnimation(CharacterType.Mother, "Preparing");
        PlayCharacterAnimation(CharacterType.Father, "Ready");
        PlayCharacterAnimation(CharacterType.Baby, "Almost_Ready");
        
        Debug.Log("[NarratorMainMenu] Playing Day 13 animations: Final preparations");
    }

    private void PlayDay14MainMenuAnimation()
    {
        // Day 14: Completion/Birth
        PlayCharacterAnimation(CharacterType.Mother, "Complete");
        PlayCharacterAnimation(CharacterType.Father, "Joyful");
        PlayCharacterAnimation(CharacterType.Baby, "Born");
        
        Debug.Log("[NarratorMainMenu] Playing Day 14 animations: Completion");
    }
    
    #endregion

    #region Public Utility Methods
    
    /// <summary>
    /// Force play animations for specific day (for testing/debugging)
    /// </summary>
    public void ForcePlayDayAnimation(int day)
    {
        Debug.Log($"[NarratorMainMenu] Forcing animations for Day {day}");
        PlayAnimationsForDay(day);
    }
    
    /// <summary>
    /// Get current save day (public accessor for other scripts)
    /// </summary>
    public int GetCurrentDay()
    {
        return GetCurrentSaveDay();
    }
    
    /// <summary>
    /// Refresh main menu animations (useful after save file changes)
    /// </summary>
    public void RefreshMainMenu()
    {
        PlayMainMenuSequence();
    }
    
    #endregion

    #region Context Menu Debug Methods
    
    [ContextMenu("Test Day 1")]
    private void TestDay1() => ForcePlayDayAnimation(1);
    
    [ContextMenu("Test Day 7")]
    private void TestDay7() => ForcePlayDayAnimation(7);
    
    [ContextMenu("Test Day 14")]
    private void TestDay14() => ForcePlayDayAnimation(14);
    
    [ContextMenu("Get Current Save Day")]
    private void DebugCurrentDay()
    {
        int day = GetCurrentSaveDay();
        Debug.Log($"[NarratorMainMenu] Current save day: {day}");
    }
    
    #endregion
}
