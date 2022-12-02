using Assets.Agent.Sensors;
using System;

namespace Assets.Agent.StateMachine {
    public class Input<T> : IInput<T> where T : Sensor {
        private readonly Func<T, bool> activation;

        public Input(Func<T, bool> activation) {
            this.activation = activation;
        }

        public bool Activated(T sensor) {
            return activation(sensor);
        }
    }
}
