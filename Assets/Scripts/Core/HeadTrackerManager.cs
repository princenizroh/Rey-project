using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterHeadTracker
{
    [Header("Character Info")]
    public CharacterType characterType;
    public Transform headBone; // Assign mixamorig:Head here
    
    [Header("Settings")]
    public float trackingSpeed = 3f;
    public float maxAngle = 60f;
    public bool enableTracking = true;
    
    [Header("Runtime Info")]
    public Quaternion originalRotation;
    public bool isInitialized = false;
}

public class HeadTrackerManager : MonoBehaviour
{
    public static HeadTrackerManager Instance;
    
    [Header("Character Head Trackers")]
    [SerializeField] private CharacterHeadTracker[] characterTrackers;
    
    [Header("Global Settings")]
    [SerializeField] private bool globalEnable = true;
    [SerializeField] private float globalTrackingSpeed = 3f;
    [SerializeField] private Camera targetCamera;
    
    private Dictionary<CharacterType, CharacterHeadTracker> trackerDict;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            InitializeTrackers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        if (!globalEnable || targetCamera == null) return;
        
        UpdateHeadTracking();
    }

    private void InitializeTrackers()
    {
        trackerDict = new Dictionary<CharacterType, CharacterHeadTracker>();
        
        foreach (var tracker in characterTrackers)
        {
            if (tracker.headBone != null)
            {
                // Store original rotation
                tracker.originalRotation = tracker.headBone.localRotation;
                tracker.isInitialized = true;
                trackerDict[tracker.characterType] = tracker;
                
                Debug.Log($"[HeadTracker] Initialized {tracker.characterType} head tracking");
            }
        }
    }

    private void UpdateHeadTracking()
    {
        foreach (var tracker in characterTrackers)
        {
            if (tracker.enableTracking && tracker.isInitialized && tracker.headBone != null)
            {
                TrackCameraForCharacter(tracker);
            }
        }
    }

    private void TrackCameraForCharacter(CharacterHeadTracker tracker)
    {
        // Calculate direction to camera
        Vector3 directionToCamera = targetCamera.transform.position - tracker.headBone.position;
        directionToCamera.Normalize();
        
        // Convert to local space relative to character's body
        Transform characterRoot = tracker.headBone.root;
        Vector3 localDirection = characterRoot.InverseTransformDirection(directionToCamera);
        
        // Calculate target rotation
        Quaternion targetLookRotation = Quaternion.LookRotation(localDirection);
        
        // Apply angle constraints
        Vector3 eulerAngles = targetLookRotation.eulerAngles;
        eulerAngles.x = ClampAngle(eulerAngles.x, -tracker.maxAngle, tracker.maxAngle);
        eulerAngles.y = ClampAngle(eulerAngles.y, -tracker.maxAngle, tracker.maxAngle);
        eulerAngles.z = 0; // Lock Z rotation for natural look
        
        Quaternion constrainedRotation = Quaternion.Euler(eulerAngles);
        
        // Blend with original rotation
        Quaternion finalRotation = Quaternion.Slerp(
            tracker.originalRotation, 
            tracker.originalRotation * constrainedRotation, 
            0.7f // Blend factor
        );
        
        // Apply smooth rotation
        tracker.headBone.localRotation = Quaternion.Slerp(
            tracker.headBone.localRotation,
            finalRotation,
            Time.deltaTime * tracker.trackingSpeed
        );
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    #region Public Methods for NarratorBase Integration
    
    /// <summary>
    /// Enable/disable head tracking for specific character
    /// </summary>
    public void EnableHeadTracking(CharacterType characterType, bool enable)
    {
        if (trackerDict.TryGetValue(characterType, out CharacterHeadTracker tracker))
        {
            tracker.enableTracking = enable;
            
            // If disabling, smoothly return to original rotation
            if (!enable && tracker.headBone != null)
            {
                StartCoroutine(ReturnToOriginalRotation(tracker));
            }
            
            Debug.Log($"[HeadTracker] {characterType} head tracking: {(enable ? "ENABLED" : "DISABLED")}");
        }
        else
        {
            Debug.LogWarning($"[HeadTracker] Character {characterType} not found in trackers!");
        }
    }
    
    /// <summary>
    /// Enable/disable all head tracking
    /// </summary>
    public void EnableAllHeadTracking(bool enable)
    {
        foreach (var tracker in characterTrackers)
        {
            EnableHeadTracking(tracker.characterType, enable);
        }
    }
    
    /// <summary>
    /// Reset character head to original rotation
    /// </summary>
    public void ResetHeadToOriginal(CharacterType characterType)
    {
        if (trackerDict.TryGetValue(characterType, out CharacterHeadTracker tracker))
        {
            if (tracker.headBone != null)
            {
                StartCoroutine(ReturnToOriginalRotation(tracker));
            }
        }
    }
    
    private System.Collections.IEnumerator ReturnToOriginalRotation(CharacterHeadTracker tracker)
    {
        if (tracker.headBone == null) yield break;
        
        Quaternion startRotation = tracker.headBone.localRotation;
        float duration = 1f; // 1 second to return
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            tracker.headBone.localRotation = Quaternion.Slerp(
                startRotation,
                tracker.originalRotation,
                t
            );
            
            yield return null;
        }
        
        tracker.headBone.localRotation = tracker.originalRotation;
    }
    
    #endregion

    #region Debug Methods
    
    [ContextMenu("Test Enable All")]
    private void TestEnableAll()
    {
        EnableAllHeadTracking(true);
    }
    
    [ContextMenu("Test Disable All")]
    private void TestDisableAll()
    {
        EnableAllHeadTracking(false);
    }
    
    [ContextMenu("Test Mother Only")]
    private void TestMotherOnly()
    {
        EnableAllHeadTracking(false);
        EnableHeadTracking(CharacterType.Mother, true);
    }
    
    #endregion
}
