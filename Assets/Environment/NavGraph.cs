using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Environment {
    [Serializable]
    public class NavGraph {
        public Vector2Int[,][] edgeCoords;
        private int numVerts;
        public float[,] adjMatrix;
        public List<int>[] paths;

        public NavGraph(Vector2Int[,][] edgeCoords) { 
            this.edgeCoords = edgeCoords;
            numVerts = edgeCoords.GetLength(0);
            adjMatrix = new float[numVerts, numVerts];
            paths = new List<int>[numVerts];
        }

        public void UpdatePaths() {
            int srcVert = 0;

            float[] shortestDist = new float[numVerts];
            bool[] added = new bool[numVerts];

            // Initialize all distances as inf
            for (int vertI = 0; vertI < numVerts; vertI++) {
                shortestDist[vertI] = int.MaxValue;
                added[vertI] = false;
            }

            // Distance of source vertex from itself is always 0
            shortestDist[srcVert] = 0;

            // Parent array to store shortest path tree
            int[] parents = new int[numVerts];

            // The starting vertex does not have a parent
            parents[srcVert] = -1;

            // Find shortest path for all vertices
            for (int i = 1; i < numVerts; i++) {
                // Pick the minimum distance vertex
                // from the set of vertices not yet
                // processed. nearestVertex is
                // always equal to startNode in
                // first iteration.
                int nearestVertex = -1;
                float shortestDistance = float.MaxValue;
                for (int vertexIndex = 0; vertexIndex < numVerts; vertexIndex++) {
                    if (!added[vertexIndex] && shortestDist[vertexIndex] < shortestDistance) {
                        nearestVertex = vertexIndex;
                        shortestDistance = shortestDist[vertexIndex];
                    }
                }

                // Mark the picked vertex as
                // processed
                added[nearestVertex] = true;

                // Update dist value of the
                // adjacent vertices of the
                // picked vertex.
                for (int vertI = 0; vertI < numVerts; vertI++) {
                    float edgeDistance = adjMatrix[nearestVertex, vertI];

                    if (edgeDistance > 0 && ((shortestDistance + edgeDistance) < shortestDist[vertI])) {
                        parents[vertI] = nearestVertex;
                        shortestDist[vertI] = shortestDistance + edgeDistance;
                    }
                }
            }

            for (int dest = 0; dest < numVerts; dest++) {
                List<int> path = new List<int>();
                BuildPath(ref path, dest, parents);
                paths[dest] = path;
            }
        }

        private void BuildPath(ref List<int> path, int dest, int[] parents) {
            if (dest == -1) {
                return;
            }
            BuildPath(ref path, parents[dest], parents);
            path.Add(dest);
        }

        public void UpdateAdjMatrix(ref float[,] layer) {
            for (int src = 0; src < numVerts; src++) {
                for (int dest = 0; dest < numVerts; dest++) {
                    adjMatrix[src, dest] = 0f;
                    for (int i = 0; i < edgeCoords[src,dest].Length; i++) {
                        Vector2Int pos = edgeCoords[src, dest][i];
                        adjMatrix[src,dest] += layer[pos.y, pos.x];
                    }
                }
            }
        }
    }
}
