using System;
using System.Collections;
using UnityEngine;

namespace Assets.Environment {
    public class LayerTicker: MonoBehaviour {
        private Layer[] layers;
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

            for (int x = 0; x < layers[z].w; x++) {
                for (int y = 0; y < layers[z].h; y++) {
                    GameObject instance = Instantiate(dis);
                    instance.transform.parent = transform;
                    instance.transform.position = new Vector3(
                        transform.localScale.x * x, 
                        transform.localScale.y * y, 
                        transform.localScale.z * z * -10);
                    instance.name = z + "" + x + "," + y;

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

        public void ClearLayer(int z, float value) {
            layers[z].Fill(value);
        }

        public void AdvanceLayers() {
            for (int z = 0; z < layers.Length; z++) {
                layers[z].Advance();
            }
            UpdateDisplays();
        }

        private void UpdateDisplays() {
            for (int z = 0; z < layers.Length; z++) {
                for (int x = 0; x < layers[z].w; x++) {
                    for (int y = 0; y < layers[z].h; y++) {
                        display[z][x, y].GetComponent<SpriteRenderer>().color = layers[z].GetDisplayData(x, y);
                    }
                }
            }
        }
    }
}