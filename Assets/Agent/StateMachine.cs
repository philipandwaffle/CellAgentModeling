using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Agent {
    public class StateMachine {
        
    } 

    public class State : MonoBehaviour {
        public string name;
        public Action act;

        public State(string name, Action act) {
            this.name = name;
            this.act = act;
        }
    }
    public class Input {
        public string name;

        public Input(string name) {
            this.name = name;
        }
    }
}
