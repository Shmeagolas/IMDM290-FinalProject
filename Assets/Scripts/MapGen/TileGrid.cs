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
    public static Vector2 WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / width);

        // Check if this is an odd column
        bool isOdd = x % 2 != 0;

        // Adjust z position if it's offset
        float adjustedZ = worldPos.z - (isOdd ? height / 2f : 0f);
        int y = Mathf.RoundToInt(adjustedZ / height);

        return new Vector2(x, y);
    }
}
