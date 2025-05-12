using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class TestPathFollower : MonoBehaviour
{
    HashSet<(Vector2, HexTile.StartDir, HexTile.TileType)> map;
    public float speed;
    Vector2 gridPos = Vector2.down * 100000;
    Vector3 startPos;
    Vector3 endPos;
    HexTile.StartDir startDir;
    HexTile.StartDir? leftEndDir = null;
    HexTile.StartDir? rightEndDir = null;
    HexTile.StartDir? straightDir = null;
    MovementController.TurnMode? turnMode = null;
    float t = 0;
    void Start()
    {
        map = PremadeMaps.mapOne;
    }

    HexTile.StartDir GetDir(MovementController.TurnMode mode)
    {
        switch(mode)
        {
            case MovementController.TurnMode.Left:
                return leftEndDir.Value;
            case MovementController.TurnMode.Straight:
                return straightDir.Value;
            case MovementController.TurnMode.Right:
                return rightEndDir.Value;
            default:
                Debug.LogError("Movement Controller Turn Mode undefined");
                return straightDir.Value;
        }
    }
    void Update()
    {
        Vector2 grid = TileGrid.WorldToGrid(transform.position);
        
        if (grid != gridPos)
        {
            print("new grid " + grid);
            gridPos = grid;

            switch (turnMode != null? GetDir(turnMode.Value): HexTile.StartDir.N)
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
            if (straightDir.HasValue)
            {
                endPos = GetPosition(gridPos, straightDir.Value);
                turnMode = MovementController.TurnMode.Straight;
            }
            else if (rightEndDir.HasValue)
            {
                endPos = GetPosition(gridPos, rightEndDir.Value);
                turnMode = MovementController.TurnMode.Right;
            }
            else if (leftEndDir.HasValue)
            {
                endPos = GetPosition(gridPos, leftEndDir.Value);
                turnMode = MovementController.TurnMode.Left;
            }
            else
            {
                print("FFFFFFFFAAAA");
            }
            
            
        }
        t += speed * Time.deltaTime;
        //var strPos = GetPosition(gridPos, GetDir((GetDeg(startDir) + 180) % 360));
        transform.position = Vector3.LerpUnclamped(Vector3.LerpUnclamped(startPos, TileGrid.GridToWorld(gridPos), t), Vector3.LerpUnclamped(TileGrid.GridToWorld(gridPos), endPos, t),t);
        //print($"{startPos}, {TileGrid.GridToWorld(gridPos)}, {endPos}");
        //print(transform.position);


        // get potential options from new grid.


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
    HexTile.StartDir GetDir(float rad, float epsilon = 0.01f)
    {
        if (Mathf.Abs(rad - (Mathf.PI / 2)) < epsilon) return HexTile.StartDir.N;
        else if (Mathf.Abs(rad - (-Mathf.PI / 2)) < epsilon) return HexTile.StartDir.N;
        else if (Mathf.Abs(rad - (4 * Mathf.PI / 6)) < epsilon) return HexTile.StartDir.N;
        else if (Mathf.Abs(rad - (-4 * Mathf.PI / 6)) < epsilon) return HexTile.StartDir.N;
        else if (Mathf.Abs(rad - (Mathf.PI / 6)) < epsilon) return HexTile.StartDir.N;
        else if (Mathf.Abs(rad - (-Mathf.PI / 6)) < epsilon) return HexTile.StartDir.N;
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