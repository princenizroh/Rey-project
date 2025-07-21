using TMPro;
using UnityEngine;

public class InteractionWithDoor : MonoBehaviour
{
    private NPCDialogManagerMaster npcDialogManager;
    public Transform doorTransform;
    private bool doorState = false;
    public TMP_Text uiTextInfo;
    public GameObject textUiInfo;

    private bool isPlayerInside = false;

    void Start()
    {
        
    }

    [System.Obsolete]
    void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            OnEPressed();
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            textUiInfo.SetActive(true);
            isPlayerInside = true;
            uiTextInfo.text = "Press 'E' to interact with the door.";
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            textUiInfo.SetActive(false);
            isPlayerInside = false;
        }
    }

    [System.Obsolete]
    private void OnEPressed()
    {
        Debug.Log("E key pressed while inside the trigger zone.");
        DoorInteraction();
    }

    [System.Obsolete]
    private void DoorInteraction()
    {
        if (doorState)
        {
            doorState = false;
            LeanTween.moveZ(doorTransform.gameObject, 0f, 1f).setEaseInOutQuad();

            // Add dialog test here

            if (npcDialogManager == null)
            {
                npcDialogManager = FindObjectOfType<NPCDialogManagerMaster>();
            }
            if (npcDialogManager != null)
            {
                Debug.Log("Initiating dialog with NPCDialogManager.");
                npcDialogManager.InitiateStartDialog("Quest/TestDialog/Testing");
            }

        }
        else
        {
            doorState = true;
            LeanTween.moveZ(doorTransform.gameObject, -3f, 1f).setEaseInOutQuad();
        }
    }
}
