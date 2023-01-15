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

        
        private Rigidbody2D rb;
        // The collider trigger belonging to this sensor used to handle the contact list
        protected CircleCollider2D con;
        // The collider belonging to this sensor used for collision
        protected CircleCollider2D col;
        private float conR, colR;
        // A list of peers who are in range of this sensor
        public List<Collider2D> contacts = new List<Collider2D>();

        // Use this for initialization
        void Awake() {
            GameObject sensorColGo = new GameObject("contactCol");
            sensorColGo.transform.position = transform.position;
            sensorColGo.transform.parent = transform;
            con = sensorColGo.AddComponent<CircleCollider2D>();
            con.tag = "contactCol";
            con.isTrigger = true;

            GameObject colliderGO = new GameObject("col");
            colliderGO.transform.position = transform.position;
            colliderGO.transform.parent = transform;
            col = colliderGO.AddComponent<CircleCollider2D>();

            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = false;
            rb.gravityScale = 0f;
            rb.drag = 2;
            

            id = nextId;
            nextId++;
        }

        private void Start() {
            col.radius = colR;
            con.radius = conR;
        }

        public virtual void SetConRadius(float radius) {
            conR = radius;
        }
        public virtual void SetColRadius(float radius) {
            colR = radius;
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

                    index = contacts[i].transform.parent.GetComponent<Sensor>().id;
                }
            }

            return index;
        }

        public void ApplyForce(Vector2 force) {
            rb.AddForce(force);
        }

        protected void OnTriggerEnter2D(Collider2D collision) {            
            if (collision.CompareTag("contactCol") && !contacts.Contains(collision)) { 
                contacts.Add(collision); 
            }            
        }
        protected void OnTriggerExit2D(Collider2D collision) {      
            contacts.Remove(collision);
        }
    }
}