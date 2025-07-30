using UnityEngine;
using UnityEngine.UI;

public class StressBarIndicatorIbu : MonoBehaviour
{
    private Image stressBarFillImage;
    // pindahkan ke scriptable save
    private float stressLevel = 0f;
    private float stressRate = 10f;

    void Start()
    {
        stressBarFillImage = GameObject.Find("StressBarFill").GetComponent<Image>();
        if (stressBarFillImage == null)
        {
            Debug.LogError("StressBarFill not found in the scene.");
        }
    }

    void Update()
    {
        stressLevel += stressRate * Time.deltaTime;
        stressBarFillImage.fillAmount = stressLevel / 100f;
    }
}
