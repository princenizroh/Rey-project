using System.Collections;
using TMPro;
using UnityEngine;

public class RaycastObjectCam : MonoBehaviour
{
    [Header("Raycast Settings")]
    public bool raycastStatus = false;
    public GameObject currentHitObject;
    public float rayDistance = 10f;
    public LayerMask layerMask = -1; // All layers by default
    public CoreGameManager coreGameManager;

    public TextMeshProUGUI narratorText;
    
    [Header("Visual Settings")]
    public LineRenderer lineRenderer;
    public Color hitColor = Color.green;
    public Color missColor = Color.red;
    public float lineWidth = 0.1f;
    
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private string interactionMessage = "Interaction key pressed!";
    
    private Camera playerCamera;
    private bool isHitting = false;
    private RaycastObjectBehaviour currentHitBehaviour = null;
    
    void Start()
    {
        // Get the camera component
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Setup LineRenderer if not assigned
        if (lineRenderer == null)
        {
            SetupLineRenderer();
        }
        
        // Configure LineRenderer
        ConfigureLineRenderer();
    }
    
    void Update()
    {
        PerformRaycast();
        UpdateVisual();
        HandleInteraction();
    }
    
    void PerformRaycast()
    {
        // Get ray from camera center
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        // Perform raycast
        if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
        {
            // Check if hit object has the RaycastObjectBehaviour script
            RaycastObjectBehaviour objectBehaviour = hit.collider.GetComponent<RaycastObjectBehaviour>();
            
            if (objectBehaviour != null)
            {
                isHitting = true;
                
                // Store reference to current hit behaviour for interaction
                currentHitBehaviour = objectBehaviour;
                currentHitObject = hit.collider.gameObject;
                
                // Call the behaviour script to handle the hit detection
                objectBehaviour.OnRaycastHit(hit);
                Debug.Log($"Using existing RaycastObjectBehaviour script on: {hit.collider.name}");
            }
            else
            {
                isHitting = false;
                currentHitBehaviour = null; // Clear reference when not hitting object with script
                currentHitObject = null; // Clear reference when not hitting object with script
            }
        }
        else
        {
            isHitting = false;
            currentHitBehaviour = null; // Clear reference when not hitting anything
            currentHitObject = null; // Clear reference when not hitting anything
        }
    }
    
    void UpdateVisual()
    {
        if (lineRenderer != null)
        {
            // Set line color based on hit status
            Color currentColor = isHitting ? hitColor : missColor;
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;
            lineRenderer.material.color = currentColor;
            
            // Set line positions
            Vector3 startPoint = transform.position;
            Vector3 endPoint = transform.position + (transform.forward * rayDistance);
            
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
    }
    
    void SetupLineRenderer()
    {
        // Create LineRenderer component if it doesn't exist
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
    }
    
    void ConfigureLineRenderer()
    {
        if (lineRenderer != null)
        {
            // Configure LineRenderer properties
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = true;
            
            // Create a simple material if none exists
            if (lineRenderer.material == null)
            {
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
            
            // Set initial color
            lineRenderer.startColor = missColor;
            lineRenderer.endColor = missColor;
        }
    }
    
    // Draw gizmos in scene view for debugging
    void OnDrawGizmos()
    {
        Gizmos.color = isHitting ? hitColor : missColor;
        Gizmos.DrawRay(transform.position, transform.forward * rayDistance);
    }
    
    /// <summary>
    /// Check for interaction input when an object is detected
    /// </summary>
    void HandleInteraction()
    {
        // Update raycastStatus based on current hit state
        raycastStatus = isHitting;
        
        // Check for interaction input when hitting a raycast object
        if (isHitting && currentHitBehaviour != null && Input.GetKeyDown(interactionKey))
        {
            // Trigger interaction on the hit object
            currentHitBehaviour.OnInteraction();
            Debug.Log($"Interaction triggered on: {currentHitBehaviour.gameObject.name}");
        }
    }
}
