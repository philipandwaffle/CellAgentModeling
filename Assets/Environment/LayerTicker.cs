using Assets.Agent.Sensors;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Environment {
    public class LayerTicker : MonoBehaviour {
        [SerializeField] private Sprite displaySprite;

        private Layer[] layers;
        private GameObject[][,] display;
        private GameObject[] displayContainers;
        public ComputeShader computeShader;

        public void SetNumLayers(int z) {
            layers = new Layer[z];

            // Check if the display has been set before
            if (display != null) {

                // destroy the previous environment
                for (int curLayer = 0; curLayer < displayContainers.Length; curLayer++) {
                    // Destroy container 
                    Destroy(displayContainers[curLayer]);

                    /*for (int y = 0; y < display[curLayer].GetLength(0); y++) {
                        for (int x = 0; x < display[curLayer].GetLength(1); x++) {
                            // Destroy cell
                            Destroy(display[curLayer][y, x]);
                        }
                    }*/
                }
            }

            NavLayerSensor.maxZ = z;

            // Set the lenght of the display
            display = new GameObject[z][,];
            displayContainers = new GameObject[z];
        }

        public Layer GetLayer(int z) {
            return layers[z];
        }
        public int GetNumLayers() {
            return layers.Length;
        }
        public void SetLayer(int z, Layer layer) {
            layers[z] = layer;
            SetDisplay(z);
        }

        private void SetDisplay(int z) {
            // Create new display container
            GameObject container = new GameObject("Layer: " + z);
            container.transform.parent = transform;
            container.transform.localScale = Vector3.one;
            container.layer = 6 + z;
            displayContainers[z] = container;


            // Create display cell
            GameObject dis = new GameObject {
                tag = "layer"
            };

            display[z] = new GameObject[layers[z].h, layers[z].w];

            for (int x = 0; x < layers[z].w; x++) {
                for (int y = 0; y < layers[z].h; y++) {
                    GameObject instance = Instantiate(dis);
                    instance.layer = 6 + z;
                    instance.transform.parent = container.transform;
                    instance.transform.localScale = Vector3.one;
                    instance.transform.position = new Vector3(
                        transform.localScale.x * x,
                        transform.localScale.y * y,
                        z * -CASMEditor.layerSep);
                    instance.name = z + " " + x + "," + y;

                    BoxCollider2D bc = instance.AddComponent<BoxCollider2D>();
                    bc.enabled = layers[z][y, x] == -1;

                    SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
                    sr.sprite = displaySprite;
                    sr.color = layers[z].GetDisplayData(y, x);

                    display[z][y, x] = instance;
                }
            }

            // Debug to display nav graph nodes and connections
            /*if (layers[z].navGraph is not null) {
                Vector2Int[] nodeCoords = layers[z].navGraph.nodeCoords;
                for (int i = 0; i < nodeCoords.Length; i++) {
                    GameObject instance = Instantiate(dis);
                    instance.name = i.ToString();
                    instance.layer = 6 + z;
                    instance.transform.parent = container.transform;
                    instance.transform.localScale = Vector3.one;
                    instance.transform.position = new Vector3(
                        transform.localScale.x * nodeCoords[i].x,
                        transform.localScale.y * nodeCoords[i].y,
                        z * -CASMEditor.layerSep);

                    SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
                    sr.sprite = displaySprite;
                    sr.color = Color.blue;
                }
                foreach (Vector2Int[] edge in layers[z].navGraph.edgeCoords) {
                    if (edge is null) continue;
                    foreach (Vector2Int ec in edge) {
                        GameObject instance = Instantiate(dis);
                        instance.layer = 6 + z;
                        instance.transform.parent = container.transform;
                        instance.transform.localScale = Vector3.one;
                        instance.transform.position = new Vector3(
                            transform.localScale.x * ec.x,
                            transform.localScale.y * ec.y,
                            z * -CASMEditor.layerSep);

                        SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
                        sr.sprite = displaySprite;
                        sr.color = Color.yellow;
                    }
                }
            }*/

            Destroy(dis);
        }

        public void SetValue(int z, int y, int x, float val) {
            layers[z].InsertValue(y, x, val);
            GameObject go = display[z][y, x];
            go.GetComponent<SpriteRenderer>().color = layers[z].GetDisplayData(y, x);
            go.GetComponent<BoxCollider2D>().enabled = val == -1;
        }

        public Queue<Vector2> LoadLayer(int z, string layerPath, string navPath) {
            Layer l = Layer.LoadLayer(layerPath, navPath);
            layers[z] = l;
            SetDisplay(z);
            return l.GetSpawnLocations();
        }

        public void AdvanceLayersGPU() {
            // Create padded and flattened layer tasks 
            int layerCount = layers.Length;
            Task<float[]>[] tasks = new Task<float[]>[layers.Length];
            for (int z = 0; z < layerCount; z++) {
                int layerIndex = z;
                tasks[layerIndex] = new Task<float[]>(() => {
                    return layers[layerIndex].AsPaddedFlattened();
                });
            }
            // Start tasks in parallel
            Parallel.ForEach(tasks, task => task.Start());
            Task.WaitAll(tasks);

            List<Bleed[]> layerBleed = new List<Bleed[]>();
            // Advance and get layer bleed
            for (int z = 0; z < layerCount; z++) {
                layerBleed.Add(layers[z].AdvanceGPU(computeShader, tasks[z].Result));
            }

            // Apply bleed
            for (int z = 0; z < layerCount; z++) {
                for (int i = 0; i < layerBleed[z].Length; i++) {
                    float val = layerBleed[z][i].val;
                    if (val == -1) {
                        // Null check
                        break;
                    } else if (val < 0.1f) {
                        // SKip if threshold isn't met
                        continue;
                    }
                    int x = layerBleed[z][i].x;
                    int y = layerBleed[z][i].y;

                    // Insert bleed in the above and below layers
                    if (z > 0) {
                        layers[z - 1].InsertValue(y, x, val);
                    }
                    if (z < layerCount - 1) {
                        layers[z + 1].InsertValue(y, x, val);
                    }
                }
            }
        }

        public void UpdateNavGraphs() {
            int layerCount = layers.Length;
            Task[] tasks = new Task[layers.Length];
            for (int z = 0; z < layerCount; z++) {
                int layerIndex = z;
                tasks[layerIndex] = new Task(() => {
                    layers[layerIndex].UpdateNavGraph();
                });
            }
            // Start tasks in parallel
            Parallel.ForEach(tasks, task => task.Start());
            Task.WaitAll(tasks);
        }

        public void UpdateDisplay(int z) {
            for (int y = 0; y < layers[z].h; y++) {
                for (int x = 0; x < layers[z].w; x++) {
                    display[z][y, x].GetComponent<SpriteRenderer>().color = layers[z].GetDisplayData(y, x);
                }
            }
        }
    }
}