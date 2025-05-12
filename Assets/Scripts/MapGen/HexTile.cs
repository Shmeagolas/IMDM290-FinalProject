using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.TextCore;
using System.IO;
using System.Linq;
//using System.Numerics;

public class HexTile
{
    public enum StartDir
    {
        N, NE, SE, S, SW, NW
    }

    public enum TileType
    {
        STRAIGHT, LCURVE, RCURVE, DCURVE, LCSTRAIGHT, RCSTRAIGHT, NULL //, LOOP
    }

    public StartDir dir { get; set; }
    public TileType type { get; set; }
    public Vector2 fromPos { get; set; }
    public List<Vector2> toPos { get; set; }
    public List<Vector2> toTiles { get; }
    public Vector2 pos;
    public Stack<TileType> possibleTypes = new();

    public HexTile(Vector2 p, StartDir d, Vector2 fp, TileType t, Stack<TileType> pt)
    {
        pos = p;
        dir = d;
        fromPos = fp;
        type = t;
        possibleTypes = pt;
        toPos = GetToPos();
    }

    public bool IsTouchingDir(StartDir otherDir)
    {
        if (type == TileType.NULL) return false;
        return GetToDirs(type, dir).Contains(OppositeDir(otherDir));
    }

    public bool SwitchType()
    {
        if (possibleTypes.Count == 0) return false;
        type = possibleTypes.Pop();
        return true;
    }

    public bool OneToTile()
    {
        return (type == TileType.STRAIGHT || type == TileType.LCURVE || type == TileType.RCURVE);
    }

    public List<(Vector2, StartDir)> GetToPosDirs()
    {
        var poss = GetToPos();
        var dirs = GetToDirs(type, dir);
        return poss.Zip(dirs, (tpos, tdir) => (tpos, tdir)).ToList();
    }

    private List<Vector2> GetToPos()
    {
        return DirsToPos(GetToDirs(type, dir), pos);
    }

    public static List<StartDir> GetToDirs(TileType t, StartDir dir)
    {
        List<StartDir> dirList = new();
        switch (t)
        {
            case TileType.STRAIGHT:
                dirList.Add(OppositeDir(dir));
                break;
            case TileType.LCURVE:
                dirList.Add(LeftDir(dir));
                break;
            case TileType.RCURVE:
                dirList.Add(RightDir(dir));
                break;
            case TileType.DCURVE:
                dirList.Add(LeftDir(dir));
                dirList.Add(RightDir(dir));
                break;
            case TileType.LCSTRAIGHT:
                dirList.Add(LeftDir(dir));
                dirList.Add(OppositeDir(dir));
                break;
            case TileType.RCSTRAIGHT:
                dirList.Add(RightDir(dir));
                dirList.Add(OppositeDir(dir));
                break;
            case TileType.NULL:
                break;
        }
        return dirList;
    }

    private static List<Vector2> DirsToPos(List<StartDir> l, Vector2 pos)
    {
        List<Vector2> toPos = new();
        foreach (StartDir dir in l)
        {
            toPos.Add(DirToPos(dir, pos));
        }
        return toPos;
    }

    public static Vector2 DirToPos(StartDir dir, Vector2 pos)
    {
        bool isEvenCol = ((int)pos.x % 2) == 0;
        Vector2 offset = new();
        switch (dir)
        {
            case StartDir.N:
                offset = new Vector2(0, 1);
                break;
            case StartDir.NE:
                offset = isEvenCol ? new Vector2(1, 0) : new Vector2(1, 1);
                break;
            case StartDir.SE:
                offset = isEvenCol ? new Vector2(1, -1) : new Vector2(1, 0);
                break;
            case StartDir.S:
                offset = new Vector2(0, -1);
                break;
            case StartDir.SW:
                offset = isEvenCol ? new Vector2(-1, -1) : new Vector2(-1, 0);
                break;
            case StartDir.NW:
                offset = isEvenCol ? new Vector2(-1, 0) : new Vector2(-1, 1);
                break;
        }
        return pos + offset;
    }

    public static Stack<TileType> GetPossibleTypes(StartDir dir, Vector2 tilePos, bool debug)
    {
        UnityEngine.Vector2 left = DirToPos(LeftDir(dir), tilePos);
        UnityEngine.Vector2 right = DirToPos(RightDir(dir), tilePos);
        UnityEngine.Vector2 opposite = DirToPos(OppositeDir(dir), tilePos);

        if (debug) Debug.Log("opposite:" + opposite);
        if (debug) Debug.Log("left:" + left);
        if (debug) Debug.Log("right:" + right);

        bool canLeft = false, canRight = false, canStraight = false;
        bool shouldLeft = false, shouldRight = false, shouldStraight = false;

        if (HexTileMap.MAP.ContainsKey(left))
        {
            if (HexTileMap.MAP[left].IsTouchingDir(LeftDir(dir)))
            {
                shouldLeft = true; canLeft = true;
            }
        }
        else canLeft = true;

        if (HexTileMap.MAP.ContainsKey(right))
        {
            if (HexTileMap.MAP[right].IsTouchingDir(RightDir(dir)))
            {
                shouldRight = true; canRight = true;
            }
        }
        else canRight = true;

        if (HexTileMap.MAP.ContainsKey(opposite))
        {
            if (HexTileMap.MAP[opposite].IsTouchingDir(OppositeDir(dir)))
            {
                shouldStraight = true; canStraight = true;
            }
        }
        else canStraight = true;

        if (left == Vector2.zero) { canLeft = false; shouldLeft = false; }
        if (right == Vector2.zero) { canRight = false; shouldRight = false; }
        if (opposite == Vector2.zero) { canStraight = false; shouldStraight = false; }

        if (debug)
        {
            Debug.Log("canLeft: " + canLeft);
            Debug.Log("canRight: " + canRight);
            Debug.Log("canStraight: " + canStraight);
            Debug.Log("shouldLeft: " + shouldLeft);
            Debug.Log("shouldRight: " + shouldRight);
            Debug.Log("shouldStraight: " + shouldStraight);
        }

        List<TileType> validTypes = GetTypesFromMoves(canLeft, canRight, canStraight, shouldLeft, shouldRight, shouldStraight);
        Stack<TileType> typeStack = new();

        while (validTypes.Count > 0)
        {
            int index = Random.Range(0, validTypes.Count);
            if (debug) Debug.Log("valid :" + validTypes[index]);
            typeStack.Push(validTypes[index]);
            validTypes.RemoveAt(index);
        }
        return typeStack;
    }

    public static List<TileType> ALLTYPES = new()
    {
        TileType.STRAIGHT,
        TileType.LCURVE,
        TileType.RCURVE,
        TileType.DCURVE,
        TileType.LCSTRAIGHT,
        TileType.RCSTRAIGHT
    };

    private static List<TileType> GetTypesFromMoves(bool canLeft, bool canRight, bool canStraight, bool shouldLeft, bool shouldRight, bool shouldStraight)
    {
        List<TileType> types = new List<TileType>(ALLTYPES);

        if (!canLeft)
        {
            types.Remove(TileType.LCURVE);
            types.Remove(TileType.DCURVE);
            types.Remove(TileType.LCSTRAIGHT);
        }

        if (!canRight)
        {
            types.Remove(TileType.RCURVE);
            types.Remove(TileType.DCURVE);
            types.Remove(TileType.RCSTRAIGHT);
        }

        if (!canStraight)
        {
            types.Remove(TileType.LCSTRAIGHT);
            types.Remove(TileType.RCSTRAIGHT);
            types.Remove(TileType.STRAIGHT);
        }

        if (shouldLeft)
        {
            types.Remove(TileType.STRAIGHT);
            types.Remove(TileType.RCSTRAIGHT);
            types.Remove(TileType.RCURVE);
        }

        if (shouldRight)
        {
            types.Remove(TileType.STRAIGHT);
            types.Remove(TileType.LCSTRAIGHT);
            types.Remove(TileType.LCURVE);
        }

        if (shouldStraight)
        {
            types.Remove(TileType.RCURVE);
            types.Remove(TileType.LCURVE);
            types.Remove(TileType.DCURVE);
        }

        return types;
    }

    private static List<StartDir> AllDirs(StartDir dir)
    {
        return new List<StartDir>
        {
            OppositeDir(dir),
            LeftDir(dir),
            RightDir(dir)
        };
    }

    public static StartDir OppositeDir(StartDir dir)
    {
        return (StartDir)(((int)dir + 3) % 6);
    }

    private static StartDir LeftDir(StartDir dir)
    {
        return (StartDir)(((int)dir + 2) % 6);
    }

    private static StartDir RightDir(StartDir dir)
    {
        return (StartDir)(((int)dir + 4) % 6);
    }
}
