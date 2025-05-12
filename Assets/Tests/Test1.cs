using System.Collections;
using System.Collections.Generic;
using System.Linq;

//using System.Numerics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Test1
{
    [SetUp]
    public void Setup()
    {
        HexTileMap.MAP = new System.Collections.Generic.Dictionary<Vector2, HexTile>();
    }

    [Test]
    public void OppositeDir_ReturnsCorrectValue()
    {
        Assert.AreEqual(HexTile.StartDir.S, HexTile.OppositeDir(HexTile.StartDir.N));
        Assert.AreEqual(HexTile.StartDir.NW, HexTile.OppositeDir(HexTile.StartDir.SE));
    }

    [Test]
    public void LeftDir_ReturnsCorrectValue()
    {
        Assert.AreEqual(HexTile.StartDir.SE, typeof(HexTile)
            .GetMethod("LeftDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { HexTile.StartDir.N }));
    }

    [Test]
    public void RightDir_ReturnsCorrectValue()
    {
        Assert.AreEqual(HexTile.StartDir.SW, typeof(HexTile)
            .GetMethod("RightDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { HexTile.StartDir.N }));
    }

    [Test]
    public void GetToDirs_ReturnsCorrectDirections()
    {
        var dirs = HexTile.GetToDirs(HexTile.TileType.LCSTRAIGHT, HexTile.StartDir.S);
        CollectionAssert.AreEquivalent(new[] { HexTile.StartDir.N, HexTile.StartDir.NW }, dirs);
    }


    [Test]
    public void DirToPos_ReturnsCorrectOffset()
    {
        Vector2 pos = new Vector2(2, 2); // even row
        Vector2 expected = new Vector2(2, 3); // N
        Assert.AreEqual(expected, HexTile.DirToPos(HexTile.StartDir.N, pos));
    }

    [Test]
    public void IsTouchingDir_ReturnsTrueIfMatchingDirection()
    {
        //var ftile = new HexTile(Vector2.down, HexTile.StartDir.S, Vector2.down, HexTile.TileType.STRAIGHT, new Stack<HexTile.TileType>());
        var tile = new HexTile(Vector2.zero, HexTile.StartDir.S, Vector2.down, HexTile.TileType.STRAIGHT, new Stack<HexTile.TileType>());
        Assert.IsTrue(tile.IsTouchingDir(HexTile.StartDir.S));
    }

    [Test]
    public void IsTouchingDir_ReturnsFalseIfNotMatching()
    {
        var tile = new HexTile(Vector2.zero, HexTile.StartDir.N, Vector2.down, HexTile.TileType.LCURVE, new Stack<HexTile.TileType>());
        Assert.IsFalse(tile.IsTouchingDir(HexTile.StartDir.S));
    }


    [Test]
    public void GetPossibleTypes_ReturnsNonEmptyStack()
    {
        var pos = new Vector2(2, 2);
        HexTileMap.MAP = new Dictionary<Vector2, HexTile>(); // mock empty map
        var stack = HexTile.GetPossibleTypes(HexTile.StartDir.N, pos, true);
        Assert.IsTrue(stack.Count > 0);
    } 


    [Test]
    public void GetPossibleTypes_1()
    {
        var pos1 = new Vector2(0, 0);
        var pos2 = new Vector2(0, 1);
        var pos3 = new Vector2(0, 2);
        HexTileMap.MAP = new Dictionary<Vector2, HexTile>();
        HexTileMap.MAP.Add(pos1 ,new HexTile(pos1, HexTile.StartDir.N, pos1, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HexTileMap.MAP.Add(pos3 ,new HexTile(pos3, HexTile.StartDir.N, pos2, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HashSet<HexTile.TileType> set =  new (HexTile.GetPossibleTypes(HexTile.StartDir.S, pos2, true));
        HashSet<HexTile.TileType> expected = new();
        expected.Add(HexTile.TileType.STRAIGHT); 
        expected.Add(HexTile.TileType.LCSTRAIGHT);
        expected.Add(HexTile.TileType.RCSTRAIGHT);
        foreach(var i in expected)
        {
            Assert.IsTrue(set.Remove(i));
        }
   
        Assert.IsTrue(set.Count == 0);
    }

    [Test]
    public void GetPossibleTypes_1A()
    {
        var pos1 = new Vector2(0, 0);
        var pos2 = new Vector2(0, 1);
        var pos3 = new Vector2(0, 2);
        HexTileMap.MAP = new Dictionary<Vector2, HexTile>();
        HexTileMap.MAP.Add(pos1 ,new HexTile(pos1, HexTile.StartDir.S, pos1, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HexTileMap.MAP.Add(pos3 ,new HexTile(pos3, HexTile.StartDir.N, pos2, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HashSet<HexTile.TileType> set =  new (HexTile.GetPossibleTypes(HexTile.StartDir.S, pos2, true));
        HashSet<HexTile.TileType> expected = new();
        expected.Add(HexTile.TileType.STRAIGHT); 
        expected.Add(HexTile.TileType.LCSTRAIGHT);
        expected.Add(HexTile.TileType.RCSTRAIGHT);
        foreach(var i in expected)
        {
            Assert.IsTrue(set.Remove(i));
        }
   
        Assert.IsTrue(set.Count == 0);
    }
    [Test]
    public void GetPossibleTypes_1B()
    {
        var pos1 = new Vector2(0, 0);
        var pos2 = new Vector2(1, 0);
        var pos3 = new Vector2(2, 0);
        HexTileMap.MAP = new Dictionary<Vector2, HexTile>();
        HexTileMap.MAP.Add(pos1 ,new HexTile(pos1, HexTile.StartDir.SW, pos1, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HexTileMap.MAP.Add(pos3 ,new HexTile(pos3, HexTile.StartDir.SE, pos2, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HashSet<HexTile.TileType> set =  new (HexTile.GetPossibleTypes(HexTile.StartDir.SW, pos2, true));
        HashSet<HexTile.TileType> expected = new(); 
        expected.Add(HexTile.TileType.RCSTRAIGHT);
        expected.Add(HexTile.TileType.RCURVE);
        expected.Add(HexTile.TileType.DCURVE);
        foreach(var i in expected)
        {
            Assert.IsTrue(set.Remove(i));
        }
   
        Assert.IsTrue(set.Count == 0);
    }

    [Test]
    public void GetPossibleTypes_1C()
    {
        var pos1 = new Vector2(0, 1);
        var pos2 = new Vector2(1, 0);
        var pos3 = new Vector2(2, 1);
        var pos4 = new Vector2(2,0);
        HexTileMap.MAP = new Dictionary<Vector2, HexTile>();
        HexTileMap.MAP.Add(pos1 ,new HexTile(pos1, HexTile.StartDir.NW, pos1, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HexTileMap.MAP.Add(pos3 ,new HexTile(pos3, HexTile.StartDir.NE, pos2, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HexTileMap.MAP.Add(pos4 ,new HexTile(pos4, HexTile.StartDir.SE, pos2, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HashSet<HexTile.TileType> set =  new (HexTile.GetPossibleTypes(HexTile.StartDir.NW, pos2, true));
        HashSet<HexTile.TileType> expected = new(); 
        expected.Add(HexTile.TileType.LCSTRAIGHT);

        foreach(var i in expected)
        {
            Assert.IsTrue(set.Remove(i));
        }
   
        Assert.IsTrue(set.Count == 0);
    }

    [Test]
    public void GetPossibleTypes_1D()
    {
        var pos1 = new Vector2(-2, -1);
        var pos2 = new Vector2(-1, -2);
        var pos3 = new Vector2(0, -1);
        var pos4 = new Vector2(0,-2);
        HexTileMap.MAP = new Dictionary<Vector2, HexTile>();
        HexTileMap.MAP.Add(pos1 ,new HexTile(pos1, HexTile.StartDir.NW, pos1, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HexTileMap.MAP.Add(pos3 ,new HexTile(pos3, HexTile.StartDir.NE, pos2, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HexTileMap.MAP.Add(pos4 ,new HexTile(pos4, HexTile.StartDir.SE, pos2, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HashSet<HexTile.TileType> set =  new (HexTile.GetPossibleTypes(HexTile.StartDir.NW, pos2, true));
        HashSet<HexTile.TileType> expected = new(); 
        expected.Add(HexTile.TileType.LCSTRAIGHT);

        foreach(var i in expected)
        {
            Assert.IsTrue(set.Remove(i));
        }
   
        Assert.IsTrue(set.Count == 0);
    }

    [Test]
    public void GetPossibleTypes_1E()
    {
        var pos1 = new Vector2(0, 0);
        var pos2 = new Vector2(1, -1);
        HexTileMap.MAP = new Dictionary<Vector2, HexTile>();
        HexTileMap.MAP.Add(pos1 ,new HexTile(pos1, HexTile.StartDir.S, pos1, HexTile.TileType.STRAIGHT , new Stack<HexTile.TileType>()));
        HashSet<HexTile.TileType> set =  new (HexTile.GetPossibleTypes(HexTile.StartDir.NE, pos2, true));
        HashSet<HexTile.TileType> expected = new(); 
        expected.Add(HexTile.TileType.LCURVE);
        expected.Add(HexTile.TileType.LCSTRAIGHT);
        expected.Add(HexTile.TileType.STRAIGHT);

        foreach(var i in expected)
        {
            Assert.IsTrue(set.Remove(i));
        }
   
        Assert.IsTrue(set.Count == 0);
    }
}