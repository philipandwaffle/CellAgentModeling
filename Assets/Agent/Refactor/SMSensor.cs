using System.Collections;
using UnityEngine;

namespace Assets.Agent.Refactor {
    public class SMSensor : MonoBehaviour {
        public int curState { get; set; }
        public int id;
        private static int nextId = 0;

        public static SMSensor[] peers;

        // Use this for initialization
        void Start() {
            id = nextId;
            nextId++;
        }

        public static void SetPeers() {
            peers = FindObjectsOfType<SMSensor>();
        }

        public int GetClosestPeer() {
            int index = -1;

            if (peers.Length == 0 || peers.Length == 1) {
                Debug.LogWarning("Calling closest may cause problems when peers <= 1");
                return index;
            } else {                
                float minDist = float.PositiveInfinity;

                for (int i = 0; i < peers.Length; i++) {
                    if (id == peers[i].id) {
                        continue;
                    }

                    float dist = Vector3.Distance(gameObject.transform.position, peers[i].transform.position);
                    if (minDist > dist) {
                        minDist = dist;
                        index = i;
                    }
                }

                if (minDist == 0) {
                    return -1;
                }
            }

            return index;
        }
    }
}