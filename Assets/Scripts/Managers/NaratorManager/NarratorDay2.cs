using UnityEngine;
using System.Collections;

public class NarratorDay2 : NarratorBase
{
    [Header("Position Setup Tools")]
    private Vector3[] lastSpawnPositions;
    private Quaternion[] lastSpawnRotations;

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
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        PlayCharacterAnimation(CharacterType.Father, "Sit");
        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(MoveCharacterToPosition(CharacterType.Baby, 0, 2f));
        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Bidan, 0));
    }
    
    [System.Obsolete]
    private IEnumerator PlayAfternoonSequence()
    {
        TimeManager.instance.TimeOfDay = 13.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Afternoon sequence.");
        yield return null;
    }
    
    private IEnumerator PlayEveningSequence()
    {
        TimeManager.instance.TimeOfDay = 19.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Evening sequence.");
        yield return null;
    }
    
    [System.Obsolete]
    private IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f;
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        Debug.Log("Playing narration for Day 2 Night sequence.");
        yield return null;
    }

    // Public methods for Editor Tools
    public void SnapCharacterToSpawn(CharacterType characterType, int spawnIndex)
    {
        #if UNITY_EDITOR
        var characterData = System.Array.Find(charactersDataArray, c => c.characterType == characterType);
        if (characterData != null && characterData.HasValidSpawnPosition(spawnIndex))
        {
            characterData.characterObject.transform.position = characterData.spawnPositions[spawnIndex].position;
            characterData.characterObject.transform.rotation = characterData.spawnPositions[spawnIndex].rotation;
            
            Debug.Log($"Snapped {characterType} to spawn position {spawnIndex}");
        }
        else
        {
            Debug.LogWarning($"Cannot snap {characterType} to spawn position {spawnIndex}. Check if spawn position exists.");
        }
        #endif
    }
    
    public void SnapAllCharactersToSpawn(int spawnIndex)
    {
        #if UNITY_EDITOR
        foreach (var characterData in charactersDataArray)
        {
            if (characterData.HasValidSpawnPosition(spawnIndex))
            {
                SnapCharacterToSpawn(characterData.characterType, spawnIndex);
            }
        }
        #endif
    }
    
    // New method: Snap characters to different spawn indices
    public void SnapCharactersToMultipleSpawns(int motherIndex, int fatherIndex, int babyIndex, int bidanIndex)
    {
        #if UNITY_EDITOR
        SnapCharacterToSpawn(CharacterType.Mother, motherIndex);
        SnapCharacterToSpawn(CharacterType.Father, fatherIndex);
        SnapCharacterToSpawn(CharacterType.Baby, babyIndex);
        SnapCharacterToSpawn(CharacterType.Bidan, bidanIndex);
        
        Debug.Log($"Snapped characters to multiple positions - Mother:{motherIndex}, Father:{fatherIndex}, Baby:{babyIndex}, Bidan:{bidanIndex}");
        #endif
    }
    
    public void SetupDay2Positions()
    {
        #if UNITY_EDITOR
        // Setup posisi untuk Day 2 berdasarkan timeline document
        // Morning: Kamar Tidur → Kamar Mandi → Kamar Ortu
        // Sekarang bisa menggunakan spawn index yang berbeda untuk setiap karakter
        SnapCharacterToSpawn(CharacterType.Mother, 0); // Kamar Ortu
        SnapCharacterToSpawn(CharacterType.Father, 0); // Kamar Ortu  
        SnapCharacterToSpawn(CharacterType.Baby, 1);   // Bisa ke spawn position berbeda
        SnapCharacterToSpawn(CharacterType.Bidan, 2);  // Bisa ke spawn position berbeda
        
        Debug.Log("Day 2 positions setup complete with varied spawn indices!");
        #endif
    }
    
    // New method: Setup positions for different times of day
    public void SetupMorningPositions()
    {
        #if UNITY_EDITOR
        // Morning specific positions
        SnapCharacterToSpawn(CharacterType.Mother, 0); // Kamar Ortu
        SnapCharacterToSpawn(CharacterType.Father, 0); // Kamar Ortu
        SnapCharacterToSpawn(CharacterType.Baby, 0);   // Kamar Ortu
        SnapCharacterToSpawn(CharacterType.Bidan, 1);  // Different position for Bidan
        
        Debug.Log("Morning positions setup complete!");
        #endif
    }
    
    public void SetupAfternoonPositions()
    {
        #if UNITY_EDITOR
        // Afternoon specific positions
        SnapCharacterToSpawn(CharacterType.Mother, 1); // Different room
        SnapCharacterToSpawn(CharacterType.Father, 2); // Different room
        SnapCharacterToSpawn(CharacterType.Baby, 1);   // With mother
        SnapCharacterToSpawn(CharacterType.Bidan, 3);  // Different position
        
        Debug.Log("Afternoon positions setup complete!");
        #endif
    }
    
    public void SetupEveningPositions()
    {
        #if UNITY_EDITOR
        // Evening specific positions
        SnapCharacterToSpawn(CharacterType.Mother, 2); // Living room
        SnapCharacterToSpawn(CharacterType.Father, 2); // Living room
        SnapCharacterToSpawn(CharacterType.Baby, 2);   // Living room
        SnapCharacterToSpawn(CharacterType.Bidan, 4);  // Gone home or different area
        
        Debug.Log("Evening positions setup complete!");
        #endif
    }
    
    public void ResetAllCharacterPositions()
    {
        #if UNITY_EDITOR
        // Reset semua character ke posisi spawn 0
        foreach (var characterData in charactersDataArray)
        {
            if (characterData.HasValidSpawnPosition(0))
            {
                SnapCharacterToSpawn(characterData.characterType, 0);
            }
        }
        Debug.Log("All characters reset to default positions (spawn index 0)");
        #endif
    }
    
    // Utility method to get available spawn positions for a character
    public int GetMaxSpawnIndex(CharacterType characterType)
    {
        #if UNITY_EDITOR
        var characterData = System.Array.Find(charactersDataArray, c => c.characterType == characterType);
        if (characterData != null && characterData.spawnPositions != null)
        {
            return characterData.spawnPositions.Length - 1;
        }
        return 0;
        #else
        return 0;
        #endif
    }
    
    // Method to validate if all characters have required spawn positions
    public bool ValidateSpawnPositions(int requiredPositions)
    {
        #if UNITY_EDITOR
        foreach (var characterData in charactersDataArray)
        {
            if (characterData.spawnPositions == null || characterData.spawnPositions.Length < requiredPositions)
            {
                Debug.LogWarning($"Character {characterData.characterType} doesn't have {requiredPositions} spawn positions!");
                return false;
            }
        }
        return true;
        #else
        return false;
        #endif
    }
}
