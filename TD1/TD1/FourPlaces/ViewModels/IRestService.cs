using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TD1.FourPlaces.Models;

namespace TD1.FourPlaces.ViewModels
{
    internal interface IRestService
    {
        Task<ObservableCollection<Place>> loadPlaces();
    }
}