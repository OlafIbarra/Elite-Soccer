using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Elite_Soccer.Vistas
{
    public partial class PaginaUsuario : ContentPage
    {
        public PaginaUsuario()
        {
            InitializeComponent();
        }

        private async void VerGoleadores_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GoleadoresPage());
        }

        private async void VerTabla_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TablaPosicionesPage());
        }

        private async void VerCalendario_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CalendarioPage());
        }

        private async void CerrarSesion_Clicked(object sender, EventArgs e)
        {
     //       MainPage.IdTokenUsuario = null; // Limpia el token de sesión
            await Navigation.PopToRootAsync(); // Regresa al login
        }
        private async void Facebook_Clicked(object sender, EventArgs e)
        {
            string url = "https://www.facebook.com/EliteSoccerStadioMx/about?locale=es_LA"; // <-- cambia esta por la real
            try
            {
                await Launcher.OpenAsync(url);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No se pudo abrir la página de Facebook", "OK");
            }
        }


    }
}
