using Assets.Agent.Sensors;
using Assets.Agent.StateMachine;
using Newtonsoft.Json.Linq;
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
        public void SetSpawnLocations(SerializableVector3[] spawnLocations) {            
            agentSpawner.InitAgents(ref sm, ref sensors, spawnLocations);
        }

        public void SaveAgentPos(string filePath) {
            AgentCoords ac = new AgentCoords(sensors);
            ac.Save(filePath);
        }

        void Awake() {
            agentSpawner = GetComponent<AgentSpawner>();
        }

        public void AdvanceSensors() {
            if (sensors is null || sm is null) return;

            // Cast the state machine to the correct type
            switch (sm) {
                case IStateMachine<NavLayerSensor> sm:
                    sm.AdvanceSensors(Array.ConvertAll(sensors, sen => (NavLayerSensor)sen));
                    break;
                case IStateMachine<MultiLayerSensor> sm:
                    sm.AdvanceSensors(Array.ConvertAll(sensors, sen => (MultiLayerSensor)sen));
                    break;
                case IStateMachine<LayerSensor> sm:
                    sm.AdvanceSensors(Array.ConvertAll(sensors, sen => (LayerSensor)sen));
                    break;
                case IStateMachine<Sensor> sm:
                    sm.AdvanceSensors(Array.ConvertAll(sensors, sen => (Sensor)sen));
                    break;
                default:
                    Debug.LogError("Advance call not implemented for SM of type :" + sm.GetType().Name);
                    break;

            }            
        }

        public void Pause() {
            foreach (BaseSensor sen in sensors) {
                sen.Pause();
            }
        }
        public void Play() {
            foreach (BaseSensor sen in sensors) {
                sen.Play();
            }
        }
    }
}