using System;
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

    }
}
