using UnityEngine;
using UnityEngine.UI;

public class RightClickToOpenMenu : MonoBehaviour
{
    public GameObject menuOptionPrefab;
    private GameObject spawnedMenu;
    private bool isPlayerInside = false;

    private GameObject npc; // Store reference to NPC

    [System.Obsolete]
    void Update()
    {
        if (isPlayerInside && Input.GetMouseButtonDown(1) && spawnedMenu == null)
        {
            // Find or create a Canvas in the scene
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas", typeof(Canvas));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Find the first GameObject whose tag contains "npc" (case-insensitive)
            npc = null;
            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.CompareTag("Untagged")) continue;
                if (obj.tag.ToLower().Contains("npc"))
                {
                    npc = obj;
                    break;
                }
            }
            Vector3 spawnWorldPos = transform.position + new Vector3(1.5f, 0, 0); // Default offset

            if (npc != null)
            {
                // Spawn menu right next to the NPC (to the right)
                spawnWorldPos = npc.transform.position + new Vector3(-0.5f, 0, 0);
            }

            // Spawn the menu as a child of the Canvas
            spawnedMenu = Instantiate(menuOptionPrefab, canvas.transform);
            spawnedMenu.SetActive(true); // Ensure the menu is active

            // Set the position of the menu in screen space next to the NPC
            Vector3 screenPos = Camera.main.WorldToScreenPoint(spawnWorldPos);
            RectTransform rectTransform = spawnedMenu.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.position = screenPos;
                rectTransform.localScale = Vector3.zero; // Start from scale 0
                LeanTween.scale(spawnedMenu, Vector3.one, 0.3f).setEaseInOutBack();
            }
        }

        // Keep menu following the NPC while it's open
        if (spawnedMenu != null && npc != null)
        {
            Vector3 followWorldPos = npc.transform.position + new Vector3(-0.5f, 1, 0);
            Vector3 followScreenPos = Camera.main.WorldToScreenPoint(followWorldPos);
            RectTransform rectTransform = spawnedMenu.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.position = followScreenPos;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (spawnedMenu != null)
            {
                // Animate scale down, then destroy after animation
                LeanTween.scale(spawnedMenu, Vector3.zero, 0.3f).setEaseInOutBack().setOnComplete(() =>
                {
                    // Destroy the dialog if it exists
                    var handler = spawnedMenu.GetComponent<DialogController>();
                    if (handler != null)
                    {
                        handler.DestroyDialogInstance();
                    }
                    Destroy(spawnedMenu);
                    spawnedMenu = null;
                    npc = null;
                });
            }
        }
    }

    public void manualDestroyMenu()
    {
        LeanTween.scale(spawnedMenu, Vector3.zero, 0.3f).setEaseInOutBack().setOnComplete(() =>
            {
                Destroy(spawnedMenu);
                spawnedMenu = null;
                npc = null;
            });
    }
}
