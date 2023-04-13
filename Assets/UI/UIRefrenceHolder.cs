using Assets.Environment;
using SFB;
using System.IO;
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
        public void LoadSim() {
            string[] path = StandaloneFileBrowser.OpenFolderPanel(
                "Load Layer",
                Application.dataPath + "/Layers",
                false);

            if (path.Length == 0) {
                Debug.Log("Folder panel exited, cancelling load");
                return;
            }
            DirectoryInfo environment = new DirectoryInfo(path[0]);

            casm.LoadSim(environment);
        }
        public void SaveSim() {
            string[] path = StandaloneFileBrowser.OpenFolderPanel(
                "Load Layer",
                Application.dataPath + "/Layers",
                false);

            if (path.Length == 0) {
                Debug.Log("Folder panel exited, cancelling save");
                return;
            }

            casm.SaveSim(path[0]);
        }
    }
}
