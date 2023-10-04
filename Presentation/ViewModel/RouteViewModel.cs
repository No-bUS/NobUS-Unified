using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Repository;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public record RouteViewModel(Route Route)
    {
        private readonly IRepository<Station> _stationRepository =
            CommonServiceLocator.ServiceLocator.Current.GetInstance<IRepository<Station>>();
        public Task<string> OriginName =>
            _stationRepository
                .GetAll()
                .ContinueWith(r => r.Result.First(s => s.Code == Route.Origin.Id).Name);
    }
}
