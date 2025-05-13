using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class MovementController : MonoBehaviour
{
    public GameObject cam;
    public float maxSpeed;
    float speed;
    public float accel;
    public float decel;
    public float minDiff = 0.01f;
    public float minTurnAngle = 10.0f;
    public GameObject leftController;
    public GameObject rightController;
    public TMPro.TextMeshProUGUI tmpro;
    Vector3 lastLeftPos;
    Vector3 lastRightPos;
    public enum TurnMode
    {
        Left, Right, Straight
    }
    TurnMode turnMode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float diff = Mathf.Abs((lastLeftPos - leftController.transform.position).magnitude) + Mathf.Abs((lastRightPos - rightController.transform.position).magnitude) * Time.deltaTime / 2f;
        var angle = cam.transform.parent.localEulerAngles;
        //tmpro.text = $"turnmode: {(turnMode == TurnMode.Left ? "left" : turnMode == TurnMode.Right ? "right" : "straight")}\n angle: {angle.x},{angle.y},{angle.z}, diff: {diff}";
        tmpro.text = $"turnmode: {(turnMode == TurnMode.Left ? "left" : turnMode == TurnMode.Right ? "right" : "straight")} speed: {speed}";
        if (angle.z < minTurnAngle || angle.z > 360 - minTurnAngle)
        {
            turnMode = TurnMode.Straight;
        }
        else if (angle.z > 180) 
        {
            turnMode = TurnMode.Right;
        } 
        else if (angle.z < 180)  
        {
            turnMode = TurnMode.Left;
        }

        if (diff >= minDiff)
        {
            speed += accel * Time.deltaTime;
            speed = Mathf.Min(speed, maxSpeed);
        }
        else
        {
            speed -= decel * Time.deltaTime;
            speed = Mathf.Max(speed, 0);
        }
        lastLeftPos = leftController.transform.position;
        lastRightPos = rightController.transform.position;
    }
}
