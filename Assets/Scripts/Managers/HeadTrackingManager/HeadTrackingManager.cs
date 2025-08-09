using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;

public enum HeadTarget
{
    None, Camera, Mother, Father, Baby, Bidan
}

[System.Serializable]
public class CharacterHeadRig
{
    [Header("Character Info")]
    public CharacterType characterType;
    public MultiAimConstraint headConstraint;
    
    [Header("Single Target")]
    public Transform headTarget; // Single target that will be moved to different positions
    
    [Header("Settings")]
    public float transitionSpeed = 5f;
    
    [System.NonSerialized]
    public bool isInitialized = false;
}

public class HeadTrackingManager : MonoBehaviour
{
    public static HeadTrackingManager Instance;
    
    [Header("Character Head Rigs")]
    [SerializeField] private CharacterHeadRig[] characterRigs;
    
    [Header("Global Target References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform motherTransform;
    [SerializeField] private Transform fatherTransform;
    [SerializeField] private Transform babyTransform;
    [SerializeField] private Transform bidanTransform;
    
    [Header("Global Settings")]
    [SerializeField] private bool enableHeadTracking = true;
    
    private Dictionary<CharacterType, CharacterHeadRig> rigDict;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            InitializeHeadRigs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void InitializeHeadRigs()
    {
        rigDict = new Dictionary<CharacterType, CharacterHeadRig>();
        
        foreach (var rig in characterRigs)
        {
            if (rig.headConstraint != null && rig.headTarget != null)
            {
                rigDict[rig.characterType] = rig;
                rig.isInitialized = true;
                
                // Set initial constraint weight to 0 (disabled)
                rig.headConstraint.weight = 0f;
                
                Debug.Log($"[HeadTrackingManager] Initialized {rig.characterType} head rig");
            }
            else
            {
                Debug.LogWarning($"[HeadTrackingManager] {rig.characterType} head constraint or target not assigned!");
            }
        }
    }

    #region Public API Methods

    /// <summary>
    /// Set head target for specific character
    /// </summary>
    public void SetHeadTarget(CharacterType characterType, HeadTarget target)
    {
        if (!enableHeadTracking)
        {
            Debug.LogWarning("[HeadTrackingManager] Head tracking is globally disabled!");
            return;
        }

        if (rigDict.TryGetValue(characterType, out CharacterHeadRig rig))
        {
            SetConstraintTarget(rig, target);
            Debug.Log($"[HeadTrackingManager] {characterType} head target set to: {target}");
        }
        else
        {
            Debug.LogWarning($"[HeadTrackingManager] Character {characterType} not found in rigs!");
        }
    }

    /// <summary>
    /// Disable head tracking for specific character
    /// </summary>
    public void DisableHeadTracking(CharacterType characterType)
    {
        SetHeadTarget(characterType, HeadTarget.None);
    }

    /// <summary>
    /// Enable/disable head tracking globally
    /// </summary>
    public void EnableGlobalHeadTracking(bool enable)
    {
        enableHeadTracking = enable;
        
        if (!enable)
        {
            // Disable all character tracking
            foreach (var rig in characterRigs)
            {
                if (rig.headConstraint != null)
                {
                    rig.headConstraint.weight = 0f;
                }
            }
        }
        
        Debug.Log($"[HeadTrackingManager] Global head tracking: {(enable ? "ENABLED" : "DISABLED")}");
    }

    /// <summary>
    /// Set multiple characters to look at same target
    /// </summary>
    public void SetMultipleHeadTargets(CharacterType[] characters, HeadTarget target)
    {
        foreach (var character in characters)
        {
            SetHeadTarget(character, target);
        }
    }

    #endregion

    #region Private Helper Methods

    private void SetConstraintTarget(CharacterHeadRig rig, HeadTarget target)
    {
        if (rig.headConstraint == null || rig.headTarget == null) return;

        if (target == HeadTarget.None)
        {
            // Disable constraint
            rig.headConstraint.weight = 0f;
        }
        else
        {
            // Enable constraint and move target
            rig.headConstraint.weight = 1f;
            
            Vector3 targetPosition = GetTargetPosition(target);
            
            if (targetPosition != Vector3.zero)
            {
                // Move the single target to the desired position
                rig.headTarget.position = targetPosition;
            }
        }
    }

    private Vector3 GetTargetPosition(HeadTarget target)
    {
        switch (target)
        {
            case HeadTarget.Camera:
                if (mainCamera != null)
                {
                    // Slightly offset from camera for natural look
                    return mainCamera.transform.position + mainCamera.transform.forward * 0.5f;
                }
                break;
                
            case HeadTarget.Mother:
                if (motherTransform != null)
                    return motherTransform.position + Vector3.up * 1.6f; // Head height offset
                break;
                
            case HeadTarget.Father:
                if (fatherTransform != null)
                    return fatherTransform.position + Vector3.up * 1.7f; // Head height offset
                break;
                
            case HeadTarget.Baby:
                if (babyTransform != null)
                    return babyTransform.position + Vector3.up * 0.3f; // Baby head height
                break;
                
            case HeadTarget.Bidan:
                if (bidanTransform != null)
                    return bidanTransform.position + Vector3.up * 1.6f; // Head height offset
                break;
        }
        
        return Vector3.zero;
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test All Look at Camera")]
    private void TestAllLookAtCamera()
    {
        CharacterType[] allCharacters = { CharacterType.Mother, CharacterType.Father, CharacterType.Bidan };
        SetMultipleHeadTargets(allCharacters, HeadTarget.Camera);
    }

    [ContextMenu("Test Mother Look at Father")]
    private void TestMotherLookAtFather()
    {
        SetHeadTarget(CharacterType.Mother, HeadTarget.Father);
    }

    [ContextMenu("Disable All Head Tracking")]
    private void TestDisableAll()
    {
        EnableGlobalHeadTracking(false);
    }

    [ContextMenu("Enable All Head Tracking")]
    private void TestEnableAll()
    {
        EnableGlobalHeadTracking(true);
    }

    #endregion
}
