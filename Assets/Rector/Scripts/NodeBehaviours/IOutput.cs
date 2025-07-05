using R3;

namespace Rector.NodeBehaviours
{
    public interface IOutput
    {
        string Name { get; }
    }

    public class ObservableOutput<T> : IObservableOutput<T>
    {
        public string Name { get; }
        public Observable<T> Observable { get; }

        public ObservableOutput(string name, Observable<T> observable)
        {
            Name = name;
            Observable = observable;
        }
    }
}
