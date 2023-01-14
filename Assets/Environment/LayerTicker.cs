using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Assets.Environment {
    public class LayerTicker: MonoBehaviour {
        private Layer[] layers;
        public int GetLayerCount() {
            return layers.Length;
        }
        public void SetNumLayers(int z) {
            layers = new Layer[z];
            display = new GameObject[z][,];
        }

        public Layer GetLayer(int z) {
            return layers[z];
        }
        public void SetLayer(int z, Layer layer) {
            layers[z] = layer;
            display[z] = new GameObject[layers[z].w, layers[z].h];
            SetDisplay(z);
        }

        [SerializeField] private Sprite displaySprite;

        private GameObject[][,] display;

        private void SetDisplay(int z) {
            GameObject dis = new GameObject();
            dis.transform.parent = transform;
            display[z] = new GameObject[layers[z].w, layers[z].h];

            for (int x = 0; x < layers[z].w; x++) {
                for (int y = 0; y < layers[z].h; y++) {
                    GameObject instance = Instantiate(dis);
                    //instance.transform.localScale = transform.localScale;
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
            display[z][x, y].GetComponent<SpriteRenderer>().color = layers[z].GetDisplayData(x, y);
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