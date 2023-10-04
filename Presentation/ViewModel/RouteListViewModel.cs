using CommonServiceLocator;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Repository;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public class RouteListViewModel
    {
        private readonly IRepository<Route> _routeRepository = ServiceLocator.Current.GetInstance<
            IRepository<Route>
        >();

        public async Task<List<RouteViewModel>> GetAll() =>
            (await _routeRepository.GetAll()).Select(s => new RouteViewModel(s)).ToList();
    }
}
