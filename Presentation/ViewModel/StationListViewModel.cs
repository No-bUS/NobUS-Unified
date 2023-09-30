using CommonServiceLocator;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Repository;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public record StationListViewModel
    {
        private readonly IRepository<Station> _stationRepository =
            ServiceLocator.Current.GetInstance<IRepository<Station>>();

        public async Task<List<StationViewModel>> GetAll() =>
            (await _stationRepository.GetAll()).Select(s => new StationViewModel(s)).ToList();
    }
}
