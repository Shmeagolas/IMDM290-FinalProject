using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using static MovementController;
using static UnityEngine.Rendering.DebugUI.Table;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;

public class PathFollower : MonoBehaviour
{
    HashSet<(Vector2, HexTile.StartDir, HexTile.TileType)> map;
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
    Vector2 gridPos = Vector2.down * 100000;
    Vector3 startPos;
    Vector3 endPos;
    HexTile.StartDir startDir;
    HexTile.StartDir? leftEndDir = null;
    HexTile.StartDir? rightEndDir = null;
    HexTile.StartDir? straightDir = null;
    public bool vrEnabled = false;
    public enum TurnMode
    {
        Left, Right, Straight
    }
    TurnMode? turnMode = null;
    float t = 0;
    void Start()
    {
        map = PremadeMaps.mapOne;
    }

    HexTile.StartDir GetDir(TurnMode mode)
    {
        switch(mode)
        {
            case TurnMode.Left:
                return leftEndDir.Value;
            case TurnMode.Straight:
                return straightDir.Value;
            case TurnMode.Right:
                return rightEndDir.Value;
            default:
                Debug.LogError("Movement Controller Turn Mode undefined");
                return straightDir.Value;
        }
    }
    void UpdateTurnMode()
    {
        if (vrEnabled && turnMode.HasValue) // VR MODE
        {
            var angle = cam.transform.parent.localEulerAngles;
            //tmpro.text = $"turnmode: {(turnMode == TurnMode.Left ? "left" : turnMode == TurnMode.Right ? "right" : "straight")} speed: {speed}";
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
        }
        else if (turnMode.HasValue) // NON VR MODE
        {
            if (Input.GetKey(KeyCode.LeftArrow)) turnMode = TurnMode.Left;
            else if (Input.GetKey(KeyCode.RightArrow)) turnMode = TurnMode.Right;
            else turnMode = TurnMode.Straight;
        }
    }
    void Update()
    {
        float diff = 0;
        if (vrEnabled && turnMode.HasValue) // VR MODE
        {
            diff = Mathf.Abs((lastLeftPos - leftController.transform.position).magnitude) + Mathf.Abs((lastRightPos - rightController.transform.position).magnitude) * Time.deltaTime / 2f;
        }

        // Acceleration
        bool doAccel = vrEnabled ? diff >= minDiff : Input.GetKey(KeyCode.Space);
        if (doAccel)
        {
            speed += accel * Time.deltaTime;
            speed = Mathf.Min(speed, maxSpeed);
        }
        else
        {
            speed -= decel * Time.deltaTime;
            speed = Mathf.Max(speed, 0);
        }

        // Update Previous Positions.
        if (vrEnabled)
        {
            lastLeftPos = leftController.transform.position;
            lastRightPos = rightController.transform.position;
        }

        Vector2 grid = TileGrid.WorldToGrid(transform.position);

        if (grid != gridPos)
        {
            print("new grid " + grid + ", time: " + Time.time);
            gridPos = grid;

            switch (turnMode.HasValue ? GetDir(turnMode.Value) : HexTile.StartDir.N)
            {
                case HexTile.StartDir.N: startDir = HexTile.StartDir.S; break;
                case HexTile.StartDir.S: startDir = HexTile.StartDir.N; break;
                case HexTile.StartDir.NE: startDir = HexTile.StartDir.SW; break;
                case HexTile.StartDir.SW: startDir = HexTile.StartDir.NE; break;
                case HexTile.StartDir.NW: startDir = HexTile.StartDir.SE; break;
                case HexTile.StartDir.SE: startDir = HexTile.StartDir.NW; break;
                default: startDir = HexTile.StartDir.S; break;
            }
            startPos = GetPosition(gridPos, startDir);
            ComputeEndDirs();

            t = 0;

            // Select Turn
            UpdateTurnMode();

            // Correct Turn Mode if an option does not exist.
            if (turnMode == TurnMode.Left && !leftEndDir.HasValue)
                turnMode = straightDir.HasValue ? TurnMode.Straight : TurnMode.Right;
            else if (turnMode == TurnMode.Right && !rightEndDir.HasValue)
                turnMode = straightDir.HasValue ? TurnMode.Straight : TurnMode.Left;
            else if (turnMode == TurnMode.Straight && !straightDir.HasValue)
                turnMode = leftEndDir.HasValue ? TurnMode.Left : TurnMode.Right;
            else if (turnMode == null)
                turnMode = TurnMode.Straight;

            print("turnmode " + turnMode);
            switch (turnMode)
            {
                case TurnMode.Straight: endPos = GetPosition(gridPos, straightDir.Value); break;
                case TurnMode.Right: endPos = GetPosition(gridPos, rightEndDir.Value); break;
                case TurnMode.Left: endPos = endPos = GetPosition(gridPos, leftEndDir.Value); break;
                default: print("TurnMode Unknown ERROR"); break;
            }
        }

        // Update Position.
        t += speed * Time.deltaTime;
        var oldPos = transform.position;
        transform.position = Vector3.LerpUnclamped(Vector3.LerpUnclamped(startPos, TileGrid.GridToWorld(gridPos), t), Vector3.LerpUnclamped(TileGrid.GridToWorld(gridPos), endPos, t), t);
        if (oldPos != transform.position)
        {
            var currDir = transform.position - oldPos;
            var angle = (currDir.x < 0? -1 : 1) * Vector2.Angle(Vector2.up, new Vector2(currDir.x, currDir.z));
            transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        }
    }
    float GetRotation(HexTile.StartDir dir)
    {
        switch (dir)
        {
            case HexTile.StartDir.N: return Mathf.PI / 2;
            case HexTile.StartDir.S: return -Mathf.PI / 2;
            case HexTile.StartDir.NW: return 5 * Mathf.PI / 6;
            case HexTile.StartDir.SW: return -5 * Mathf.PI / 6;
            case HexTile.StartDir.NE: return Mathf.PI / 6;
            case HexTile.StartDir.SE: return -Mathf.PI / 6;
            default: return 0;
        }
    }
    int GetDeg(HexTile.StartDir dir)
    {
        switch (dir)
        {
            case HexTile.StartDir.N:
                return 180;
            case HexTile.StartDir.NE:
                return 240;
            case HexTile.StartDir.SE:
                return 300;
            case HexTile.StartDir.S:
                return 0;
            case HexTile.StartDir.SW:
                return 60;
            case HexTile.StartDir.NW:
                return 120;
        }
        return 0;
    }
    HexTile.StartDir GetDir(int deg)
    {
        switch (deg)
        {
            case 180:
                return HexTile.StartDir.N;
            case 240:
                return HexTile.StartDir.NE;
            case 300:
                return HexTile.StartDir.SE;
            case 0:
                return HexTile.StartDir.S;
            case 60:
                return HexTile.StartDir.SW;
            case 120:
                return HexTile.StartDir.NW;
        }
        return 0;
    }
    Vector3 GetPosition(Vector2 gridPos, HexTile.StartDir dir)
    {
        float rot = 0;
        rot = GetRotation(dir);
        return TileGrid.GridToWorld(gridPos) + new Vector3(Mathf.Cos(rot), 0, Mathf.Sin(rot));
    }
    void ComputeEndDirs()
    {
        var entry = map.FirstOrDefault(x => x.Item1 == gridPos);
        List<int> dirs = new List<int>();
        switch(entry.Item3)
        {
            case HexTile.TileType.STRAIGHT:
                dirs.Add(GetDeg(HexTile.StartDir.S));
                dirs.Add(GetDeg(HexTile.StartDir.N));
                break;
            case HexTile.TileType.DCURVE:
                dirs.Add(GetDeg(HexTile.StartDir.S));
                dirs.Add(GetDeg(HexTile.StartDir.NE));
                dirs.Add(GetDeg(HexTile.StartDir.NW));
                break;
            case HexTile.TileType.LCURVE:
                dirs.Add(GetDeg(HexTile.StartDir.S));
                dirs.Add(GetDeg(HexTile.StartDir.NW));
                break;
            case HexTile.TileType.RCURVE:
                dirs.Add(GetDeg(HexTile.StartDir.S));
                dirs.Add(GetDeg(HexTile.StartDir.NE));
                break;
            case HexTile.TileType.LCSTRAIGHT:
                dirs.Add(GetDeg(HexTile.StartDir.S));
                dirs.Add(GetDeg(HexTile.StartDir.N));
                dirs.Add(GetDeg(HexTile.StartDir.NW));
                break;
            case HexTile.TileType.RCSTRAIGHT:
                dirs.Add(GetDeg(HexTile.StartDir.S));
                dirs.Add(GetDeg(HexTile.StartDir.N));
                dirs.Add(GetDeg(HexTile.StartDir.NE));
                break;
            case HexTile.TileType.NULL:
                print("FFAAAACCC");
                break;
            default:
                print("FACCC DEFAULT");
                break;
        }
        int degOffset = GetDeg(entry.Item2);
        leftEndDir = null;
        straightDir = null;
        rightEndDir = null;
        for (int i = 0; i < dirs.Count; i++)
        {
            dirs[i] += degOffset;
            dirs[i] %= 360;
            print($"dirs[{i}] -> " + dirs[i]);
            if (GetDir(dirs[i]) == startDir)
            {
                //Debug.LogError("remove");
                dirs.RemoveAt(i);
                i--;
                continue;
            }
            //Debug.LogError($"aaa  {(GetDeg(startDir) + 180) % 360}");

            if (dirs[i] == (GetDeg(startDir) + 120) % 360)
                leftEndDir = GetDir(dirs[i]);
            else if (dirs[i] == (GetDeg(startDir) + 180) % 360)
                straightDir = GetDir(dirs[i]);
            else if (dirs[i] == (GetDeg(startDir) + 240) % 360)
                rightEndDir = GetDir(dirs[i]);                

            print($"{leftEndDir}, {straightDir}, {rightEndDir}");
        }
    }
}