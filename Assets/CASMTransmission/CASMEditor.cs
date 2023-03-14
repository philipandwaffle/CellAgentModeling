using Assets.Agent;
using Assets.UI;
using System;
using System.IO;
using TMPro;
using UnityEngine;

namespace Assets.Environment {
    public class CASMEditor : MonoBehaviour, IToggleable {
        [SerializeField] private LayerTicker layerTicker;
        [SerializeField] private AgentTicker agentTicker;

        private float brushVal = -1;
        private int z = 0;
        private int numLayers = 0;
        [SerializeField] private int brushRadius = 3;
        [SerializeField] private bool paused = true;
        [SerializeField] private TMP_Text text;

        public static int layerSep = 50;

        private void Awake() {
            if (layerTicker is null) Debug.LogError("A layer ticker hasn't been assigned to the editor");
        }

        // Use this for initialization
        void Start() {
            // Camera setup
            Vector3 newPos = Camera.main.transform.position;
            newPos.z = z * Camera.main.transform.localScale.z * -layerSep;
            newPos.z -= layerSep / 2;
            Camera.main.transform.position = newPos;

            // Load default layers
            string path = Application.dataPath + "/Layers/default/";
            //string path = Application.dataPath + "/Layers/single_grenfell_53x53/";
            Debug.Log(path);
            DirectoryInfo d = new DirectoryInfo(path);
            LoadLayers(d.GetFiles("*.layer"), d.GetFiles("*.nav"));
        }

        int navGraphUpdates = 0;
        // Update is called once per frame
        void Update() {
            if (!paused) {
                if (navGraphUpdates % 10 == 0) {
                    layerTicker.UpdateNavGraphs();
                    navGraphUpdates = 0;
                }
                navGraphUpdates++;

                layerTicker.AdvanceLayersGPU();
                agentTicker.AdvanceSensors();

                layerTicker.UpdateDisplay(z);
            }

            if (Input.GetMouseButton(0)) {
                // Get the current mouse position in the world
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                float xScale = transform.localScale.x;
                float yScale = transform.localScale.y;
                // Guard clause if mouse is outside of bounds
                if (mPos.x < 0 || mPos.x > layerTicker.GetLayer(z).w * xScale || mPos.y < 0 || mPos.y > layerTicker.GetLayer(z).h * yScale) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curEditPos = new Vector2Int((int)(mPos.x / xScale), (int)(mPos.y / yScale));
                for (int x = -brushRadius; x < brushRadius + 1; x++) {
                    for (int y = -brushRadius; y < brushRadius + 1; y++) {
                        layerTicker.SetValue(z, curEditPos.y + y, curEditPos.x + x, brushVal);
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.W)) {
                MoveLayer(-1);
            } else if (Input.GetKeyUp(KeyCode.S)) {
                MoveLayer(1);
            }
        }


        private void MoveLayer(int deltaZ) {
            z = Math.Clamp(z + deltaZ, 0, numLayers - 1);

            Vector3 newPos = Camera.main.transform.position;
            newPos.z = z * (Camera.main.transform.localScale.z * -layerSep);
            newPos.z -= 5;

            text.text = "Layer: " + z;

            Camera.main.transform.position = newPos;
            int mask = 1 << (6 + z);
            Camera.main.cullingMask = mask;
            layerTicker.UpdateDisplay(z);
        }


        public void SetBrushValue(float val) {
            brushVal = val;
        }

        public void TogglePaused() {
            paused = !paused;
        }
        public bool GetState() {
            return paused;
        }

        public void LoadLayer(string layerpath, string navPath) {
            layerTicker.LoadLayer(z, layerpath, navPath);
        }
        public void SaveLayer(string layerpath, string navPath) {
            layerTicker.GetLayer(z).Save(layerpath, navPath);
        }

        public void LoadLayers(FileInfo[] layerFiles, FileInfo[] navFiles) {
            int numNavs = navFiles.Length;
            int numLayers = layerFiles.Length;

            if (numNavs != numLayers) {
                Debug.LogError(".layer and .nav file count mismatch");
                return;
            } else if (numLayers == 0) Debug.LogError("No layers were loaded");

            this.numLayers = numLayers;
            layerTicker.SetNumLayers(numLayers);

            for (int i = 0; i < numLayers; i++) {
                string layerPath = layerFiles[i].FullName;
                string navPath = navFiles[i].FullName;
                Debug.Log("loading layer: " + i + " <- " + layerPath);
                Debug.Log("loading nav: " + i + " <- " + navPath);

                layerTicker.LoadLayer(i, layerPath, navPath);
            }
        }
        public void SaveLayers(string path) {
            for (int i = 0; i < layerTicker.GetNumLayers(); i++) {
                string layerPath = path + "/" + i + ".layer";
                string navpath = path + "/" + i + ".layer";
                Debug.Log("saving layer: " + i + " -> " + layerPath);
                Debug.Log("saving nav: " + i + " -> " + navpath);

                layerTicker.GetLayer(i).Save(layerPath, navpath);
            }
        }
    }
}