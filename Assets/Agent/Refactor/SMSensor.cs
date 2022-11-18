using System.Collections;
using UnityEngine;

namespace Assets.Agent.Refactor {
    public class SMSensor : MonoBehaviour {
        private int curState;
        private int id;
        private static int nextId = 0;

        // Use this for initialization
        void Start() {
            id = nextId;
            nextId++;
        }        
    }
}