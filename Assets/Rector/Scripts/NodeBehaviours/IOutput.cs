using R3;

namespace Rector.NodeBehaviours
{
    public interface IOutput
    {
        string Name { get; }
    }

    public class ObservableOutput<T> : IOutput
    {
        public string Name { get; }
        public readonly Observable<T> Observable;

        public ObservableOutput(string name, Observable<T> observable)
        {
            Name = name;
            Observable = observable;
        }
    }
}
