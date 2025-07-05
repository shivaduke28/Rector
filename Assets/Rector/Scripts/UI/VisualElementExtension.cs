using R3;
using UnityEngine.UIElements;

namespace Rector.UI
{
    public static class VisualElementExtension
    {
        public static Observable<T> AsObservable<T>(this CallbackEventHandler handler) where T : EventBase<T>, new()
        {
            return Observable.FromEvent<EventCallback<T>, T>(
                h => e => h(e),
                h => handler.RegisterCallback(h),
                h => handler.UnregisterCallback(h)
            );
        }

        public static Observable<ChangeEvent<T>> OnValueChangeAsObservable<T>(this INotifyValueChanged<T> source)
        {
            return Observable.FromEvent<EventCallback<ChangeEvent<T>>, ChangeEvent<T>>(
                h => new EventCallback<ChangeEvent<T>>(h),
                h => source.RegisterValueChangedCallback(h),
                h => source.UnregisterValueChangedCallback(h));
        }
    }
}
