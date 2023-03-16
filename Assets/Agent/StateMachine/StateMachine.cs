using Assets.Agent.Sensors;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Assets.Agent.StateMachine {
    public class StateMachine<T> : IStateMachine<T> where T : BaseSensor {
        private IState<T>[] states;
        private IInput<T>[] inputs;
        // Key being the source state
        // Value being the input index with a corrosponding new state index
        private Dictionary<int, (int, int)[]> transitions;

        private bool deterministic;

        public StateMachine(IState<T>[] states, IInput<T>[] inputs, Dictionary<int, (int, int)[]> transitions, bool deterministic = false) {
            this.states = states;
            this.inputs = inputs;
            this.transitions = transitions;
            this.deterministic = deterministic;
        }

        public void AdvanceSensors(T[] sensors) {
            for (int i = 0; i < sensors.Length; i++) {
                AdvanceSensor(sensors[i]);
            }
        }

        public void AdvanceSensor(T sensor) {
            // The transitions possible for the current state
            if (!transitions.ContainsKey(sensor.curState)) {
                states[sensor.curState].Act(sensor);
                return;
            }

            (int, int)[] possibleTrans = transitions[sensor.curState];

            List<int> finalStates = new List<int>();

            // Loop through each transition
            for (int i = 0; i < possibleTrans.Length; i++) {
                (int, int) trans = possibleTrans[i];

                // Check if the transition is valid
                if (inputs[trans.Item1].Activated(sensor)) {

                    // If deterministic the first valid transition is selected
                    if (deterministic) {
                        // Set the current state to the first valid state
                        sensor.curState = trans.Item2;
                        states[sensor.curState].Act(sensor);
                        return;
                    } else {
                        finalStates.Add(trans.Item2);
                    }
                }
            }

            // Set the current state one of the possible states
            int count = finalStates.Count;
            if (count != 0) {
                sensor.curState = finalStates[Random.Range(0, count - 1)];
            }

            // Preform the action of the state
            states[sensor.curState].Act(sensor);
        }
    }
}