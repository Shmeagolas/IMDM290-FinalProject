using System.ComponentModel;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class CAWCAWCAW : MonoBehaviour
{
    public InputActionProperty rightTriggerAction;
    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        rightTriggerAction.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        float triggerValue = rightTriggerAction.action.ReadValue<float>();
        //Debug.Log("Trigger value: " + triggerValue);

        if (triggerValue == 1)
        {
            audioSource.PlayOneShot(yourClip);
        }
    }
}

