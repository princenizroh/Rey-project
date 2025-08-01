using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class ChargeMeter : MonoBehaviour
{
    private TMP_Text spaceSpamIndicator;
    private Image chargeMeterFillImage;
    private float chargeLevel = 0;
    private float chargeRate = 10f;

    void Start()
    {
        spaceSpamIndicator = GameObject.Find("TextSpam").GetComponent<TMP_Text>();
        if (spaceSpamIndicator == null)
        {
            Debug.LogError("SpaceSpamIndicator not found in the scene.");
        }

        chargeMeterFillImage = GameObject.Find("Indicator").GetComponent<Image>();
        if (chargeMeterFillImage == null)   
        {
            Debug.LogError("ChargeMeterFill not found in the scene.");
        }
    }

    public void changeChargeRate(float newChargeRate)
    {
        chargeRate = newChargeRate;
        Debug.Log("Charge rate changed to: " + chargeRate);
    }

    public void resetChargeLevel()
    {
        chargeLevel = 0;
        Debug.Log("Charge level reset to: " + chargeLevel);
    }

    [System.Obsolete]
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            spaceSpamIndicator.fontSize = 50;
            chargeLevel += chargeRate;

            if (Input.GetKey(KeyCode.X))
            {
                CoreGameManager coreGameManager = FindObjectOfType<CoreGameManager>();
                if (coreGameManager != null)
                {
                    coreGameManager.StartCoreGame("Rey");
                }
                else
                {
                    Debug.LogError("CoreGameManager not found in the scene.");
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            spaceSpamIndicator.fontSize = 60;
        }

        chargeMeterFillImage.fillAmount = chargeLevel / 100f;

        if (chargeLevel >= 100f)
        {
            gameObject.SetActive(false);
            chargeLevel = 0f;
        }
    }
}
