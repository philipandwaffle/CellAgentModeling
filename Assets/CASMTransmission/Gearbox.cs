using Assets.Environment;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CASMTransmission {
    public class Gearbox : MonoBehaviour {
        [SerializeField] private LayerTicker lt;

        public void WriteValue(float val, int z, int y, int x) {
            lt.GetLayer(z)[y, x] = val;
        }

        public float ReadVaue(int z, int y, int x) {
            x = (int)(x / lt.transform.localScale.x);
            y = (int)(y / lt.transform.localScale.y);
            return lt.GetLayer(z)[y, x];
        }

        public Vector2 DirectionOfLowest(int z, int y, int x) {
            x = (int)(x / lt.transform.localScale.x);
            y = (int)(y / lt.transform.localScale.y);
            Vector2 dir = Vector2.zero;
            float lowestValid = float.PositiveInfinity;
            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    float val = lt.GetLayer(z)[y + j, x + i];
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

        public Queue<Vector2> GetPath(int z, int y, int x) {
            Debug.Log("Getting path");
            float minDist = float.MaxValue;
            Vector2Int pos = new Vector2Int(x, y);
            Vector2Int[] nodeCoords = lt.GetLayer(z).navGraph.nodeCoords;
            
            int curNode = 0;
            for (int i = 0; i < nodeCoords.Length; i++) {
                float curDist = Mathf.Abs(pos.sqrMagnitude - nodeCoords[i].sqrMagnitude);
                if (minDist > curDist) {
                    curNode = i;
                    minDist = curDist;
                }
            }

            int[] nodePath = lt.GetLayer(z).navGraph.paths[curNode];
            Queue<Vector2> path = new Queue<Vector2>();
            if (nodePath.Length == 0) return path;

            for (int i = nodePath.Length - 1; i >= 0; i--) {
                path.Enqueue(lt.GetLayer(z).navGraph.nodeCoords[nodePath[i]]);
            }
            return path;
        }

        public Vector2 WeightedDirectionOfLowest(int z, int y, int x) {
            x = (int)(x / lt.transform.localScale.x);
            y = (int)(y / lt.transform.localScale.y);
            Vector2 dir = Vector2.zero;

            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    float val = lt.GetLayer(z)[y + j, x + i];
                    float weight = 1 - val;
                    dir += weight * new Vector2(i, j);
                }
            }

            if (dir != Vector2.zero) {
                dir.Normalize();
            }

            return dir;
        }
    }
}