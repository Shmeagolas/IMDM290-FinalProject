using UnityEngine;
using UnityEngine.Tilemaps;

public class PremadeMap : MonoBehaviour
{
    public GameObject[] tiles;
    
    void Start()
    {
        TileSpawner.tiles = tiles;
        TileSpawner.SpawnMap(PremadeMaps.mapTwo);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
