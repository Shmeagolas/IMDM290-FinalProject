using UnityEngine;

public class PoisonEffect : MonoBehaviour
{
    public float delayBeforeFlash = 10f; //120
    public float flashSpeed = 1f;         

    private CanvasGroup canvasGroup;
    private bool shouldFlash = false;
    private float direction = 1f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("No CanvasGroup component found on this GameObject.");
            enabled = false;
            return;
        }

        StartFlashing();
    }

    void StartFlashing()
    {
        Debug.Log("FlashCanvasAfterDelay: Starting flashing...");
        shouldFlash = true;
    }

    void Update()
    {
        if (shouldFlash)
        {
            canvasGroup.alpha += direction * flashSpeed * Time.deltaTime;
            Debug.Log(canvasGroup.alpha);

            if (canvasGroup.alpha >= 1f)
            {
                canvasGroup.alpha = 1f;
                direction = -1f;
            }
            else if (canvasGroup.alpha <= 0f)
            {
                canvasGroup.alpha = 0f;
                direction = 1f;
            }
        }
    }
}
