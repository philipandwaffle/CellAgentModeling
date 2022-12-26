using Assets.Agent.Sensors;
using System;

namespace Assets.Agent.StateMachine {
    /// <summary>
    /// An input is "activated" when its activation function returns true
    /// </summary>
    /// <typeparam name="T">The type of sensor that the input belongs to</typeparam>
    public class Input<T> : IInput<T> where T : Sensor {
        // The check done to see if the input is stimulated
        private readonly Func<T, bool> activation;

        public Input(Func<T, bool> activation) {
            this.activation = activation;
        }

        public bool Activated(T sensor) {
            return activation(sensor);
        }
    }
}
