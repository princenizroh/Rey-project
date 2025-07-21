using UnityEngine;

public class DialogController : MonoBehaviour
{   
    public GameObject npcDialogThemplate;
    public GameObject npcQuestionThemplate;
    private GameObject dialogInstance;
    private GameObject questionInstance;

    [System.Obsolete]
    public GameObject summonDialogBar()
    {

        Debug.Log("Summoning dialog bar!");

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return null;
        }

        dialogInstance = Instantiate(npcDialogThemplate, canvas.transform, false); // Store reference

        if (dialogInstance == null)
        {
            Debug.LogError("Failed to instantiate npcDialogNene prefab!");
            return null;
        }
        dialogInstance.SetActive(true);

        // Stick to bottom and stretch horizontally, start off-screen
        RectTransform rect = dialogInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0, 0);    // left-bottom
            rect.anchorMax = new Vector2(1, 0);    // right-bottom
            rect.pivot = new Vector2(0.5f, 0);     // bottom center
            rect.sizeDelta = new Vector2(0, rect.sizeDelta.y); // stretch width, keep height

            // Start off the bottom of the screen
            float parentHeight = ((RectTransform)rect.parent).rect.height;
            rect.anchoredPosition = new Vector2(0, -rect.rect.height);

            // Animate up to visible position (flush with bottom)
            LeanTween.value(dialogInstance, rect.anchoredPosition.y, 0, 0.3f)
                .setEaseInOutBack()
                .setOnUpdate((float val) =>
                {
                    Vector2 pos = rect.anchoredPosition;
                    pos.y = val;
                    rect.anchoredPosition = pos;
                });
            Debug.Log("Dialog bar summoned!");
        }
        else
        {
            Debug.LogWarning("Dialog prefab has no RectTransform!");
        }
        return dialogInstance;
    }

    [System.Obsolete]
    public GameObject summonQuestionBar()
    {

        Debug.Log("Summoning dialog bar!");

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene!");
            return null;
        }

        questionInstance = Instantiate(npcQuestionThemplate, canvas.transform, false); // Store reference

        if (questionInstance == null)
        {
            Debug.LogError("Failed to instantiate npcDialogNene prefab!");
            return null;
        }
        questionInstance.SetActive(true);

        // Stick to top and stretch horizontally, start off-screen
        RectTransform rect = questionInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);    // center
            rect.anchorMax = new Vector2(0.5f, 0.5f);    // center
            rect.pivot = new Vector2(0.5f, 0f);        // center
            rect.anchoredPosition = new Vector2(0, ((RectTransform)rect.parent).rect.height / 2 + rect.rect.height); // Start above the screen

            // Animate down to center of the screen
            LeanTween.value(questionInstance, rect.anchoredPosition.y, 0, 0.3f)
            .setEaseInOutBack()
            .setOnUpdate((float val) =>
            {
                Vector2 pos = rect.anchoredPosition;
                pos.y = val;
                rect.anchoredPosition = pos;
            });
            Debug.Log("Question bar summoned!");
        }
        else
        {
            Debug.LogWarning("Question prefab has no RectTransform!");
        }
        return questionInstance;
    }

    public void DestroyDialogInstance()
    {
        Debug.Log("Destroying dialog instance!");
        if (dialogInstance != null)
        {
            // Animate dialogInstance moving completely off the bottom of the screen, then destroy
            RectTransform rect = dialogInstance.GetComponent<RectTransform>();

            if (rect != null)
            {
                // Move anchoredPosition.y to negative 100% of the parent height (off screen)
                float parentHeight = ((RectTransform)rect.parent).rect.height;
                float targetY = -parentHeight;
                LeanTween.value(dialogInstance, rect.anchoredPosition.y, targetY, 0.3f)
                    .setEaseOutQuint()
                    .setOnUpdate((float val) =>
                    {
                        Vector2 pos = rect.anchoredPosition;
                        pos.y = val;
                        rect.anchoredPosition = pos;
                    })
                    .setOnComplete(() =>
                    {
                        Destroy(dialogInstance);
                        dialogInstance = null;
                    });
            }
            else
            {
                Destroy(dialogInstance);
                dialogInstance = null;
            }

            if (questionInstance != null)
            {
                // Optionally animate out, or just destroy
                Destroy(questionInstance);
                questionInstance = null;
            }
        }
    }

    public static void DestroyAllQuestionBars()
    {
        // If you use a tag:
        foreach (var obj in GameObject.FindGameObjectsWithTag("QuestionBar"))
        {
            GameObject.Destroy(obj);
        }

        // Or if you use a specific component:
        // foreach (var qb in GameObject.FindObjectsOfType<QuestionBarComponent>())
        // {
        //     GameObject.Destroy(qb.gameObject);
        // }
    }

    public void destroyDialogMenu()
    {
        Destroy(gameObject);
    }
}
