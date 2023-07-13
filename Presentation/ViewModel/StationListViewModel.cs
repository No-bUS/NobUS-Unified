using NobUS.DataContract.Model.Entity;
using NobUS.Frontend.MAUI.Repository;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public partial record StationListViewModel()
    {
        private readonly IRepository<Station> _stationRepository =
            CommonServiceLocator.ServiceLocator.Current.GetInstance<IRepository<Station>>();
        
        public async Task<List<StationViewModel>> GetAll()
        {
            return (await _stationRepository.GetAll()).Select(s => new StationViewModel(s)).ToList();
        }
    }
}
