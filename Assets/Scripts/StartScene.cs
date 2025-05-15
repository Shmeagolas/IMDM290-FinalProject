using System.ComponentModel;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class StartScene : MonoBehaviour
{
    public InputActionProperty rightTriggerAction;
    private void OnEnable()
    {
        rightTriggerAction.action.Enable();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float triggerValue = rightTriggerAction.action.ReadValue<float>();
        //Debug.Log("Trigger value: " + triggerValue);

        if (triggerValue == 1)
        {
            gameObject.SetActive(false);
        }
    }
}