using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Environment {
    public class EnvTester : MonoBehaviour {
        private Layer layer;
        [SerializeField] Sprite displaySprite;
        private GameObject[,] displayLayer;
        private GameObject[,] debugLayer;

        private int w = 100, h = 100;
        private float[,] m = {
            { 1f, 1f, 1f },
            { 1f, 1.05f, 1f },
            { 1f, 1f, 1f }
        };

        // Use this for initialization
        void Start() {
            /*float[,] m = {
                { .0f, .25f, 0.0f },
                { .25f, .50f, .25f },
                { 0.0f, .25f, 0.0f }
            };*/

            
            for (int i = 0; i < m.GetLength(0); i++) {
                for (int j = 0; j < m.GetLength(1); j++) {
                    m[i, j] = m[i, j] / 9;
                }
            }
            

            displayLayer = new GameObject[w, h];
            debugLayer = new GameObject[w + 2 * (m.GetLength(0) / 2), h + 2 * (m.GetLength(1) / 2)];

            //layer = new Layer(w, h, m, (x, y) => (x + y) / (float)(w + h - 2));
            //layer = new Layer(w, h, m, (x, y) => Random.Range(0,2));

            layer = new Layer(w, h, m, (x, y) => {
                if (x >= 45 && x <= 55 && y >= 45 && y <= 55) {
                    return 1;
                } else if (
                x == 10 || x == 20 || x == 30 || x == 40 || x == 50 || x == 60 || x == 70 || x == 80 || x == 90 ||
                y == 10 || y == 20 || y == 30 || y == 40 || y == 50 || y == 60 || y == 70 || y == 80 || y == 90
                ) {
                    return -1;
                } else {
                    return 0;
                }
            });
            layer.LoopMatrix();
            InitLayerDisplay(Vector2.zero, displayLayer);

            //InitLayerDisplay(new Vector2(30, -2), debugLayer);
            //UpdateLayerDisplay(layer.values, debugLayer);
            layer = new Layer("test.txt");

            layer.SetHoodFn(HoodFunctions.BoundedAvgSpread);
            InvokeRepeating(nameof(Convolve), 0,0.1f);
            //layer.SaveValues("test.txt");
        }

        private void InitLayerDisplay(Vector2 offset, GameObject[,] display) {
            for (int i = 0; i < display.GetLength(0); i++) {
                for (int j = 0; j < display.GetLength(1); j++) {
                    GameObject go = new GameObject();
                    SpriteRenderer sr =  go.AddComponent<SpriteRenderer>();
                    sr.sprite = displaySprite;
                    go.transform.position = offset + new Vector2(j, i);                    
                    display[j, i] = go;
                }
            }
        }

        private void UpdateLayerDisplay(float[,] values, GameObject[,] display) {
            for (int i = 0; i < display.GetLength(0); i++) {
                for (int j = 0; j < display.GetLength(1); j++) { 
                    float val = values[i, j];
                    if (val < 0) {
                        display[i, j].GetComponent<SpriteRenderer>().material.color = new Color(0, 0, 0);                        
                    } else {
                        display[i, j].GetComponent<SpriteRenderer>().material.color = Color.HSVToRGB(Mathf.Min(val, 1), 1, 1);
                    }
                }
            }
        }

        private void MaskConvolve() {
            UpdateLayerDisplay(layer.MaskConvolute(), displayLayer);
        }
        private void Convolve() {
            UpdateLayerDisplay(layer.Convolute(), displayLayer);
        }
    }
}

