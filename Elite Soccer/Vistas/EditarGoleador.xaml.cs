using Elite_Soccer.Modelo;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;

namespace Elite_Soccer.Vistas
{
    public partial class EditarGoleador : ContentPage
    {
        private readonly Goleador goleador;
        private const string BaseUrl = "https://clubeliteapp-default-rtdb.firebaseio.com/goleadores/";
        private static readonly HttpClient cliente = new HttpClient();

        public EditarGoleador(Goleador goleador)
        {
            InitializeComponent();
            this.goleador = goleador;

            txtNombre.Text = goleador.nombre;
            txtEquipo.Text = goleador.equipo;
            txtGoles.Text = goleador.goles.ToString();
        }

        private async void BtnGuardar_Clicked(object sender, EventArgs e)
        {
            if (!int.TryParse(txtGoles.Text, out int nuevosGoles))
            {
                await DisplayAlert("Error", "Ingresa una cantidad válida", "OK");
                return;
            }

            goleador.goles = nuevosGoles;

            string json = JsonConvert.SerializeObject(goleador);
            string url = $"{BaseUrl}{goleador.IdFirebase}.json?auth={MainPage.IdTokenUsuario}";
            await cliente.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            await DisplayAlert("Actualizado", "Goles actualizados correctamente", "OK");
            await Navigation.PopAsync();
        }

        private async void BtnEliminar_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmar", "¿Eliminar este goleador?", "Sí", "No");

            if (confirm)
            {
                string url = $"{BaseUrl}{goleador.IdFirebase}.json?auth={MainPage.IdTokenUsuario}";
                await cliente.DeleteAsync(url);

                await DisplayAlert("Eliminado", "Goleador eliminado correctamente", "OK");
                await Navigation.PopAsync();
            }
        }
    }
}
