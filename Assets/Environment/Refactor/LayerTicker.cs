using System;
using System.Collections;
using UnityEngine;

namespace Assets.Environment.Refactor {
    public class LayerTicker: MonoBehaviour {
        public Layer<float> layer;

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

        public void LoadLayer(string path) {
            layer = Layer<float>.LoadLayer(path);

            SetDisplay();
        }

        public void ClearLayer(int w, int h) {
            layer = new Layer<float>()
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