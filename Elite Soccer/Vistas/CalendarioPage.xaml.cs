using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Elite_Soccer.Vistas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CalendarioPage : ContentPage
	{
		public CalendarioPage ()
		{
			InitializeComponent ();
		}
        private async void Volver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync(); // Vuelve a PaginaUsuario
        }

    }
}