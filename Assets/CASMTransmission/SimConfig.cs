using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Environment {
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public class SimConfig {
        [JsonProperty]
        public int bleedCount;
        [JsonProperty]
        public float bleedModifier;
        [JsonProperty]
        public float convolveModifier;
        [JsonProperty]
        public bool loadAgentCoords;

        private static SimConfig instance;
        public static SimConfig GetInstance(string path = "") {
            if (instance is null || path != "") {
                if (path == "") {
                    Debug.LogError("Attempted to access environment config without it being loaded");
                    return null;
                }

                try {
                    using (StreamReader sr = new StreamReader(path)) {
                        string cfgJson = sr.ReadToEnd();
                        instance = JsonConvert.DeserializeObject<SimConfig>(cfgJson);
                    }
                } catch (Exception ex) {
                    // TODO: better error handling 
                    Debug.LogError("Error loading environemnt config " + ex.ToString());
                }
            }
            return instance;
        }
        public static void SaveInstance(string path, Formatting format = Formatting.None) {
            if (instance is null) {
                Debug.LogError("Attempted to save null environment config, cancelling");
                return;
            }

            try {
                string cfgJson = JsonConvert.SerializeObject(instance, format);
                using (StreamWriter sw = new StreamWriter(path)) {
                    sw.WriteLine(cfgJson);
                }
            } catch (Exception ex) {
                // TODO: better error handling 
                Debug.LogError("Error saving environemnt config " + ex.ToString());
            }
        }

        [JsonConstructor]
        private SimConfig(int bleedCount, float bleedModifier, float convolveModifier, bool loadAgentCoords) {
            this.bleedCount = bleedCount;
            this.bleedModifier = bleedModifier;
            this.convolveModifier = convolveModifier;
            this.loadAgentCoords = loadAgentCoords;
        }
    }
}
