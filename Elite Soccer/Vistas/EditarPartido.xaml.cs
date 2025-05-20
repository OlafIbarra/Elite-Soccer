using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Elite_Soccer.Modelo;

namespace Elite_Soccer.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditarPartido : ContentPage
    {
        private string partidoId;
        private Partido partidoOriginal;
        private const string dbBase = "https://clubeliteapp-default-rtdb.firebaseio.com/partidos/";
        private static readonly HttpClient cliente = new HttpClient();

        private readonly List<string> equiposVaronil = new List<string>
        {
            "FÉNIX", "MACRO PLAZA", "VALENCIA", "REAL PRIMERA", "AJAX", "BRASIL",
            "TAZOS DORADOS", "PUEBLA", "TDR", "PURO CHACOTEO", "SAN PACHO", "UNAM",
            "DVO. TACHIRA", "SAN RAFA", "ALEMANIA"
        };

        private readonly List<string> equiposFemenil = new List<string>
        {
            "TEAM INFIERNO", "MAJESTIC", "MIAMI", "BRASIL", "CHELSEA",
            "BARCELONA", "FENIX", "PUMAS", "TBT"
        };

        public EditarPartido(string id, Partido partido)
        {
            InitializeComponent();
            partidoId = id;
            partidoOriginal = partido;

            pickerCategoria.Items.Add("Varonil");
            pickerCategoria.Items.Add("Femenil");

            pickerCategoria.SelectedItem = partido.categoria;
            LlenarEquiposSegunCategoria(partido.categoria);

            pickerEquipoLocal.SelectedItem = partido.equipoLocal;
            pickerEquipoVisitante.SelectedItem = partido.equipoVisitante;

            if (DateTime.TryParse(partido.fecha, out DateTime fechaParsed))
                dateFecha.Date = fechaParsed;

            if (TimeSpan.TryParse(partido.hora, out TimeSpan horaParsed))
                timeHora.Time = horaParsed;
        }

        private void pickerCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            string categoria = pickerCategoria.SelectedItem as string;
            LlenarEquiposSegunCategoria(categoria);
        }

        private void LlenarEquiposSegunCategoria(string categoria)
        {
            if (categoria == "Varonil")
            {
                pickerEquipoLocal.ItemsSource = equiposVaronil;
                pickerEquipoVisitante.ItemsSource = equiposVaronil;
            }
            else if (categoria == "Femenil")
            {
                pickerEquipoLocal.ItemsSource = equiposFemenil;
                pickerEquipoVisitante.ItemsSource = equiposFemenil;
            }
        }

        private async void BtnGuardar_Clicked(object sender, EventArgs e)
        {
            var partidoActualizado = new Partido
            {
                categoria = pickerCategoria.SelectedItem as string,
                equipoLocal = pickerEquipoLocal.SelectedItem as string,
                equipoVisitante = pickerEquipoVisitante.SelectedItem as string,
                fecha = dateFecha.Date.ToString("yyyy-MM-dd"),
                hora = timeHora.Time.ToString(@"hh\:mm")
            };

            string json = JsonConvert.SerializeObject(partidoActualizado);
            string url = $"{dbBase}{partidoId}.json?auth={MainPage.IdTokenUsuario}";
            await cliente.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            await DisplayAlert("Éxito", "Partido actualizado", "OK");
            await Navigation.PopAsync();
        }

        private async void BtnEliminar_Clicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirmar", "¿Deseas eliminar este partido?", "Sí", "No");
            if (confirm)
            {
                string url = $"{dbBase}{partidoId}.json?auth={MainPage.IdTokenUsuario}";
                HttpResponseMessage resp = await cliente.DeleteAsync(url);

                if (resp.IsSuccessStatusCode)
                {
                    await DisplayAlert("Eliminado", "Partido eliminado correctamente", "OK");
                    await Navigation.PopAsync(); // <- Esto regresa a AdminPage y hace el refresh
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo eliminar el partido", "OK");
                }
            }
        }
        private async void Volver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
