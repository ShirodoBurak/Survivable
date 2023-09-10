using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour {
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
    public NoiseProperties1D[] Noise1D;
    public NoiseProperties2D[] Noise2D;
    public Tilemap tm01;
    public TileBase t0;
    public Camera cam;
    List<Vector2Int> loadedChunks = new List<Vector2Int>();
    int chunksize = 32;
    Vector2Int lastpos;
    private void Start() {

    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            loadedChunks.Clear();
            tm01.ClearAllTiles();
            Vector2Int loc = WorldCoordinatesToChunkCoordinates(Camera.main.transform.position);
            lastpos = loc;
            GenerateByChunkCoordinate(loc);
        }
        Vector2Int location = WorldCoordinatesToChunkCoordinates(Camera.main.transform.position);
        if (lastpos == null) {
            lastpos = location;
        } else if (location != lastpos) {
            GenerateByChunkCoordinate(location);
            lastpos = location;
        } else { }
    }
    public void GenerateByChunkCoordinate(Vector2Int pos) {
        for (int i = -8; i < 8; i++) {
            for (int j = -4; j < 4; j++) {
                if (!loadedChunks.Contains(new Vector2Int(i + pos.x, j + pos.y))) {
                    loadedChunks.Add(new Vector2Int(i + pos.x, j + pos.y));
                    GenerateChunk(i + pos.x, j + pos.y);
                }
            }
        }
    }
    public void GenerateChunk(int x, int y) {
        for (int i = x * chunksize; i < x * chunksize + chunksize; i++) {
            float h = 0;
            foreach (var n1 in Noise1D) {
                if (n1.active) h += Mathf.PerlinNoise1D((i + n1.offset.x + 8914984) / (n1.size + 0.05f)) * n1.amplifier * n1.weight;
            }
            for (int j = y * chunksize; j < y * chunksize + chunksize; j++) {
                float cavity = 0f;
                foreach (var n2 in Noise2D) {
                    if (n2.active) {
                        if (!n2.negative) {
                            cavity += Mathf.PerlinNoise((i + n2.offset.x + 1561560.5f) / (n2.size + 0.05f), (j + n2.offset.y + 1561560.5f) / (n2.size + 0.05f)) * n2.amplifier * n2.weight;
                        } else {
                            float negativecavity = Mathf.PerlinNoise((i + n2.offset.x + 1561560.5f) / (n2.size + 0.05f), (j + n2.offset.y + 1561560.5f) / (n2.size + 0.05f)) * n2.amplifier;
                            if (negativecavity >= 0.89f) {
                                cavity -= negativecavity*n2.weight;
                            }
                        }
                    }
                }
                // This section makes caves much less occusing in surfaces.
                // Caves tend disappear as they are getting closer to surface level
                // A value between 0 and 1
                float surfaceincavityness = 0f;
                float distance = Vector2Int.Distance(new Vector2Int(i, j), new Vector2Int(i, (int)h));
                if (distance < 30) {
                    surfaceincavityness = 2 / distance;
                }
                if (j < h) {
                    
                    if (cavity < .6f + surfaceincavityness) {
                        tm01.SetTile(new Vector3Int(i, j, 0), t0);
                    }
                }
            }
        }
    }
    public Vector2Int WorldCoordinatesToChunkCoordinates(Vector2 worldCoordinates) {
        return new Vector2Int((int)worldCoordinates.x / chunksize, (int)worldCoordinates.y / chunksize);
    }
}
