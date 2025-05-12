using Unity.Collections;
using UnityEngine;

public static class TileGrid
{
    private static float scalar = .75f;
    public static float width = 10f * scalar;
    public static float height = (10f / 0.8660254f) * scalar;
    
    public static Vector3 GridToWorld(Vector2 pos)
    {
        int x = (int) pos.x;
        int y = (int) pos.y;
        if(x % 2 == 0)
        {
            return new Vector3(x * width, 0f, y * height);
        }
        else
        {
            return new Vector3(x * width , 0f, y * height + (height / 2f));
        }
    }
}
