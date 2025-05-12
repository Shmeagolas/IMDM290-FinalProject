using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class TileSpawner
{
    [SerializeField] public static GameObject[] tiles = new GameObject[7];

    private static readonly Dictionary<HexTile.TileType, Queue<GameObject>> pool = new();
    private static readonly List<GameObject> activeTiles = new();


    public static void SpawnMap(HashSet<(Vector2, HexTile.StartDir, HexTile.TileType)> tiles)
    {
        ClearActive();
        foreach(var(pos, dir, type) in tiles)
        {
            GameObject go = GetFromPool(type);
            go.transform.position = TileGrid.GridToWorld(pos);
            go.transform.rotation = Rotation(dir);
            go.SetActive(true);
            activeTiles.Add(go);
        }
    }

    public static void SpawnAll()
    {
        ClearActive();

        foreach (HexTile tile in HexTileMap.MAP.Values)
        {
            GameObject go = GetFromPool(tile.type);
            go.transform.position = TileGrid.GridToWorld(tile.pos);
            go.transform.rotation = Rotation(tile.dir);
            go.SetActive(true);
            activeTiles.Add(go);
        }
    }

    private static GameObject GetFromPool(HexTile.TileType type)
    {
        if (!pool.ContainsKey(type))
            pool[type] = new Queue<GameObject>();

        if (pool[type].Count > 0)
        {
            return pool[type].Dequeue();
        }

      
        GameObject newObj = GameObject.Instantiate(tiles[(int)type]);
        newObj.tag = "Tile"; 
        return newObj;
    }

    public static void ClearActive()
    {
        foreach (var go in activeTiles)
        {
            go.SetActive(false);
            HexTile.TileType type = (HexTile.TileType)System.Array.FindIndex(tiles, t => t.name == go.name.Replace("(Clone)", "").Trim());
            if (!pool.ContainsKey(type))
                pool[type] = new Queue<GameObject>();
            pool[type].Enqueue(go);
        }

        activeTiles.Clear();
    }

    private static Quaternion Rotation(HexTile.StartDir dir)
    {

        switch (dir)
        {
            case HexTile.StartDir.N:
                return Quaternion.Euler(0, 180, 0);
            case HexTile.StartDir.NE:
                return Quaternion.Euler(0, 240, 0);
            case HexTile.StartDir.SE:
                return Quaternion.Euler(0, 300, 0);
            case HexTile.StartDir.S:
                return Quaternion.Euler(0, 0, 0);
            case HexTile.StartDir.SW:
                return Quaternion.Euler(0, 60, 0);
            case HexTile.StartDir.NW:
                return Quaternion.Euler(0, 120, 0);
                
        }
        Debug.Log("Improper dir in TileSpawner Rotation");
        return Quaternion.Euler(0, 0, 0);
    }
}
