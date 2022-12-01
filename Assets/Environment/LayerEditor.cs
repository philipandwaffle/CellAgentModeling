using Assets.UI;
using System;
using System.Collections;
using System.IO;
using Unity.Burst;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Environment {
    public class LayerEditor : MonoBehaviour, IToggleable{
        private LayerTicker ticker;

        private float brushVal = -1;
        [SerializeField] private int brushRadius = 3;
        [SerializeField] private bool paused = true;

        // Use this for initialization
        void Start() {
            string path = Application.dataPath + "/Layers/testing_layer.json";

            float stopVal = -1;
            Layer l = new Layer(
                100,100,
                /*new float[,] {
                    { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f },
                    { -1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, -1f },
                    { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f },
                },*/
                /*new float[,] {
                    { -1f, -1f, -1f, -1f, -1f },
                    { -1f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, -1f },
                    { -1f, 0f, 0f, 0f, -1f },
                    { -1f, -1f, -1f, -1f, -1f },
                },*/
                /*new float[,] {
                    { -1f,-1f, -1f },
                    { -1f, 0f, -1f },
                    { -1f, -1f, -1f },
                },*/
                new float[,] {
                    { 1f, 1f, 1f },
                    { 1f, 1f, 1f },
                    { 1f, 1f, 1f }
                },
                (a, b) => {
                    if (a == stopVal) {
                        a = -a;
                    }                    
                    return Mathf.Clamp(a * b, stopVal, 1f);
                },
                (a, b) => {
                    return Mathf.Clamp(a + b, stopVal, 1f);
                },
                (a) => {
                    if (a == stopVal) {
                        return Color.black;
                    }
                    return Color.HSVToRGB(a, 0.7f, 0.5f);
                },
                (a) => {
                    if (a == stopVal) {
                        return stopVal;
                    }
                    return Mathf.Clamp(a, 0f, 1f);
                }
            );
            l.SetBorder(-1);
            //l.Save(path);

            //Layer<float> l = null;

            ticker = gameObject.GetComponent<LayerTicker>();
            ticker.SetLayer(l);
            //ticker.LoadLayer(path);
        }

        // Update is called once per frame
        void Update() {
            if (!paused) {
                ticker.AdvanceLayer();
            }

            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {                
                // Get the current mouse position in the world
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                float xScale = transform.localScale.x;
                float yScale = transform.localScale.y;
                // Guard clause if mouse is outside of bounds
                if (mPos.x < 0 || mPos.x > ticker.GetLayer().w * xScale || mPos.y < 0 || mPos.y > ticker.GetLayer().h * yScale) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curEditPos = new Vector2Int((int)(mPos.x / xScale), (int)(mPos.y / yScale));
                for (int i = -brushRadius; i < brushRadius + 1; i++) {
                    for (int j = -brushRadius; j < brushRadius + 1; j++) {
                        ticker.SetValue(curEditPos.x + i, curEditPos.y + j, brushVal);
                    }
                }
            }
        }


        public void SetBrushValue(string val) {
            if (!float.TryParse(val, out brushVal)) {
                Debug.LogError("Invalid brush value: " + val);
            }
        }

        public void TogglePaused() {
            paused = !paused;
        }
        public bool GetState() {
            return paused;
        }

        public void LoadLayer() {
            string path = EditorUtility.OpenFilePanel(
                "Load Layer",
                Application.dataPath + "/Layers",
                "json");

            if (path.Equals("")) {
                Debug.Log("Empty file path, exiting");
                return;
            }
            ticker.LoadLayer(path);
        }
        public void SaveLayer() {
            string path = EditorUtility.SaveFilePanel(
                "Save Layer",
                Application.dataPath + "/Layers",
                "",
                "json");
            if (!path.Equals("")) {
                ticker.GetLayer().Save(path);
            } else {
                Debug.Log("Empty file path, exiting");
            }
        }
    }
}