using Assets.CASMTransmission;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public class LayerSensor : Sensor {
        public static Gearbox gb;
        private float offSet = 0.5f;

        public void WriteValue(float val) {
            gb.WriteValue(val, (int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }
        public float ReadValue() {
            return gb.ReadVaue((int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }
        public Vector2 DirectionOfLowest() {
            return gb.DirectionOfLowest((int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }
    }
}