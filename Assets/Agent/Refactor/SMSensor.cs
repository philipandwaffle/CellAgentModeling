using System.Collections;
using UnityEngine;

namespace Assets.Agent.Refactor {
    public class SMSensor : MonoBehaviour {
        public int curState { get; set; }
        private int id;
        private static int nextId = 0;

        private static SMSensor[] peers;

        // Use this for initialization
        void Start() {
            id = nextId;
            nextId++;
        }

        public static void SetPeers() {
            peers = FindObjectsOfType<SMSensor>();
        }

        public Vector3 GetClosestPos() {
            Vector3 closest = Vector3.zero;

            if (peers.Length == 0) {
                return Vector3.zero;
            } else {
                int iClosest;
                float maxDist = 0;                

                for (int i = 0; i < peers.Length; i++) {                    
                    float dist = Vector3.Distance(gameObject.transform.position, peers[i].transform.position);
                    if (maxDist <= dist) {
                        maxDist = dist;
                        closest = peers[i].transform.position;
                    }
                }
            }

            return closest;
        }
    }
}