using System;
using UnityEngine;

public class MenuButtonHandler : MonoBehaviour
{
    [Header("Assign in Inspector (on the prefab)")]
    
    private bool isDialogActive = false;
    private NPCDialogManagerMaster nPC_NameDialogManager;

    // Call this after instantiating the menu
    public void SetNPC(GameObject npcRef)
    {
        // No longer needed, but kept for compatibility
    }

    [System.Obsolete]
    public void OnFirstButtonClicked()
    {
        // Find the first GameObject in the scene with a tag containing "npc"
        GameObject npc = null;
        nPC_NameDialogManager = FindObjectOfType<NPCDialogManagerMaster>();

        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (!string.IsNullOrEmpty(obj.tag) && obj.tag.ToLower().Contains("npc"))
            {
                npc = obj;
                break;
            }
        }

        if (npc == null)
        {
            Debug.LogWarning("No NPC GameObject with tag containing 'npc' found!");
            return;
        }

        Debug.Log("First button clicked! NPC tag: " + npc.tag);

        // Show different dialog options based on the NPC tag using CompareTag
        if (npc.CompareTag("npc-nene") && isDialogActive == false)
        {
            nPC_NameDialogManager.InitiateStartDialog("NPC_Nene");
        }
        else if (npc.CompareTag("npc-shopkeeper") && isDialogActive == false)
        {
            nPC_NameDialogManager.InitiateStartDialog("NPC_Shopkeeper");
        }
        else if (npc.CompareTag("villager") && isDialogActive == false)
        {
            Debug.Log("Show villager dialog options.");
        }
    }

    public void OnSecondButtonClicked()
    {
        Debug.Log("Second button clicked!");
        // Add your logic here
    }

    internal void DestroyDialogInstance()
    {
        throw new NotImplementedException();
    }
}