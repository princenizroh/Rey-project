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
    
    [Header("Target Transforms")]
    public Transform cameraTarget;
    public Transform motherTarget;
    public Transform fatherTarget;
    public Transform babyTarget;
    public Transform bidanTarget;
    
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
        
        // Setup target references
        SetupTargetReferences();
    }
    
    private void SetupTargetReferences()
    {
        foreach (var rig in characterRigs)
        {
            // Auto-assign global references if individual ones not set
            if (rig.cameraTarget == null && mainCamera != null)
            {
                // Create a target object for camera if not exists
                GameObject cameraTargetObj = new GameObject($"{rig.characterType}_CameraTarget");
                rig.cameraTarget = cameraTargetObj.transform;
                cameraTargetObj.transform.SetParent(mainCamera.transform);
                cameraTargetObj.transform.localPosition = Vector3.forward * 0.5f;
            }
            
            if (rig.motherTarget == null && motherTransform != null)
                rig.motherTarget = motherTransform;
                
            if (rig.fatherTarget == null && fatherTransform != null)
                rig.fatherTarget = fatherTransform;
                
            if (rig.babyTarget == null && babyTransform != null)
                rig.babyTarget = babyTransform;
                
            if (rig.bidanTarget == null && bidanTransform != null)
                rig.bidanTarget = bidanTransform;
        }
    }

    private void InitializeHeadRigs()
    {
        rigDict = new Dictionary<CharacterType, CharacterHeadRig>();
        
        foreach (var rig in characterRigs)
        {
            if (rig.headConstraint != null)
            {
                rigDict[rig.characterType] = rig;
                rig.isInitialized = true;
                
                // Set initial constraint weight to 0 (disabled)
                rig.headConstraint.weight = 0f;
                
                // Clear existing source objects if any
                var data = rig.headConstraint.data;
                data.sourceObjects.Clear();
                
                Debug.Log($"[HeadTrackingManager] Initialized {rig.characterType} head rig");
            }
            else
            {
                Debug.LogWarning($"[HeadTrackingManager] {rig.characterType} head constraint not assigned!");
            }
        }
    }

    #region Public API Methods

    /// <summary>
    /// Set head target for specific character
    /// </summary>
    public void SetHeadTarget(CharacterType characterType, HeadTarget target)
    {
        SetHeadTarget(characterType, target, 1f);
    }

    /// <summary>
    /// Set head target for specific character with custom weight
    /// </summary>
    public void SetHeadTarget(CharacterType characterType, HeadTarget target, float weight)
    {
        if (!enableHeadTracking)
        {
            Debug.LogWarning("[HeadTrackingManager] Head tracking is globally disabled!");
            return;
        }

        if (rigDict.TryGetValue(characterType, out CharacterHeadRig rig))
        {
            SetConstraintTarget(rig, target, weight);
            Debug.Log($"[HeadTrackingManager] {characterType} head target set to: {target} with weight: {weight}");
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

    private void SetConstraintTarget(CharacterHeadRig rig, HeadTarget target, float weight = 1f)
    {
        if (rig.headConstraint == null) return;

        var data = rig.headConstraint.data;
        
        if (target == HeadTarget.None)
        {
            // Disable constraint entirely
            rig.headConstraint.weight = 0f;
            data.sourceObjects.Clear();
        }
        else
        {
            // Enable constraint
            rig.headConstraint.weight = 1f;
            
            // Get target transform
            Transform targetTransform = GetTargetTransform(rig, target);
            
            if (targetTransform != null)
            {
                // Clear existing source objects
                data.sourceObjects.Clear();
                
                // Add new source object with specified weight
                var weightedTransform = new WeightedTransform(targetTransform, weight);
                data.sourceObjects.Add(weightedTransform);
                
                Debug.Log($"[HeadTrackingManager] Added target {target} with weight {weight} to {rig.characterType}");
            }
            else
            {
                Debug.LogWarning($"[HeadTrackingManager] Target transform for {target} not found in {rig.characterType} rig!");
            }
        }
    }

    private Transform GetTargetTransform(CharacterHeadRig rig, HeadTarget target)
    {
        switch (target)
        {
            case HeadTarget.Camera:
                if (rig.cameraTarget != null) return rig.cameraTarget;
                if (mainCamera != null) return mainCamera.transform;
                break;
                
            case HeadTarget.Mother:
                if (rig.motherTarget != null) return rig.motherTarget;
                if (motherTransform != null) return motherTransform;
                break;
                
            case HeadTarget.Father:
                if (rig.fatherTarget != null) return rig.fatherTarget;
                if (fatherTransform != null) return fatherTransform;
                break;
                
            case HeadTarget.Baby:
                if (rig.babyTarget != null) return rig.babyTarget;
                if (babyTransform != null) return babyTransform;
                break;
                
            case HeadTarget.Bidan:
                if (rig.bidanTarget != null) return rig.bidanTarget;
                if (bidanTransform != null) return bidanTransform;
                break;
        }
        
        return null;
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
