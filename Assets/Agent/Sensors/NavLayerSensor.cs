using Assets.CASMTransmission;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public class NavLayerSensor : Sensor {
        public static Gearbox gb;
        protected float offSet = 0.5f;
        private Vector2 targetNode;
        private Queue<Vector2> path;
        public void UpdatePath() {
            path = gb.GetPath(0, (int)(transform.position.y + offSet), (int)(transform.position.x + offSet));
            targetNode = path.Dequeue();
        }
        public Vector2 GetDir() {
            if (path.Count > 0) {
                if (Mathf.Abs(transform.position.sqrMagnitude - targetNode.sqrMagnitude) < 1.41421f) {
                    //Debug.Log("Changing target dir for agent: "+ name);
                    targetNode = path.Dequeue();
                }
            }
            Vector2 dir = targetNode - (Vector2)transform.position;
            dir.Normalize();
            return dir;
        }

        public bool HasPath() {
            return path is not null && path.Count > 0;
        }
    }
}
