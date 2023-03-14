using Assets.Environment;
using Newtonsoft.Json.Serialization;
using SFB;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Assets.UI {
    public class UIRefrenceHolder : MonoBehaviour {
        [SerializeField] private CASMEditor casm;
        [SerializeField] 

        public void SetBrushVal(string val) {
            float brushVal;
            if (!float.TryParse(val, out brushVal)) {
                Debug.LogError("Invalid brush value: " + val);
                return;
            }
            casm.SetBrushValue(brushVal);
        }
        public void TogglePaused() {
            casm.TogglePaused();
        }
        public void LoadLayers() {
            string[] path = StandaloneFileBrowser.OpenFolderPanel(
                "Load Layer",
                Application.dataPath + "/Layers",
                false);

            if (path.Length == 0) {
                Debug.Log("Folder panel exited, cancelling load");
                return;
            }
            DirectoryInfo d = new DirectoryInfo(path[0]);
            FileInfo[] layerFiles = d.GetFiles("*.layer");
            FileInfo[] navFiles = d.GetFiles("*.nav");

            casm.LoadLayers(layerFiles, navFiles);
        }
        public void SaveLayers() {
            string[] path = StandaloneFileBrowser.OpenFolderPanel(
                "Load Layer",
                Application.dataPath + "/Layers",
                false);

            if (path.Length == 0) {
                Debug.Log("Folder panel exited, cancelling save");
                return;
            }

            casm.SaveLayers(path[0]);
        }
    }
}
