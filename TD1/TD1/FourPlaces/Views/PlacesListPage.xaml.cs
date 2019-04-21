using Storm.Mvvm.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TD1.FourPlaces.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TD1.FourPlaces.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PlacesListPage : BaseContentPage
	{
		public PlacesListPage ()
		{
			InitializeComponent ();
            BindingContext = new PlacesListViewModel();
		}
        
    }
}