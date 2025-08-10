using UnityEngine;

public class RaycastObjectBehaviour : MonoBehaviour
{
    [Header("Character Identity")]
    [SerializeField] private string characterIdentity = ""; // e.g., "Mulyono", "Linda"
    [SerializeField] private string interactionDialogPath = ""; // e.g., "GameData/Dialog/Day2/Seq12AAyah"
    
    [Header("Raycast Detection Settings")]
    [SerializeField] private string logMessage = "Raycast hit detected!";
    [SerializeField] private bool showRaycastInfo = true;
    
    [Header("UI Spawn Settings")]
    [SerializeField] private GameObject raycastUIPrefab;
    [SerializeField] private bool destroyPreviousUI = true;
    [SerializeField] private string targetCanvasName = "Canvas3D";
    [SerializeField] private float contactLostDelay = 0.1f; // Delay before destroying UI when contact is lost
    
    [Header("Spawn Position Settings")]
    public Vector3 spawnOffset = new Vector3(1f, 1f, 1f); // Public - editable spawn offset from the object
    [SerializeField] private bool useWorldSpaceOffset = false; // Toggle between local and world space offset
    
    private GameObject spawnedUI;
    private Canvas targetCanvas;
    private bool isCurrentlyBeingRaycast = false;
    private bool wasBeingRaycast = false;
    private float lastRaycastTime = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the GameObject has a collider for raycast detection
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"GameObject '{gameObject.name}' doesn't have a Collider component. Adding BoxCollider for raycast detection.");
            gameObject.AddComponent<CapsuleCollider>();
        }
        
        // Try to find RaycastUI prefab if not assigned
        if (raycastUIPrefab == null)
        {
            FindRaycastUIPrefabInAssets();
        }
        
        // Find the target canvas
        FindTargetCanvas();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if contact with raycast has been lost
        CheckRaycastContact();
    }
    
    /// <summary>
    /// Called when a raycast hits this object's collider
    /// This method can be called by other scripts when they detect a raycast hit
    /// </summary>
    public void OnRaycastHit(RaycastHit hitInfo)
    {
        // Update raycast contact state
        isCurrentlyBeingRaycast = true;
        lastRaycastTime = Time.time;
        
        if (showRaycastInfo)
        {
            Debug.Log($"{logMessage} - GameObject: {gameObject.name}, Hit Point: {hitInfo.point}, Distance: {hitInfo.distance:F2}");
        }
        else
        {
            Debug.Log($"{logMessage} - GameObject: {gameObject.name}");
        }
        
        // Check if we need to spawn UI (only spawn when first detected)
        if (!wasBeingRaycast && spawnedUI == null)
        {
            SpawnRaycastUI();
        }
        
        wasBeingRaycast = true;
    }
    
    /// <summary>
    /// Simple version - just logs that this object was hit
    /// </summary>
    public void OnRaycastHit()
    {
        // Update raycast contact state
        isCurrentlyBeingRaycast = true;
        lastRaycastTime = Time.time;
        
        Debug.Log($"{logMessage} - GameObject: {gameObject.name}");
        
        // Check if we need to spawn UI (only spawn when first detected)
        if (!wasBeingRaycast && spawnedUI == null)
        {
            SpawnRaycastUI();
        }
        
        wasBeingRaycast = true;
    }
    
    /// <summary>
    /// Called when a raycast hits this object with custom message
    /// </summary>
    public void OnRaycastHit(string customMessage)
    {
        // Update raycast contact state
        isCurrentlyBeingRaycast = true;
        lastRaycastTime = Time.time;
        
        Debug.Log($"{customMessage} - GameObject: {gameObject.name}");
        
        // Check if we need to spawn UI (only spawn when first detected)
        if (!wasBeingRaycast && spawnedUI == null)
        {
            SpawnRaycastUI();
        }
        
        wasBeingRaycast = true;
    }
    
    /// <summary>
    /// Spawns the RaycastUI prefab slightly to the right of this object
    /// </summary>
    private void SpawnRaycastUI()
    {
        if (raycastUIPrefab == null)
        {
            Debug.LogError("RaycastUI prefab is not assigned! Please assign it in the inspector or place it in Resources folder.");
            return;
        }
        
        if (targetCanvas == null)
        {
            Debug.LogError($"Target canvas '{targetCanvasName}' not found! Please make sure the canvas exists in the scene.");
            return;
        }
        
        // Don't spawn if already exists
        if (spawnedUI != null)
        {
            return;
        }
        
        // Calculate spawn position with configurable offset
        Vector3 spawnPosition;
        if (useWorldSpaceOffset)
        {
            // World space offset - absolute position adjustment
            spawnPosition = transform.position + spawnOffset;
        }
        else
        {
            // Local space offset - relative to object's rotation (default)
            spawnPosition = transform.position + transform.TransformDirection(spawnOffset);
        }
        
        // Spawn the UI prefab as a child of the target canvas
        spawnedUI = Instantiate(raycastUIPrefab, targetCanvas.transform);
        
        // Set the world position while keeping it as a child of the canvas
        spawnedUI.transform.position = spawnPosition;
        spawnedUI.transform.rotation = transform.rotation;
        
        Debug.Log($"RaycastUI spawned at position: {spawnPosition} for object: {gameObject.name} under canvas: {targetCanvasName}");
    }
    
    /// <summary>
    /// Check if raycast contact has been lost and handle UI destruction
    /// </summary>
    private void CheckRaycastContact()
    {
        // Reset the current raycast state at the beginning of each frame
        isCurrentlyBeingRaycast = false;
        
        // Check if we've lost contact (no raycast hit for the specified delay)
        if (wasBeingRaycast && Time.time - lastRaycastTime > contactLostDelay)
        {
            // Contact lost - destroy the UI
            if (spawnedUI != null)
            {
                DestroySpawnedUI();
                Debug.Log($"Raycast contact lost. UI destroyed for object: {gameObject.name}");
            }
            
            wasBeingRaycast = false;
        }
    }
    
    /// <summary>
    /// Manually destroy the spawned UI
    /// </summary>
    public void DestroySpawnedUI()
    {
        if (spawnedUI != null)
        {
            Destroy(spawnedUI);
            spawnedUI = null;
            Debug.Log($"Spawned RaycastUI destroyed for object: {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Force destroy spawned UI and reset raycast state
    /// </summary>
    public void ForceResetRaycastState()
    {
        DestroySpawnedUI();
        isCurrentlyBeingRaycast = false;
        wasBeingRaycast = false;
        lastRaycastTime = 0f;
    }
    
    /// <summary>
    /// Check if UI is currently spawned
    /// </summary>
    public bool HasSpawnedUI()
    {
        return spawnedUI != null;
    }
    
    /// <summary>
    /// Search for RaycastUI prefab in the project assets
    /// </summary>
    private void FindRaycastUIPrefabInAssets()
    {
#if UNITY_EDITOR
        // Search for RaycastUI prefab in the project assets
        string[] guids = UnityEditor.AssetDatabase.FindAssets("RaycastUI t:GameObject");
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            
            if (prefab != null && prefab.name.Contains("RaycastUI"))
            {
                raycastUIPrefab = prefab;
                Debug.Log($"RaycastUI prefab found and assigned automatically: {assetPath}");
                return;
            }
        }
        
        Debug.LogWarning("RaycastUI prefab not found in project assets. Please assign it manually in the inspector.");
#else
        Debug.LogWarning("RaycastUI prefab not assigned. Please assign it manually in the inspector.");
#endif
    }
    
    /// <summary>
    /// Find the target canvas by name in the scene
    /// </summary>
    private void FindTargetCanvas()
    {
        // Search for canvas by name
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.name == targetCanvasName)
            {
                targetCanvas = canvas;
                Debug.Log($"Target canvas '{targetCanvasName}' found and assigned.");
                return;
            }
        }
        
        Debug.LogError($"Target canvas '{targetCanvasName}' not found in the scene! Please make sure a canvas with this name exists.");
    }
    
    /// <summary>
    /// Called when the player interacts with this object (presses E while looking at it)
    /// </summary>
    public virtual void OnInteraction()
    {
        Debug.Log($"Player interacted with {gameObject.name} (Identity: {characterIdentity})");
        // Override this method in derived classes for custom interaction behavior
    }
    
    /// <summary>
    /// Get the character identity for this raycast object
    /// </summary>
    public string GetCharacterIdentity()
    {
        return characterIdentity;
    }
    
    /// <summary>
    /// Get the dialog path for this character's interaction
    /// </summary>
    public string GetInteractionDialogPath()
    {
        return interactionDialogPath;
    }
    
    /// <summary>
    /// Set character identity and dialog path programmatically
    /// </summary>
    public void SetCharacterData(string identity, string dialogPath)
    {
        characterIdentity = identity;
        interactionDialogPath = dialogPath;
    }
}
