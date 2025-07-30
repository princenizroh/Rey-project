using UnityEngine;

public class CoreGameplay : MonoBehaviour
{
    private NPCDialogManagerMaster npcDialogManagerMaster;
    private MotherStats motherStats;

    [System.Obsolete]
    void Start()
    {
        npcDialogManagerMaster = FindObjectOfType<NPCDialogManagerMaster>();
        motherStats = FindObjectOfType<MotherStats>();
    }

    public void playerChoisess(int choice)
    {
        // Placeholder for player choices logic
        Debug.Log("Player made a choice: " + choice);
    }
    
}
