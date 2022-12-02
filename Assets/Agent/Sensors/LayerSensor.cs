using Assets.CASMTransmission;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public class LayerSensor : Sensor {
        public static Gearbox gb;

        public void WriteValue(float val) {
            gb.WriteValue(val, (int)transform.position.x, (int)transform.position.y);
        }
        public float ReadValue() {
            return gb.ReadVaue((int)transform.position.x, (int)transform.position.y);
        }
        public Vector2 DirectionOfLowest() {
            return gb.DirectionOfLowest((int)transform.position.x, (int)transform.position.y);
        }
    }
}