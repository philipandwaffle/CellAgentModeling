using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Environment {
    public class LayerTicker: MonoBehaviour {
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
                for (int curLayer = 0; curLayer < z; curLayer++) {
                    // Destroy container 
                    Destroy(displayContainers[curLayer]);

                    for (int row = 0; row < display[curLayer].GetLength(0); row++) {
                        for (int col = 0; col < display[curLayer].GetLength(1); col++) {
                            // Destroy cell
                            Destroy(display[curLayer][row, col]);
                        }
                    }
                }
            }

            // Set the lenght of the display
            display = new GameObject[z][,];
            displayContainers = new GameObject[z];
        }

        public Layer GetLayer(int z) {
            return layers[z];
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
            GameObject dis = new GameObject();
            display[z] = new GameObject[layers[z].w, layers[z].h];

            for (int x = 0; x < layers[z].w; x++) {
                for (int y = 0; y < layers[z].h; y++) {
                    GameObject instance = Instantiate(dis);
                    instance.layer = 6 + z;
                    instance.transform.parent = container.transform;
                    instance.transform.localScale = Vector3.one;
                    instance.transform.position = new Vector3(
                        transform.localScale.x * x, 
                        transform.localScale.y * y, 
                        z * -LayerEditor.layerSep);
                    instance.name = z + " " + x + "," + y;

                    BoxCollider2D bc = instance.AddComponent<BoxCollider2D>();
                    bc.enabled = layers[z][x, y] == -1;

                    SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
                    sr.sprite = displaySprite;
                    sr.color = layers[z].GetDisplayData(x, y);

                    display[z][x, y] = instance;
                }
            }
            Destroy(dis);
        }

        public void SetValue(int z, int x, int y, float val) {
            layers[z].InsertValue(x, y, val);
            GameObject go = display[z][x, y];
            go.GetComponent<SpriteRenderer>().color = layers[z].GetDisplayData(x, y);
            go.GetComponent<BoxCollider2D>().enabled = val == -1;
        }

        public void LoadLayer(int z, string path) {            
            layers[z] = Layer.LoadLayer(path);
            SetDisplay(z);
        }

        public void AdvanceLayersGPU() {
            List<Bleed[]> layerBleed = new List<Bleed[]>();
            for (int z = 0; z < layers.Length; z++) {
                layerBleed.Add(layers[z].AdvanceGPU(computeShader));
            }

            for (int z = layerBleed.Count - 1; z > 0; z--) {
                Debug.Log("Looking at layer: " + z);
                for (int i = 0; i < layerBleed[z].Length; i++) {
                    float val = layerBleed[z][i].val;
                    if (val == -1) {
                        break;
                    }
                    int x = layerBleed[z][i].x;
                    int y = layerBleed[z][i].y;


                    layers[z - 1].InsertValue(x, y, val);
                }
            }
        }

        public void UpdateDisplay(int z) {
            for (int x = 0; x < layers[z].w; x++) {
                for (int y = 0; y < layers[z].h; y++) {
                    display[z][x, y].GetComponent<SpriteRenderer>().color = layers[z].GetDisplayData(x, y);
                }
            }
        }
    }
}