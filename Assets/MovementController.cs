using System;
using Unity.Mathematics;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GameObject cam;
    public float speed;
    public float minTurnAngle = 30.0f;
    public GameObject leftController;
    public GameObject rightController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var angle = cam.transform.rotation;
        if (angle.x < -Mathf.Deg2Rad * minTurnAngle) { 
            // TURN LEFT
        } else if (angle.x > Mathf.Deg2Rad * minTurnAngle) {
            // TURN RIGHT
        } else {
            // STAY STILL
        }
    }
}
