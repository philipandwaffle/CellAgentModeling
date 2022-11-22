using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Assets.Environment {
    public class CAEditor : MonoBehaviour {
        [SerializeField] int w, h;
        float[,] m = {
            {1f, 1f, 1f },
            {1f, 1f, 1f },
            {1f, 1f, 1f }
        };
        CAController cac;
        float brushVal = -1;
        private Vector2Int prevMouse = new Vector2Int(-1, -1);

        // Use this for initialization
        void Start() {
            cac = gameObject.GetComponent<CAController>();
            cac.SetLayer(new Layer(w, h, m));

            Refactor.Layer<float> l = new Refactor.Layer<float>(
                new float[,] { { 1f, 2f, 3f }, { 1f, 2f, 3f }, { 1f, 2f, 3f } },
                new float[,] { { 1f, 2f, 3f }, { 1f, 2f, 3f }, { 1f, 2f, 3f } },
                (a,b) => {
                    return a * b;
                }
            );
            l.Save(Application.dataPath + "/Layers/test.json");

            MyClass<float> mc = new MyClass<float>(1, 1f, "1");
            string json = JsonUtility.ToJson(mc);
            Debug.Log(json);
        }

        // Update is called once per frame
        void Update() {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
                // Get the current mouse position in the world
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Guard clause if mouse is outside of bounds
                if (mPos.x < 0 || mPos.x > w * cac.scale || mPos.y < 0 || mPos.y > h * cac.scale) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curMouse = new Vector2Int((int)(mPos.x / cac.scale), (int)(mPos.y / cac.scale));

                // If the mouse has moved
                if (prevMouse != curMouse) {
                    prevMouse = curMouse;
                    SetLayerValue(curMouse.x, curMouse.y);
                }
            }
        }
        private void SetLayerValue(int x, int y) {
            cac.SetPoint(x, y, brushVal);
        }

        public void SetBrushValue(string value) {
            Debug.Log(value);
            float val;
            if (float.TryParse(value, out val)) {
                brushVal = val;
            }
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
                Application.dataPath + "/Layers",
                "txt");

            if (path.Equals("")) {
                Debug.Log("Empty file path, exiting");
                return;
            }

            Layer l = new Layer(path);
            l.SetHoodFn(HoodFunctions.BoundedAvgSpread);
            cac.SetLayer(l);
        }
        public void SaveLayer() {
            string path = EditorUtility.SaveFilePanel(
                "Save Layer",
                Application.dataPath + "/Layers",
                "",
                "txt");
            if (!path.Equals("")) {
                cac.l.SaveValues(path);
            } else {
                Debug.Log("Empty file path, exiting");
            }
        }
    }

    [Serializable]
    public class MyClass<T> {
        public int level;
        public float timeElapsed;
        public string playerName;
        public int[] twoD = new int[] { 1, 2, 3 };

        public MyClass(int level, float timeElapsed, string playerName) {
            this.level = level;
            this.timeElapsed = timeElapsed;
            this.playerName = playerName;
        }
    }
}