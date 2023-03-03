using Assets.CASMTransmission;
using UnityEngine;

namespace Assets.Agent.Sensors {    
    /// <summary>
    /// A LayerSensor is the body of an agent, it represents the agent's presence on a layer.
    /// It can also be can be extended through inheritence.
    /// </summary>
    public class LayerSensor : Sensor {
        public static Gearbox gb;
        protected float offSet = 0.5f;

        public virtual void WriteValue(float val) {
            gb.WriteValue(val, 0, (int)(transform.position.y + offSet), (int)(transform.position.x + offSet));
        }
        public virtual float ReadValue() {
            return gb.ReadVaue(0, (int)(transform.position.y + offSet), (int)(transform.position.x + offSet));
        }
        public virtual Vector2 DirectionOfLowest() {
            return gb.DirectionOfLowest(0, (int)(transform.position.y + offSet), (int)(transform.position.x + offSet));
        }
    }
}