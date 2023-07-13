using System.Collections.Immutable;
using NobUS.DataContract.Reader.OfficialAPI;

namespace NobUS.Frontend.MAUI.Repository
{
    public record StaticRepository<T> : IRepository<T> where T : class
    {
        private readonly Task _initialization;
        private readonly List<Func<Task<IEnumerable<T>>>> _sources = new();

        private List<T> _values = new();

        public StaticRepository(IClient client)
        {
            _sources.Add(async () => await client.GetAsync<T>());
            _initialization = Refresh();
        }

        public IRepository<T> RegisterSource(Func<Task<IEnumerable<T>>> source)
        {
            _sources.Add(source);
            return this;
        }

        public async Task<IEnumerable<T>> GetAll() =>
            await _initialization.ContinueWith(_ => _values.ToImmutableList());

        public async Task<IRepository<T>> Refresh() =>
            await Task
                .WhenAny(_sources.Select(async s => _values.AddRange(await s())))
                .ContinueWith(_ => _values = _values.Distinct().ToList())
                .ContinueWith(_ => this);
    }
}