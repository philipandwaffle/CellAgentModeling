﻿using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Assets.Agent.Refactor {
    public class SM<T> where T : SMSensor {
        private State<T>[] states;
        private Input<T>[] inputs;
        // Key being the source state
        // Value being the input index with a corrosponding new state index
        private Dictionary<int, (int, int)[]> transitions;

        private bool deterministic;

        public SM(State<T>[] states, Input<T>[] inputs, Dictionary<int, (int, int)[]> transitions, bool deterministic = false) {
            this.states = states;
            this.inputs = inputs;
            this.transitions = transitions;
            this.deterministic = deterministic;
        }

        public void AdvanceSensors(ref T[] sensors) {
            for (int i = 0; i < sensors.Length; i++) {
                AdvanceSensor(sensors[i]);
            }
        }

        public void AdvanceSensor(T sensor) {
            (int, int)[] possibleTrans = transitions[sensor.curState];


            if (deterministic) {
                for (int i = 0; i < possibleTrans.Length; i++) {
                    (int, int) trans = possibleTrans[i];
                    if (inputs[trans.Item1].Activated(sensor)) {
                        sensor.curState = trans.Item2;
                        break;
                    }
                }
            } else {
                List<int> finalStates = new List<int>();
                for (int i = 0; i < possibleTrans.Length; i++) {
                    (int, int) trans = possibleTrans[i];

                    if (inputs[trans.Item1].Activated(sensor)) {
                        finalStates.Add(trans.Item2);
                    }
                }
                int count = finalStates.Count;
                if (count != 0) {
                    sensor.curState = finalStates[Random.Range(0, count - 1)];
                }
            }

            states[sensor.curState].Act(sensor);
        }
    }

    public class Input<T> where T : SMSensor {        
        private Func<T, bool> activation;

        public Input(Func<SMSensor, bool> activation) {
            this.activation = activation;
        }

        public bool Activated(T sensor) {
            return activation(sensor);
        }
    }

    public class State<T> where T: SMSensor {        
        private Action<T> act;

        public State(Action<T> act) {            
            this.act = act;
        }

        public void Act(T sensor) {
            act(sensor);
        }
    }
}