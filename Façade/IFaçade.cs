using System.Collections.Immutable;

namespace NobUS.Frontend.MAUI.Façade
{
    public interface IFaçade
    {
        Task<IImmutableList<TResult>> GetAsync<TResult>() where TResult : class;

        Task<IImmutableList<TResult>> GetAsync<TResult, TQuery>(TQuery query)
            where TResult : class
            where TQuery : class;
    }
}
