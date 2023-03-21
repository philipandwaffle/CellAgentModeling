using System.Collections.Generic;
using UnityEngine;

namespace Assets.Agent.Sensors {
    /// <summary>
    /// A sensor is the body of an agent, it represents the agent's presence in relation to other agents.
    /// It can also be can be extended through inheritence.
    /// </summary>
    public class Sensor : BaseSensor {
        // Every sensor in the scene
        public static Sensor[] peers;

        // The collider trigger belonging to this sensor used to handle the contact list
        protected CircleCollider2D contactCol;

        // A list of peers who are in range of this sensor
        public List<Collider2D> contacts = new List<Collider2D>();

        // Use this for initialization
        void Awake() {
            // Contact collider
            GameObject sensorColGo = new GameObject("contactCol");
            sensorColGo.transform.position = transform.position;
            sensorColGo.transform.parent = transform;
            contactCol = sensorColGo.AddComponent<CircleCollider2D>();
            contactCol.tag = "contactCol";
            contactCol.isTrigger = true;
        }

        public virtual void SetConRadius(float radius) {
            contactCol.radius = radius;
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