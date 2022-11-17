using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Agent {
    public class StateMachineSensor : MonoBehaviour {
        public int id;
        private static int nextId = 0;
        public static StateMachineSensor[] sensors;

        public bool push;
        public bool coin;

        private void Start() {
            id = nextId;
            nextId++;

            push = false;
            coin = false;
        }

        public void Push() {
            push = !push;
        }

        public void Coin() {
            coin = !coin;
        }
    }
}