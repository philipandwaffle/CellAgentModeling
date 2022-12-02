namespace Assets.Agent.StateMachine {

    public interface IState<T> {
        public void Act(T sensor);
    }
}
