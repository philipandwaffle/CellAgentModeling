using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using Timer = System.Threading.Timer;

namespace Assets.Environment {
    public class Display : MonoBehaviour {
        [SerializeField] Sprite displaySprite;

        private Layer l;
        public void SetLayer(Layer l) {
            this.l = l;
            SetD(l.w, l.h);
        }

        private GameObject[,] d;

        public void SetD(int w, int h) {
            d = new GameObject[w, h];

            GameObject go = new GameObject();
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = displaySprite;
            
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    d[x, y] = Instantiate(go, offset + new Vector2(x, y), Quaternion.identity);
                }
            }
            Destroy(go);
        }

        private Vector2 offset = Vector2.zero;
        public void SetOffset(Vector2 offset) {
            this.offset = offset;
        }

        public void Play() {
            InvokeRepeating(nameof(ConvolveLayer), 0f, 0.1f);
        }
        public void Pause() {
            CancelInvoke();
        }
        public void LoadLayer() {
            string path = EditorUtility.OpenFilePanel(
                "Load Layer",
                Application.dataPath,
                "txt");
            Layer l = new Layer(path);
            l.SetHoodFn(HoodFunctions.BoundedAvgSpread);
            SetLayer(l);
        }
        public void SaveLayer() {
            string path = EditorUtility.SaveFilePanel(
                "Save Layer",
                Application.dataPath,
                "",
                "txt");
            if (!path.Equals("")) {
                l.SaveValues(path);
            } else {
                Debug.Log("Empty file path, exiting");
            }
        }

        private void ConvolveLayer() {
            UpdateDisplay(l.Convolute());
        }
        private void UpdateDisplay(float[,] values){
            for (int y = 0; y < l.h; y++) {
                for (int x = 0; x < l.w; x++) {
                    d[x, y].GetComponent<SpriteRenderer>().color = GenColor(values[x, y]);
                }
            }
        }

        private Color GenColor(float val) {
            if (val == -1) {
                return new Color(0, 0, 0);
            } else {
                return Color.HSVToRGB(Mathf.Min(val, 1), 1, 1);
            }
        }
    }
}
