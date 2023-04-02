using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static System.Windows.Forms.DataFormats;

namespace Assets.Environment {
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public class EnvironmentConfig {
        [JsonProperty]
        public int bleedCount;
        [JsonProperty]
        public float bleedModifier;
        [JsonProperty]
        public float convolveModifier;

        private static EnvironmentConfig instance;
        public static EnvironmentConfig GetInstance(string path = "") {
            if (instance is null) {
                if (path == "") {
                    Debug.LogError("Attempted to access environment config without it being loaded");
                    return null;
                }

                try {
                    using (StreamReader reader = new StreamReader(path)) {
                        string cfgJson = reader.ReadToEnd();
                        instance = JsonConvert.DeserializeObject<EnvironmentConfig>(cfgJson);
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
                using (StreamWriter writer = new StreamWriter(path)) {
                    writer.WriteLine(cfgJson);
                }
            } catch (Exception ex) {
                // TODO: better error handling 
                Debug.LogError("Error saving environemnt config " + ex.ToString());
            }
        }

        [JsonConstructor]
        private EnvironmentConfig(int bleedCount, float bleedModifier, float convolveModifier) {
            this.bleedCount = bleedCount;
            this.bleedModifier = bleedModifier;
            this.convolveModifier = convolveModifier;
        }
    }
}
