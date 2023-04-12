using Assets.Agent.Sensors;
using Assets.Agent.StateMachine;
using Ookii.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Agent {
    /// <summary>
    /// Advances each agent every tick
    /// </summary>
    public class AgentController : MonoBehaviour {
        private BaseSensor[] sensors;
        private IStateMachine sm;

        private AgentSpawner agentSpawner;

        public void SetSpawnLocations(Queue<Vector2>[] spawnLocations) {
            agentSpawner.InitAgents(ref sm, ref sensors, spawnLocations);
        }
        public void SetSpawnLocations(Vector2[] spawnLocations) {
            throw new NotImplementedException();
            //agentSpawner.InitAgents(ref sm, ref sensors, spawnLocations);
        }
        
        void Awake() {
            agentSpawner = GetComponent<AgentSpawner>();
        }

        public void AdvanceSensors() {
            if (sensors is null) return;
            // Cast the state machine to the correct type
            switch (sm) {
                case IStateMachine<NavLayerSensor> sm:
                for (int i = 0; i < sensors.Length; i++) {
                    sm.AdvanceSensor((NavLayerSensor)sensors[i]);
                }
                break;
                case IStateMachine<MultiLayerSensor> sm:
                for (int i = 0; i < sensors.Length; i++) {
                    sm.AdvanceSensor((MultiLayerSensor)sensors[i]);
                }
                break;
                case IStateMachine<LayerSensor> sm:
                for (int i = 0; i < sensors.Length; i++) {
                    sm.AdvanceSensor((LayerSensor)sensors[i]);
                }
                break;
                case IStateMachine<Sensor> sm:
                for (int i = 0; i < sensors.Length; i++) {
                    sm.AdvanceSensor((Sensor)sensors[i]);
                }
                break;
                default:
                Debug.LogError("Advance call not implemented for SM of type :" + sm.GetType().Name);
                break;

            }
            
        }
    }
}