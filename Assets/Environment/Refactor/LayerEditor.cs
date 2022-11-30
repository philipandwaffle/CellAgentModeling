using Assets.UI;
using System;
using System.Collections;
using System.IO;
using Unity.Burst;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Environment.Refactor {
    public class LayerEditor : MonoBehaviour, IToggleable{
        private LayerTicker ticker;

        private float brushVal = -1;
        private Vector2Int editPos = new Vector2Int(-1, -1);
        [SerializeField] private bool paused = true;

        // Use this for initialization
        void Start() {
            string path = Application.dataPath + "/Layers/small_layer.json";

            float c = 9f;
            float stopVal = -1;
            Layer<float> l = new Layer<float>(
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
                new float[,] {
                    { -1f,-1f, -1f },
                    { -1f, 0f, -1f },
                    { -1f, -1f, -1f },
                },
                new float[,] {
                    { 1f/c, 1f/c, 1f/c },
                    { 1f/c, 1f/c, 1f/c },
                    { 1f/c, 1f/c, 1f/c }
                },
                stopVal,
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
                    return Color.HSVToRGB(a, 1, 1);
                },
                (a) => {
                    if (a == stopVal) {
                        return stopVal;
                    }
                    return Mathf.Clamp(a, 0f, 1f);
                }
            );
            l.Save(path);

            //Layer<float> l = null;

            ticker = gameObject.GetComponent<LayerTicker>();            
            ticker.LoadLayer(path);
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
                if (mPos.x < 0 || mPos.x > ticker.layer.w * xScale || mPos.y < 0 || mPos.y > ticker.layer.h * yScale) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curEditPos = new Vector2Int((int)(mPos.x / xScale), (int)(mPos.y / yScale));

                editPos = curEditPos;
                ticker.SetValue(curEditPos.x, curEditPos.y, brushVal);

                /*// If the mouse has moved
                if (editPos != curEditPos) {
                    //ticker.layer[curEditPos.x, curEditPos.y] = brushVal;
                }*/
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
                ticker.layer.Save(path);
            } else {
                Debug.Log("Empty file path, exiting");
            }
        }
    }
}