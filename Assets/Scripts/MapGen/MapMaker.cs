using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.GameUI.Checkin;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    [SerializeField] int minHexes = 8, maxHexes = 15;
    [SerializeField] private GameObject[] tilePrefabs = new GameObject[7];
    private Stack<Vector2> placedStack = new();
    private Queue<(Vector2, Vector2, HexTile.StartDir)> toQueue = new();
    private bool inProgress = false, closureInProgress = false;
    private int d = 0, resetD = 0;
    [SerializeField] private int maxD = 100;


    void Start()
    {
        TileSpawner.tiles = tilePrefabs;
        TileSpawner.SpawnMap(PremadeMaps.mapOne);
        //MakeMap();
    }

    void Update()
    {
    }

    private void ResetMap()
    {
        Debug.Log("resetting");
        d++;
        inProgress = false;
        toQueue.Clear();
        placedStack.Clear();
        MakeMap();
    }

    private void MakeMap()
    {
        if(inProgress){Debug.LogWarning("can't make map, already in progress"); return;}
        inProgress = true;
        d = 0;
        HexTileMap.MAP.Clear();
        HexTile startTile = new HexTile(Vector2.zero, HexTile.StartDir.S, new Vector2(0, -1), HexTile.TileType.STRAIGHT, new Stack<HexTile.TileType>());
        HexTile startFrom = new HexTile(new Vector2(0, -1), HexTile.StartDir.S, Vector2.zero, HexTile.TileType.NULL, new Stack<HexTile.TileType>());
        HexTileMap.MAP.Add(Vector2.zero, startTile);
        HexTileMap.MAP.Add(new Vector2(0, -1), startFrom);

        placedStack.Push(Vector2.zero);
        QueueNext(startTile);
        ProcessQueueLevel();
        inProgress = false;
    }

    private void QueueNext(HexTile tile)
    {
        if(!inProgress){return;}
        foreach (var (tpos, tdir) in tile.GetToPosDirs())
        {
            if (!HexTileMap.MAP.ContainsKey(tpos))
            {
                toQueue.Enqueue((tpos, tile.pos, HexTile.OppositeDir(tdir)));
            }
        }
    }

    private void Finished()
    {
        if (!inProgress && HexTileMap.MAP.Count > 1){return;}
        if(d > maxD){inProgress = false;Debug.Log($"over max reset depth. d: {d}"); return;}
        inProgress = false;
        if (placedStack.Count < minHexes && HexTileMap.MAP.Count > 1)
        {
            Debug.Log("map was too small, resetting");
            ResetMap();
            return;
        }
        if(!CheckClosure()){Debug.Log("failed close check, resetting"); ResetMap();return;}
        toQueue.Clear();
        TileSpawner.SpawnAll();
    }

    private void UndoPlace(Vector2 from)
    {
        if(!inProgress){return;}
        toQueue.Clear();
        Vector2 tempPos;
        while (placedStack.Count > 1 && placedStack.Peek() != from)
        {
            if(placedStack.Count == 0){return;}
            tempPos = placedStack.Pop();
            if(HexTileMap.MAP.ContainsKey(tempPos)){HexTileMap.MAP.Remove(tempPos);}
            //if (tempPos == from || tempPos == Vector2.zero) break;
            
        }
        if(placedStack.Count == 0 || !HexTileMap.MAP.ContainsKey(from))
        {
            Debug.Log("Undo Place has empty stack or map doesn't contain from");
            ResetMap();
            return;
        }
        var tile = HexTileMap.MAP[from];
        if (tile.SwitchType())
        {
            QueueNext(tile);
            ProcessQueueLevel();
        }
        else
        {
            if(placedStack.Peek() == from)
            {
                placedStack.Pop();
            }
            HexTileMap.MAP.Remove(from);
            //UndoPlace(fromfrom);
            //if(from == Vector2.zero){ResetMap();}
            UndoPlace(tile.fromPos);

        }
    }

    private void TrimToMin()
    {
        if(!inProgress){return;}
        toQueue.Clear();
        //Vector2 tempPos = placedStack.Peek();
        while (placedStack.Count > minHexes || (placedStack.Count > 1 && !HexTileMap.MAP[placedStack.Peek()].OneToTile()))
        {
            if (placedStack.Count <= 1 && placedStack.Peek() == Vector2.zero) break;
            if (placedStack.Count == 0) break;

            Vector2 removedPos = placedStack.Pop();
            HexTileMap.MAP.Remove(removedPos);

            if(placedStack.Count == 0){Debug.Log("popped it all in trim, resetting"); ResetMap(); return;}
        }
        if (placedStack.Count < 3 && placedStack.Count >= 0){Debug.Log("trimmed to small, restting"); ResetMap(); return;}
        QueueNext(HexTileMap.MAP[placedStack.Peek()]);
        ProcessQueueLevel();
    }

    private void ProcessQueueLevel()
    {
        if(!inProgress){return;}
        if (toQueue.Count == 0) { Finished(); return; }
        if (placedStack.Count >= maxHexes) { TrimToMin(); return; }
        int levelNum = toQueue.Count;
        List<(Vector2 pos, Vector2 fpos, HexTile.StartDir dir)> levelTiles = new List<(Vector2, Vector2, HexTile.StartDir)>();
        for(int i = 0; i < levelNum; i++){ levelTiles.Add(toQueue.Dequeue());}
        HashSet<Vector2> levelQueued = new();
        bool recallProcess = false;
        
        foreach(var (pos, fpos, dir) in levelTiles)
        {
            if(!inProgress){return;}
            if (placedStack.Count >= maxHexes) { TrimToMin(); return; }
            if (HexTileMap.MAP.ContainsKey(pos)) { continue; }

            if(levelQueued.Contains(pos))
            {
                UndoPlace(fpos);
                recallProcess = true;
                break;
            }
            levelQueued.Add(pos);
            var types = HexTile.GetPossibleTypes(dir, pos, false);
            if(types.TryPop(out HexTile.TileType type))
            {
                var newTile = new HexTile(pos, dir, fpos, type, types);
                HexTileMap.MAP.Add(pos, newTile);
                placedStack.Push(pos);
                QueueNext(newTile);
            }
            else
            {
                UndoPlace(fpos);
                recallProcess = true;
                break;
            }
        }

        if(!recallProcess && inProgress)
        {
            ProcessQueueLevel();
        }
        
    }


    private bool CheckClosure()
    {
        foreach(var (pos, tile) in HexTileMap.MAP)
        {
            foreach(var (tpos, tdir) in tile.GetToPosDirs())
            {
                if(!HexTileMap.MAP.ContainsKey(tpos))
                {
                    Debug.Log($"to ({tpos}) from {pos} dne resetting");
                    return false;
                }

                if(HexTileMap.MAP[tpos].dir != HexTile.OppositeDir(tdir))
                {
                    Debug.Log($"to ({tpos}) from {pos} not connected resetting");
                    return false;
                }
                
            }
        }
        return true;
    }
}
