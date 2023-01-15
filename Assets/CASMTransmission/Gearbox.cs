using Assets.Environment;
using UnityEngine;

namespace Assets.CASMTransmission {
    public class Gearbox : MonoBehaviour {
        [SerializeField] private LayerTicker lt;

        public void WriteValue(float val, int z, int x, int y) {
            lt.GetLayer(z)[x, y] = val;
        }

        public float ReadVaue(int z, int x, int y) {
            x = (int)(x / lt.transform.localScale.x);
            y = (int)(y / lt.transform.localScale.y);
            return lt.GetLayer(z)[x, y];
        }

        public Vector2 DirectionOfLowest(int z, int x, int y) {
            x = (int)(x / lt.transform.localScale.x);
            y = (int)(y / lt.transform.localScale.y);
            Vector2 dir = Vector2.zero;
            float lowestValid = float.PositiveInfinity;
            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    float val = lt.GetLayer(z)[x + i, y + j];
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

        public Vector2 WeightedDirectionOfLowest(int z, int x, int y) {
            x = (int)(x / lt.transform.localScale.x);
            y = (int)(y / lt.transform.localScale.y);
            Vector2 dir = Vector2.zero;

            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    float val = lt.GetLayer(z)[x + i, y + j];
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