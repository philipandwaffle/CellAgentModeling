using Assets.Agent.Sensors;
using Assets.Agent.StateMachine;
using Assets.CASMTransmission;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Agent {
    public class AgentTicker : MonoBehaviour {
        private Sensor[][] sensors;
        private IStateMachine[] stateMachines;

        public void SetAgents(Sensor[][] sensors, IStateMachine[] stateMachines) {
            this.sensors = sensors;
            this.stateMachines = stateMachines;
        }

        private void Update() {
            if (sensors != null) {
                AdvanceSensors();
            }
        }

        private void AdvanceSensors() {
            for (int i = 0; i < sensors.Length; i++) {
                switch (stateMachines[i]) {
                    case IStateMachine<LayerSensor> sm:
                        for (int j = 0; j < sensors[i].Length; j++) {
                            sm.AdvanceSensor((LayerSensor)sensors[i][j]);
                        }
                    break;
                    case IStateMachine<Sensor> sm:
                        for (int j = 0; j < sensors[i].Length; j++) {
                            sm.AdvanceSensor(sensors[i][j]);
                        }
                    break;
                }
            }
        }
    }
}