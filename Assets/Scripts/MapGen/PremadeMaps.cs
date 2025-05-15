 using UnityEngine;
using System.Collections.Generic;

public static class PremadeMaps 
{
    static public HashSet<(Vector2, HexTile.StartDir, HexTile.TileType)> mapOne = new(){
        (Vector2.zero, HexTile.StartDir.S, HexTile.TileType.STRAIGHT),
        (new Vector2(0,1), HexTile.StartDir.S, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(-1,1), HexTile.StartDir.SE, HexTile.TileType.STRAIGHT),
        (new Vector2(-2,2), HexTile.StartDir.SE, HexTile.TileType.STRAIGHT),
        (new Vector2(-3,2), HexTile.StartDir.SE, HexTile.TileType.RCURVE),
        (new Vector2(-3,3), HexTile.StartDir.S, HexTile.TileType.RCURVE),    
        (new Vector2(-2,4), HexTile.StartDir.SW, HexTile.TileType.RCURVE),   
        (new Vector2(-1,3), HexTile.StartDir.S, HexTile.TileType.DCURVE), 
        (new Vector2(0,2), HexTile.StartDir.S, HexTile.TileType.DCURVE),
        (new Vector2(-1,2), HexTile.StartDir.SE, HexTile.TileType.RCURVE),
        (new Vector2(0,4), HexTile.StartDir.SW, HexTile.TileType.RCURVE),
        (new Vector2(1,2), HexTile.StartDir.SW, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(1,3), HexTile.StartDir.S, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(1,4), HexTile.StartDir.S, HexTile.TileType.RCURVE),
        (new Vector2(2,5), HexTile.StartDir.SW, HexTile.TileType.RCURVE),
        (new Vector2(3,4), HexTile.StartDir.NW, HexTile.TileType.RCURVE),
        (new Vector2(3,3), HexTile.StartDir.N, HexTile.TileType.RCURVE),
        (new Vector2(2,3), HexTile.StartDir.NE, HexTile.TileType.STRAIGHT),
        };

        static public HashSet<(Vector2, HexTile.StartDir, HexTile.TileType)> mapTwo = new(){
        (Vector2.zero, HexTile.StartDir.S, HexTile.TileType.STRAIGHT),
        (new Vector2(0,1), HexTile.StartDir.N, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(0,2), HexTile.StartDir.S, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(-1,2), HexTile.StartDir.SE, HexTile.TileType.STRAIGHT),
        (new Vector2(-2,3), HexTile.StartDir.SE, HexTile.TileType.STRAIGHT),
        (new Vector2(-3,3), HexTile.StartDir.SE, HexTile.TileType.RCURVE),
        (new Vector2(-3,4), HexTile.StartDir.S, HexTile.TileType.RCURVE),    
        (new Vector2(-2,5), HexTile.StartDir.SW, HexTile.TileType.RCURVE),   
        (new Vector2(-1,4), HexTile.StartDir.S, HexTile.TileType.DCURVE), 
        (new Vector2(0,3), HexTile.StartDir.S, HexTile.TileType.DCURVE),
        (new Vector2(-1,3), HexTile.StartDir.SE, HexTile.TileType.RCURVE),
        (new Vector2(0,5), HexTile.StartDir.SW, HexTile.TileType.RCURVE),
        (new Vector2(1,3), HexTile.StartDir.SW, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(1,4), HexTile.StartDir.S, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(1,5), HexTile.StartDir.S, HexTile.TileType.RCURVE),
        (new Vector2(2,6), HexTile.StartDir.SW, HexTile.TileType.RCURVE),
        (new Vector2(3,5), HexTile.StartDir.NW, HexTile.TileType.RCURVE),
        (new Vector2(3,4), HexTile.StartDir.N, HexTile.TileType.RCURVE),
        (new Vector2(2,4), HexTile.StartDir.NE, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(1,0), HexTile.StartDir.NW, HexTile.TileType.LCSTRAIGHT),
        (new Vector2(2,0), HexTile.StartDir.NW, HexTile.TileType.LCURVE),
        (new Vector2(2,1), HexTile.StartDir.SW, HexTile.TileType.LCURVE),
        (new Vector2(3,0), HexTile.StartDir.SW, HexTile.TileType.LCURVE),
        (new Vector2(3,1), HexTile.StartDir.S, HexTile.TileType.STRAIGHT),
        (new Vector2(3,2), HexTile.StartDir.S, HexTile.TileType.LCURVE),
        (new Vector2(2,2), HexTile.StartDir.S, HexTile.TileType.STRAIGHT),
        (new Vector2(2,2), HexTile.StartDir.S, HexTile.TileType.STRAIGHT),
        (new Vector2(2,3), HexTile.StartDir.N, HexTile.TileType.LCSTRAIGHT),

        };

}
