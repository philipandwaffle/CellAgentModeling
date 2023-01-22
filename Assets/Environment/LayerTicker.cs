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

        public int GetLayerCount() {
            return layers.Length;
        }
        public void SetNumLayers(int z) {
            layers = new Layer[z];

            // Check if the display has been set before
            if (display != null) {

                // destroy the previous environment
                for (int curLayer = 0; curLayer < z; curLayer++) {
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
        }

        public Layer GetLayer(int z) {
            return layers[z];
        }
        public void SetLayer(int z, Layer layer) {
            layers[z] = layer;
            SetDisplay(z);
        }

        private void SetDisplay(int z) {
            Debug.Log("attempting to set layer " + z);

            GameObject dis = new GameObject();
            dis.transform.parent = transform;
            display[z] = new GameObject[layers[z].w, layers[z].h];

            for (int x = 0; x < layers[z].w; x++) {
                for (int y = 0; y < layers[z].h; y++) {
                    GameObject instance = Instantiate(dis);
                    instance.layer = 6+z;

                    instance.transform.parent = transform;
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

        public void AdvanceLayers() {
            for (int z = layers.Length-1; z >= 0; z--) {
                List<(int, int, float)> layerBleed = layers[z].Advance();
                if (z != 0) {
                    for (int i = 0; i < layerBleed.Count; i++) {
                        int x = layerBleed[i].Item1;
                        int y = layerBleed[i].Item2;
                        float val = layerBleed[i].Item3;
                        if (val == -2) {
                            continue;
                        }
                        layers[z - 1].InsertValue(x, y, val);                        
                    }
                }
            }
            UpdateDisplays();
        }

        public void AdvanceLayersParallel() {
            
            Task<List<(int, int, float)>>[] tasks = new Task<List<(int, int, float)>>[layers.Length];
            for (int z = layers.Length - 1; z >= 0; z--) {
                int layerIndex = z;
                tasks[layerIndex] = new Task<List<(int, int, float)>>(() => {
                    return layers[layerIndex].Advance(); 
                });
            }

            Parallel.ForEach(tasks, task => task.Start());
            Task.WaitAll(tasks);

            for (int z = tasks.Length - 1; z > 0; z--) {
                List<(int, int, float)> layerBleed = tasks[z].Result;

                for (int i = 0; i < layerBleed.Count; i++) {
                    int x = layerBleed[i].Item1;
                    int y = layerBleed[i].Item2;
                    float val = layerBleed[i].Item3;
                    if (val == -2) {
                        continue;
                    }
                    layers[z - 1].InsertValue(x, y, val);
                }                
            }
            UpdateDisplays();
        }

        private void UpdateDisplays() {
            for (int z = 0; z < layers.Length; z++) {
                for (int x = 0; x < layers[z].w; x++) {
                    for (int y = 0; y < layers[z].h; y++) {
                        /* GameObject go = display[z][x, y];
                         SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                         BoxCollider2D bc = go.GetComponent<BoxCollider2D>();

                         bc.enabled = layers[z][x, y] == -1;
                         sr.color = layers[z].GetDisplayData(x, y);*/
                        display[z][x, y].GetComponent<SpriteRenderer>().color = layers[z].GetDisplayData(x, y);
                    }
                }
            }
        }
    }
}