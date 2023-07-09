using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Model.ValueObject.Snapshot;
using NobUS.DataContract.Reader.OfficialAPI.Client;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobUS.DataContract.Reader.OfficialAPI
{
  public interface IClient
  {
        public Task<IImmutableList<Station>> GetStationsAsync();
        public Task<IImmutableList<Route>> GetRoutesAsync();
        public Task<IImmutableList<Vehicle>> GetVehiclesAsync();
        public Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync();
        public Task<IImmutableList<Station>> GetStationsAsync(Route route);
        public Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station);
        public Task<IImmutableList<T>> GetAsync<T>() where T : class;
  }
}
