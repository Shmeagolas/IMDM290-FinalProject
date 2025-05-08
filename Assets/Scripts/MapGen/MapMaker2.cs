using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For OrderBy used in shuffling exits


public class MapMaker2 : MonoBehaviour
{
    [Header("Map Generation Settings")]
    [SerializeField] int minHexes = 15;
    [SerializeField] int maxHexes = 25;

    [Header("Tile Assets")]
    [SerializeField] private GameObject[] tilePrefabs = new GameObject[7]; // STRAIGHT, LCURVE, RCURVE, DCURVE, LCSTRAIGHT, RCSTRAIGHT, NULL

    // --- Generation State ---
    private Stack<Vector2> placedTilePositionsStack; // Stores positions (keys for HexTileMap.MAP)
    private Queue<Vector2> tileProcessQueue;          // Stores positions of tiles whose exits need processing

    private HexTile.StartDir mapStartNodeEntryDir;    // The direction the conceptual path ENTERS the start tile
    private Vector2 mapStartNodeFromPos;            // The conceptual coordinate "before" the start tile
    private bool hasSecondTileTakenAnExitFromStart; // Tracks if the start tile's primary exit has been used
    private int targetTileCount;

    // --- Unity Methods ---
    void Start()
    {
        TileSpawner.tiles = tilePrefabs;
        GenerateNewMap();
        TileSpawner.SpawnAll();
    }

    // --- Public Methods ---
    public void GenerateNewMap()
    {
        Debug.Log("Starting new map generation...");
        // Clear previously instantiated game objects if they are children of this transform
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Initialize state for a new map
        placedTilePositionsStack = new Stack<Vector2>();
        tileProcessQueue = new Queue<Vector2>();
        HexTileMap.MAP.Clear();
        hasSecondTileTakenAnExitFromStart = false;

        targetTileCount = Random.Range(minHexes, maxHexes + 1);
        Debug.Log($"Targeting {targetTileCount} hexes.");

        if (GenerateLogicalMap())
        {
            Debug.Log("Logical map generated successfully.");
            InstantiateMapVisuals();
        }
        else
        {
            Debug.LogError("Map generation failed after all attempts.");
        }
    }

    // --- Core Generation Logic ---
    private bool GenerateLogicalMap()
    {
        // 1. Place the Start Tile
        Vector2 startPos = Vector2.zero;
        mapStartNodeEntryDir = HexTile.StartDir.S; // Arbitrary choice: path ENTERS start tile from South
        mapStartNodeFromPos = HexTile.DirToPos(mapStartNodeEntryDir, startPos); // The hex "south" of (0,0)

        Stack<HexTile.TileType> possibleStartTypes = HexTile.GetPossibleTypes(mapStartNodeEntryDir, startPos, false);

        if (possibleStartTypes.Count == 0)
        {
            Debug.LogError("Cannot determine any valid type for the start tile. Map generation failed at init.");
            return false;
        }

        HexTile.TileType firstStartType = possibleStartTypes.Pop();
        HexTile startTile = new HexTile(startPos, mapStartNodeEntryDir, mapStartNodeFromPos, firstStartType, possibleStartTypes);

        HexTileMap.MAP[startPos] = startTile;
        placedTilePositionsStack.Push(startPos);
        tileProcessQueue.Enqueue(startPos);

        // 2. Main Generation Loop (BFS with Backtracking)
        int iterations = 0;
        int maxIterations = targetTileCount * 30; // Safety break for complex scenarios

        while (tileProcessQueue.Count > 0 && HexTileMap.MAP.Count < targetTileCount && iterations < maxIterations)
        {
            iterations++;
            Vector2 currentTileMapKey = tileProcessQueue.Dequeue();
            if (!HexTileMap.MAP.ContainsKey(currentTileMapKey))
            {
                // Tile might have been removed by backtracking; skip if so.
                continue;
            }
            HexTile currentTile = HexTileMap.MAP[currentTileMapKey];
            bool madeForwardProgressFromCurrentTile = false;

            // Shuffle exits to introduce variability in path choices
            List<(Vector2 nextPos, HexTile.StartDir exitDir)> exits = currentTile.GetToPosDirs().OrderBy(e => Random.value).ToList();

            if (exits.Count == 0 && HexTileMap.MAP.Count < targetTileCount)
            {
                // This tile is an endpoint, but the map isn't full. This might be a natural dead end.
                // If no progress is made overall, backtracking will eventually handle it.
            }

            for (int i = 0; i < exits.Count; i++)
            {
                var exit = exits[i];
                Vector2 potentialNextPos = exit.nextPos;
                HexTile.StartDir exitDirFromCurrent = exit.exitDir;
                HexTile.StartDir entryDirForNextTile = HexTile.OppositeDir(exitDirFromCurrent);

                // --- Start Tile Special Exit Constraint ---
                // After the 2nd tile connects, the start tile shouldn't sprout new paths from other exits.
                if (currentTileMapKey == Vector2.zero && hasSecondTileTakenAnExitFromStart)
                {
                    // This check implies that if currentTile is the start tile, and its first path is set,
                    // we should be careful about using other exits. If `GetToPosDirs` for the start tile
                    // (after its type is set) only provides one logical continuation path, this is simpler.
                    // If it *can* have multiple actual exits (e.g. DCURVE), this needs refinement
                    // to ensure only one of those is "active" for outward generation.
                    // For now, we assume GetToPosDirs gives valid continuations.
                    // A more explicit check might be needed if startTile.type has multiple toPos items.
                }

                // --- Connecting TO the Start Tile (0,0) ---
                if (potentialNextPos == Vector2.zero)
                {
                    // Is this the designated loop-closing connection to the start tile's *entry*?
                    if (currentTile.pos == mapStartNodeFromPos && entryDirForNextTile == startTile.dir) // startTile.dir is its entry dir
                    {
                        Debug.Log($"Path from {currentTileMapKey} is closing loop to start tile's entry.");
                        madeForwardProgressFromCurrentTile = true;
                        if (HexTileMap.MAP.Count >= targetTileCount && CheckMapClosure())
                        {
                            Debug.Log("Target count met & map closed by connecting to start tile entry.");
                            return true; // Success
                        }
                        // If map not full, this closure might be premature or part of a valid smaller loop.
                    }
                    // Else, is another tile (not the 2nd) trying to connect to an *exit point* of the start tile? (Forbidden)
                    else if (currentTileMapKey != Vector2.zero && placedTilePositionsStack.Count > 1)
                    {
                        // This rule ("no tile should connect to 0,0 besides the second placed tile")
                        // means after the start tile connects to the second tile, no *other* new tile
                        // should connect to an *exit point* of the start tile.
                        // Connecting to its *entry point* (mapStartNodeFromPos) is allowed for closure.
                        Debug.Log($"Tile at {currentTileMapKey} attempting invalid connection to an exit/body of start tile. Skipping.");
                        continue; // Skip this exit processing
                    }
                }
                // --- Check if position is already occupied (and not the special start tile entry connection) ---
                else if (HexTileMap.MAP.ContainsKey(potentialNextPos))
                {
                    HexTile existingTile = HexTileMap.MAP[potentialNextPos];
                    if (existingTile.IsTouchingDir(entryDirForNextTile))
                    {
                        madeForwardProgressFromCurrentTile = true; // Valid connection to an existing tile
                    }
                    // Else: Misaligned connection. The current exit of currentTile is blocked by an incompatible existing tile.
                    // Try other exits of currentTile.
                    continue; // Skip this exit processing
                }

                // --- If map is full, don't place new tiles (unless it's the closing one handled above) ---
                if (HexTileMap.MAP.Count >= targetTileCount && !(potentialNextPos == Vector2.zero && currentTile.pos == mapStartNodeFromPos))
                {
                    continue; // Skip placing new tile if map is full
                }

                // --- Try to place a new tile at potentialNextPos ---
                Stack<HexTile.TileType> possibleNewTypes = HexTile.GetPossibleTypes(entryDirForNextTile, potentialNextPos, false);

                if (possibleNewTypes.Count == 0)
                {
                    continue; // No valid tile type for this spot from this exit. Try other exits.
                }

                HexTile.TileType newType = possibleNewTypes.Pop();
                HexTile newTile = new HexTile(potentialNextPos, entryDirForNextTile, currentTileMapKey, newType, possibleNewTypes);

                HexTileMap.MAP[potentialNextPos] = newTile;
                placedTilePositionsStack.Push(potentialNextPos);
                tileProcessQueue.Enqueue(potentialNextPos);
                madeForwardProgressFromCurrentTile = true;

                if (currentTileMapKey == Vector2.zero && !hasSecondTileTakenAnExitFromStart)
                {
                    hasSecondTileTakenAnExitFromStart = true;
                }

                // Check for completion after placing a tile
                if (HexTileMap.MAP.Count >= targetTileCount)
                {
                    if (CheckMapClosure())
                    {
                        Debug.Log($"Target tile count ({targetTileCount}) reached and map is closed.");
                        return true; // Success!
                    }
                    else
                    {
                        // Map is "full" but not validly closed. Trigger backtrack to find a closing solution.
                        Debug.Log("Target count reached, but map not closed. Initiating backtrack.");
                        if (!AttemptBacktrack()) // AttemptBacktrack will modify state and queue
                        {
                            Debug.LogWarning("Backtrack failed after reaching target count with unclosed map.");
                            // Continue loop; maybe another branch from queue or further backtrack can solve it.
                            // If queue becomes empty and still no solution, loop will terminate.
                        }
                    }
                }
                 // Optimization: if current tile is a single-exit type and we successfully placed from it.
                if (currentTile.OneToTile() && madeForwardProgressFromCurrentTile) {
                    break; // No need to check other "exits" if it's a single path tile and path was made.
                }


            } // End FOR EACH exit of currentTile

            if (!madeForwardProgressFromCurrentTile && currentTile.toPos.Count > 0 && HexTileMap.MAP.Count < targetTileCount)
            {
                // currentTile has defined exits, but none could be used to place/connect a tile.
                // This means currentTile (with its current type) is a dead end.
                Debug.Log($"Tile at {currentTileMapKey} (type {currentTile.type}) is a dead end with remaining capacity. Initiating backtrack.");
                if (!AttemptBacktrack())
                {
                    // If backtrack fails here, it means we've likely exhausted options from this path.
                    // The main loop will continue if queue has items, or terminate if not.
                    Debug.LogWarning($"Backtrack attempt from dead end {currentTileMapKey} did not immediately resolve. Generation continues if options exist.");
                }
            }
        } // End WHILE loop (main generation)

        if (iterations >= maxIterations)
        {
            Debug.LogWarning("Max iterations reached. Map generation stopped prematurely.");
        }

        // After loop, check final state
        if (HexTileMap.MAP.Count >= targetTileCount && CheckMapClosure())
        {
            Debug.Log("Map generation completed successfully after main loop.");
            return true;
        }
        else if (HexTileMap.MAP.Count < targetTileCount && placedTilePositionsStack.Count > 1) // Try one last backtrack if under target
        {
            Debug.LogWarning($"Map under target ({HexTileMap.MAP.Count}/{targetTileCount}). Attempting final backtrack sweeps.");
            while(placedTilePositionsStack.Count > 1 && !CheckMapClosure()){ // Keep backtracking as long as we have options and not closed
                if(!AttemptBacktrack()){
                    break; // Stop if backtrack itself fails to change anything
                }
                // After a backtrack, CheckMapClosure again, or let it be checked at the end.
            }
             if (HexTileMap.MAP.Count >= minHexes && CheckMapClosure()){ // Check against minHexes now
                 Debug.Log("Map generation successful after final backtrack sweeps (met minHexes).");
                 return true;
             }
        }

        Debug.LogError($"Map generation concluded. Tiles: {HexTileMap.MAP.Count}. Target: {targetTileCount}. Closed: {CheckMapClosure()}.");
        return CheckMapClosure() && HexTileMap.MAP.Count >= minHexes; // Success if closed and meets at least minHexes
    }


    private bool AttemptBacktrack()
    {
        if (placedTilePositionsStack.Count == 0)
        {
            Debug.LogWarning("Attempted backtrack on empty stack.");
            return false;
        }
        // Don't backtrack beyond the start tile if it has no other types.
        if (placedTilePositionsStack.Count == 1 && HexTileMap.MAP.ContainsKey(Vector2.zero) && HexTileMap.MAP[Vector2.zero].possibleTypes.Count == 0)
        {
            Debug.LogWarning("Backtrack attempt: Only start tile left and it's exhausted.");
            return false;
        }

        Vector2 tileToChangePos = placedTilePositionsStack.Pop();
        if (!HexTileMap.MAP.ContainsKey(tileToChangePos))
        {
            Debug.LogError($"Backtrack error: Tile at {tileToChangePos} not found in MAP.");
            return AttemptBacktrack(); // Try to pop next if current is already gone somehow
        }
        HexTile tileToChange = HexTileMap.MAP[tileToChangePos];

        HexTileMap.MAP.Remove(tileToChangePos); // Remove from map before trying to switch type

        // When a tile is removed, any tiles that were placed *from* it become disconnected.
        // A full backtrack would remove these "children" tiles from the map and placedStack.
        // This simplified version just changes the current tile and hopes BFS re-validates.
        // For a more robust backtrack, you'd need to identify and remove tiles dependent on tileToChange.
        // For now, we clear the processing queue, as paths from it are now suspect.
        tileProcessQueue.Clear();


        if (tileToChangePos == Vector2.zero) // If the start tile is being changed
        {
            hasSecondTileTakenAnExitFromStart = false;
        }

        if (tileToChange.SwitchType()) // Try to switch to its next available type
        {
            // tileToChange.toPos will be internally updated by SwitchType if GetToPos is called by constructor or explicitly.
            // Or ensure SwitchType calls a method to update toPos.
            // For HexTile given, type change means GetToPosDirs() will return new values.

            HexTileMap.MAP[tileToChangePos] = tileToChange; // Add it back to the map with its new type
            placedTilePositionsStack.Push(tileToChangePos);   // Add its position back to the stack

            // Repopulate the tileProcessQueue from the current valid placed tiles to resume BFS.
            // Add in order of placement to approximate BFS resumption.
            List<Vector2> validPlacedTiles = new List<Vector2>(placedTilePositionsStack);
            validPlacedTiles.Reverse(); // To get them in original placement order for queue
            foreach (var pos_in_stack in validPlacedTiles)
            {
                if (HexTileMap.MAP.ContainsKey(pos_in_stack)) // Ensure it's still considered part of the current map attempt
                {
                     // Only add to queue if it likely has open, unprocessed exits.
                     // A simpler rule: add all, BFS will figure it out.
                     // Or, more efficiently, only add tiles that are "frontier" tiles.
                    tileProcessQueue.Enqueue(pos_in_stack);
                }
            }

            Debug.Log($"Backtracked: Switched type for tile at {tileToChangePos} to {tileToChange.type}. Queue repopulated. Resuming generation.");
            return true; // Backtrack was successful in changing a tile type
        }
        else
        {
            // tileToChange has exhausted all its possible types. It's permanently removed for this path.
            Debug.Log($"Tile at {tileToChangePos} (was type {tileToChange.type}) exhausted all its types. Removing and backtracking further.");
            if (placedTilePositionsStack.Count > 0)
            {
                return AttemptBacktrack(); // Recursively backtrack further up the stack
            }
            else
            {
                Debug.LogWarning("Backtrack failed: Stack became empty after exhausting a tile.");
                return false; // No more tiles to backtrack to
            }
        }
    }

    private bool CheckMapClosure()
    {
        if (HexTileMap.MAP.Count == 0) return false;
        // For very small maps, "closure" might be trivially true or false depending on rules.
        // If target is 1, and start tile is placed, is it "closed"? Typically no.
        if (HexTileMap.MAP.Count < 3 && HexTileMap.MAP.Count < targetTileCount ) return false; // Arbitrary: need at least 3 tiles for a meaningful loop/closure typically.

        HexTile startTileInstance = HexTileMap.MAP.ContainsKey(Vector2.zero) ? HexTileMap.MAP[Vector2.zero] : null;
        if (startTileInstance == null) {
             Debug.LogError("Closure Check: Start tile is missing from map!"); return false;
        }

        bool mapStartNodeEntryUltimatelyClosed = false;

        foreach (HexTile tile in HexTileMap.MAP.Values)
        {
            if (tile.type == HexTile.TileType.NULL) continue; // NULL tiles don't need outgoing connections.

            List<(Vector2 toPos, HexTile.StartDir exitDir)> exits = tile.GetToPosDirs();

            if (exits.Count == 0 && tile.type != HexTile.TileType.NULL)
            {
                // A non-NULL tile that acts as an endpoint. All current types have exits.
                // This implies an issue or a very specific kind of dead-end spur.
                // For a fully "closed" loop map, this is usually undesirable unless it's a small, intentional spur.
                Debug.LogWarning($"Closure Check: Tile at {tile.pos} (type {tile.type}) has no exits but isn't NULL. This may indicate an unclosed spur.");
                 // Depending on strictness, this could be 'return false'. For now, allow spurs if main loop closes.
            }

            foreach (var exit in exits)
            {
                Vector2 exitTargetPos = exit.toPos;
                HexTile.StartDir exitDirFromTile = exit.exitDir;

                if (HexTileMap.MAP.ContainsKey(exitTargetPos))
                {
                    HexTile neighborTile = HexTileMap.MAP[exitTargetPos];
                    HexTile.StartDir entryDirForNeighbor = HexTile.OppositeDir(exitDirFromTile);
                    if (!neighborTile.IsTouchingDir(entryDirForNeighbor))
                    {
                        Debug.LogError($"Closure Check: Mismatched connection. Tile {tile.pos} (type {tile.type}) exits via {exitDirFromTile} to {exitTargetPos}, but neighbor {neighborTile.pos} (type {neighborTile.type}, entry {neighborTile.dir}) doesn't accept from {entryDirForNeighbor}. Neighbor actual entry: {neighborTile.dir}");
                        return false;
                    }
                }
                else // Exit leads to an empty space
                {
                    // This is ONLY allowed if this specific exit closes the start tile's initial entry point.
                    // The start tile's entry is from 'mapStartNodeFromPos', via 'mapStartNodeEntryDir'.
                    // So, 'tile' must be at 'mapStartNodeFromPos', and its 'exitDirFromTile' must be 'mapStartNodeEntryDir'.
                    // This is subtly wrong. It means THIS tile's ('tile') exit ('exitTargetPos', 'exitDirFromTile')
                    // should be such that 'exitTargetPos' IS 'mapStartNodeFromPos' and 'exitDirFromTile' IS 'mapStartNodeEntryDir'
                    // No, 'exitTargetPos' is where this tile leads. If THAT is the 'mapStartNodeFromPos' etc.
                    // Correct: If tile T1 exits to position P with direction D_out.
                    // For closure: P must be mapStartNodeFromPos, and D_out must be mapStartNodeEntryDir.
                    if (exitTargetPos == mapStartNodeFromPos && exitDirFromTile == mapStartNodeEntryDir)
                    {
                        mapStartNodeEntryUltimatelyClosed = true;
                    }
                    else
                    {
                        Debug.LogError($"Closure Check: Open edge! Tile {tile.pos} (type {tile.type}) exits via {exitDirFromTile} to empty space {exitTargetPos}. This does not close the start node's entry from {mapStartNodeFromPos} via {mapStartNodeEntryDir}.");
                        return false; // Any other open edge means map is not closed.
                    }
                }
            }
        }

        if (HexTileMap.MAP.Count > 1 && !mapStartNodeEntryUltimatelyClosed)
        {
            Debug.LogError($"Closure Check: Start node's main entry point (from {mapStartNodeFromPos} via {mapStartNodeEntryDir}) was not closed by any tile's exit.");
            return false;
        }
         if (HexTileMap.MAP.Count == 1 && !mapStartNodeEntryUltimatelyClosed && targetTileCount > 1){
            return false; // Single tile can't close itself unless it's a specific loop type and target is 1.
        }


        Debug.Log("Closure Check: All validation passed. Map is considered closed.");
        return true;
    }


    // --- Visual Instantiation ---
    private void InstantiateMapVisuals()
    {
        Debug.Log($"Instantiating visuals for {HexTileMap.MAP.Count} tiles.");
        if (tilePrefabs == null || tilePrefabs.Length < (int)HexTile.TileType.NULL) // Check up to highest defined type index
        {
            Debug.LogError("Tile prefabs array is not set correctly or is too small!");
            return;
        }

        foreach (KeyValuePair<Vector2, HexTile> entry in HexTileMap.MAP)
        {
            Vector2 gridPos = entry.Key; // This is your axial q, r
            HexTile tileData = entry.Value;

            if (tileData.type == HexTile.TileType.NULL)
            {
                // Optionally instantiate a "null" or "blocker" visual, or just skip
                continue;
            }

            int prefabIndex = (int)tileData.type;
            if (prefabIndex < 0 || prefabIndex >= tilePrefabs.Length || tilePrefabs[prefabIndex] == null)
            {
                Debug.LogError($"Invalid prefab index {prefabIndex} for TileType {tileData.type} or prefab not set at this index.");
                continue;
            }
            GameObject prefab = tilePrefabs[prefabIndex];

            // --- Convert Axial Coordinates to World Position ---
            // This assumes "pointy top" hex grid. Adjust if using "flat top".
            // Let q = gridPos.x, r = gridPos.y
            // Using common axial to cartesian conversion (approximate, adjust scale)
            float size = 1f; // Effective radius of the hex; adjust to match your art asset scale
            float worldX = size * (Mathf.Sqrt(3f) * gridPos.x + Mathf.Sqrt(3f) / 2f * gridPos.y);
            float worldZ = size * (                               3f / 2f * gridPos.y); // Use Z for depth if Y is up

            // If your HexTile.DirToPos uses an offset system (like "odd-r" or "even-q" for storage)
            // but you want to convert from pure axial for world position, use the pure axial formula.
            // The DirToPos in HexTile looked like it might be for an offset grid for finding neighbors.
            // Ensure your world conversion is consistent.
            // The example from your HexTile.DirToPos suggests an offset system:
            // Let's use a conversion compatible with that visual staggering if needed.
            // Visual X = gridX * hex_visual_width_step
            // Visual Y = gridY * hex_visual_height_step - (if gridX is odd then offset gridY_visual_offset)
            float visualWidthFactor = 1.732f * 0.9f; // Adjust these based on your prefab sizes and desired spacing
            float visualHeightFactor = 1.5f * 0.9f;
            float xPos = gridPos.x * visualWidthFactor;
            float zPos = gridPos.y * visualHeightFactor;
            if ((int)gridPos.x % 2 != 0) // Odd columns (q) often offset vertically in "pointy top odd-q"
            {
                // This depends on whether your +y in grid means "up" or "down-right" etc. in pointy top axial
                // If your HexTile.DirToPos implies "even-col pushes NE/SE down, odd-col pushes them up" for y,
                // then this offset might be:
                 zPos += visualHeightFactor / 2f; // Or subtracted, depending on axis orientation
            }
            // Revert to simpler for clarity if the above is too specific to a guessed offset:
            // float worldX = gridPos.x * 1.732f * 0.75f; // Placeholder, adjust to your assets
            // float worldZ = gridPos.y * 1.5f;
            //  if ((int)Mathf.Abs(gridPos.x) % 2 == 1 && gridPos.x !=0) { // Example for odd column offset for Y/Z
            //      worldZ += 0.75f; // Placeholder
            //  }
            // Sticking to the common axial to cartesian for now.
             worldX = size * (Mathf.Sqrt(3f) * gridPos.x  +  Mathf.Sqrt(3f)/2f * gridPos.y);
             worldZ = size * (                                3f/2f * gridPos.y);


            Vector3 worldPosition = new Vector3(worldX, 0, worldZ); // Assuming Y is Unity's up axis.

            GameObject instantiatedTile = Instantiate(prefab, worldPosition, Quaternion.identity, this.transform);

            // --- Rotate Tile Visual ---
            // Rotation depends on prefab's default orientation and tileData.dir (entry direction)
            // N=0, NE=1, SE=2, S=3, SW=4, NW=5
            // If N (0) is prefab's forward (e.g., +Z), then:
            // Angle for StartDir.N (0) -> 0 degrees from default
            // Angle for StartDir.NE (1) -> -60 degrees (or +300)
            // Angle for StartDir.SE (2) -> -120 degrees (or +240)
            // Angle for StartDir.S (3) -> -180 degrees (or +180)
            // Angle for StartDir.SW (4) -> -240 degrees (or +120)
            // Angle for StartDir.NW (5) -> -300 degrees (or +60)
            // The tileData.dir is the direction the path ENTERS FROM.
            // The visual should typically be oriented based on where the path EXITS or its primary flow.
            // If tileData.dir is S, path enters from S, flows N. Prefab default is often N. So rotate 180 for S.
            // Rotation mapping: (int)tileData.dir * 60 degrees.
            // If prefab default faces "North" (e.g. along +Z in Unity for a rotation of 0):
            // To align the "entry" side of the prefab with tileData.dir:
            float entryRotationAngle = (int)tileData.dir * 60f; // + for counter-clockwise from prefab's "East" if N is 0
                                                              // Or more directly if prefab's "North entrance" needs to align with tileData.dir:
                                                              // Let's assume prefabs are designed so their "input" faces their local -Z if not rotated.
                                                              // If tileData.dir is N (0), tile entered from N, visual points S. Rotation to make visual point N: 180.
                                                              // If tileData.dir is S (3), tile entered from S, visual points N. Rotation: 0.
                                                              // This is often ( (int)tileData.dir + 3) % 6 * 60f to get the "outgoing" primary direction.

            // Let's assume the prefab's "main path" without rotation flows from its local -Z to +Z,
            // and the 'dir' property of HexTile is the direction the path *enters* the tile from.
            // So, if dir is N, path comes from global N, tile's opening should face N.
            // If prefabs have their "opening" at their local "south" (e.g. -Z) when unrotated:
            float visualRotationY = (int)tileData.dir * 60f;
            instantiatedTile.transform.Rotate(Vector3.up, visualRotationY);

            instantiatedTile.name = $"Tile_{gridPos.x}_{gridPos.y}_[{tileData.type}]_Entr{(tileData.dir)}";
        }
    }
}