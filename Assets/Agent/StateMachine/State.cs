using Assets.Agent.Sensors;
using System;

namespace Assets.Agent.StateMachine {
    /// <summary>
    /// Contains the action associated with this state
    /// </summary>
    /// <typeparam name="T">The type of sensor that the state belongs to</typeparam>
    public class State<T> : IState<T> where T : BaseSensor {
        // The action that's performed when in this state
        private readonly Action<T> act;

        public State(Action<T> act) {
            this.act = act;
        }

        public void Act(T sensor) {
            act(sensor);
        }
    }
}
