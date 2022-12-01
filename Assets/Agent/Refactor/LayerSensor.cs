using Assets.CASMTransmission;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Assets.Agent.Refactor {
    public class LayerSensor : SMSensor {
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