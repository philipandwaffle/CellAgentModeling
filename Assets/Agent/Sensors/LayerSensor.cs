using Assets.CASMTransmission;
using TMPro.EditorUtilities;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public class LayerSensor : Sensor {
        public static Gearbox gb;
        protected float offSet = 0.5f;

        public virtual void WriteValue(float val) {
            gb.WriteValue(val, 0, (int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }
        public virtual float ReadValue() {
            return gb.ReadVaue(0, (int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }
        public virtual Vector2 DirectionOfLowest() {
            return gb.DirectionOfLowest(0, (int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }
    }
}