using Assets.Agent.Sensors;
using Assets.Agent.StateMachine;
using UnityEditor.UIElements;
using UnityEngine;

namespace Assets.Agent {
    /// <summary>
    /// Advances each agent every tick
    /// </summary>
    public class AgentTicker : MonoBehaviour {
        private Sensor[][] sensors;
        private IStateMachine[] stateMachines;
        [SerializeField] private bool isPaused = true;

        public void SetAgents(Sensor[][] sensors, IStateMachine[] stateMachines) {
            this.sensors = sensors;
            this.stateMachines = stateMachines;
        }
        public void TogglePaused() {
            isPaused = !isPaused;
        }

        private void Update() {
            // If the sim isn't paused and there are sensors set
            if (!isPaused && sensors != null) {
                AdvanceSensors();
            }
        }

        private void AdvanceSensors() {
            for (int i = 0; i < sensors.Length; i++) {
                // Cast the state machine to the correct type
                switch (stateMachines[i]) {
                    case IStateMachine<NavLayerSensor> sm:
                        for (int j = 0; j < sensors[i].Length; j++) {
                            sm.AdvanceSensor((NavLayerSensor)sensors[i][j]);
                        }
                        break;
                    case IStateMachine<MultiLayerSensor> sm:
                        for (int j = 0; j < sensors[i].Length; j++) {
                            sm.AdvanceSensor((MultiLayerSensor)sensors[i][j]);
                        }
                        break;
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
                    default:
                        Debug.LogError("Statemachine advance call not implemented");
                        break;
                    
                }
            }
        }
    }
}