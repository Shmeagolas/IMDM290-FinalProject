using System.ComponentModel;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cubesensor : MonoBehaviour
{
    public InputActionProperty rightTriggerAction;
    GameObject cube;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 offset = new Vector3(0, 0.5f, 2);
        cube.transform.localPosition = offset;

        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        UnityEngine.Color color = UnityEngine.Color.HSVToRGB(30f / 360f, 0.3f, 0.85f);
        cubeRenderer.material.color = color;
    }

    private void OnEnable()
    {
        rightTriggerAction.action.Enable();
    }

    // Update is called once per fram

    private void Update()
    {
        Renderer cubeRenderer = cube.GetComponent<Renderer>();
        UnityEngine.Color red = UnityEngine.Color.HSVToRGB(0f, 1f, 1f);
        UnityEngine.Color green = UnityEngine.Color.HSVToRGB(0.333f, 1f, 1f);

        float triggerValue = rightTriggerAction.action.ReadValue<float>();
        //Debug.Log("Trigger value: " + triggerValue);

        if (triggerValue == 1)
        {
            Debug.Log("right trigger pressed this frame");
            cubeRenderer.material.color = red;
        }
        else
        {
            cubeRenderer.material.color = green;
        }
    }
}
