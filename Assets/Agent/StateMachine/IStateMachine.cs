namespace Assets.Agent.StateMachine {
    /// <summary>
    /// Allows for a collection of generic state machines that can be casted appropriately at a later time
    /// </summary>
    public interface IStateMachine { }

    /// <summary>
    /// Intermediate interface to allow for generic sensors 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStateMachine<T> : IStateMachine {
        /// <summary>
        /// Advances an array of sensors by advancing the current state of the sensor, then act once a new state is chosen 
        /// </summary>
        /// <param name="sensors">The sensors that will be advanced</param>
        public void AdvanceSensors(T[] sensors);
    }
}
