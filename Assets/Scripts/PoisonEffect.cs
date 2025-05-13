using UnityEngine;
using UnityEngine.UI;

public class PoisonEffect : MonoBehaviour
{
    private RawImage rawImage;
    private float alpha = 0f;
    public GameObject finalScreen;
    private float startTime;
    private bool canFade = false;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        finalScreen.SetActive(false);
        SetAlpha(0f);
        startTime = Time.time;
    }

    void Update()
    {
        if (!canFade)
        {
            if (Time.time - startTime >= 120f) // 2 minutes
            {
                Debug.Log("starting the poisons ooooh");
                canFade = true;
            }
            else
            {
                return;
            }
        }

        if (alpha < 0.25f)
        {
            alpha += 0.00001f; // increase by 1% per frame
            if (alpha > 1f) alpha = 1f; // clamp to 1
            SetAlpha(alpha);
        } else
        {
            // Game Over
            DisplayFinalScore();
        }
    }

    void SetAlpha(float a)
    {
        Color color = rawImage.color;
        color.a = a;
        rawImage.color = color;
    }

    void DisplayFinalScore()
    {
        finalScreen.SetActive(true);
    }
}
