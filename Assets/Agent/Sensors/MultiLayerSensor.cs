using Assets.CASMTransmission;
using System;
using UnityEditor.U2D.Animation;
using UnityEngine;

namespace Assets.Agent.Sensors {
    public class MultiLayerSensor : LayerSensor {        
        public static int maxZ { set; private get; }
        private int z = 0;

        public override void WriteValue(float val) {
            gb.WriteValue(val, z, (int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }
        public override float ReadValue() {
            return gb.ReadVaue(z, (int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }
        public override Vector2 DirectionOfLowest() {
            return gb.DirectionOfLowest(z, (int)(transform.position.x + offSet), (int)(transform.position.y + offSet));
        }

        public void MoveLayer(int deltaZ) {
            z = Math.Clamp(z + deltaZ, 0, maxZ);
            Vector3 newPos = transform.position;
            newPos.z = (z * -10) - 1;

            transform.position = newPos;
        }
    }
}