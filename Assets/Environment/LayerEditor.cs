using Assets.UI;
using System;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Assets.Environment {
    public class LayerEditor : MonoBehaviour, IToggleable{
        private LayerTicker ticker;

        private float brushVal = -1;
        private int z = 0;
        private int numLayers = 0;
        [SerializeField] private int brushRadius = 3;
        [SerializeField] private bool paused = true;
        [SerializeField] private int w = 100, h = 100;
        [SerializeField] private TMP_Text text;

        public static int layerSep = 50;

        // Use this for initialization
        void Start() {
            // Camera setup
            Vector3 newPos = Camera.main.transform.position;
            newPos.z = z * Camera.main.transform.localScale.z * -layerSep;
            newPos.z -= layerSep/2;
            Camera.main.transform.position = newPos;

            ticker = gameObject.GetComponent<LayerTicker>();

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
                ticker.AdvanceLayersGPU();
                ticker.UpdateDisplay(z);

                if (navGraphUpdates % 10 == 0) { 
                    ticker.UpdateNavGraphs();
                    navGraphUpdates = 0;
                }
                navGraphUpdates++;
            }

            if (Input.GetMouseButton(0)) {                
                // Get the current mouse position in the world
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                float xScale = transform.localScale.x;
                float yScale = transform.localScale.y;
                // Guard clause if mouse is outside of bounds
                if (mPos.x < 0 || mPos.x > ticker.GetLayer(z).w * xScale || mPos.y < 0 || mPos.y > ticker.GetLayer(z).h * yScale) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curEditPos = new Vector2Int((int)(mPos.x / xScale), (int)(mPos.y / yScale));
                for (int x = -brushRadius; x < brushRadius + 1; x++) {
                    for (int y = -brushRadius; y < brushRadius + 1; y++) {
                        ticker.SetValue(z, curEditPos.y + y, curEditPos.x + x, brushVal);
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
            //Debug.Log("On Layer: " + z + "\nUsing mask:"+mask.ToBinaryString());
            ticker.UpdateDisplay(z);
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
            /*string path = EditorUtility.OpenFilePanel(
                "Load Layer",
                Application.dataPath + "/Layers",
                "json");

            if (path.Equals("")) {
                Debug.Log("Empty file path, exiting");
                return;
            }
            ticker.LoadLayer(z, path);*/
        }
        public void SaveLayer() {
            /*string path = EditorUtility.SaveFilePanel(
                "Save Layer",
                Application.dataPath + "/Layers",
                "",
                "layer");
            if (!path.Equals("")) {
                ticker.GetLayer(z).Save(path);
            } else {
                Debug.Log("Empty file path, exiting");
            }*/
        }
        public void LoadLayers() {
            string path = EditorUtility.OpenFolderPanel(
                "Load Layer",
                Application.dataPath + "/Layers",
                "");

            if (path.Equals("")) {
                Debug.Log("Empty folder path, exiting");
                return;
            }
            DirectoryInfo d = new DirectoryInfo(path);
            //DirectoryInfo d = new DirectoryInfo("C:\\Users\\phili\\Documents\\code_stuff\\unity\\CellAgentModeling\\Assets\\Layers\\grenfell_110x110");
            FileInfo[] layerFiles = d.GetFiles("*.layer");
            FileInfo[] navFiles = d.GetFiles("*.nav");

            LoadLayers(layerFiles, navFiles);
        }

        private void LoadLayers(FileInfo[] layerFiles, FileInfo[] navFiles) {
            int numNavs = navFiles.Length;
            int numLayers = layerFiles.Length;

            if (numNavs != numLayers) {
                Debug.LogError(".layer and .nav file count mismatch");
                return;
            }else if (numLayers == 0) Debug.LogError("No layers were loaded");

            this.numLayers = numLayers;
            ticker.SetNumLayers(numLayers);

            for (int i = 0; i < numLayers; i++) {
                string layerPath = layerFiles[i].FullName;
                string navPath = navFiles[i].FullName;
                Debug.Log("loading layer: " + i + " <- " + layerPath);
                Debug.Log("loading nav: " + i + " <- " + navPath);

                ticker.LoadLayer(i, layerPath, navPath);
            }
        }
        public void SaveLayers() {
            /*string path = EditorUtility.OpenFolderPanel(
                "Load Layer",
                Application.dataPath + "/Layers",
                "");

            if (path.Equals("")) {
                Debug.Log("Empty folder path, exiting");
                return;
            }        
            for (int i = 0; i < ticker.GetLayerCount(); i++) {

                string name = path + "/" + i + ".layer";
                Debug.Log("saving layer: " + i + " -> " + name);
                ticker.GetLayer(i).Save(name);
            }*/
        }
    }
}