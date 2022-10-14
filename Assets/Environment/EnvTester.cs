using System.Collections;
using System.Text;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Assets.Environment {
    public class EnvTester : MonoBehaviour {
        Layer layer;
        GameObject[,] displayLayer;
        GameObject[,] debugLayer;
        int count = 1;

        // Use this for initialization
        void Start() {
            int w = 150, h = 150;
            /*float[,] m = {
                { .0f, .25f, 0.0f },
                { .25f, .50f, .25f },
                { 0.0f, .25f, 0.0f }
            };
            for (int i = 0; i < m.GetLength(0); i++) {
                for (int j = 0; j < m.GetLength(1); j++) {
                    m[i, j] = m[i, j];
                }
            }*/

            float[,] m = {
                { 1f, 1f, 1f },
                { 1f, 0f, 1f },
                { 1f, 1f, 1f }
            };

            displayLayer = new GameObject[w, h];
            debugLayer = new GameObject[w + 2 * (m.GetLength(0) / 2), h + 2 * (m.GetLength(1) / 2)];

            //layer = new Layer(w, h, m, (x, y) => (x + y) / (float)(w + h - 2));
            layer = new Layer(w, h, m, (x, y) => Random.Range(0,2));
            layer.LoopMatrix();
            InitLayerDisplay(Vector2.zero, displayLayer);
            //InitLayerDisplay(new Vector2(30, -2), debugLayer);
            
            //UpdateLayerDisplay(layer.values, debugLayer);
        }

        private void InitLayerDisplay(Vector2 offset, GameObject[,] display) {
            for (int i = 0; i < display.GetLength(0); i++) {
                for (int j = 0; j < display.GetLength(1); j++) {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.position = offset + new Vector2(j, i);                    
                    display[i, j] = go;
                }
            }
        }

        private void UpdateLayerDisplay(float[,] values, GameObject[,] display) {
            for (int i = 0; i < display.GetLength(0); i++) {
                for (int j = 0; j < display.GetLength(1); j++) { 
                    float val = values[i, j];
                    display[i, j].GetComponent<Renderer>().material.color = new Color(val, 1 - val, 0.5f); ;
                }
            }
        }

        // Update is called once per frame
        void Update() {            
            UpdateLayerDisplay(layer.Convolute(), displayLayer);
            /*if (count % 10 == 0) {
                //UpdateLayerDisplay(layer.values, debugLayer);
            }
            count++;*/
        }
    }
}