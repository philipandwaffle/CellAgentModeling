using Assets.CASMTransmission;
using Assets.Environment;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public class NavLayerSensor : BaseSensor {
        public static int maxZ { set; private get; }
        private int z = 0;

        public static Gearbox gb;
        protected float offSet = 0.5f;
        private Vector2 targetNode;
        private Queue<Vector2> path;
        public void UpdatePath() {
            path = gb.GetPath(z, transform.position.y, transform.position.x);
            if (path.Count == 0 ) return; 
            targetNode = path.Dequeue();
        }
        public Vector2 GetDir() {
            Vector2 dir = targetNode - (Vector2)transform.position;
            float mag = dir.magnitude;
            if (path.Count > 0) {
                if (mag < 1f) {
                    targetNode = path.Dequeue();
                }
            }

            dir.Normalize();
            dir *= math.clamp(mag, 1, 10);
            return dir;
        }

        public bool HasPath() {
            return path is not null && path.Count > 0;
        }

        public virtual float ReadValue() {
            return gb.ReadVaue(z, (int)(transform.position.y + offSet), (int)(transform.position.x + offSet));
        }

        public void MoveLayer(int deltaZ) {
            z = Math.Clamp(z + deltaZ, 0, maxZ);
            gameObject.layer = 6 + z;
            collisionCol.gameObject.layer = 6 + z;

            Vector3 newPos = transform.position;
            newPos.z = (z * -CASMEditor.layerSep) - 1;

            transform.position = newPos;
        }
    }
}
