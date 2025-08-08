using UnityEngine;

public class RaycastArrow : MonoBehaviour
{
    public GameObject raycasrArrow;

    void Start()
    {
        raycasrArrow = this.gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object has "RaycastObject" tag
        if (other.CompareTag("RaycastObject"))
        {
            // Align Y axis with the RaycastObject's Y axis
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = new Vector3(currentPosition.x, other.transform.position.y, currentPosition.z);
            transform.position = targetPosition;
            
            Debug.Log($"RaycastArrow aligned Y axis with RaycastObject: {other.name}");
        } else if (other.CompareTag("Player"))
        {
            // Face the player by looking at their position (Y-axis rotation only)
            Vector3 directionToPlayer = other.transform.position - transform.position;
            
            // Only use X and Z components for horizontal rotation, ignore Y difference
            directionToPlayer.y = 0;
            
            // Make sure we have a valid direction
            if (directionToPlayer.magnitude > 0.01f)
            {
                // Calculate only Y-axis rotation to face the player horizontally
                float targetYRotation = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg;
                
                // Apply only Y-axis rotation, keeping current X and Z rotations
                Vector3 currentRotation = transform.eulerAngles;
                transform.rotation = Quaternion.Euler(currentRotation.x, targetYRotation, currentRotation.z);
                
                Debug.Log($"RaycastArrow is now facing Player: {other.name}");
            }
        }
    }
}
