using Assets.Agent.Sensors;
using Assets.Environment;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Agent {
    [JsonObject(MemberSerialization.OptIn)]
    [Serializable]
    public class AgentCoords {

        [JsonProperty]
        public SerializableVector3[] coords;
        
        public AgentCoords(BaseSensor[] sensors) { 
            coords = new SerializableVector3[sensors.Length];
            for (int i = 0; i < sensors.Length; i++) {
                BaseSensor sen = sensors[i];
                Vector3 pos = sen.transform.position;
                pos.z = sen.z;

                coords[i] = new SerializableVector3(pos.x, pos.y, sen.z);
            }
        }

        [JsonConstructor]
        private AgentCoords(SerializableVector3[] coords) {
            this.coords = coords;
        }

        public static AgentCoords Load(string path) {
            try {
                using (StreamReader sr = new StreamReader(path)) {
                    string coordsJson = sr.ReadToEnd();
                    AgentCoords ac = JsonConvert.DeserializeObject<AgentCoords>(coordsJson);
                    return ac;
                }
            } catch (Exception ex) {
                // TODO: better error handling 
                Debug.LogError("Error loading file(s) " + ex.ToString());
                return null;
            }
        }
        public void Save(string path, Formatting format = Formatting.None) {
            string coordsJson = JsonConvert.SerializeObject(this, format);

            try {
                using (StreamWriter sr = new StreamWriter(path)) {
                    sr.WriteLine(coordsJson);
                }
            } catch (Exception ex) {
                // TODO: better error handling 
                Debug.LogError("Error saving file(s) " + ex.ToString());
            }
        }
    }
}
