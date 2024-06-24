using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LightingSystem : MonoBehaviour {
    public Tilemap world;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    List<Vector3Int> edges = new List<Vector3Int>();
    public List<Vector3Int> FindEdges() {
        edges.Clear();
        foreach(var tilePosition in world.cellBounds.allPositionsWithin) {
            if(world.GetTile(tilePosition) != null) {

                Vector3Int[] neighbors = {
                new Vector3Int(tilePosition.x, tilePosition.y + 1, tilePosition.z), // North
                new Vector3Int(tilePosition.x, tilePosition.y - 1, tilePosition.z), // South
                new Vector3Int(tilePosition.x + 1, tilePosition.y, tilePosition.z), // East
                new Vector3Int(tilePosition.x - 1, tilePosition.y, tilePosition.z)  // West
            };
                foreach(var neighbor in neighbors) {
                    if(world.GetTile(neighbor) == null) {
                        edges.Add(tilePosition);
                    }
                }
            }
        }
        return edges;
    }
    void CheckIfEdge() {

    }
}
