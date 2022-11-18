using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Assets.Agent.Refactor {
    public class SM<T> where T : SMSensor {
        State<T>[] states;
        Input<T>[] inputs;

        // Key being the source state
        // Value being the input index with a corrosponding new state index
        private Dictionary<int, (int, int)[]> transitions;

        public SM() {
            
        }
    }

    public class Input<T> where T : SMSensor {
        private string name;
        private Func<T, bool> activation;

        public Input(string name, Func<SMSensor, bool> activation) {
            this.name = name;
            this.activation = activation;
        }
    }

    public class State<T> where T: SMSensor {
        private string name;
        private Action<T> act;

        public State(string name, Action<T> act) {
            this.name = name;
            this.act = act;
        }
    }
}