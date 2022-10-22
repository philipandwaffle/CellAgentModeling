using System;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Agent {
    public class StateMachine {
        private int curState;
        private State[] states;
        private Input[] inputs;
        
        // Key being the source state
        // Value being the input index with a corrosponding new state index
        private Dictionary<int, (int, int)[]> transitions;

        public StateMachine(List<State> states, List<Input> inputs, Dictionary<int, (int, int)[]> transitions) {
            curState = 0;
            this.states = states.ToArray();
            this.inputs = inputs.ToArray();
            this.transitions = transitions;
        }

        public StateMachine(State[] states, Input[] inputs, Dictionary<int, (int, int)[]> transitions) {
            curState = 0;
            this.states = states;
            this.inputs = inputs;
            this.transitions = transitions;
        }        

        public void Advance(GameObject go) {
            Debug.Log("advancing");
            (int, int)[] t = transitions[curState];
            List<int> newStates = new List<int>();

            // Loop through possible transitions for current state
            for (int i = 0; i < t.Length; i++) {
                (int, int) transition = t[i];                
                // If transition requirements are met
                if (inputs[transition.Item1].Check(go)) {
                    newStates.Add(transition.Item2);
                }
            }
            Debug.Log(newStates.Count);
            // Pick new state from possible ones
            if (newStates.Count != 0) {
                int foo = newStates[Random.Range(0, newStates.Count)];
                Debug.Log("new state: " + states[foo].name);
                curState = foo;
            }
        }
    } 

    public class State {
        public string name;
        private Action act;

        public State(string name, Action act) {
            this.name = name;
            this.act = act;
        }

        public void Act() {
            act();
        }
    }
    public class Input{
        private string name;
        private Func<GameObject, bool> check;

        public Input(string name, Func<GameObject, bool> check) {
            this.name = name;
            this.check = check;
        }
        
        public bool Check(GameObject go) {
            return check(go);
        }
    }
}
