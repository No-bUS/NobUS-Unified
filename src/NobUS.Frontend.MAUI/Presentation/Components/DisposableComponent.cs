namespace NobUS.Frontend.MAUI.Presentation.Components
{
    internal abstract class DisposableComponent : Component, IDisposable
    {
        protected internal readonly List<WeakReference<IDisposable>> _disposables = new();

        public void Dispose()
        {
            Task.Run(
                () =>
                    _disposables
                        .Select(cr =>
                        {
                            cr.TryGetTarget(out var c);
                            return c;
                        })
                        .ToList()
                        .ForEach(c => c.Dispose())
            );
        }

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

        public void Dispose()
        {
            Task.Run(
                () =>
                    _disposables
                        .Select(cr =>
                        {
                            cr.TryGetTarget(out var c);
                            return c;
                        })
                        .ToList()
                        .ForEach(c => c.Dispose())
            );
        }

        protected internal void RegisterResource(IDisposable resource) =>
            _disposables.Add(new(resource));

        protected override void OnWillUnmount()
        {
            Dispose();
            base.OnWillUnmount();
        }
    }
}
