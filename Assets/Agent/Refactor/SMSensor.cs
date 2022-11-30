using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Agent.Refactor {
    public interface ISensor { }
    public class SMSensor : MonoBehaviour, ISensor {
        public int curState { get; set; }
        public int id;
        private static int nextId = 0;

        public static SMSensor[] peers;

        private CircleCollider2D col;
        public List<Collider2D> colliders = new List<Collider2D>(500);

        // Use this for initialization
        void Awake() {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;

            id = nextId;
            nextId++;
        }

        public void SetColliderRadius(float radius) {
            col.radius = radius;
        }

        public static void SetPeers() {
            peers = FindObjectsOfType<SMSensor>();

            SMSensor[] orderedPeers = new SMSensor[peers.Length];

            for (int i = 0; i < peers.Length; i++) {
                orderedPeers[peers[i].id] = peers[i];
            }

            peers = orderedPeers;
        }

        public int GetClosestPeer() {
            int index = -1;

            float minDistSquared = float.PositiveInfinity;

            for (int i = 0; i < colliders.Count; i++) {
                Vector3 direction = gameObject.transform.position - colliders[i].transform.position;

                float distSquared = direction.sqrMagnitude;

                if (minDistSquared > direction.sqrMagnitude) {
                    minDistSquared = distSquared;
                    index = colliders[i].GetComponent<SMSensor>().id;
                }

                if (minDistSquared == 0) {
                    return -1;
                }
            }

            return index;
        }
        private void OnTriggerEnter2D(Collider2D collision) {
            if (!colliders.Contains(collision)) { 
                colliders.Add(collision); 
            }            
        }
        private void OnTriggerExit2D(Collider2D collision) {      
            colliders.Remove(collision);
        }
    }
}