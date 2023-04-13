using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public abstract class BaseSensor : MonoBehaviour {
        public int curState;
        public int id;
        public int z;
        public static int nextId = 0;

        protected Vector2 pausedVelocity;

        protected Rigidbody2D rb;
        // The collider belonging to this sensor used for collision
        protected CircleCollider2D collisionCol;

        private void Awake() {
            // Collision collider
            GameObject colliderGO = new GameObject("collisionCol");
            colliderGO.transform.position = transform.position;
            colliderGO.transform.parent = transform;
            collisionCol = colliderGO.AddComponent<CircleCollider2D>();

            // Rigidbody 
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = false;
            rb.gravityScale = 0f;
            rb.drag = 1;

            // Set ID
            id = nextId;
            nextId++;
            gameObject.name = "Agent: " + id;
        }
        public virtual void SetColRadius(float radius) {
            collisionCol.radius = radius;
        }

        public void ApplyForce(Vector2 force) {
            rb.AddForce(force);
        }
        private void Update() {
            if (rb != null) {
                Debug.Log(rb.velocity);
            }
        }

        public void Pause() {
            pausedVelocity = rb.velocity;
            rb.velocity = Vector2.zero;
        }
        public void Play() {
            rb.velocity = pausedVelocity;
        }
    }
}
