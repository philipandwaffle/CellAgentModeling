namespace Assets.Agent.StateMachine {
    public interface IState<T> {
        /// <summary>
        /// Preforms a set action on the given sensor
        /// </summary>
        /// <param name="sensor">The sensor the action is performed on</param>
        public void Act(T sensor);
    }
}
