using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Awake()
    {
        MinerSpawn minerSpawn = FindObjectOfType<MinerSpawn>();
        if (minerSpawn != null)
        {
            minerSpawn.RegisterMinerSpawnPoint(this.gameObject);
        }
    }
}
