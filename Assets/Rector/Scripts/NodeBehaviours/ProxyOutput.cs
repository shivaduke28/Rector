using System;
using R3;

namespace Rector.NodeBehaviours
{
    // Base classes for proxy outputs
    internal abstract class ProxyOutput : IOutput
    {
        public abstract string Name { get; }
        public abstract void UpdateOutput(IOutput output);
    }

    internal sealed class ProxyObservableOutput<T> : ProxyOutput, IObservableOutput<T>
    {
        ObservableOutput<T> currentOutput;
        readonly Subject<T> subject = new();
        IDisposable subscription;

        public ProxyObservableOutput(ObservableOutput<T> output)
        {
            currentOutput = output;
            Name = output.Name;
            subscription = output.Observable.Subscribe(x => subject.OnNext(x));
        }

        public override string Name { get; }
        public Observable<T> Observable => subject;

        public override void UpdateOutput(IOutput output)
        {
            if (output is ObservableOutput<T> observableOutput)
            {
                currentOutput = observableOutput;
                subscription?.Dispose();
                subscription = observableOutput.Observable.Subscribe(x => subject.OnNext(x));
            }
        }
    }
}
