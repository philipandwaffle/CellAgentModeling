using Assets.Environment.Refactor;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.CASMTransmission {
    public class Gearbox : MonoBehaviour {
        [SerializeField] private LayerTicker lt;
        Layer l;


        public 
        // Use this for initialization
        void Start() {
            l = lt.GetLayer();
        }

        public void WriteValue(float val, int x, int y) {
            l[x, y] = val;
        }

        public float ReadVaue(int x, int y) {
            return l[x, y];
        }

        public Vector2 DirectionOfLowest(int x, int y) {
            Vector2 dir = Vector2.zero;
            float lowestValid = float.PositiveInfinity;
            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    float val = l[x + i, y + j];
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
    }
}