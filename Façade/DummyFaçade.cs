using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NobUS.DataContract.Reader.OfficialAPI;

namespace NobUS.Frontend.MAUI.Façade
{
    internal class DummyFaçade : IFaçade
    {
        private readonly IClient _client = CommonServiceLocator.ServiceLocator.Current.GetInstance<IClient>();

        public Task<IImmutableList<TResult>> GetAsync<TResult>() where TResult : class => _client.GetAsync<TResult>();

        public Task<IImmutableList<TResult>> GetAsync<TResult, TQuery>(TQuery query) where TResult : class where TQuery : class => _client.GetAsync<TResult, TQuery>(query);
    }
}
