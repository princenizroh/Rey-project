using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    public GameObject playerModel;
    public GameObject npcModel;
    public GameObject npcCollider;

    void Start()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        // Ensure this trigger is only handled by the specific npcCollider
        if (npcCollider != null && other != null && other.CompareTag("Player"))
        {
            Debug.Log("Trigger detected with: " + other.name);

            if (playerModel == null)
            {
                playerModel = other.gameObject;
            }
            if (npcModel == null)
            {
                npcModel = gameObject;
            }

            // Make the NPC look at the player's X and Z position (ignore Y)
            Vector3 targetPosition = new Vector3(
                playerModel.transform.position.x,
                npcModel.transform.position.y,
                playerModel.transform.position.z
            );
            npcModel.transform.LookAt(targetPosition);
        }
    }

    void UpdateNpcStatus()
    {
        npcModel = GameObject.FindGameObjectWithTag("NPC");
        if (npcModel == null)
        {
            Debug.LogError("NPC Model not found in the scene.");
        }
    }
}
