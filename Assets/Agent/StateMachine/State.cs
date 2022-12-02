using Assets.Agent.Sensors;
using System;

namespace Assets.Agent.StateMachine {
    public class State<T> : IState<T> where T : Sensor {
        private readonly Action<T> act;

        public State(Action<T> act) {
            this.act = act;
        }

        public void Act(T sensor) {
            act(sensor);
        }
    }
}
