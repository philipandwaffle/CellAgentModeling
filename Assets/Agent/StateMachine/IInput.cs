namespace Assets.Agent.StateMachine {
    public interface IInput<T> {
        public bool Activated(T sensor);
    }
}
