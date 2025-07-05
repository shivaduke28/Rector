using R3;

namespace Rector.NodeBehaviours
{
    public interface IObservableOutput<T> : IOutput
    {
        Observable<T> Observable { get; }
    }
}