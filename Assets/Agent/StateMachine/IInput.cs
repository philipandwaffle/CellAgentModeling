namespace Assets.Agent.StateMachine {
    public interface IInput<T> {
        /// <summary>
        /// Checks if the input has been activated
        /// </summary>
        /// <param name="sensor">The sensor being checked</param>
        /// <returns></returns>
        public bool Activated(T sensor);
    }
}
