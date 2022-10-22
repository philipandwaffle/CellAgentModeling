using System.Collections;
using UnityEngine;

namespace Assets.Agent {
    public class StateMachineSensor : MonoBehaviour {
        public bool push;
        public bool coin;

        private void Start() {
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