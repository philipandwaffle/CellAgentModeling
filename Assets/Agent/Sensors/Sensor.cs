using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Agent.Sensors {
    /// <summary>
    /// A sensor is the body of an agent, it represents the agent's presence in relation to other agents.
    /// It can also be can be extended through inheritence.
    /// </summary>
    public class Sensor : MonoBehaviour {
        public int curState { get; set; }
        public int id;
        protected static int nextId = 0;

        // Every sensor in the scene
        public static Sensor[] peers;

        // The collider belonging to this sensor
        private CircleCollider2D col;
        // A list of peers who are in range of this sensor
        public List<Collider2D> contacts = new List<Collider2D>();

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

        public virtual void SetColliderRadius(float radius) {
            col.radius = radius;
        }

        /// <summary>
        /// Sets the peers, make sure this is called after every sensor has been made
        /// </summary>
        public static void SetPeers() {
            peers = FindObjectsOfType<Sensor>();

            Sensor[] orderedPeers = new Sensor[peers.Length];

            for (int i = 0; i < peers.Length; i++) {
                orderedPeers[peers[i].id] = peers[i];
            }

            peers = orderedPeers;
        }


        public virtual void SetTrigger(bool isTrigger) {
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

        /// <summary>
        /// Gets the closest peer in the contacts list
        /// </summary>
        /// <returns>The id/index of the closest sensor in peers static array</returns>
        public virtual int GetClosestPeer() {
            int index = -1;

            float minDistSquared = float.PositiveInfinity;

            // Loop through each contact
            for (int i = 0; i < contacts.Count; i++) {
                Vector3 direction = gameObject.transform.position - contacts[i].transform.position;

                float distSquared = direction.sqrMagnitude;

                if (minDistSquared > direction.sqrMagnitude) {
                    minDistSquared = distSquared;

                    // Check if the closest peer is in the same place as the sensor
                    if (minDistSquared == 0) {
                        // returns -1 so that this edge case can be delt with properly
                        return -1;
                    }

                    index = contacts[i].GetComponent<Sensor>().id;
                }
            }

            return index;
        }
        protected void OnTriggerEnter2D(Collider2D collision) {
            if (!contacts.Contains(collision)) { 
                contacts.Add(collision); 
            }            
        }
        protected void OnTriggerExit2D(Collider2D collision) {      
            contacts.Remove(collision);
        }
    }
}