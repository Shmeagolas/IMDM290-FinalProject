using System.ComponentModel;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class CAWCAWCAW : MonoBehaviour
{
    public InputActionProperty rightTriggerAction;
    public AudioSource audioSource;
    public AudioClip clip;

    private bool isPlaying = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        rightTriggerAction.action.Enable();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float triggerValue = rightTriggerAction.action.ReadValue<float>();
        //Debug.Log("Trigger value: " + triggerValue);

        if (triggerValue == 1 && !isPlaying)
        {
            audioSource.PlayOneShot(clip);
            StartCoroutine(ResetIsPlaying(clip.length));
            isPlaying = true;
        }
    }

    // make sure sound doesn't play again each frame
    private System.Collections.IEnumerator ResetIsPlaying(float delay)
    {
        yield return new WaitForSeconds(delay);
        isPlaying = false;
    }
}