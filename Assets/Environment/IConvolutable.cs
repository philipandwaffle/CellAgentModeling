namespace Assets.Environment {
    public interface IConvolutable<T> {
        public T Mul(T a);
        public void Add(T a);
    }
}
