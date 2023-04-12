﻿using Assets.Agent.Sensors;
using Assets.Environment;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.CASMTransmission {
    public class Gearbox : MonoBehaviour {
        [SerializeField] private EnvironmentController environmentController;

        // Put a refrence of me into the agents
        private void Awake() {
            NavLayerSensor.gb = this;
            LayerSensor.gb = this;
        }

        public void WriteValue(float val, int z, int y, int x) {
            environmentController.GetLayer(z).data[y, x] = val;
        }

        public float ReadVaue(int z, int y, int x) {
            x = (int)(x / environmentController.transform.localScale.x);
            y = (int)(y / environmentController.transform.localScale.y);
            return environmentController.GetLayer(z).data[y, x];
        }

        public Vector2 DirectionOfLowest(int z, int y, int x) {
            x = (int)(x / environmentController.transform.localScale.x);
            y = (int)(y / environmentController.transform.localScale.y);
            Vector2 dir = Vector2.zero;
            float lowestValid = float.PositiveInfinity;
            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    float val = environmentController.GetLayer(z).data[y + j, x + i];
                    if (val != -1 && val < lowestValid) {
                        lowestValid = val;
                        dir = new Vector2(i, j);
                    }
                }
            }

            if (dir != Vector2.zero) {
                dir.Normalize();
            }

            return dir;
        }

        public Queue<Vector2> GetPath(int z, float y, float x) {
            float minDist = float.MaxValue;
            Vector2 pos = new Vector2(x, y);
            Vector2Int[] nodeCoords = environmentController.GetLayer(z).navGraph.GetNodeCoords();

            int curNode = -1;
            for (int i = 0; i < nodeCoords.Length; i++) {
                Vector3 dir = nodeCoords[i] - pos;
                float curDist = dir.magnitude;
                if (minDist < curDist) continue;

                RaycastHit2D[] hits = Physics2D.RaycastAll(pos, dir, curDist);
                bool hasLineOfSight = true;

                for (int j = 0; j < hits.Length; j++) { 
                    if (hits[j].collider.enabled && hits[j].collider.CompareTag("layer")) {
                        hasLineOfSight = false;
                    }
                }
                if (!hasLineOfSight) continue;

                curNode = i;
                minDist = curDist;                
            }

            Queue<Vector2> path = new Queue<Vector2>();
            if (curNode == -1) {
                Debug.LogError("No node in sight");
                return path;
            }

            int[] nodePath = environmentController.GetLayer(z).navGraph.Getpaths()[curNode];
            if (nodePath.Length == 0) return path;

            for (int i = nodePath.Length - 1; i >= 0; i--) {
                path.Enqueue(environmentController.GetLayer(z).navGraph.GetNodeCoords()[nodePath[i]]);
            }
            return path;
        }
    }
}