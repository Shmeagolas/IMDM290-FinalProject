using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// this class should keep track of all the spawn points in the game and randomly spread the miners throughout them, 
// spawning a miner in the location of each spawn point. 

// this class should also keep track of all the mushroom spawn points and randomly put mushrooms throughout them,
public class MinerSpawn : MonoBehaviour
{
    public InputActionProperty rightTriggerAction;
    // miner vars
    public int numMiners; // set number of miners
    public List<GameObject> minerSpawnPoints; // should add in unity

    private List<GameObject> miners = new List<GameObject>(); 
    private List<bool> minerSavedStatus = new List<bool>();

    // mushroom vars

    public int numShrooms; // set number of miners
    public List<GameObject> shroomSpawnPoints; // should add in unity

    private List<GameObject> mushrooms = new List<GameObject>(); 
    private List<bool> shoomGlowStatus = new List<bool>();

    private HashSet<IBubbleTarget> targetsInBubble = new HashSet<IBubbleTarget>();

    //randomly spawn the miners throughout the spawnpoints
    void Start()
    {
        SpawnMiners();
        SpawnMushrooms();
    }

    void SpawnMiners() 
    {
        List<GameObject> availableMinerSpawns = new List<GameObject>(minerSpawnPoints);

        for (int i = 0; i < numMiners && availableMinerSpawns.Count > 0; i++)
        {
            // gets a random spawn point
            int index = Random.Range(0, availableMinerSpawns.Count);
            Transform spawnPoint = availableMinerSpawns[index].transform;

            // create miner and set it's position to that spawn point
            GameObject miner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 offset = new Vector3(0, 0.5f, 0);
            miner.transform.position = spawnPoint.position + offset;
            miner.transform.rotation = spawnPoint.rotation;

            miner minerComponent = miner.AddComponent<miner>();
            minerComponent.index = i;

            Renderer sphereRenderer = miner.GetComponent<Renderer>();
            Color color = Color.HSVToRGB(30f / 360f, 0.3f, 0.85f); // Full saturation and brightness
            sphereRenderer.material.color = color;

            // initalize the miner to true so it can be counted
            miners.Add(miner);
            minerSavedStatus.Add(false);

            availableMinerSpawns.RemoveAt(index);
        }
    }

    //when a miner is saved, set it's index to true and make it disappear from the map
    public void SaveMiner(int index)
    {
        if (index >= 0 && index < miners.Count && !minerSavedStatus[index])
        {
            minerSavedStatus[index] = true;
            // Make the miner disappear 
            miners[index].SetActive(false);
        }
    }

    //count all the true in the miners saved list of bools. If everything is true: Win
    // otherwise return x/total miners saved 
    void ReturnFinalCount()
    {
        int saved = 0;
        for (int i = 0; i < minerSavedStatus.Count; i++)
        {
            if (minerSavedStatus[i] == true) {
                saved++;
            }
        }

        if (saved == minerSavedStatus.Count) {
            Debug.Log("You Win! You saved all the miners");
        } else if (saved == 0) {
            Debug.Log("You may have escaped, but you saved no miners... everyone died");
        } else {
            Debug.Log("So Close! You saved " + saved + " miners out of " + minerSavedStatus.Count + " miners");
        }
    }

    // if a miner is in a certain range of the player
    private void OnTriggerEnter(Collider other)
    {
        IBubbleTarget target = other.GetComponent<IBubbleTarget>();
        if (target != null && miners.Contains(other.gameObject))
        {
            targetsInBubble.Add(target);
        }
    }

    private void OnEnable()
    {
        rightTriggerAction.action.Enable();
    }

    public void TriggerBubbleEffect()
    {
        float triggerValue = rightTriggerAction.action.ReadValue<float>();
        //Debug.Log("Trigger value: " + triggerValue);

        // if the trigger is pressed, save the miners in the bubble around the player
        if (triggerValue == 1)
        {
            foreach (var target in targetsInBubble)
            {
                GameObject obj = ((MonoBehaviour)target).gameObject;
                miner minerComponent = obj.GetComponent<miner>();
                if (minerComponent != null)
                {
                    SaveMiner(minerComponent.index);
                }
            }
        }
    }

    void SpawnMushrooms() 
    {
        List<GameObject> availableShroomSpawns = new List<GameObject>(shroomSpawnPoints);

        for (int i = 0; i < numShrooms && availableShroomSpawns.Count > 0; i++)
        {
            // gets a random spawn point
            int index = Random.Range(0, availableShroomSpawns.Count);
            Transform spawnPoint = availableShroomSpawns[index].transform;

            // create miner and set it's position to that spawn point
            GameObject shroom = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Vector3 offset = new Vector3(0, 0.5f, 0);
            shroom.transform.position = spawnPoint.position + offset;
            shroom.transform.rotation = spawnPoint.rotation;
            shroom.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // initalize the miner to true so it can be counted
            mushrooms.Add(shroom);
            shoomGlowStatus.Add(false);

            availableShroomSpawns.RemoveAt(index);
        }
    }

    public void glow(int index)
    {
        if (index >= 0 && index < mushrooms.Count && shoomGlowStatus[index])
        {
            shoomGlowStatus[index] = true;
            // Make the miner disappear 
            Renderer sphereRenderer = mushrooms[index].GetComponent<Renderer>();
            Color color = Color.HSVToRGB(180f / 360f, 1f, 1f); // neon color
            sphereRenderer.material.color = color;
            
        }
    }
}
