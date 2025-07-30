using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    private GameObject playerModel;
    public GameObject npcModel;
    private bool playerDetected = false;

    // Dialogue system
    private NPCDialogManagerMaster dialogManager;

    [System.Obsolete]
    void Start()
    {

        playerModel = GameObject.FindGameObjectWithTag("Player");
        if (playerModel == null)
        {
            Debug.LogError("PlayerModel not found in the scene.");
        }

        dialogManager = FindObjectOfType<NPCDialogManagerMaster>();
        if (dialogManager == null)
        {
            Debug.LogError("NPCDialogManagerMaster not found in the scene.");
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trigger detected with: " + other.name);

            playerDetected = true;

            var player_model_pos = other.transform.position;

            Debug.Log("Player Model Position: " + player_model_pos.x + ", " + player_model_pos.y + ", " + player_model_pos.z);

            if (npcModel != null)
            {
                // In this special case, y controls the x and z axis
                Vector3 npcPos = npcModel.transform.position;
                Vector3 direction = new Vector3(
                    player_model_pos.y - npcPos.y, // x axis controlled by y
                    0,
                    player_model_pos.y - npcPos.y  // z axis controlled by y
                ).normalized;

                if (direction != Vector3.zero)
                {
                    npcModel.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trigger exited with: " + other.name);
            playerDetected = false;
        }
    }

    [System.Obsolete]
    void Update()
    {
        if (playerDetected && Input.GetKeyDown(KeyCode.E))
        {
            dialogManager.InitiateStartDialog("Rey/Linda/LindaTesting");
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
