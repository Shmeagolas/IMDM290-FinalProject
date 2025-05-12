using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assets.Scripts.MapGen;

public class TestTiles
{
    [SetUp]
    public void Setup()
    {
        HexTileMap.MAP = new Dictionary<Vector2, HexTile>();
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
        Assert.AreEqual(HexTile.StartDir.NW, typeof(HexTile)
            .GetMethod("LeftDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { HexTile.StartDir.N }));
    }

    [Test]
    public void RightDir_ReturnsCorrectValue()
    {
        Assert.AreEqual(HexTile.StartDir.NE, typeof(HexTile)
            .GetMethod("RightDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { HexTile.StartDir.N }));
    }

    [Test]
    public void GetToDirs_ReturnsCorrectDirections()
    {
        var dirs = HexTile.GetToDirs(HexTile.TileType.LCSTRAIGHT, HexTile.StartDir.N);
        CollectionAssert.AreEquivalent(new[] { HexTile.StartDir.NW, HexTile.StartDir.S }, dirs);
    }

    [Test]
    public void SwitchType_PopsStackCorrectly()
    {
        var stack = new Stack<HexTile.TileType>();
        stack.Push(HexTile.TileType.STRAIGHT);
        var tile = new HexTile(Vector2.zero, HexTile.StartDir.N, Vector2.down, HexTile.TileType.RCURVE, stack);

        bool result = tile.SwitchType();

        Assert.IsTrue(result);
        Assert.AreEqual(HexTile.TileType.STRAIGHT, tile.type);
    }

    [Test]
    public void SwitchType_ReturnsFalseWhenStackEmpty()
    {
        var tile = new HexTile(Vector2.zero, HexTile.StartDir.N, Vector2.down, HexTile.TileType.RCURVE, new Stack<HexTile.TileType>());
        bool result = tile.SwitchType();

        Assert.IsFalse(result);
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
        var tile = new HexTile(Vector2.zero, HexTile.StartDir.N, Vector2.down, HexTile.TileType.STRAIGHT, new Stack<HexTile.TileType>());
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
        var stack = HexTile.GetPossibleTypes(HexTile.StartDir.N, pos);
        Assert.IsTrue(stack.Count > 0);
    }
}