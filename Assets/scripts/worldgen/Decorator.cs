using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Decorator : MonoBehaviour {
    [System.Serializable]
    public class DecoratorNoiseProperties2D {
        public enum type {
            TREE,
            FOLIAGE,
            ROCKS
        };
        public type Type;
        public bool active;
        public bool negative;
        public float size;
        [Range(0.1f, 1f)]
        public float weight;
        public float amplifier;
        public Vector2Int offset;
    }
    [Header("Decorator Noise Properties")]
    public DecoratorNoiseProperties2D[] decoratorNoise2D;

    [Header("Dependencies")]
    public WorldHandler worldHandler;
    [Header("Decoration Properties")]
    public float treeSizeMultiplier = 5; //default : 5


    public void DecorateFloor(int x, int y) {
        float tree = 0;
        float foliage = 0;
        float rock = 0;
        // Accumulate noise values depending on their type.
        foreach(var noise in decoratorNoise2D) {
            switch(noise.Type) {
                case DecoratorNoiseProperties2D.type.TREE:
                    tree += Mathf.PerlinNoise(
                        (x + noise.offset.x * worldHandler.seed) / (noise.size + 0.05f),
                        (y + noise.offset.y * worldHandler.seed) / (noise.size + 0.05f)
                        ) * noise.amplifier * noise.weight;
                    break;
                case DecoratorNoiseProperties2D.type.FOLIAGE:
                    foliage += Mathf.PerlinNoise(
                        (x + noise.offset.x * worldHandler.seed) / (noise.size + 0.05f),
                        (y + noise.offset.y * worldHandler.seed) / (noise.size + 0.05f)
                        ) * noise.amplifier * noise.weight;
                    break;
                case DecoratorNoiseProperties2D.type.ROCKS:
                    rock += Mathf.PerlinNoise(
                        (x + noise.offset.x * worldHandler.seed) / (noise.size + 0.05f),
                        (y + noise.offset.y * worldHandler.seed) / (noise.size + 0.05f)
                        ) * noise.amplifier * noise.weight;
                    break;
                default:
                    break;
            }
        }
        // Get the biggest noise value
        if(tree > foliage) {
            if(tree > rock) {
                if(tree > .3f) {
                    GenerateTree(x, y, tree);
                }
            } else {
                GenerateRock(x, y);
            }
        } else if(foliage > tree) {
            if(foliage > rock) {
                GenerateFoliage(x, y);
            } else {
                GenerateRock(x, y);
            }
        }
    }
    public void GenerateTree(int x, int y, float tree) {
        int treesize = (int)(tree * treeSizeMultiplier);
        treesize += (int)(treesize / (Mathf.PerlinNoise1D(x + (Mathf.PerlinNoise1D(y) + 1) + 1)) / (Mathf.PerlinNoise1D(x) + 1) * treeSizeMultiplier);
        if(treesize > 22) {
            treesize = 22;
        }
        Debug.Log(treesize);
        worldHandler.decorationTileMap.SetTile(new Vector3Int(x, y), worldHandler.Tile("treestump"));
        for(int i = 1;i < treesize;i++) {
            if(i < treesize / 3) {
                worldHandler.decorationTileMap.SetTile(new Vector3Int(x, y + i), worldHandler.Tile("treebody_m"));
            } else {
                worldHandler.decorationTileMap.SetTile(new Vector3Int(x, y + i), worldHandler.Tile("treebody_s"));
            }
        }
        worldHandler.decorationTileMap.SetTile(new Vector3Int(x, y + treesize), worldHandler.Tile("treebody_end"));
    }
    public void GenerateFoliage(int x, int y) {
        //TBI
        worldHandler.decorationTileMap.SetTile(new Vector3Int(x, y), worldHandler.Tile("foliage_1"));
    }
    public void GenerateRock(int x, int y) {
        //TBI
        worldHandler.decorationTileMap.SetTile(new Vector3Int(x, y), worldHandler.Tile("rock_1"));
    }
}
