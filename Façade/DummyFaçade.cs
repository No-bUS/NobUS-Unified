using System.Collections.Immutable;
using CommonServiceLocator;
using NobUS.DataContract.Reader.OfficialAPI;

namespace NobUS.Frontend.MAUI.Façade
{
    internal class DummyFaçade : IFaçade
    {
        private readonly IClient _client = ServiceLocator.Current.GetInstance<IClient>();

        public Task<IImmutableList<TResult>> GetAsync<TResult>()
            where TResult : class => _client.GetAsync<TResult>();

        public Task<IImmutableList<TResult>> GetAsync<TResult, TQuery>(TQuery query)
            where TResult : class
            where TQuery : class => _client.GetAsync<TResult, TQuery>(query);
    }
}
