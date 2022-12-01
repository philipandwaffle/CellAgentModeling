using System.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Agent.Refactor {
    public class SensorBatch<T> : IJob where T : SMSensor {
        private SM<T> sm;
        private T[] sensors;
        
        public SensorBatch(ref SM<T> sm, ref T[] sensors) {
            this.sm = sm;
            this.sensors = sensors;
        }

        public void Execute() {
            sm.AdvanceSensors(sensors);
        }
    }
}