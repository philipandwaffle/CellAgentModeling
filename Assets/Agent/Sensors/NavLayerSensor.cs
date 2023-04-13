using Assets.CASMTransmission;
using Assets.Environment;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public class NavLayerSensor : BaseSensor {
        public static int maxZ { set; private get; }

        public static Gearbox gb;
        protected float offSet = 0.5f;
        private Vector2 targetPos;
        private Queue<Vector2> path;

        // agents start on a different counter so that a lag spike doesn't occur 
        private static int nextMoveCounter = 1;
        public int moveCounter = nextMoveCounter++;

        public void InitPath() {
            path = gb.GetPath(z, transform.position.y, transform.position.x);
            if (path.Count == 0 ) return; 
            targetPos = path.Dequeue();
        }

        public void UpdatePath() {
            Queue<Vector2> newPath = gb.GetPath(z, transform.position.y, transform.position.x);
            if (newPath is null || newPath.Count - 1 == path.Count) return;

            Vector2 oldTargetPos = targetPos;
            while (newPath.Contains(oldTargetPos)) {
                targetPos = newPath.Dequeue();
            }

            path = newPath;
        }

        public Vector2 GetDir() {

            Vector2 dir = targetPos - (Vector2)transform.position;
            float mag = dir.magnitude;
            if (path.Count > 0) {
                if (mag < 0.5f) {
                    targetPos = path.Dequeue();
                }
            }

            dir.Normalize();
            dir *= math.clamp(mag, 1, 10);

            return dir;
        }

        public bool HasPath() {
            return path is not null;
        }

        public virtual float ReadValue() {
            return gb.ReadVaue(z, (int)(transform.position.y + offSet), (int)(transform.position.x + offSet));
        }

        public void MoveLayer(int deltaZ) {
            z = Math.Clamp(z + deltaZ, 0, maxZ);
            gameObject.layer = 6 + z;
            collisionCol.gameObject.layer = 6 + z;

            Vector3 newPos = transform.position;
            // Small number used to prevent z fighting with layer
            newPos.z = (z * -CASMEditor.layerSep) - 0.001f;

            transform.position = newPos;
            path = null;
        }
    }
}
