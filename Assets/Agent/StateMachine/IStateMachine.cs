namespace Assets.Agent.StateMachine {
    public interface IStateMachine { }
    public interface IStateMachine<T> : IStateMachine {
        public void AdvanceSensors(T[] sensors);
        public void AdvanceSensor(T sensor);
    }
}
