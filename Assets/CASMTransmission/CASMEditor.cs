using Assets.Agent;
using Assets.UI;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace Assets.Environment {
    public class CASMEditor : MonoBehaviour, IToggleable {
        [SerializeField] private EnvironmentController environmentController;
        [SerializeField] private AgentController agentController;

        private float brushVal = -1;
        private int z = 0;
        private int numLayers = 0;
        [SerializeField] private int brushRadius = 3;
        [SerializeField] private bool paused = true;
        [SerializeField] private TMP_Text text;

        public static int layerSep = 2;

        private void Awake() {
            if (environmentController is null) Debug.LogError("An environment controller hasn't been assigned to the editor");
            if (agentController is null) Debug.LogError("An agent controller hasn't been assigned to the editor");
        }

        // Use this for initialization
        void Start() {
            // Camera setup
            Vector3 newPos = Camera.main.transform.position;
            newPos.z = -layerSep / 2f;
            Camera.main.transform.position = newPos;

            // Load default layers
            string path = Application.dataPath + "/Layers/default/";
            
            Debug.Log(path);
            DirectoryInfo environment = new DirectoryInfo(path);
            LoadSim(environment);
        }

        int navGraphUpdates = 0;
        // Update is called once per frame
        void Update() {
            if (!paused) {
                if (navGraphUpdates % 10 == 0) {
                    environmentController.UpdateNavGraphs();
                    navGraphUpdates = 0;
                }
                navGraphUpdates++;

                environmentController.AdvanceLayersGPU();
                agentController.AdvanceSensors();

                environmentController.UpdateDisplay(z);
            }

            if (Input.GetMouseButton(0)) {
                // Get the current mouse position in the world
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                float xScale = transform.localScale.x;
                float yScale = transform.localScale.y;
                // Guard clause if mouse is outside of bounds
                if (mPos.x < 0 || mPos.x > environmentController.GetLayer(z).w * xScale || mPos.y < 0 || mPos.y > environmentController.GetLayer(z).h * yScale) {
                    return;
                }

                // Convert mouse position to an integer
                Vector2Int curEditPos = new Vector2Int((int)(mPos.x+0.5 / xScale), (int)(mPos.y+0.5/ yScale));
                for (int x = -brushRadius; x < brushRadius + 1; x++) {
                    for (int y = -brushRadius; y < brushRadius + 1; y++) {
                        environmentController.SetValue(z, curEditPos.y + y, curEditPos.x + x, brushVal);
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
            environmentController.UpdateDisplay(z);
        }


        public void SetBrushValue(float val) {
            brushVal = val;
        }

        public void TogglePaused() {
            paused = !paused;
            if (paused) {
                agentController.Pause();
            }else {
                agentController.Play();
            }
        }
        public bool GetState() {
            return paused;
        }

        public void LoadSim(DirectoryInfo environment) {
            List<FileInfo> navFiles = new List<FileInfo>();
            List<FileInfo> layerFiles = new List<FileInfo>();

            DirectoryInfo[] dirs = environment.GetDirectories();
            numLayers = dirs.Length;

            bool validEnv = true;

            // Load environment config
            FileInfo[] envCfg = environment.GetFiles("*.cfg");
            if (envCfg.Length != 1) {
                validEnv = false;
                Debug.LogError("There should be only 1 .cfg file in " + environment.FullName);
            } else {
                SimConfig.GetInstance(envCfg[0].FullName);
            }

            // Load layer and nav files
            for (int i = 0; i < numLayers; i++) {
                FileInfo[] nav = dirs[i].GetFiles("*.nav");
                FileInfo[] layer = dirs[i].GetFiles("*.layer");

                if(nav.Length != 1 || layer.Length != 1) {
                    validEnv = false;
                    Debug.LogError("There should be only 1 .layer and .nav file in " + dirs[i].FullName);
                    continue;
                }

                navFiles.Add(nav[0]);
                layerFiles.Add(layer[0]);
            }
            environmentController.SetNumLayers(numLayers);

            // Check if agent positions are to be loaded from a file or not
            if (SimConfig.GetInstance().loadAgentCoords) {
                // Load agent positions
                FileInfo[] agentCoords = environment.GetFiles("*.coords");
                if (envCfg.Length != 1) {
                    validEnv = false;
                    Debug.LogError("There should be only 1 .coords file in " + environment.FullName);
                } else {
                    AgentCoords ac = AgentCoords.Load(agentCoords[0].FullName);

                    for (int i = 0; i < numLayers; i++) {
                        string layerPath = layerFiles[i].FullName;
                        string navPath = navFiles[i].FullName;
                        Debug.Log("loading layer: " + i + " <- " + layerPath);
                        Debug.Log("loading nav: " + i + " <- " + navPath);

                        environmentController.LoadLayer(i, layerPath, navPath);
                    }
                    agentController.SetSpawnLocations(ac.coords);
                }

            } else {
                // Calc valid spawn locations for each layer
                Queue<Vector2>[] spawnsLocations = new Queue<Vector2>[numLayers];
                for (int i = 0; i < numLayers; i++) {
                    string layerPath = layerFiles[i].FullName;
                    string navPath = navFiles[i].FullName;
                    Debug.Log("loading layer: " + i + " <- " + layerPath);
                    Debug.Log("loading nav: " + i + " <- " + navPath);

                    spawnsLocations[i] = environmentController.LoadLayer(i, layerPath, navPath);
                }
                agentController.SetSpawnLocations(spawnsLocations);
            }

            if (!validEnv) {
                Debug.LogError("There are problems with loading the simultion, loaded as much as possible anyway but issues may occur");
            }
        }

        public void SaveSim(string path) {
            // Clearing old dir if exists
            if (Directory.Exists(path)) Directory.Delete(path, true);
            Directory.CreateDirectory(path);

            // Save environment config
            SimConfig.SaveInstance(path + "/settings.cfg");

            // Save agent coords
            agentController.SaveAgentPos(path + "/agents.coords");

            // Save each .layer and .nav files
            for (int i = 0; i < environmentController.GetNumLayers(); i++) {
                string curPath = path + "/" + i;

                Directory.CreateDirectory(curPath);
                string layerPath = curPath + "/" + i + ".layer";
                string navpath = curPath + "/" + i + ".nav";
                Debug.Log("saving layer: " + i + " -> " + layerPath);
                Debug.Log("saving nav: " + i + " -> " + navpath);

                environmentController.GetLayer(i).Save(layerPath, navpath);
            }
        }
    }
}