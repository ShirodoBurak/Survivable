using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class WorldHandler : MonoBehaviour {
    [System.Serializable]
    public class NoiseProperties1D {
        public bool active;
        public float size;
        [Range(0.1f, 1f)]
        public float weight;
        public float amplifier;
        public Vector2Int offset;
    }
    [System.Serializable]
    public class NoiseProperties2D {
        public bool active;
        public bool negative;
        public float size;
        [Range(0.1f, 1f)]
        public float weight;
        public float amplifier;
        public Vector2Int offset;
    }
    [System.Serializable]
    public class TileType {
        public string tileName;
        public TileBase tile;
    }
    [Header("Tiles")]
    public List<TileType> TileTypes;
    public class TileCache {
        public Dictionary<Vector3Int, TileBase> tiles = new Dictionary<Vector3Int, TileBase>();
    }
    public TileCache tileCache;
    public TileCache tileCacheBG;
    [Header("Noise Properties")]
    public NoiseProperties1D[] Noise1D;
    public NoiseProperties2D[] Noise2D;
    [Header("Dependencies")]
    public Decorator decorator;
    public Tilemap mainTileMap;
    public LightingSystem lightingSystem;
    public Tilemap backgroundTileMap;
    public Tilemap decorationTileMap;
    public TileBase notATile;
    public Camera cam;
    public int seed;

    List<Vector2Int> loadedChunks = new List<Vector2Int>();
    int chunksize = 32;
    Vector2Int lastpos;
    Vector3 lastcampos;
    private void Start() {
        //Initialize variables and arrays
        tileCache = new TileCache();
        tileCacheBG = new TileCache();
        //Initial chunk generation
        Vector2Int loc = WorldPosToChunkPos(Camera.main.transform.position);
        lastpos = loc;
        lastcampos = new Vector3Int((int)Camera.main.transform.position.x, (int)Camera.main.transform.position.y, 0);
        GenerateByChunkCoordinate(loc);
        GenerateView();
        //Initialize unused chunk cleaning
        StartCoroutine(GarbageCleaner());
    }
    void Update() {
        Vector3Int camloc = new Vector3Int((int)Camera.main.transform.position.x, (int)Camera.main.transform.position.y, 0);
        if(camloc != lastcampos) {
            lastcampos = camloc;
            GenerateView();
        }
        if(Input.GetKeyDown(KeyCode.M)) {
            loadedChunks.Clear();
            mainTileMap.ClearAllTiles();
            Vector2Int loc = WorldPosToChunkPos(camloc);
            lastpos = loc;
            GenerateByChunkCoordinate(loc);
        }

        Vector2Int location = WorldPosToChunkPos(camloc);
        if(lastpos == null) {
            lastpos = location;
        } else if(location != lastpos) {
            GenerateByChunkCoordinate(location);
            lastpos = location;
        } else {
        }
    }
    //private void FixedUpdate() {
        //if(Input.GetKeyDown(KeyCode.S)) {
    //    List<Vector3Int> edgeTiles = lightingSystem.FindEdges();
        //Debug.Log("Hi, on lighting system. edgeTile list size : " + edgeTiles.Count);
    //    foreach(var position in edgeTiles) {
            //mainTileMap.SetTile(position, Tile("grass"));
    //    }
        //}
    //}
    //<summary>
    // Generate chunks, add to cache and display on tilemaps.
    // Also, remove unused, distant chunks.
    //</summary>
    #region Chunk generation
    // It's kind of stupid. I don't think I am required to explain this one.
    // Anyway, it generates a set of chunks depending on the point where the camera is.
    // I guess it generates 12(width) by 6(height)?.
    public void GenerateByChunkCoordinate(Vector2Int pos) {
        for(int i = -6;i < 6;i++) {
            for(int j = -3;j < 3;j++) {
                Vector2Int cpos = new Vector2Int(i + pos.x, j + pos.y);
                float distance = Vector2.Distance(cpos, Camera.main.transform.position / chunksize);
                if(!loadedChunks.Contains(cpos) && distance < 6) {
                    loadedChunks.Add(cpos);
                    GenerateChunk(cpos);
                }
            }
        }
    }
    public void GenerateChunk(Vector2Int cpos) {
        for(int i = cpos.x * chunksize;i < cpos.x * chunksize + chunksize;i++) {
            // Yeah, this is the point where we use One-Dimensional noise to generate our One-Dimensional
            // noise map. We generate our world with this height values.
            float h = 0;
            foreach(var n1 in Noise1D) {
                if(n1.active)
                    h += Mathf.PerlinNoise1D((i + n1.offset.x * seed) / (n1.size + 0.05f)) * n1.amplifier * n1.weight;
            }
            for(int j = cpos.y * chunksize;j < cpos.y * chunksize + chunksize;j++) {
                // This part of code generates the noise for caves. Depending on the value, tile gets ignored.
                float cavity = 0f;
                foreach(var n2 in Noise2D) {
                    if(n2.active) {
                        if(!n2.negative) {
                            cavity += Mathf.PerlinNoise(
                                (i + n2.offset.x * seed) / (n2.size + 0.05f),
                                (j + n2.offset.y * seed) / (n2.size + 0.05f)
                            ) * n2.amplifier * n2.weight;
                        } else {
                            float negativecavity = Mathf.PerlinNoise(
                                (i + n2.offset.x * seed) / (n2.size + 0.05f),
                                (j + n2.offset.y * seed) / (n2.size + 0.05f)
                            ) * n2.amplifier;

                            if(negativecavity >= 0.89f) {
                                cavity -= negativecavity * n2.weight;
                            }
                        }
                    }
                }
                // This section makes caves much less occuring in surfaces.
                // Caves tend disappear as they are getting closer to surface level
                // A value between 0 and 1
                float surfaceincavityness = 0f;
                float distance = Vector2Int.Distance(new Vector2Int(i, j), new Vector2Int(i, (int)h));
                if(distance < 30) {
                    surfaceincavityness = 2 / distance;
                }
                if(j < h + 1) {
                    if(j >= h) {
                        //
                        // Ground level decorator hits.
                        //
                        decorator.DecorateFloor(i, j + 1);
                        AddToCache(new Vector3Int(i, j, 0), Tile("grass"), false);

                    } else if(j >= h - 5 && j < h) {
                        AddToCache(new Vector3Int(i, j, 0), Tile("dirt"), false);
                    } else if(cavity < .6f + surfaceincavityness) {
                        AddToCache(new Vector3Int(i, j, 0), Tile("stone"), false);
                    } else {
                        AddToCache(new Vector3Int(i, j, 0), null, false);
                    }
                    if(j >= h - 6) {
                        AddToCache(new Vector3Int(i, j, 0), Tile("dirt"), true);
                    } else {
                        AddToCache(new Vector3Int(i, j, 0), Tile("stone"), true);
                    }
                }

            }
        }
    }
    Vector3Int tilepos;
    // This function generates the view depending on the Camera viewport. 
    // Since it gets tilecache by chunks, instead of generating every tile
    // even if its not required to be rendered, we only generate
    // ones that are "probably visible" to the player.
    void GenerateView() {
        Vector3 min = Camera.main.ViewportToWorldPoint(new Vector3(-0.25f, -0.25f, 0));
        Vector3 max = Camera.main.ViewportToWorldPoint(new Vector3(1.25f, 1.25f, 0));
        for(int x = (int)min.x;x < (int)max.x;x++) {
            for(int y = (int)min.y;y < (int)max.y;y++) {
                tilepos = new Vector3Int(x, y);
                if(tileCache.tiles.ContainsKey(tilepos)) {
                    mainTileMap.SetTile(tilepos, tileCache.tiles[tilepos]);
                }
                if(tileCacheBG.tiles.ContainsKey(tilepos)) {
                    backgroundTileMap.SetTile(tilepos, tileCacheBG.tiles[tilepos]);
                }
            }
        }
    }
    List<Vector2Int> garbageList = new List<Vector2Int>();
    IEnumerator GarbageCleaner() {
        while(true) {
            // Loop through the existing chunks
            foreach(var chunk in loadedChunks) {
                // Get distance between the camera and chunks
                float distance = Vector2.Distance(chunk, WorldPosToChunkPos(Camera.main.transform.position));
                if(distance > 6/*chunksize*/) {
                    garbageList.Add(chunk);
                }
            }
            // Loop through the Garbage chunk list and clear the garbages from 
            // Existing chunk lists. Then purge the chunk.
            int chunkcleanlimit = 0;
            foreach(var garbage in garbageList) {
                if(chunkcleanlimit > 5) {
                    yield return new WaitForSeconds(1f / 3f);
                    chunkcleanlimit = 0;
                }
                float distance = Vector2.Distance(garbage, Camera.main.transform.position / chunksize);
                if(distance > 6 /*chunksize*/) {
                    loadedChunks.Remove(garbage);
                    PurgeChunk(garbage);
                    yield return new WaitForFixedUpdate();
                }
                chunkcleanlimit++;
            }
            // Clean the garbageList after garbage cleaning
            garbageList.Clear();
            yield return new WaitForSeconds(5f);
        }
    }
    // Remove chunks from view.
    void PurgeChunk(Vector2Int c_pos) {
        // Weird way to implement it but I guess I'll leave it like this.
        // I used two arrays to hold tile properties.
        // One holds the tile type, other hold the position of the tile. 
        TileBase[] tileArray = new Tile[chunksize * chunksize];
        Vector3Int[] tilePosArray = new Vector3Int[chunksize * chunksize];
        int index = 0;
        // It loops through the tiles in determined chunk area
        for(int x = c_pos.x * chunksize;x < c_pos.x * chunksize + chunksize;x++) {
            for(int y = c_pos.y * chunksize;y < c_pos.y * chunksize + chunksize;y++) {
                // Adds them into our two arrays and increases the index value.
                // I also can simply remove the index value and get index with " (x - c_pos.x * chunksize) + (y - c_pos.y * chunksize) "
                // But I feel like it looks cleaner and more readable here. So I'll leave it like this now.
                tileArray[index] = null;
                tilePosArray[index] = new Vector3Int(x, y, 0);
                index++;
            }
        }
        // Yes, it uses Tilemap.SetTiles() functions to remove the tiles here.
        mainTileMap.SetTiles(tilePosArray, tileArray);
        backgroundTileMap.SetTiles(tilePosArray, tileArray);
    }
    #endregion

    // Get tile by its name as string. It's set on tileholder object.
    // Look! it even shoots out an error message if it fails!
    public TileBase Tile(string tilename) {
        foreach(var tile in TileTypes) {
            if(tile.tileName == tilename) {
                return tile.tile;
            }
        }
        Debug.Log("Error 01:\nCouldn't find the tile '" + tilename + "' in the tile list. Oops!");
        return notATile;
    }
    //Convert world position to chunk position by dividing the coordinates by chunksize
    public Vector2Int WorldPosToChunkPos(Vector3 worldCoordinates) {
        return new Vector2Int((int)worldCoordinates.x / chunksize, (int)worldCoordinates.y / chunksize);
    }

    // Checks if tile exists in the cache. If not, adds it. If so, it simply ignores it.
    // It should not attempt to regenerate the same tile anyway. Why did I even do this check now?
    // I guess I messed up something up on the chunk generation. I believe I check if "chunk" exists already? 
    // So, what is the point of checks here?
    // I guess I'll leave it here for now.
    //
    //  Fix this later.
    //
    public void AddToCache(Vector3Int pos, TileBase tile, bool bg) {
        if(bg) {
            if(!tileCacheBG.tiles.ContainsKey(pos)) {
                tileCacheBG.tiles.Add(pos, tile);
            }
        } else {
            if(!tileCache.tiles.ContainsKey(pos)) {
                tileCache.tiles.Add(pos, tile);
            }
        }
    }
}
