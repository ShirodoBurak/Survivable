using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct AsyncChunkGeneration : IJob {
    public NativeArray<int> active1D;
    public NativeArray<int> negative1D;
    public NativeArray<float> size1D;
    public NativeArray<float> weight1D;
    public NativeArray<float> amplifier1D;
    public NativeArray<Vector2Int> offset1D;
    public NativeArray<int> active2D;
    public NativeArray<int> negative2D;
    public NativeArray<float> size2D;
    public NativeArray<float> weight2D;
    public NativeArray<float> amplifier2D;
    public NativeArray<Vector2Int>  offset2D;
    public Vector3Int cpos;
    public int chunksize;
    public NativeArray<Vector3Int> tilePositions;
    public NativeArray<int> tileTypes;
    public void Execute() {
        active1D = new NativeArray<int>();
        int indexTile = 0;
        for(int i = cpos.x * chunksize;i < cpos.x * chunksize + chunksize;i++) {
            float h = 0;
            int index1D = 0;
            foreach(var n1 in active1D) {
                if(n1 == 1 && !(negative1D[index1D] == 0))
                    h += Mathf.PerlinNoise1D((i + offset1D[index1D].x + 8914984) / (size1D[index1D] + 0.05f)) * amplifier1D[index1D] * weight1D[index1D];
                else if(n1 == 1 && negative1D[index1D] == 1)
                    h -= Mathf.PerlinNoise1D((i + offset1D[index1D].x + 8914984) / (size1D[index1D] + 0.05f)) * amplifier1D[index1D] * weight1D[index1D];
                index1D++;
            }
            index1D = 0;
            int index2D = 0;
            for(int j = cpos.y * chunksize;j < cpos.y * chunksize + chunksize;j++) {
                float cavity = 0f;
                foreach(var n2 in active2D) {
                    if(active2D[index2D] == 1) {
                        if(!(negative2D[index2D] == 1)) {
                            cavity += Mathf.PerlinNoise(
                                (i + offset2D[index2D].x + 1561560.5f) / (size2D[index2D] + 0.05f),
                                (j + offset2D[index2D].y + 1561560.5f) / (size2D[index2D] + 0.05f)
                            ) * amplifier2D[index2D] * weight2D[index2D];
                        } else {
                            float negativecavity = Mathf.PerlinNoise(
                                (i + offset2D[index2D].x + 1561560.5f) / (size2D[index2D] + 0.05f),
                                (j + offset2D[index2D].y + 1561560.5f) / (size2D[index2D] + 0.05f)
                            ) * amplifier2D[index2D];

                            if(negativecavity >= 0.89f) {
                                cavity -= negativecavity * weight2D[index2D];
                            }
                        }
                    }
                    index2D++;
                }
                index2D = 0;
                // This section makes caves much less occusing in surfaces.
                // Caves tend disappear as they are getting closer to surface level
                // A value between 0 and 1
                float surfaceincavityness = 0f;
                float distance = Vector2Int.Distance(new Vector2Int(i, j), new Vector2Int(i, (int)h));
                if(distance < 30) {
                    surfaceincavityness = 2 / distance;
                }
                if(j < h) {
                    // 0 means SetTile, else means SetTiles mode.
                    if(cavity < .6f + surfaceincavityness) {
                        tilePositions[indexTile] = new Vector3Int(i, j, 0);
                        tileTypes[indexTile] = 1;
                    } else {
                        tilePositions[indexTile] = new Vector3Int(i, j, 0);
                        tileTypes[indexTile] = 0;
                    }
                }
                indexTile++;
            }
        }
    }
}
