using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal abstract class DisposableComponent : Component, IDisposable
{
    protected internal readonly List<WeakReference<IDisposable>> _disposables = new();

    public void Dispose() =>
        Task.Run(() =>
        {
            foreach (var disposable in _disposables.ToList())
            {
                if (disposable.TryGetTarget(out var target))
                {
                    target.Dispose();
                }
            }
        });

    protected internal void RegisterResource(IDisposable resource) =>
        _disposables.Add(new(resource));

    protected override void OnWillUnmount()
    {
        Dispose();
        base.OnWillUnmount();
    }
}

internal abstract class DisposableComponent<T> : Component<T>, IDisposable
    where T : class, new()
{
    protected internal readonly List<WeakReference<IDisposable>> _disposables = new();

    public void Dispose() =>
        Task.Run(() =>
        {
            foreach (var disposable in _disposables.ToList())
            {
                if (disposable.TryGetTarget(out var target))
                {
                    target.Dispose();
                }
            }
        });

    protected internal void RegisterResource(IDisposable resource) =>
        _disposables.Add(new(resource));

    protected override void OnWillUnmount()
    {
        Dispose();
        base.OnWillUnmount();
    }
}
