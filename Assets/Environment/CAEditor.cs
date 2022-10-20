﻿using Newtonsoft.Json.Linq;
using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Environment {
    public class CAEditor : MonoBehaviour {
        int w, h;
        float[,] m = {
            {1f, 1f, 1f },
            {1f, 1f, 1f },
            {1f, 1f, 1f }
        };
        CAController cac;
        float curVal;
        private Vector2Int prevMouse = new Vector2Int(-1, -1);

        // Use this for initialization
        void Start() {
            cac = gameObject.GetComponent<CAController>();
        }

        // Update is called once per frame
        void Update() {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
                // Get the current mouse position in the world
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Guard clause if mouse is outside of bounds
                if (mPos.x < 0 || mPos.x > w || mPos.y < 0 || mPos.y > h) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curMouse = new Vector2Int((int)mPos.x, (int)mPos.y);

                // If the mouse has moved
                if (prevMouse != curMouse) {
                    prevMouse = curMouse;
                    SetLayerValue(curMouse.x, curMouse.y);
                }
            }
        }

        public void SetLayerValue(int x, int y) {
            cac.l.SetValue(x, y, curVal);
        }
        public void Play() {
            cac.ResumeSim();
        }
        public void Pause() {
            cac.PauseSim();
        }
        public void Clear() {
            cac.SetLayer(new Layer(w, h, m));
        }
        public void LoadLayer() {
            string path = EditorUtility.OpenFilePanel(
                "Load Layer",
                Application.dataPath,
                "txt");
            Layer l = new Layer(path);
            l.SetHoodFn(HoodFunctions.BoundedAvgSpread);
            cac.SetLayer(l);
        }
        public void SaveLayer() {
            string path = EditorUtility.SaveFilePanel(
                "Save Layer",
                Application.dataPath,
                "",
                "txt");
            if (!path.Equals("")) {
                cac.l.SaveValues(path);
            } else {
                Debug.Log("Empty file path, exiting");
            }
        }
    }
}