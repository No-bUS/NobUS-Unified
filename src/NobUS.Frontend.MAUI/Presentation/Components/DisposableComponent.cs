using System;
using System.Collections.Generic;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal abstract class DisposableComponent : Component, IDisposable
{
    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
    {
        foreach (var disposable in _disposables.ToArray())
        {
            disposable.Dispose();
        }
        _disposables.Clear();
    }

    protected internal void RegisterResource(IDisposable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        _disposables.Add(resource);
    }

    protected override void OnWillUnmount()
    {
        Dispose();
        base.OnWillUnmount();
    }
}

internal abstract class DisposableComponent<T> : Component<T>, IDisposable
    where T : class, new()
{
    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
    {
        foreach (var disposable in _disposables.ToArray())
        {
            disposable.Dispose();
        }
        _disposables.Clear();
    }

    protected internal void RegisterResource(IDisposable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        _disposables.Add(resource);
    }

    protected override void OnWillUnmount()
    {
        Dispose();
        base.OnWillUnmount();
    }
}
