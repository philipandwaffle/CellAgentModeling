using System;
using System.Collections;
using UnityEngine;

namespace Assets.Environment {
    public class LayerTicker: MonoBehaviour {
        private Layer layer;
        public Layer GetLayer() {
            return layer;
        }
        public void SetLayer(Layer layer) {
            this.layer = layer;
            SetDisplay();
        }

        [SerializeField] private Sprite displaySprite;

        private GameObject[,] display;

        private void SetDisplay() {
            display = new GameObject[layer.w, layer.h];

            GameObject dis = new GameObject();
            dis.transform.parent = transform;

            for (int i = 0; i < layer.w; i++) {
                for (int j = 0; j < layer.h; j++) {
                    GameObject instance = Instantiate(dis);
                    instance.transform.parent = transform;
                    instance.transform.position = transform.localScale * new Vector2(i, j);
                    instance.name = i + "," + j;

                    SpriteRenderer sr = instance.AddComponent<SpriteRenderer>();
                    sr.sprite = displaySprite;
                    sr.color = layer.GetDisplayData(i, j);

                    display[i, j] = instance;
                }
            }
            Destroy(dis);
        }

        public void SetValue(int x, int y, float val) {
            layer.InsertValue(x, y, val);
            display[x, y].GetComponent<SpriteRenderer>().color = layer.GetDisplayData(x, y);
        }

        public void LoadLayer(string path) {
            layer = Layer.LoadLayer(path);

            SetDisplay();
        }

        public void ClearLayer(float value) {
            layer.Fill(value);
        }

        public void AdvanceLayer() {
            layer.Advance();
            UpdateDisplay();
        }

        private void UpdateDisplay() {
            for (int i = 0; i < layer.w; i++) {
                for (int j = 0; j < layer.h; j++) {
                    display[i, j].GetComponent<SpriteRenderer>().color = layer.GetDisplayData(i, j);
                }
            }
        }
    }
}