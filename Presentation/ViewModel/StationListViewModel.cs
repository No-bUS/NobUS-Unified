using NobUS.Frontend.MAUI.Repository;
using NobUS.DataContract.Model;
using System.Linq;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public partial record StationListViewModel()
    {
        private readonly IRepository<Station> _stationRepository =
            CommonServiceLocator.ServiceLocator.Current.GetInstance<IRepository<Station>>();

        public async Task<List<StationViewModel>> GetAll() => (await _stationRepository.GetAll()).Select(s => new StationViewModel(s)).ToList();
    }
}
