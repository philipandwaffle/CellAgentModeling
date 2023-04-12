using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Environment {
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public class DijkstraNavGraph : INavGraph {
        public Vector2Int[,][] edgeCoords;
        public Vector2Int[] nodeCoords;
        //public Dictionary<Vector2Int, int>
        private int numVerts;
        private float[,] adjMatrix;

        private int[][] paths;
        int srcNode;

        [JsonConstructor]
        public DijkstraNavGraph(int[,] graph) {
            CC cc = new CC(graph);
            edgeCoords = cc.CalcEdgeCoords();
            nodeCoords = cc.nodeCoords;
            srcNode = cc.srcNode;

            numVerts = edgeCoords.GetLength(0);
        }

        public void UpdatePaths() {
            paths = new int[numVerts][];

            float[] shortestDist = new float[numVerts];
            bool[] added = new bool[numVerts];

            // Initialize all distances as inf
            for (int vertI = 0; vertI < numVerts; vertI++) {
                shortestDist[vertI] = int.MaxValue;
                added[vertI] = false;
            }

            // Distance of source vertex from itself is always 0
            shortestDist[srcNode] = 0;

            // Parent array to store shortest path tree
            int[] parents = new int[numVerts];

            // The starting vertex does not have a parent
            parents[srcNode] = -1;

            // Find shortest path for all vertices
            for (int i = 0; i < numVerts; i++) {
                // Pick the minimum distance vertex from the set of vertices not yet processed
                int nearestVertex = -1;
                float shortestDistance = float.MaxValue;
                for (int vertexIndex = 0; vertexIndex < numVerts; vertexIndex++) {
                    if (!added[vertexIndex] && shortestDist[vertexIndex] < shortestDistance) {
                        nearestVertex = vertexIndex;
                        shortestDistance = shortestDist[vertexIndex];
                    }
                }

                // Mark the picked vertex as processed
                added[nearestVertex] = true;

                // Update dist value of the adjacent vertices of the picked vertex.
                for (int vertI = 0; vertI < numVerts; vertI++) {
                    float edgeDistance = adjMatrix[vertI, nearestVertex];

                    if (edgeDistance > 0 && ((shortestDistance + edgeDistance) < shortestDist[vertI])) {
                        parents[vertI] = nearestVertex;
                        shortestDist[vertI] = shortestDistance + edgeDistance;
                    }
                }
            }

            for (int dest = 0; dest < numVerts; dest++) {
                List<int> path = new List<int>();
                BuildPath(ref path, dest, parents);
                paths[dest] = path.ToArray();
            }
        }

        private void BuildPath(ref List<int> path, int dest, int[] parents) {
            if (dest == -1) return;

            BuildPath(ref path, parents[dest], parents);
            path.Add(dest);
        }

        public void UpdateEdges(ref float[,] layer) {
            adjMatrix = new float[numVerts, numVerts];
            for (int src = 0; src < numVerts; src++) {
                for (int dest = 0; dest < numVerts; dest++) {
                    // Default to 0
                    adjMatrix[src, dest] = 0f;

                    // Skip if connection doesn't exist
                    if (edgeCoords[src, dest] is null) continue;

                    // Loop through each edge coord and set weight
                    for (int i = 0; i < edgeCoords[src, dest].Length; i++) {
                        Vector2Int pos = edgeCoords[src, dest][i];
                        float val = layer[pos.y, pos.x];

                        if (val == -2) {
                            val = 0;
                        }

                        // Set to infinite when too hot
                        adjMatrix[src, dest] += 1 + (val * 15f);
                    }
                }
            }
        }

        public Vector2Int[] GetNodeCoords() {
            return nodeCoords;
        }

        public int[][] Getpaths() {
            return paths;
        }
    }

    // Helper class to calculate the connected components from a matrix
    public class CC {
        private int[,] graph;
        private int[,] labels;
        private bool[,] visited;

        private int h, w;

        private int[] dx = { -1, 0, 1, 0 };
        private int[] dy = { 0, 1, 0, -1 };
        public int srcNode { get; private set; } = -1;

        List<List<Vector2Int>> conCoords = new List<List<Vector2Int>> {
            new List<Vector2Int>()
        };
        public Vector2Int[] nodeCoords { get; private set; }

        public CC(int[,] graph) {
            if (dx.Length != dy.Length) {
                throw new Exception("dx and dy length mismatch");
            }

            this.graph = graph;

            h = graph.GetLength(0);
            w = graph.GetLength(1);

            // Allocate labels and initilise
            labels = new int[h, w];
            visited = new bool[h, w];
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    labels[y, x] = 0;
                    visited[y, x] = false;
                }
            }
        }

        private bool DFS(int x, int y, int curLabel) {
            // Out of bounds
            if (OutBounds(x, y)) return false;

            // Mark as visited
            visited[x, y] = true;

            // Already labeled or not a valid connection
            if (labels[y, x] != 0 || graph[y, x] < 1) return false;

            // Mark the current cell
            conCoords[curLabel - 1].Add(new Vector2Int(x, y));
            labels[y, x] = curLabel;

            // Recursively mark the neighbors
            for (int i = 0; i < dx.Length; i++) {
                DFS(x + dx[i], y + dy[i], curLabel);
            }
            return true;
        }
        private bool OutBounds(int x, int y) {
            return x < 0 || x >= w || y < 0 || y >= h;
        }

        public Vector2Int[,][] CalcEdgeCoords() {
            int curLabel = 1;

            List<Vector2Int> nodes = new List<Vector2Int>();

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    // Add to node list if node is detected
                    if (graph[y, x] < 0) nodes.Add(new Vector2Int(x, y));

                    if (visited[y, x]) continue;

                    if (DFS(y, x, curLabel)) {
                        conCoords.Add(new List<Vector2Int>());
                        curLabel++;
                    }
                }
            }
            conCoords.RemoveAt(curLabel - 1);

            // Find the src node index
            for (int i = 0; i < nodes.Count; i++) {
                int x = nodes[i].x;
                int y = nodes[i].y;
                if (graph[y, x] == -2) {
                    srcNode = i;
                    break;
                }
            }
            if (srcNode == -1) Debug.LogError("No source node in nav graph");

            nodeCoords = nodes.ToArray();
            List<int>[] nodeCons = CalcNodeCons(nodeCoords);
            int numNodes = nodeCons.Length;

            // Allocate edge coords
            Vector2Int[,][] edgeCoords = new Vector2Int[numNodes, numNodes][];

            //Loop through each src node and check connections with other nodes
            edgeCoords = new Vector2Int[numNodes, numNodes][];
            for (int src = 0; src < numNodes; src++) {
                for (int dest = 0; dest < numNodes; dest++) {
                    if (src == dest) continue;
                    int[] inter = nodeCons[src].Intersect(nodeCons[dest]).ToArray();
                    int interCount = inter.Length;
                    if (interCount == 1) {
                        edgeCoords[src, dest] = conCoords[inter[0] - 1].ToArray();
                    } else if (interCount > 1) {
                        Console.WriteLine("WTF");
                    }
                }
            }
            return edgeCoords;
        }

        private List<int>[] CalcNodeCons(Vector2Int[] nodes) {
            int numNodes = nodes.Length;
            List<int>[] cons = new List<int>[numNodes];

            // Loop through each node's pos 
            for (int node = 0; node < numNodes; node++) {
                Vector2Int curNode = nodes[node];
                List<int> curNodeCons = new List<int>();

                // Check the neighbours of node for connections
                for (int i = 0; i < dx.Length; i++) {
                    int x = curNode.x + dx[i];
                    int y = curNode.y + dy[i];

                    if (OutBounds(x, y)) continue;

                    // Add connection id to list if not there already
                    int con = labels[y, x];
                    if (con > 0 && !curNodeCons.Contains(con)) {
                        curNodeCons.Add(con);
                    }
                }

                cons[node] = curNodeCons;
            }
            return cons;
        }
    }
}
