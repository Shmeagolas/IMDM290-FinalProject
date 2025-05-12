using Unity.Collections;
using UnityEngine;

public static class TileGrid
{
    public static float width = 1.8f;
    public static float height = 2f;
    
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
