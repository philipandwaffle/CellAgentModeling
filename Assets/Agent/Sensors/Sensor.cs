using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public class Sensor : MonoBehaviour {
        public int curState { get; set; }
        public int id;
        protected static int nextId = 0;

        public static Sensor[] peers;

        private CircleCollider2D col;
        public List<Collider2D> colliders = new List<Collider2D>();

        // Use this for initialization
        void Awake() {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = false;
            rb.gravityScale = 0f;
            rb.drag = 2;
            id = nextId;
            nextId++;
        }

        public void SetColliderRadius(float radius) {
            col.radius = radius;
        }

        public static void SetPeers() {
            peers = FindObjectsOfType<Sensor>();

            Sensor[] orderedPeers = new Sensor[peers.Length];

            for (int i = 0; i < peers.Length; i++) {
                orderedPeers[peers[i].id] = peers[i];
            }

            peers = orderedPeers;
        }

        public void SetTrigger(bool isTrigger) {
            if (isTrigger) {
                col = gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;

                Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
                rb.isKinematic = true;
            } else {
                Destroy(GetComponent<CircleCollider2D>());
                Destroy(GetComponent<Rigidbody2D>());
            }
        }

        public int GetClosestPeer() {
            int index = -1;

            float minDistSquared = float.PositiveInfinity;

            for (int i = 0; i < colliders.Count; i++) {
                Vector3 direction = gameObject.transform.position - colliders[i].transform.position;

                float distSquared = direction.sqrMagnitude;

                if (minDistSquared > direction.sqrMagnitude) {
                    minDistSquared = distSquared;
                    index = colliders[i].GetComponent<Sensor>().id;
                }

                if (minDistSquared == 0) {
                    return -1;
                }
            }

            return index;
        }
        protected void OnTriggerEnter2D(Collider2D collision) {
            if (!colliders.Contains(collision)) { 
                colliders.Add(collision); 
            }            
        }
        protected void OnTriggerExit2D(Collider2D collision) {      
            colliders.Remove(collision);
        }
    }
}